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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ApiActionsShouldUseActionResultOfTCodeFixProvider)), Shared]
    public class ApiActionsShouldUseActionResultOfTCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(DiagnosticDescriptors.KK1002_ApiActionsShouldReturnActionResultOf.Id);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Properties.TryGetValue("ReturnType", out var returnTypeName))
                {
                    var title = $"Make return type {returnTypeName}";
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title,
                            createChangedDocument: cancellationToken => CreateChangedDocumentAsync(returnTypeName, cancellationToken),
                            equivalenceKey: title),
                        context.Diagnostics);
                }
            }

            return Task.CompletedTask;

            async Task<Document> CreateChangedDocumentAsync(string returnTypeName, CancellationToken cancellationToken)
            {
                var rootNode = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
                var returnStatement = (InvocationExpressionSyntax)rootNode.FindNode(context.Span);
                var methodDeclaration = returnStatement.FirstAncestorOrSelf<MethodDeclarationSyntax>();

                var editor = await DocumentEditor.CreateAsync(context.Document, cancellationToken).ConfigureAwait(false);
                editor.ReplaceNode(methodDeclaration.ReturnType, SyntaxFactory.IdentifierName(returnTypeName));

                return editor.GetChangedDocument();
            }
        }
    }
}
