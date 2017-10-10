// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ApiActionsShouldUseActionResultOfTAnalyzer : ApiControllerAnalyzerBase
    {
        public static readonly string ReturnTypeKey = "ReturnType";

        public ApiActionsShouldUseActionResultOfTAnalyzer()
            : base(DiagnosticDescriptors.MVC1002_ApiActionsShouldReturnActionResultOf)
        {
        }

        protected override void InitializeWorker(ApiControllerAnalyzerContext ApiControllerAnalyzerContext)
        {
            ApiControllerAnalyzerContext.Context.RegisterSyntaxNodeAction(context =>
            {
                var methodSyntax = (MethodDeclarationSyntax)context.Node;
                var method = context.SemanticModel.GetDeclaredSymbol(methodSyntax, context.CancellationToken);
                if (!ApiControllerAnalyzerContext.IsApiAction(method))
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

                if (!declaredReturnType.IsAssignableFrom(ApiControllerAnalyzerContext.IActionResult))
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

                    if (returnType.Type.IsAssignableFrom(ApiControllerAnalyzerContext.ObjectResult))
                    {
                        ImmutableDictionary<string, string> properties = null;
                        // Check if the method signature looks like "return Ok(userModelInstance)". If so, we can infer the type of userModelInstance
                        if (returnStatement.Expression is InvocationExpressionSyntax invocation &&
                            invocation.ArgumentList.Arguments.Count == 1)
                        {
                            var typeInfo = context.SemanticModel.GetTypeInfo(invocation.ArgumentList.Arguments[0].Expression);
                            var actionResultOfModel = ApiControllerAnalyzerContext.ActionResultOfT.Construct(typeInfo.Type);
                            var actionResultModelName = actionResultOfModel.ToMinimalDisplayString(
                                context.SemanticModel, 
                                methodSyntax.ReturnType.SpanStart);

                            properties = ImmutableDictionary.Create<string, string>(StringComparer.Ordinal)
                                .Add(ReturnTypeKey, actionResultModelName);
                        }

                        context.ReportDiagnostic(Diagnostic.Create(
                            SupportedDiagnostic,
                            returnStatement.Expression.GetLocation(),
                            properties: properties));
                    }
                }
            }, SyntaxKind.MethodDeclaration);
        }
    }
}
