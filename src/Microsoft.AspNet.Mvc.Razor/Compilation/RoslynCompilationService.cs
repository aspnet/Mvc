// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.AspNet.FileSystems;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Framework.Runtime;

namespace Microsoft.AspNet.Mvc.Razor.Compilation
{
    /// <summary>
    /// A type that uses Roslyn to compile C# content.
    /// </summary>
    public class RoslynCompilationService : ICompilationService
    {
        private readonly Lazy<bool> _supportsPdbGeneration = new Lazy<bool>(SupportsPdbGeneration);
        private readonly ConcurrentDictionary<string, AssemblyMetadata> _metadataFileCache =
            new ConcurrentDictionary<string, AssemblyMetadata>(StringComparer.OrdinalIgnoreCase);

        private readonly ILibraryManager _libraryManager;
        private readonly IApplicationEnvironment _environment;
        private readonly IAssemblyLoaderEngine _loader;

        private readonly Lazy<List<MetadataReference>> _applicationReferences;

        private readonly string _classPrefix;

        /// <summary>
        /// Initalizes a new instance of the <see cref="RoslynCompilationService"/> class.
        /// </summary>
        /// <param name="environment">The environment for the executing application.</param>
        /// <param name="loaderEngine">The loader used to load compiled assemblies.</param>
        /// <param name="libraryManager">The library manager that provides export and reference information.</param>
        /// <param name="host">The <see cref="IMvcRazorHost"/> that was used to generate the code.</param>
        public RoslynCompilationService(IApplicationEnvironment environment,
                                        IAssemblyLoaderEngine loaderEngine,
                                        ILibraryManager libraryManager,
                                        IMvcRazorHost host)
        {
            _environment = environment;
            _loader = loaderEngine;
            _libraryManager = libraryManager;
            _applicationReferences = new Lazy<List<MetadataReference>>(GetApplicationReferences);
            _classPrefix = host.MainClassNamePrefix;
        }

        /// <inheritdoc />
        public CompilationResult Compile(IFileInfo fileInfo, string compilationContent)
        {
            var syntaxTrees = new[] { SyntaxTreeGenerator.Generate(compilationContent, fileInfo.PhysicalPath) };

            var references = _applicationReferences.Value;

            var assemblyName = Path.GetRandomFileName();

            var compilation = CSharpCompilation.Create(assemblyName,
                        options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                        syntaxTrees: syntaxTrees,
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
                        var formatter = new DiagnosticFormatter();

                        var messages = result.Diagnostics
                                             .Where(IsError)
                                             .Select(d => GetCompilationMessage(formatter, d))
                                             .ToList();

                        return CompilationResult.Failed(fileInfo, compilationContent, messages);
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
                        assembly = _loader.LoadStream(ms, pdbStream: null);
                    }

                    var type = assembly.GetExportedTypes()
                                       .First(t => t.Name.
                                          StartsWith(_classPrefix, StringComparison.Ordinal));

                    return UncachedCompilationResult.Successful(type);
                }
            }
        }

        private List<MetadataReference> GetApplicationReferences()
        {
            var references = new List<MetadataReference>();

            var export = _libraryManager.GetAllExports(_environment.ApplicationName);

            foreach (var metadataReference in export.MetadataReferences)
            {
                // Taken from https://github.com/aspnet/KRuntime/blob/757ba9bfdf80bd6277e715d6375969a7f44370ee/src/...
                // Microsoft.Framework.Runtime.Roslyn/RoslynCompiler.cs#L164
                // We don't want to take a dependency on the Roslyn bit directly since it pulls in more dependencies
                // than the view engine needs (Microsoft.Framework.Runtime) for example
                references.Add(ConvertMetadataReference(metadataReference));
            }

            return references;
        }

        private MetadataReference ConvertMetadataReference(IMetadataReference metadataReference)
        {
            var roslynReference = metadataReference as IRoslynMetadataReference;

            if (roslynReference != null)
            {
                return roslynReference.MetadataReference;
            }

            var embeddedReference = metadataReference as IMetadataEmbeddedReference;

            if (embeddedReference != null)
            {
                return MetadataReference.CreateFromImage(embeddedReference.Contents);
            }

            var fileMetadataReference = metadataReference as IMetadataFileReference;

            if (fileMetadataReference != null)
            {
                return CreateMetadataFileReference(fileMetadataReference.Path);
            }

            var projectReference = metadataReference as IMetadataProjectReference;
            if (projectReference != null)
            {
                using (var ms = new MemoryStream())
                {
                    projectReference.EmitReferenceAssembly(ms);

                    return MetadataReference.CreateFromImage(ms.ToArray());
                }
            }

            throw new NotSupportedException();
        }

        private MetadataReference CreateMetadataFileReference(string path)
        {
            var metadata = _metadataFileCache.GetOrAdd(path, _ =>
            {
                return AssemblyMetadata.CreateFromStream(File.OpenRead(path));
            });

            return metadata.GetReference();
        }

        private static CompilationMessage GetCompilationMessage(DiagnosticFormatter formatter, Diagnostic diagnostic)
        {
            return new CompilationMessage(formatter.Format(diagnostic));
        }

        private static bool IsError(Diagnostic diagnostic)
        {
            return diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error;
        }

        private static bool SupportsPdbGeneration()
        {
            try
            {
                if (PlatformHelper.IsMono)
                {
                    return false;
                }

                // Check for the pdb writer component that roslyn uses to generate pdbs
                const string SymWriterGuid = "0AE2DEB0-F901-478b-BB9F-881EE8066788";

                var type =  Marshal.GetTypeFromCLSID(new Guid(SymWriterGuid));

                if (type != null)
                {
                    // This line will throw if pdb generation is not supported.
                    Activator.CreateInstance(type);
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }
    }
}
