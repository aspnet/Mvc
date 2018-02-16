// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    /// <summary>
    /// Caches the result of runtime compilation of Razor files for the duration of the application lifetime.
    /// </summary>
    public class RazorViewCompiler : IViewCompiler
    {
        private readonly object _cacheLock = new object();
        private readonly Dictionary<string, CompiledViewDescriptor> _precompiledViews;
        private readonly ConcurrentDictionary<string, string> _normalizedPathCache;
        private readonly IFileProvider _fileProvider;
        private readonly RazorProjectEngine _projectEngine;
        private readonly Action<RoslynCompilationContext> _compilationCallback;
        private readonly ILogger _logger;
        private readonly CSharpCompiler _csharpCompiler;
        private readonly IMemoryCache _cache;

        public RazorViewCompiler(
            IFileProvider fileProvider,
            RazorProjectEngine projectEngine,
            CSharpCompiler csharpCompiler,
            Action<RoslynCompilationContext> compilationCallback,
            IList<CompiledViewDescriptor> precompiledViews,
            ILogger logger)
        {
            if (fileProvider == null)
            {
                throw new ArgumentNullException(nameof(fileProvider));
            }

            if (projectEngine == null)
            {
                throw new ArgumentNullException(nameof(projectEngine));
            }

            if (csharpCompiler == null)
            {
                throw new ArgumentNullException(nameof(csharpCompiler));
            }

            if (compilationCallback == null)
            {
                throw new ArgumentNullException(nameof(compilationCallback));
            }

            if (precompiledViews == null)
            {
                throw new ArgumentNullException(nameof(precompiledViews));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _fileProvider = fileProvider;
            _projectEngine = projectEngine;
            _csharpCompiler = csharpCompiler;
            _compilationCallback = compilationCallback;
            _logger = logger;

            _normalizedPathCache = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

            // This is our L0 cache, and is a durable store. Views migrate into the cache as they are requested
            // from either the set of known precompiled views, or by being compiled.
            _cache = new MemoryCache(new MemoryCacheOptions());

            // We need to validate that the all of the precompiled views are unique by path (case-insenstive).
            // We do this because there's no good way to canonicalize paths on windows, and it will create
            // problems when deploying to linux. Rather than deal with these issues, we just don't support
            // views that differ only by case.
            _precompiledViews = new Dictionary<string, CompiledViewDescriptor>(
                precompiledViews.Count,
                StringComparer.OrdinalIgnoreCase);

            foreach (var precompiledView in precompiledViews)
            {
                if (_precompiledViews.TryGetValue(precompiledView.RelativePath, out var otherValue))
                {
                    var message = string.Join(
                        Environment.NewLine,
                        Resources.RazorViewCompiler_ViewPathsDifferOnlyInCase,
                        otherValue.RelativePath,
                        precompiledView.RelativePath);
                    throw new InvalidOperationException(message);
                }

                _precompiledViews.Add(precompiledView.RelativePath, precompiledView);
            }
        }

        /// <inheritdoc />
        public Task<CompiledViewDescriptor> CompileAsync(string relativePath)
        {
            if (relativePath == null)
            {
                throw new ArgumentNullException(nameof(relativePath));
            }

            // Attempt to lookup the cache entry using the passed in path. This will succeed if the path is already
            // normalized and a cache entry exists.
            Task<CompiledViewDescriptor> cachedResult;
            if (_cache.TryGetValue(relativePath, out cachedResult))
            {
                return cachedResult;
            }

            var normalizedPath =  GetNormalizedPath(relativePath);
            if (_cache.TryGetValue(normalizedPath, out cachedResult))
            {
                return cachedResult;
            }

            // Entry does not exist. Attempt to create one.
            cachedResult = OnCacheMiss(normalizedPath);
            return cachedResult;
        }

        private Task<CompiledViewDescriptor> OnCacheMiss(string normalizedPath)
        {
            ViewCompilerWorkItem item;
            TaskCompletionSource<CompiledViewDescriptor> taskSource;
            MemoryCacheEntryOptions cacheEntryOptions;

            // Safe races cannot be allowed when compiling Razor pages. To ensure only one compilation request succeeds
            // per file, we'll lock the creation of a cache entry. Creating the cache entry should be very quick. The
            // actual work for compiling files happens outside the critical section.
            lock (_cacheLock)
            {
                // Double-checked locking to handle a possible race.
                if (_cache.TryGetValue(normalizedPath, out Task<CompiledViewDescriptor> result))
                {
                    return result;
                }

                if (_precompiledViews.TryGetValue(normalizedPath, out var precompiledView))
                {
                    item = CreatePrecompiledWorkItem(normalizedPath, precompiledView);
                }
                else
                {
                    item = CreateRuntimeCompilationWorkItem(normalizedPath);
                }

                // At this point, we've decided what to do - but we should create the cache entry and
                // release the lock first.
                cacheEntryOptions = new MemoryCacheEntryOptions();

                Debug.Assert(item.ExpirationTokens != null);
                for (var i = 0; i < item.ExpirationTokens.Count; i++)
                {
                    cacheEntryOptions.ExpirationTokens.Add(item.ExpirationTokens[i]);
                }

                taskSource = new TaskCompletionSource<CompiledViewDescriptor>();
                if (item.SupportsCompilation)
                {
                    // We'll compile in just a sec, be patient.
                }
                else
                {
                    // If we can't compile, we should have already created the descriptor
                    Debug.Assert(item.Descriptor != null);
                    taskSource.SetResult(item.Descriptor);
                }

                _cache.Set(normalizedPath, taskSource.Task, cacheEntryOptions);
            }

            // Now the lock has been released so we can do more expensive processing.
            if (item.SupportsCompilation)
            {
                Debug.Assert(taskSource != null);

                if (item.Descriptor?.Item != null &&
                    ChecksumValidator.IsItemValid(_projectEngine.FileSystem, item.Descriptor.Item))
                {
                    // If the item has checksums to validate, we should also have a precompiled view.
                    Debug.Assert(item.Descriptor != null);

                    taskSource.SetResult(item.Descriptor);
                    return taskSource.Task;
                }

                try
                {
                    var descriptor = CompileAndEmit(normalizedPath);
                    descriptor.ExpirationTokens = cacheEntryOptions.ExpirationTokens;
                    taskSource.SetResult(descriptor);
                }
                catch (Exception ex)
                {
                    taskSource.SetException(ex);
                }
            }

            return taskSource.Task;
        }

        private ViewCompilerWorkItem CreatePrecompiledWorkItem(string normalizedPath, CompiledViewDescriptor precompiledView)
        {
            // We have a precompiled view - but we're not sure that we can use it yet.
            //
            // We need to determine first if we have enough information to 'recompile' this view. If that's the case
            // we'll create change tokens for all of the files.
            //
            // Then we'll attempt to validate if any of those files have different content than the original sources
            // based on checksums.
            if (precompiledView.Item == null || !ChecksumValidator.IsRecompilationSupported(precompiledView.Item))
            {
                return new ViewCompilerWorkItem()
                {
                    // If we don't have a checksum for the primary source file we can't recompile.
                    SupportsCompilation = false,

                    ExpirationTokens = Array.Empty<IChangeToken>(), // Never expire because we can't recompile.
                    Descriptor = precompiledView, // This will be used as-is.
                };
            }

            var item = new ViewCompilerWorkItem()
            {
                SupportsCompilation = true,

                Descriptor = precompiledView, // This might be used, if the checksums match.

                // Used to validate and recompile
                NormalizedPath = normalizedPath,
                ExpirationTokens = new List<IChangeToken>(),
            };

            var checksums = precompiledView.Item.GetChecksumMetadata();
            for (var i = 0; i < checksums.Count; i++)
            {
                // We rely on Razor to provide the right set of checksums. Trust the compiler, it has to do a good job,
                // so it probably will.
                item.ExpirationTokens.Add(_fileProvider.Watch(checksums[i].Identifier));
            }

            // We also need to create a new descriptor, because the original one doesn't have expiration tokens on
            // it. These will be used by the view location cache, which is like an L1 cache for views (this class is
            // the L2 cache).
            item.Descriptor = new CompiledViewDescriptor()
            {
                ExpirationTokens = item.ExpirationTokens,
                IsPrecompiled = true,
                Item = precompiledView.Item,
                RelativePath = precompiledView.RelativePath,
                ViewAttribute = precompiledView.ViewAttribute,
            };

            return item;
        }

        private ViewCompilerWorkItem CreateRuntimeCompilationWorkItem(string normalizedPath)
        {
            var expirationTokens = new List<IChangeToken>()
            {
                _fileProvider.Watch(normalizedPath),
            };

            var projectItem = _projectEngine.FileSystem.GetItem(normalizedPath);
            if (!projectItem.Exists)
            {
                // If the file doesn't exist, we can't do compilation right now - we still want to cache
                // the fact that we tried. This will allow us to retrigger compilation if the view file
                // is added.
                return new ViewCompilerWorkItem()
                {
                    // We don't have enough information to compile
                    SupportsCompilation = false,

                    Descriptor = new CompiledViewDescriptor()
                    {
                        RelativePath = normalizedPath,
                        ExpirationTokens = expirationTokens,
                    },

                    // We can try again if the file gets created.
                    ExpirationTokens = expirationTokens,
                };
            }

            // OK this means we can do compilation. For now let's just identify the other files we need to watch
            // so we can create the cache entry. Compilation will happen after we release the lock.

            var importFeature = _projectEngine.ProjectFeatures.OfType<IImportProjectFeature>().FirstOrDefault();

            // There should always be an import feature unless someone has misconfigured their RazorProjectEngine.
            // In that case once we attempt to parse the Razor file we'll explode and give the a user a decent
            // error message; for now, lets just be extra protective and assume 0 imports to not give a bad error.
            var imports = importFeature?.GetImports(projectItem) ?? Enumerable.Empty<RazorProjectItem>();
            var physicalImports = imports.Where(import => import.FilePath != null);
            
            // Now that we have non-dynamic imports we need to get their RazorProjectItem equivalents so we have their
            // physical file paths (according to the FileSystem).
            foreach (var physicalImport in physicalImports)
            {
                expirationTokens.Add(_fileProvider.Watch(physicalImport.FilePath));
            }

            return new ViewCompilerWorkItem()
            {
                SupportsCompilation = true,

                NormalizedPath = normalizedPath,
                ExpirationTokens = expirationTokens,
            };
        }

        protected virtual CompiledViewDescriptor CompileAndEmit(string relativePath)
        {
            var projectItem = _projectEngine.FileSystem.GetItem(relativePath);
            var codeDocument = _projectEngine.Process(projectItem);
            var cSharpDocument = codeDocument.GetCSharpDocument();

            if (cSharpDocument.Diagnostics.Count > 0)
            {
                throw CompilationFailedExceptionFactory.Create(
                    codeDocument,
                    cSharpDocument.Diagnostics);
            }

            var assembly = CompileAndEmit(codeDocument, cSharpDocument.GeneratedCode);

            // Anything we compile from source will use Razor 2.1 and so should have the new metadata.
            var loader = new RazorCompiledItemLoader();
            var item = loader.LoadItems(assembly).SingleOrDefault();
            var attribute = assembly.GetCustomAttribute<RazorViewAttribute>();

            return new CompiledViewDescriptor(item, attribute);
        }

        internal Assembly CompileAndEmit(RazorCodeDocument codeDocument, string generatedCode)
        {
            _logger.GeneratedCodeToAssemblyCompilationStart(codeDocument.Source.FilePath);

            var startTimestamp = _logger.IsEnabled(LogLevel.Debug) ? Stopwatch.GetTimestamp() : 0;

            var assemblyName = Path.GetRandomFileName();
            var compilation = CreateCompilation(generatedCode, assemblyName);

            using (var assemblyStream = new MemoryStream())
            using (var pdbStream = new MemoryStream())
            {
                var result = compilation.Emit(
                    assemblyStream,
                    pdbStream,
                    options: _csharpCompiler.EmitOptions);

                if (!result.Success)
                {
                    throw CompilationFailedExceptionFactory.Create(
                        codeDocument,
                        generatedCode,
                        assemblyName,
                        result.Diagnostics);
                }

                assemblyStream.Seek(0, SeekOrigin.Begin);
                pdbStream.Seek(0, SeekOrigin.Begin);

                var assembly = Assembly.Load(assemblyStream.ToArray(), pdbStream.ToArray());
                _logger.GeneratedCodeToAssemblyCompilationEnd(codeDocument.Source.FilePath, startTimestamp);

                return assembly;
            }
        }

        private CSharpCompilation CreateCompilation(string compilationContent, string assemblyName)
        {
            var sourceText = SourceText.From(compilationContent, Encoding.UTF8);
            var syntaxTree = _csharpCompiler.CreateSyntaxTree(sourceText).WithFilePath(assemblyName);
            var compilation = _csharpCompiler
                .CreateCompilation(assemblyName)
                .AddSyntaxTrees(syntaxTree);
            compilation = ExpressionRewriter.Rewrite(compilation);

            var compilationContext = new RoslynCompilationContext(compilation);
            _compilationCallback(compilationContext);
            compilation = compilationContext.Compilation;
            return compilation;
        }

        private string GetNormalizedPath(string relativePath)
        {
            Debug.Assert(relativePath != null);
            if (relativePath.Length == 0)
            {
                return relativePath;
            }

            if (!_normalizedPathCache.TryGetValue(relativePath, out var normalizedPath))
            {
                normalizedPath = ViewPath.NormalizePath(relativePath);
                _normalizedPathCache[relativePath] = normalizedPath;
            }

            return normalizedPath;
        }

        private class ViewCompilerWorkItem
        {
            public bool SupportsCompilation { get; set; }

            public string NormalizedPath { get; set; }

            public IList<IChangeToken> ExpirationTokens { get; set; }

            public CompiledViewDescriptor Descriptor { get; set; }
        }
    }
}
