using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ModelPropertyRequiredAttribueBindRequiredAttributeAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            DiagnosticDescriptors.MVC1005_AttributeAvoidUsingRequiredAndBindRequired);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationStartAnalysisContext =>
            {
                InitializeWorker(compilationStartAnalysisContext);
            });
        }

        private void InitializeWorker(CompilationStartAnalysisContext compilationStartAnalysisContext)
        {
            compilationStartAnalysisContext.RegisterSymbolAction(symbolAnalysisContext =>
            {
                var property = (IPropertySymbol)symbolAnalysisContext.Symbol;

                if (property.DeclaredAccessibility != Accessibility.Public ||
                        property.IsStatic )
                {
                    return;
                }

                if (property.GetAttributes().Length == 0)
                {
                    return;
                }

                var typeCache = new SymbolCache(symbolAnalysisContext.Compilation);
                var location = property.Locations.Length != 0 ?
                               property.Locations[0] :
                               Location.None;

                if (IsNonNullableValueType(property.Type, typeCache.SystemNullableType) && property.HasAttribute(typeCache.RequiredAttribute, inherit: false))
                {
                    {
                        symbolAnalysisContext.ReportDiagnostic(
                            Diagnostic.Create(
                                DiagnosticDescriptors.MVC1005_AttributeAvoidUsingRequiredAndBindRequired,
                                location,
                                "RequiredAttribute", "BindRequiredAttribute"));
                    }
                }
                else if (IsComplexType(property.Type) && property.HasAttribute(typeCache.BindRequiredAttribute, inherit: false))
                {
                    symbolAnalysisContext.ReportDiagnostic(
                            Diagnostic.Create(
                                DiagnosticDescriptors.MVC1005_AttributeAvoidUsingRequiredAndBindRequired,
                                location,
                                "BindRequiredAttribute", "RequiredAttribute"));
                }


            }, SymbolKind.Property);
        }

        public static bool IsComplexType(ITypeSymbol type) => type.SpecialType == SpecialType.None;

        public static bool IsNonNullableValueType(ITypeSymbol type, INamedTypeSymbol systemNullableType)
        {
            return !type.IsReferenceType && !(type.OriginalDefinition.Equals(systemNullableType));
        }

        internal readonly struct SymbolCache
        {
            public SymbolCache(Compilation compilation)
            {
                RequiredAttribute = compilation.GetTypeByMetadataName(SymbolNames.RequiredAttribute);
                BindRequiredAttribute = compilation.GetTypeByMetadataName(SymbolNames.BindRequiredAttribute);
                SystemNullableType = compilation.GetSpecialType(SpecialType.System_Nullable_T);
            }

            public ITypeSymbol BindRequiredAttribute { get; }
            public INamedTypeSymbol SystemNullableType { get; }
            public INamedTypeSymbol RequiredAttribute { get; }
        }
    }
}
