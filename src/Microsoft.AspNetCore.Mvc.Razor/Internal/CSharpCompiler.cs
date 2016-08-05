// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    public class CSharpCompiler
    {
        private readonly CSharpCompilationOptions _compilationOptions;
        private readonly CSharpParseOptions _parseOptions;
        private readonly RazorReferenceManager _referenceManager;
        private readonly Action<RoslynCompilationContext> _compilationCallback;
        private readonly DebugInformationFormat _pdbFormat =
#if NET451
            SymbolsUtility.SupportsFullPdbGeneration() ?
                DebugInformationFormat.Pdb :
                DebugInformationFormat.PortablePdb;
#else
            DebugInformationFormat.PortablePdb;
#endif

        public CSharpCompiler(RazorReferenceManager manager, IOptions<RazorViewEngineOptions> optionsAccessor)
        {
            _referenceManager = manager;
            _compilationOptions = optionsAccessor.Value.CompilationOptions;
            _parseOptions = optionsAccessor.Value.ParseOptions;
            _compilationCallback = optionsAccessor.Value.CompilationCallback;
            EmitOptions = new EmitOptions(debugInformationFormat: _pdbFormat);
        }

        public EmitOptions EmitOptions { get; } 

        public SyntaxTree CreateSyntaxTree(SourceText sourceText)
        {
            return CSharpSyntaxTree.ParseText(
                sourceText,
                options: _parseOptions);
        }

        public CSharpCompilation CreateCompilation(string assemblyName)
        {
            return CSharpCompilation.Create(
                assemblyName,
                options: _compilationOptions,
                references: _referenceManager.CompilationReferences);
        }

        public CSharpCompilation ProcessCompilation(CSharpCompilation compilation)
        {
            var compilationContext = new RoslynCompilationContext(compilation);
            Rewrite(compilationContext);
            _compilationCallback(compilationContext);

            return compilationContext.Compilation;
        }

        private void Rewrite(RoslynCompilationContext compilationContext)
        {
            var compilation = compilationContext.Compilation;
            var rewrittenTrees = new List<SyntaxTree>();
            foreach (var tree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(tree, ignoreAccessibility: true);
                var rewriter = new ExpressionRewriter(semanticModel);

                var rewrittenTree = tree.WithRootAndOptions(rewriter.Visit(tree.GetRoot()), tree.Options);
                rewrittenTrees.Add(rewrittenTree);
            }

            compilationContext.Compilation = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(rewrittenTrees);
        }
    }
}
