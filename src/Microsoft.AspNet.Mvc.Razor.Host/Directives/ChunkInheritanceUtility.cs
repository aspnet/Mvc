// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNet.FileSystems;
using Microsoft.AspNet.Razor;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.Parser;

namespace Microsoft.AspNet.Mvc.Razor.Directives
{
    /// <summary>
    /// A utility type for supporting inheritance of tag helpers and chunks into a page from applicable _ViewStart
    /// pages.
    /// </summary>
    public class ChunkInheritanceUtility
    {
        private readonly Dictionary<string, CodeTree> _parsedCodeTrees;
        private readonly MvcRazorHost _razorHost;
        private readonly IFileSystem _fileSystem;
        private readonly IList<Chunk> _defaultInheritedChunks;

        /// <summary>
        /// Initializes a new instance of <see cref="ChunkInheritanceUtility"/>.
        /// </summary>
        /// <param name="razorHost">The <see cref="MvcRazorHost"/> used to parse _ViewStart pages.</param>
        /// <param name="fileSystem">The filesystem that represents the application.</param>
        /// <param name="defaultInheritedChunks">Sequence of <see cref="Chunk"/>s inherited by default.</param>
        public ChunkInheritanceUtility([NotNull] MvcRazorHost razorHost,
                                       [NotNull] IFileSystem fileSystem,
                                       [NotNull] IList<Chunk> defaultInheritedChunks)
        {
            _razorHost = razorHost;
            _fileSystem = fileSystem;
            _defaultInheritedChunks = defaultInheritedChunks;
            _parsedCodeTrees = new Dictionary<string, CodeTree>(StringComparer.Ordinal);
        }

        /// <summary>
        /// Gets a mapping of _ViewStart paths to the <see cref="Chunk"/>s containing parsed results of the _ViewStart
        /// file. The result specifically contains results for _ViewStarts that are applicable to the page located at
        /// <paramref name="pagePath"/>.
        /// </summary>
        /// <param name="pagePath">The path of the page to locate inherited chunks for.</param>
        /// <returns>A <see cref="IDictionary{string, IList{Chunk}}"/> of parsed _ViewStart chunks.</returns>
        public IDictionary<string, IList<Chunk>> GetInheritedChunks([NotNull] string pagePath)
        {
            var inheritedChunks = new Dictionary<string, IList<Chunk>>(StringComparer.Ordinal);

            var templateEngine = new RazorTemplateEngine(_razorHost);
            foreach (var viewStartPath in ViewStartUtility.GetViewStartLocations(pagePath))
            {
                CodeTree codeTree;

                if (_parsedCodeTrees.TryGetValue(viewStartPath, out codeTree))
                {
                    inheritedChunks.Add(viewStart, codeTree.Chunks);
                }
                else
                {
                    var fileInfo = _fileSystem.GetFileInfo(viewStartPath);
                    if (fileInfo.Exists)
                    {
                        // viewStartPath contains the app-relative path of the ViewStart.
                        // Since the parsing of a _ViewStart would cause parent _ViewStarts to be parsed
                        // we need to ensure the paths are app-relative to allow the GetViewStartLocations
                        // for the current _ViewStart to succeed.
                        codeTree = ParseViewFile(templateEngine, fileInfo, viewStartPath);
                        _parsedCodeTrees.Add(viewStartPath, codeTree);
                        inheritedChunks.AddRange(codeTree.Chunks);
                    }
                }
            }

            if (_defaultInheritedChunks.Count > 0)
            {
                inheritedChunks.Add(string.Empty, _defaultInheritedChunks);
            }

            return inheritedChunks;
        }

        /// <summary>
        /// Merges a list of chunks into the specified <paramref name="codeTree"/>.
        /// </summary>
        /// <param name="codeTree">The <see cref="CodeTree"/> to merge.</param>
        /// <param name="inheritedChunks">Chunks inherited by default and from _ViewStart files.</param>
        /// <param name="defaultModel">The list of chunks to merge.</param>
        public void MergeInheritedChunks([NotNull] CodeTree codeTree,
                                         [NotNull] IDictionary<string, IList<Chunk>> inheritedChunks,
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
            // Read the chunks outside in - that is chunks from the _ViewStart closest to the page get merged in first
            // and the furthest one last. This allows the merger to ignore a directive like @model that was previously
            // seen.
            var chunksInOrder = inheritedChunks.OrderByDescending(item => item.Key.Length)
                                               .SelectMany(item => item.Value);
            foreach (var chunk in chunksInOrder)
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
                                              string viewStartPath)
        {
            using (var stream = fileInfo.CreateReadStream())
            {
                using (var streamReader = new StreamReader(stream))
                {
                    var parseResults = engine.ParseTemplate(streamReader, viewStartPath);
                    var className = ParserHelpers.SanitizeClassName(fileInfo.Name);
                    var language = engine.Host.CodeLanguage;
                    var codeGenerator = language.CreateCodeGenerator(className,
                                                                     engine.Host.DefaultNamespace,
                                                                     viewStartPath,
                                                                     engine.Host);
                    codeGenerator.Visit(parseResults);

                    return codeGenerator.Context.CodeTreeBuilder.CodeTree;
                }
            }
        }
    }
}