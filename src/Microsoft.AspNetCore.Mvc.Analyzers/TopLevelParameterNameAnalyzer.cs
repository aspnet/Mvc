// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TopLevelParameterNameAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            DiagnosticDescriptors.MVC1004_ParameterNameCollidesWithTopLevelProperty);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationStartAnalysisContext =>
            {
                var typeCache = new SymbolCache(compilationStartAnalysisContext.Compilation);
                if (typeCache.ControllerAttribute == null || typeCache.ControllerAttribute.TypeKind == TypeKind.Error)
                {
                    // No-op if we can't find types we care about.
                    return;
                }

                InitializeWorker(compilationStartAnalysisContext, typeCache);
            });
        }

        private void InitializeWorker(CompilationStartAnalysisContext compilationStartAnalysisContext, SymbolCache symbolCache)
        {
            compilationStartAnalysisContext.RegisterSymbolAction(symbolAnalysisContext =>
            {
                var method = (IMethodSymbol)symbolAnalysisContext.Symbol;
                if (method.MethodKind != MethodKind.Ordinary)
                {
                    return;
                }

                if (method.Parameters.Length == 0)
                {
                    return;
                }

                if (!MvcFacts.IsController(method.ContainingType, symbolCache.ControllerAttribute, symbolCache.NonControllerAttribute) &&
                    !MvcFacts.IsControllerAction(method, symbolCache.NonActionAttribute, symbolCache.IDisposableDispose))
                {
                    return;
                }

                if (method.ContainingType.HasAttribute(symbolCache.IApiBehaviorMetadata, inherit: true))
                {
                    // Don't execute the analyzer on ApiController instances.
                    return;
                }

                for (var i = 0; i < method.Parameters.Length; i++)
                {
                    var parameter = method.Parameters[0];
                    if (HasProblematicTopLevelProperty(parameter.Type, parameter.Name))
                    {
                        var location = parameter.Locations.Length != 0 ?
                            parameter.Locations[0] :
                            Location.None;

                        symbolAnalysisContext.ReportDiagnostic(
                            Diagnostic.Create(
                                DiagnosticDescriptors.MVC1004_ParameterNameCollidesWithTopLevelProperty,
                                location,
                                parameter.Type.Name,
                                parameter.Name));
                    }
                }
            }, SymbolKind.Method);
        }

        private bool HasProblematicTopLevelProperty(ITypeSymbol type, string name)
        {
            while (type != null)
            {
                foreach (var member in type.GetMembers())
                {
                    if (member.DeclaredAccessibility == Accessibility.Public &&
                        !member.IsStatic &&
                        member.Kind == SymbolKind.Property &&
                        string.Equals(name, member.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                type = type.BaseType;
            }

            return false;
        }

        private readonly struct SymbolCache
        {
            public SymbolCache(Compilation compilation)
            {
                ControllerAttribute = compilation.GetTypeByMetadataName(SymbolNames.ControllerAttribute);
                IApiBehaviorMetadata = compilation.GetTypeByMetadataName(SymbolNames.IApiBehaviorMetadata);
                NonControllerAttribute = compilation.GetTypeByMetadataName(SymbolNames.ControllerAttribute);
                NonActionAttribute = compilation.GetTypeByMetadataName(SymbolNames.NonActionAttribute);

                var disposable = compilation.GetSpecialType(SpecialType.System_IDisposable);
                var members = disposable.GetMembers(nameof(IDisposable.Dispose));
                IDisposableDispose = members.Length == 1 ? (IMethodSymbol)members[0] : null;
            }

            public INamedTypeSymbol ControllerAttribute { get; }
            public INamedTypeSymbol IApiBehaviorMetadata { get; }
            public INamedTypeSymbol NonControllerAttribute { get; }
            public INamedTypeSymbol NonActionAttribute { get; }
            public IMethodSymbol IDisposableDispose { get; }
        }
    }
}
