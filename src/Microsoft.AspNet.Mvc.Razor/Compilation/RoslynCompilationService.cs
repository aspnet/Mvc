// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Mvc.Razor.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Dnx.Compilation;
using Microsoft.Dnx.Compilation.CSharp;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Internal;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc.Razor.Compilation
{
    /// <summary>
    /// A type that uses Roslyn to compile C# content.
    /// </summary>
    public class RoslynCompilationService : ICompilationService
    {
        private readonly Lazy<bool> _supportsPdbGeneration = new Lazy<bool>(SymbolsUtility.SupportsSymbolsGeneration);
        private readonly ConcurrentDictionary<string, AssemblyMetadata> _metadataFileCache =
            new ConcurrentDictionary<string, AssemblyMetadata>(StringComparer.OrdinalIgnoreCase);
        private readonly Func<IMetadataFileReference, AssemblyMetadata> _assemblyMetadataFactory;

        private readonly ILibraryExporter _libraryExporter;
        private readonly IApplicationEnvironment _environment;
        private readonly IAssemblyLoadContext _loader;
        private readonly ICompilerOptionsProvider _compilerOptionsProvider;
        private readonly IFileProvider _fileProvider;
        private readonly Lazy<List<MetadataReference>> _applicationReferences;
        private readonly string _classPrefix;

        /// <summary>
        /// Initalizes a new instance of the <see cref="RoslynCompilationService"/> class.
        /// </summary>
        /// <param name="environment">The environment for the executing application.</param>
        /// <param name="loaderAccessor">
        /// The accessor for the <see cref="IAssemblyLoadContext"/> used to load compiled assemblies.
        /// </param>
        /// <param name="libraryExporter">The <see cref="ILibraryExporter"/>.</param>
        /// <param name="compilerOptionsProvider">
        /// The <see cref="ICompilerOptionsProvider"/> that provides Roslyn compilation settings.
        /// </param>
        /// <param name="host">The <see cref="IMvcRazorHost"/> that was used to generate the code.</param>
        /// <param name="optionsAccessor">Accessor to <see cref="RazorViewEngineOptions"/>.</param>
        public RoslynCompilationService(
            IApplicationEnvironment environment,
            IAssemblyLoadContextAccessor loaderAccessor,
            ILibraryExporter libraryExporter,
            ICompilerOptionsProvider compilerOptionsProvider,
            IMvcRazorHost host,
            IOptions<RazorViewEngineOptions> optionsAccessor)
        {
            _environment = environment;
            _loader = loaderAccessor.GetLoadContext(typeof(RoslynCompilationService).GetTypeInfo().Assembly);
            _libraryExporter = libraryExporter;
            _applicationReferences = new Lazy<List<MetadataReference>>(GetApplicationReferences);
            _compilerOptionsProvider = compilerOptionsProvider;
            _fileProvider = optionsAccessor.Value.FileProvider;
            _classPrefix = host.MainClassNamePrefix;
            _assemblyMetadataFactory = GetAssemblyMetadata;
        }

        /// <inheritdoc />
        public CompilationResult Compile([NotNull] RelativeFileInfo fileInfo, [NotNull] string compilationContent)
        {
            var assemblyName = Path.GetRandomFileName();
            var compilationSettings = _compilerOptionsProvider.GetCompilationSettings(_environment);
            var syntaxTree = SyntaxTreeGenerator.Generate(compilationContent,
                                                          assemblyName,
                                                          compilationSettings);
            var references = _applicationReferences.Value;

            var compilationOptions = compilationSettings.CompilationOptions
                                                        .WithOutputKind(OutputKind.DynamicallyLinkedLibrary);

            var compilation = CSharpCompilation.Create(assemblyName,
                        options: compilationOptions,
                        syntaxTrees: new[] { syntaxTree },
                        references: references);

            using (var ms = new MemoryStream())
            {
                using (var pdb = new MemoryStream())
                {
                    EmitResult result;

                    if (_supportsPdbGeneration.Value)
                    {
                        result = compilation.Emit(ms, pdbStream: pdb);
                    }
                    else
                    {
                        result = compilation.Emit(ms);
                    }

                    if (!result.Success)
                    {
                        return GetCompilationFailedResult(
                            fileInfo.RelativePath,
                            compilationContent,
                            assemblyName,
                            result.Diagnostics);
                    }

                    Assembly assembly;
                    ms.Seek(0, SeekOrigin.Begin);

                    if (_supportsPdbGeneration.Value)
                    {
                        pdb.Seek(0, SeekOrigin.Begin);
                        assembly = _loader.LoadStream(ms, pdb);
                    }
                    else
                    {
                        assembly = _loader.LoadStream(ms, assemblySymbols: null);
                    }

                    var type = assembly.GetExportedTypes()
                                       .First(t => t.Name.StartsWith(_classPrefix, StringComparison.Ordinal));

                    return UncachedCompilationResult.Successful(type, compilationContent);
                }
            }
        }

        // Internal for unit testing
        internal CompilationResult GetCompilationFailedResult(
            string relativePath,
            string compilationContent,
            string assemblyName,
            IEnumerable<Diagnostic> diagnostics)
        {
            var diagnosticGroups = diagnostics
                .Where(IsError)
                .GroupBy(diagnostic => GetFilePath(relativePath, diagnostic), StringComparer.Ordinal);

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
                    sourceFileContent = ReadFileContentsSafely(_fileProvider, sourceFilePath);
                }

                var compilationFailure = new CompilationFailure(
                    sourceFilePath,
                    sourceFileContent,
                    compilationContent,
                    group.Select(d => d.ToDiagnosticMessage(_environment.RuntimeFramework)));

                failures.Add(compilationFailure);
            }

            return CompilationResult.Failed(failures);
        }

        private static string GetFilePath(string relativePath, Diagnostic diagnostic)
        {
            if (diagnostic.Location == Location.None)
            {
                return relativePath;
            }

            return diagnostic.Location.GetMappedLineSpan().Path;
        }

        private List<MetadataReference> GetApplicationReferences()
        {
            var references = new List<MetadataReference>();

            // Get the MetadataReference for the executing application. If it's a Roslyn reference,
            // we can copy the references created when compiling the application to the Razor page being compiled.
            // This avoids performing expensive calls to MetadataReference.CreateFromImage.
            var libraryExport = _libraryExporter.GetExport(_environment.ApplicationName);
            if (libraryExport?.MetadataReferences != null && libraryExport.MetadataReferences.Count > 0)
            {
                Debug.Assert(libraryExport.MetadataReferences.Count == 1,
                             "Expected 1 MetadataReferences, found " + libraryExport.MetadataReferences.Count);
                var roslynReference = libraryExport.MetadataReferences[0] as IRoslynMetadataReference;
                var compilationReference = roslynReference?.MetadataReference as CompilationReference;
                if (compilationReference != null)
                {
                    references.AddRange(compilationReference.Compilation.References);
                    references.Add(roslynReference.MetadataReference);
                    return references;
                }
            }

            var export = _libraryExporter.GetAllExports(_environment.ApplicationName);
            foreach (var metadataReference in export.MetadataReferences)
            {
                references.Add(metadataReference.ConvertMetadataReference(_assemblyMetadataFactory));
            }

            return references;
        }

        private AssemblyMetadata GetAssemblyMetadata(IMetadataFileReference metadataFileReference)
        {
            AssemblyMetadata assemblyMetadata;
            if (!_metadataFileCache.TryGetValue(metadataFileReference.Path, out assemblyMetadata))
            {
                assemblyMetadata = metadataFileReference.CreateAssemblyMetadata();
                _metadataFileCache.TryAdd(metadataFileReference.Path, assemblyMetadata);
            }

            return assemblyMetadata;
        }

        private static bool IsError(Diagnostic diagnostic)
        {
            return diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error;
        }

        private static string ReadFileContentsSafely(IFileProvider fileProvider, string filePath)
        {
            var fileInfo = fileProvider.GetFileInfo(filePath);
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
