// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    /// <summary>
    /// Default implementation of <see cref="IRazorCompilationService"/>.
    /// </summary>
    public class RazorCompilationService : IRazorCompilationService
    {
        private readonly ICompilationService _compilationService;
        private readonly IMvcRazorHost _razorHost;
        private readonly IMvcRazorHostWithTemplateEngineContext _razorHostWithContext;
        private readonly IFileProvider _fileProvider;
        private readonly ILogger _logger;

        /// <summary>
        /// Instantiates a new instance of the <see cref="RazorCompilationService"/> class.
        /// </summary>
        /// <param name="compilationService">The <see cref="ICompilationService"/> to compile generated code.</param>
        /// <param name="razorHost">The <see cref="IMvcRazorHost"/> to generate code from Razor files.</param>
        /// <param name="fileProviderAccessor">The <see cref="IRazorViewEngineFileProviderAccessor"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public RazorCompilationService(
            ICompilationService compilationService,
            IMvcRazorHost razorHost,
            IRazorViewEngineFileProviderAccessor fileProviderAccessor,
            ILoggerFactory loggerFactory)
        {
            _compilationService = compilationService;
            _razorHost = razorHost;
            _razorHostWithContext = razorHost as IMvcRazorHostWithTemplateEngineContext;
            _fileProvider = fileProviderAccessor.FileProvider;
            _logger = loggerFactory.CreateLogger<RazorCompilationService>();
        }

        /// <inheritdoc />
        public CompilationResult Compile(RelativeFileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            var filePath = file.FileInfo.PhysicalPath ?? file.RelativePath;
            GeneratorResults results;
            using (var inputStream = file.FileInfo.CreateReadStream())
            {
                _logger.RazorFileToCodeCompilationStart(filePath);

                var startTimestamp = _logger.IsEnabled(LogLevel.Debug) ? Stopwatch.GetTimestamp() : 0;

                if (_razorHostWithContext != null)
                {
                    var context = new TemplateEngineContext(inputStream, filePath)
                    {
                        RelativePath = file.RelativePath,
                    };

                    results = _razorHostWithContext.GenerateCode(context);
                }
                else
                {
                    results = _razorHost.GenerateCode(filePath, inputStream);
                }

                _logger.RazorFileToCodeCompilationEnd(filePath, startTimestamp);
            }

            if (!results.Success)
            {
                return GetCompilationFailedResult(file, results.ParserErrors);
            }

            return _compilationService.Compile(file, results.GeneratedCode);
        }

        // Internal for unit testing
        internal CompilationResult GetCompilationFailedResult(RelativeFileInfo file, IEnumerable<RazorError> errors)
        {
            // If a SourceLocation does not specify a file path, assume it is produced
            // from parsing the current file.
            var messageGroups = errors
                .GroupBy(
                    razorError => razorError.Location.FilePath ?? file.RelativePath,
                    StringComparer.Ordinal);

            var failures = new List<CompilationFailure>();
            foreach (var group in messageGroups)
            {
                var filePath = group.Key;
                var fileContent = ReadFileContentsSafely(file.FileInfo);
                var compilationFailure = new CompilationFailure(
                    filePath,
                    fileContent,
                    compiledContent: string.Empty,
                    messages: group.Select(parserError => CreateDiagnosticMessage(parserError, filePath)));
                failures.Add(compilationFailure);
            }

            return new CompilationResult(failures);
        }

        private DiagnosticMessage CreateDiagnosticMessage(RazorError error, string filePath)
        {
            var location = error.Location;
            return new DiagnosticMessage(
                message: error.Message,
                formattedMessage: $"{error} ({location.LineIndex},{location.CharacterIndex}) {error.Message}",
                filePath: filePath,
                startLine: error.Location.LineIndex + 1,
                startColumn: error.Location.CharacterIndex,
                endLine: error.Location.LineIndex + 1,
                endColumn: error.Location.CharacterIndex + error.Length);
        }

        private string ReadFileContentsSafely(IFileInfo fileInfo)
        {
            if (fileInfo.Exists)
            {
                try
                {
                    using (var reader = new StreamReader(fileInfo.CreateReadStream()))
                    {
                        return reader.ReadToEnd();
                    }
                }
                catch
                {
                    // Ignore any failures
                }
            }

            return null;
        }
    }
}
