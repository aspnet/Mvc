using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TopLevelParameterAttributeAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            DiagnosticDescriptors.MVC1005_ParameterAttributeAvoidUsingRequiredOncollections);

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

                if (!MvcFacts.IsController(method.ContainingType, symbolCache.ControllerAttribute, symbolCache.NonControllerAttribute) ||
                    !MvcFacts.IsControllerAction(method, symbolCache.NonActionAttribute, symbolCache.IDisposableDispose))
                {
                    return;
                }

                for (var i = 0; i < method.Parameters.Length; i++)
                {
                    var parameter = method.Parameters[i];

                    if (parameter.GetType().Name == SymbolNames.CollectionType)
                    {
                        if (IsProblematicParameter(parameter))
                        {
                            var location = parameter.Locations.Length != 0 ?
                                parameter.Locations[0] :
                                Location.None;

                            symbolAnalysisContext.ReportDiagnostic(
                                Diagnostic.Create(
                                    DiagnosticDescriptors.MVC1005_ParameterAttributeAvoidUsingRequiredOncollections,
                                    location,
                                    SymbolNames.RequiredAttribute));
                        }
                    }
                }


            }, SymbolKind.Method);
        }

        private bool IsProblematicParameter(IParameterSymbol parameter)
        {
            return parameter.GetAttributes().Any(Attribute => Equals(Attribute.AttributeClass.MetadataName, SymbolNames.RequiredAttribute));
        }

        internal readonly struct SymbolCache
        {
            public SymbolCache(Compilation compilation)
            {
                ControllerAttribute = compilation.GetTypeByMetadataName(SymbolNames.ControllerAttribute);
                NonControllerAttribute = compilation.GetTypeByMetadataName(SymbolNames.NonControllerAttribute);
                NonActionAttribute = compilation.GetTypeByMetadataName(SymbolNames.NonActionAttribute);

                var disposable = compilation.GetSpecialType(SpecialType.System_IDisposable);
                var members = disposable.GetMembers(nameof(IDisposable.Dispose));
                IDisposableDispose = members.Length == 1 ? (IMethodSymbol)members[0] : null;
            }

            public INamedTypeSymbol ControllerAttribute { get; }
            public INamedTypeSymbol NonControllerAttribute { get; }
            public INamedTypeSymbol NonActionAttribute { get; }

            public IMethodSymbol IDisposableDispose { get; }
        }
    }
}
