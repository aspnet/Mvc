// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Dnx.Compilation.CSharp;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Razor.Compilation
{
    public static class SyntaxTreeGenerator
    {
        public static SyntaxTree Generate([NotNull] string text,
                                          [NotNull] string path,
                                          [NotNull] CompilationSettings compilationSettings)
        {
            var sourceText = SourceText.From(text, Encoding.UTF8);
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceText,
                path: path,
                options: GetParseOptions(compilationSettings));

            return syntaxTree;
        }

        public static CSharpParseOptions GetParseOptions(CompilationSettings compilationSettings)
        {
            return new CSharpParseOptions(
               languageVersion: compilationSettings.LanguageVersion,
               preprocessorSymbols: compilationSettings.Defines);
        }
    }
}