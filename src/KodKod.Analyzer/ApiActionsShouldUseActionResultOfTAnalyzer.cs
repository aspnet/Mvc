using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace KodKod.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ApiActionsShouldUseActionResultOfTAnalyzer : KodKodAnalyzerAnalyzerBase
    {
        public ApiActionsShouldUseActionResultOfTAnalyzer()
            : base(DiagnosticDescriptors.KK1002_ApiActionsShouldReturnActionResultOf)
        {
        }

        protected override void InitializeWorker(KodKodContext kodKodContext)
        {
            kodKodContext.Context.RegisterSyntaxNodeAction(context =>
            {
                var methodSyntax = (MethodDeclarationSyntax)context.Node;
                var method = context.SemanticModel.GetDeclaredSymbol(methodSyntax, context.CancellationToken);
                if (!kodKodContext.IsKodKod(method))
                {
                    return;
                }

                if (method.ReturnsVoid || method.ReturnType.Kind != SymbolKind.NamedType)
                {
                    return;
                }

                var declaredReturnType = method.ReturnType;
                var namedReturnType = (INamedTypeSymbol)method.ReturnType;
                if (namedReturnType.IsGenericType && namedReturnType.TypeArguments.Length == 1)
                {
                    // Attempt to unwrap any generic type with exactly one parameter.
                    declaredReturnType = namedReturnType.TypeArguments[0];
                }

                if (!declaredReturnType.AllInterfaces.Any(i => i == kodKodContext.IActionResult))
                {
                    // Method signature does not look like IActionResult MyAction or SomeAwaitable<IActionResult>.
                    // Nothing to do here.
                    return;
                }

                // Method returns an IActionResult. Determine if the method block returns an ObjectResult
                foreach (var returnStatement in methodSyntax.DescendantNodes().OfType<ReturnStatementSyntax>())
                {
                    var returnType = context.SemanticModel.GetTypeInfo(returnStatement.Expression, context.CancellationToken);
                    if (returnType.Type == null || returnType.Type.Kind == SymbolKind.ErrorType)
                    {
                        continue;
                    }

                    if (IsAssignableFrom(returnType.Type, kodKodContext.ObjectResult))
                    {
                        ImmutableDictionary<string, string> properties = null;
                        // Check if the method signature looks like "return Ok(userModelInstance)". If so, we can infer the type of userModelInstance
                        if (returnStatement.Expression is InvocationExpressionSyntax invocation &&
                            invocation.ArgumentList.Arguments.Count == 1)
                        {
                            var typeInfo = context.SemanticModel.GetTypeInfo(invocation.ArgumentList.Arguments[0].Expression);
                            var actionResultOfModel = kodKodContext.ActionResultOfT.Construct(typeInfo.Type);

                            var builder = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
                            builder.Add("ReturnType", actionResultOfModel.ToMinimalDisplayString(context.SemanticModel, methodSyntax.ReturnType.SpanStart));
                            properties = builder.ToImmutable();
                        }

                        context.ReportDiagnostic(Diagnostic.Create(
                            SupportedDiagnostic,
                            returnStatement.Expression.GetLocation(),
                            properties: properties));
                    }
                }
            }, SyntaxKind.MethodDeclaration);
        }

        private static bool IsAssignableFrom(ITypeSymbol source, INamedTypeSymbol target)
        {
            do
            {
                if (source == target)
                {
                    return true;
                }

                source = source.BaseType;
            } while (source != null);

            return false;
        }
    }
}
