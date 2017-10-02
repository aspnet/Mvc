using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace KodKod.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ApiActionsDoNotRequireExplicitModelValidationCheckCodeFixProvider)), Shared]
    public class ApiActionsDoNotRequireExplicitModelValidationCheckCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(DiagnosticDescriptors.KK1001_ApiActionsHaveBadModelStateFilter.Id);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var rootNode = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            const string title = "Remove ModelState.IsValid check";
            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    createChangedDocument: CreateChangedDocumentAsync,
                    equivalenceKey: title),
                context.Diagnostics);

            async Task<Document> CreateChangedDocumentAsync(CancellationToken cancellationToken)
            {
                var ifNode = rootNode.FindNode(context.Span);

                var editor = await DocumentEditor.CreateAsync(context.Document, cancellationToken).ConfigureAwait(false);
                editor.RemoveNode(ifNode);

                return editor.GetChangedDocument();
            }
        }
    }
}
