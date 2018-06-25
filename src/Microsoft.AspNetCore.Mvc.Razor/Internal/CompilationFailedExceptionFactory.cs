﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    internal static class CompilationFailedExceptionFactory
    {
        // error CS0234: The type or namespace name 'C' does not exist in the namespace 'N' (are you missing
        // an assembly reference?)
        private const string CS0234 = nameof(CS0234);
        // error CS0246: The type or namespace name 'T' could not be found (are you missing a using directive
        // or an assembly reference?)
        private const string CS0246 = nameof(CS0246);

        public static CompilationFailedException Create(
            RazorCodeDocument codeDocument,
            IEnumerable<RazorDiagnostic> diagnostics)
        {
            // If a SourceLocation does not specify a file path, assume it is produced from parsing the current file.
            var messageGroups = diagnostics.GroupBy(
                razorError => razorError.Span.FilePath ?? codeDocument.Source.FilePath,
                StringComparer.Ordinal);

            var failures = new List<CompilationFailure>();
            foreach (var group in messageGroups)
            {
                var filePath = group.Key;
                var fileContent = ReadContent(codeDocument, filePath);
                var compilationFailure = new CompilationFailure(
                    filePath,
                    fileContent,
                    compiledContent: string.Empty,
                    messages: group.Select(parserError => CreateDiagnosticMessage(parserError, filePath)));
                failures.Add(compilationFailure);
            }

            return new CompilationFailedException(failures);
        }

        public static CompilationFailedException Create(
            RazorCodeDocument codeDocument,
            string compilationContent,
            string assemblyName,
            IEnumerable<Diagnostic> diagnostics)
        {
            var diagnosticGroups = diagnostics
                .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
                .GroupBy(diagnostic => GetFilePath(codeDocument, diagnostic), StringComparer.Ordinal);

            var failures = new List<CompilationFailure>();
            foreach (var group in diagnosticGroups)
            {
                var sourceFilePath = group.Key;
                string sourceFileContent;
                if (string.Equals(assemblyName, sourceFilePath, StringComparison.Ordinal))
                {
                    // The error is in the generated code and does not have a mapping line pragma
                    sourceFileContent = compilationContent;
                    sourceFilePath = Resources.GeneratedCodeFileName;
                }
                else
                {
                    sourceFileContent = ReadContent(codeDocument, sourceFilePath);
                }

                string additionalMessage = null;
                if (group.Any(g =>
                    string.Equals(CS0234, g.Id, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(CS0246, g.Id, StringComparison.OrdinalIgnoreCase)))
                {
                    additionalMessage = Resources.FormatCompilation_MissingReferences(
                        "CopyRefAssembliesToPublishDirectory");
                }

                var compilationFailure = new CompilationFailure(
                    sourceFilePath,
                    sourceFileContent,
                    compilationContent,
                    group.Select(GetDiagnosticMessage),
                    additionalMessage);

                failures.Add(compilationFailure);
            }

            return new CompilationFailedException(failures);
        }

        private static string ReadContent(RazorCodeDocument codeDocument, string filePath)
        {
            RazorSourceDocument sourceDocument;
            if (string.IsNullOrEmpty(filePath) || string.Equals(codeDocument.Source.FilePath, filePath, StringComparison.Ordinal))
            {
                sourceDocument = codeDocument.Source;
            }
            else
            {
                sourceDocument = codeDocument.Imports.FirstOrDefault(f => string.Equals(f.FilePath, filePath, StringComparison.Ordinal));
            }

            if (sourceDocument != null)
            {
                var contentChars = new char[sourceDocument.Length];
                sourceDocument.CopyTo(0, contentChars, 0, sourceDocument.Length);
                return new string(contentChars);
            }

            return string.Empty;
        }

        private static DiagnosticMessage GetDiagnosticMessage(Diagnostic diagnostic)
        {
            var mappedLineSpan = diagnostic.Location.GetMappedLineSpan();
            return new DiagnosticMessage(
                diagnostic.GetMessage(),
                CSharpDiagnosticFormatter.Instance.Format(diagnostic),
                mappedLineSpan.Path,
                mappedLineSpan.StartLinePosition.Line + 1,
                mappedLineSpan.StartLinePosition.Character + 1,
                mappedLineSpan.EndLinePosition.Line + 1,
                mappedLineSpan.EndLinePosition.Character + 1);
        }

        private static DiagnosticMessage CreateDiagnosticMessage(
            RazorDiagnostic razorDiagnostic,
            string filePath)
        {
            var sourceSpan = razorDiagnostic.Span;
            var message = razorDiagnostic.GetMessage();
            return new DiagnosticMessage(
                message: message,
                formattedMessage: razorDiagnostic.ToString(),
                filePath: filePath,
                startLine: sourceSpan.LineIndex + 1,
                startColumn: sourceSpan.CharacterIndex,
                endLine: sourceSpan.LineIndex + 1,
                endColumn: sourceSpan.CharacterIndex + sourceSpan.Length);
        }

        private static string GetFilePath(RazorCodeDocument codeDocument, Diagnostic diagnostic)
        {
            if (diagnostic.Location == Location.None)
            {
                return codeDocument.Source.FilePath;
            }

            return diagnostic.Location.GetMappedLineSpan().Path;
        }
    }
}
