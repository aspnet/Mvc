using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace KodKod.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ApiActionsAreAttributeRoutedFixProvider)), Shared]
    public class ApiActionsAreAttributeRoutedFixProvider : CodeFixProvider
    {
        private const string HttpGetAttribute = "Microsoft.AspNetCore.Mvc.HttpGetAttribute";
        private const string HttpPostAttribute = "Microsoft.AspNetCore.Mvc.HttpPostAttribute";
        private const string HttpPutAttribute = "Microsoft.AspNetCore.Mvc.HttpPutAttribute";
        private const string HttpDeleteAttribute = "Microsoft.AspNetCore.Mvc.HttpDeleteAttribute";

        private static readonly Dictionary<string, string> Attributes = new Dictionary<string, string>
        {
            { "HttpGet", HttpGetAttribute },
            { "HttpPost", HttpPostAttribute },
            { "HttpPut", HttpPutAttribute },
            { "HttpDelete", HttpDeleteAttribute },
        };

        private static readonly Dictionary<string, string> KeyWordMappings = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "Get", HttpGetAttribute },
            { "Find", HttpGetAttribute },
            { "Post", HttpPostAttribute},
            { "Put", HttpPutAttribute },
            { "Delete", HttpDeleteAttribute },
        };

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(DiagnosticDescriptors.KK1000_ApiActionsMustBeAttributeRouted.Id);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var methodName = diagnostic.Properties["MethodName"];
            var idParameter = diagnostic.Properties["IdParameterName"];

            foreach (var mapping in KeyWordMappings)
            {
                if (methodName.StartsWith(mapping.Key, StringComparison.Ordinal))
                {
                    var title = $"Add {mapping.Key} attribute";
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title,
                            createChangedDocument: cancellationToken => CreateChangedDocumentAsync(mapping.Value, cancellationToken),
                            equivalenceKey: title),
                        context.Diagnostics);

                    return Task.CompletedTask;
                }
            }

            foreach (var attribute in Attributes)
            {
                var title = $"Add {attribute.Key} attribute";
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title,
                        createChangedDocument: cancellationToken => CreateChangedDocumentAsync(attribute.Value, cancellationToken),
                        equivalenceKey: title),
                    context.Diagnostics);
            }

            return Task.CompletedTask;

            async Task<Document> CreateChangedDocumentAsync(string attributeName, CancellationToken cancellationToken)
            {
                var rootNode = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
                var methodNode = (MethodDeclarationSyntax)rootNode.FindNode(context.Span);

                var editor = await DocumentEditor.CreateAsync(context.Document, cancellationToken).ConfigureAwait(false);

                var attributeMetadata = editor.SemanticModel.Compilation.GetTypeByMetadataName(attributeName);
                attributeName = attributeMetadata.ToMinimalDisplayString(editor.SemanticModel, methodNode.SpanStart);
                attributeName = attributeName.Replace("Attribute", null);

                var route = methodName;
                if (idParameter != null)
                {
                    route += "/{" + idParameter + "}";
                }

                var attribute = SyntaxFactory.Attribute(
                    SyntaxFactory.ParseName(attributeName),
                    SyntaxFactory.AttributeArgumentList()
                        .AddArguments(SyntaxFactory.AttributeArgument(
                            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(route)))));
                editor.AddAttribute(methodNode, attribute);

                return editor.GetChangedDocument();
            }
        }
    }
}
