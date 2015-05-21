// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Razor;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.Parser;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Razor.Directives
{
    /// <summary>
    /// A utility type for supporting inheritance of directives into a page from applicable <c>_ViewImports</c> pages.
    /// </summary>
    public class ChunkInheritanceUtility
    {
        private readonly MvcRazorHost _razorHost;
        private readonly IReadOnlyList<Chunk> _defaultInheritedChunks;
        private readonly ICodeTreeCache _codeTreeCache;

        /// <summary>
        /// Initializes a new instance of <see cref="ChunkInheritanceUtility"/>.
        /// </summary>
        /// <param name="razorHost">The <see cref="MvcRazorHost"/> used to parse <c>_ViewImports</c> pages.</param>
        /// <param name="codeTreeCache"><see cref="ICodeTreeCache"/> that caches <see cref="CodeTree"/> instances.
        /// </param>
        /// <param name="defaultInheritedChunks">Sequence of <see cref="Chunk"/>s inherited by default.</param>
        public ChunkInheritanceUtility([NotNull] MvcRazorHost razorHost,
                                       [NotNull] ICodeTreeCache codeTreeCache,
                                       [NotNull] IReadOnlyList<Chunk> defaultInheritedChunks)
        {
            _razorHost = razorHost;
            _defaultInheritedChunks = defaultInheritedChunks;
            _codeTreeCache = codeTreeCache;
        }

        /// <summary>
        /// Gets an ordered <see cref="IReadOnlyList{T}"/> of parsed <see cref="CodeTree"/> for each
        /// <c>_ViewImports</c> that is applicable to the page located at <paramref name="pagePath"/>. The list is
        /// ordered so that the <see cref="CodeTree"/> for the <c>_ViewImports</c> closest to the
        /// <paramref name="pagePath"/> in the file system appears first.
        /// </summary>
        /// <param name="pagePath">The path of the page to locate inherited chunks for.</param>
        /// <returns>A <see cref="IReadOnlyList{CodeTree}"/> of parsed <c>_ViewImports</c>
        /// <see cref="CodeTree"/>s.</returns>
        public virtual IReadOnlyList<CodeTree> GetInheritedCodeTrees([NotNull] string pagePath)
        {
            var inheritedCodeTrees = new List<CodeTree>();
            var templateEngine = new RazorTemplateEngine(_razorHost);
            foreach (var viewImportsPath in ViewHierarchyUtility.GetViewImportsLocations(pagePath))
            {
                // viewImportsPath contains the app-relative path of the _ViewImports.
                // Since the parsing of a _ViewImports would cause parent _ViewImports to be parsed
                // we need to ensure the paths are app-relative to allow the GetGlobalFileLocations
                // for the current _ViewImports to succeed.
                var codeTree = _codeTreeCache.GetOrAdd(viewImportsPath,
                                                       fileInfo => ParseViewFile(templateEngine,
                                                                                 fileInfo,
                                                                                 viewImportsPath));

                if (codeTree != null)
                {
                    inheritedCodeTrees.Add(codeTree);
                }
            }

            return inheritedCodeTrees;
        }

        /// <summary>
        /// Merges <see cref="Chunk"/> inherited by default and <see cref="CodeTree"/> instances produced by parsing
        /// <c>_ViewImports</c> files into the specified <paramref name="codeTree"/>.
        /// </summary>
        /// <param name="codeTree">The <see cref="CodeTree"/> to merge in to.</param>
        /// <param name="inheritedCodeTrees"><see cref="IReadOnlyList{CodeTree}"/> inherited from <c>_ViewImports</c>
        /// files.</param>
        /// <param name="defaultModel">The list of chunks to merge.</param>
        public void MergeInheritedCodeTrees([NotNull] CodeTree codeTree,
                                            [NotNull] IReadOnlyList<CodeTree> inheritedCodeTrees,
                                            string defaultModel)
        {
            var mergerMappings = GetMergerMappings(codeTree, defaultModel);
            IChunkMerger merger;

            // We merge chunks into the codeTree in two passes. In the first pass, we traverse the CodeTree visiting
            // a mapped IChunkMerger for types that are registered.
            foreach (var chunk in codeTree.Chunks)
            {
                if (mergerMappings.TryGetValue(chunk.GetType(), out merger))
                {
                    merger.VisitChunk(chunk);
                }
            }

            // In the second phase we invoke IChunkMerger.Merge for each chunk that has a mapped merger.
            // During this phase, the merger can either add to the CodeTree or ignore the chunk based on the merging
            // rules.
            // Read the chunks outside in - that is chunks from the _ViewImports closest to the page get merged in first
            // and the furthest one last. This allows the merger to ignore a directive like @model that was previously
            // seen.
            var chunksToMerge = inheritedCodeTrees.SelectMany(tree => tree.Chunks)
                                                  .Concat(_defaultInheritedChunks);
            foreach (var chunk in chunksToMerge)
            {
                if (mergerMappings.TryGetValue(chunk.GetType(), out merger))
                {
                    merger.Merge(codeTree, chunk);
                }
            }
        }

        private static Dictionary<Type, IChunkMerger> GetMergerMappings(CodeTree codeTree, string defaultModel)
        {
            var modelType = ChunkHelper.GetModelTypeName(codeTree, defaultModel);
            return new Dictionary<Type, IChunkMerger>
            {
                { typeof(UsingChunk), new UsingChunkMerger() },
                { typeof(InjectChunk), new InjectChunkMerger(modelType) },
                { typeof(SetBaseTypeChunk), new SetBaseTypeChunkMerger(modelType) }
            };
        }

        private static CodeTree ParseViewFile(RazorTemplateEngine engine,
                                              IFileInfo fileInfo,
                                              string viewImportsPath)
        {
            using (var stream = fileInfo.CreateReadStream())
            {
                using (var streamReader = new StreamReader(stream))
                {
                    var parseResults = engine.ParseTemplate(streamReader, viewImportsPath);
                    var className = ParserHelpers.SanitizeClassName(fileInfo.Name);
                    var language = engine.Host.CodeLanguage;
                    var codeGenerator = language.CreateCodeGenerator(className,
                                                                     engine.Host.DefaultNamespace,
                                                                     viewImportsPath,
                                                                     engine.Host);
                    codeGenerator.Visit(parseResults);

                    // Rewrite the location of inherited chunks so they point to the global import file.
                    var codeTree = codeGenerator.Context.CodeTreeBuilder.CodeTree;
                    foreach (var chunk in codeTree.Chunks)
                    {
                        chunk.Start = new SourceLocation(
                            viewImportsPath,
                            chunk.Start.AbsoluteIndex,
                            chunk.Start.LineIndex,
                            chunk.Start.CharacterIndex);
                    }

                    return codeTree;
                }
            }
        }
    }
}