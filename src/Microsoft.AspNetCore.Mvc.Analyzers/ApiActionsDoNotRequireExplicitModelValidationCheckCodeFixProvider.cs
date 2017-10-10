// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class ApiActionsDoNotRequireExplicitModelValidationCheckCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(DiagnosticDescriptors.MVC1001_ApiActionsHaveBadModelStateFilter.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            const string title = "Remove ModelState.IsValid check";
            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    createChangedDocument: CreateChangedDocumentAsync,
                    equivalenceKey: title),
                context.Diagnostics);

            return Task.CompletedTask;

            async Task<Document> CreateChangedDocumentAsync(CancellationToken cancellationToken)
            {
                var rootNode = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
                var ifNode = rootNode.FindNode(context.Span);

                var editor = await DocumentEditor.CreateAsync(context.Document, cancellationToken).ConfigureAwait(false);
                editor.RemoveNode(ifNode);

                return editor.GetChangedDocument();
            }
        }
    }
}
