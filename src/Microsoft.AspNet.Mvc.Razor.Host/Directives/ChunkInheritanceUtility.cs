// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNet.FileSystems;
using Microsoft.AspNet.Razor;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.Parser;

namespace Microsoft.AspNet.Mvc.Razor.Directives
{
    /// <summary>
    /// A utility type for supporting inheritance of chunks into a page from _ViewStart pages that apply to it.
    /// </summary>
    public class ChunkInheritanceUtility
    {
        private static readonly List<Chunk> _defaultInheritedChunks = new List<Chunk>
        {
            new InjectChunk("Microsoft.AspNet.Mvc.Rendering.IHtmlHelper<TModel>", "Html"),
            new InjectChunk("Microsoft.AspNet.Mvc.IViewComponentHelper", "Component"),
        };

        /// <summary>
        /// Gets the list of chunks that are to be inherited by a specified page.
        /// Chunks are inherited from _ViewStarts that are applicable to the page.
        /// </summary>
        /// <param name="engine">The template engine used to parse a Razor page.</param>
        /// <param name="fileSystem">The filesystem that represents the application.</param>
        /// <param name="appRoot">The root of the application.</param>
        /// <param name="pagePath">The path of the page to locate inherited chunks for.</param>
        /// <returns>A list of chunks that are applicable to the given page.</returns>
        public List<Chunk> GetInheritedChunks(RazorTemplateEngine engine,
                                              IFileSystem fileSystem,
                                              string appRoot,
                                              string pagePath)
        {
            var inheritedChunks = new List<Chunk>();

            foreach (var viewStart in ViewStartUtility.GetViewStartLocations(appRoot, pagePath))
            {
                IFileInfo fileInfo;
                if (fileSystem.TryGetFileInfo(viewStart, out fileInfo))
                {
                    var parsedTree = ParseViewFile(engine, fileInfo);
                    inheritedChunks.AddRange(parsedTree.Chunks);
                }
            }

            inheritedChunks.AddRange(_defaultInheritedChunks);

            return inheritedChunks;
        }

        /// <summary>
        /// Merges a list of chunks into the <see cref="CodeTree"/> instance.
        /// </summary>
        /// <param name="codeTree">The <see cref="CodeTree"/> instance to merge chunks into.</param>
        /// <param name="inherited">The list of chunks to merge.</param>
        public void MergeInheritedChunks(CodeTree codeTree,
                                         List<Chunk> inherited)
        {
            var mergerMappings = GetMergerMappings(codeTree);
            var current = codeTree.Chunks;

            foreach (var chunk in current)
            {
                ChunkMergerBase merger;
                if (mergerMappings.TryGetValue(chunk.GetType(), out merger))
                {
                    merger.VisitChunk(chunk);
                }
            }

            foreach (var chunk in inherited)
            {
                ChunkMergerBase merger;
                if (mergerMappings.TryGetValue(chunk.GetType(), out merger))
                {
                    // TODO: When mapping chunks, we should remove mapping information since it would be incorrect
                    // to generate it in the page that inherits it. Tracked by #945
                    merger.Merge(chunk);
                }
            }
        }

        private Dictionary<Type, ChunkMergerBase> GetMergerMappings(CodeTree codeTree)
        {
            return new Dictionary<Type, ChunkMergerBase>
            {
                { typeof(UsingChunk), new UsingChunkMerger(codeTree) },
                { typeof(InjectChunk), new InjectChunkMerger(codeTree) },
                { typeof(SetBaseTypeChunk), new SetBaseTypeChunk(codeTree) }
            };
        }

        private CodeTree ParseViewFile(RazorTemplateEngine engine,
                                       IFileInfo fileInfo)
        {
            using (var stream = fileInfo.CreateReadStream())
            {
                using (var streamReader = new StreamReader(stream))
                {
                    var parseResults = engine.ParseTemplate(streamReader);
                    var className = ParserHelpers.SanitizeClassName(fileInfo.Name);
                    var codeGenerator = engine.Host.CodeLanguage.CreateCodeGenerator(className,
                                                                                     MvcRazorHost.ViewNamespace,
                                                                                     fileInfo.PhysicalPath,
                                                                                     engine.Host);
                    codeGenerator.Visit(parseResults);
                    return codeGenerator.Context.CodeTreeBuilder.CodeTree;
                }
            }
        }
    }
}