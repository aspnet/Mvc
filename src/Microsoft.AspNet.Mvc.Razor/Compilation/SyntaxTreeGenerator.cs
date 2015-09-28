// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Dnx.Compilation.CSharp;

namespace Microsoft.AspNet.Mvc.Razor.Compilation
{
    public static class SyntaxTreeGenerator
    {
        public static SyntaxTree Generate(
            string text,
            string path,
            CompilationSettings compilationSettings)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (compilationSettings == null)
            {
                throw new ArgumentNullException(nameof(compilationSettings));
            }

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