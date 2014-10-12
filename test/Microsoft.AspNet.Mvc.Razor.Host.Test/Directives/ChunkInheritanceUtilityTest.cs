// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor.Directives
{
    public class ChunkInheritanceUtilityTest
    {
        [Fact]
        public void GetInheritedChunks_ReadsChunksFromViewStartsInPath()
        {
            // Arrange
            var fileSystem = new TestFileSystem();
            fileSystem.AddFile(@"x:\myapproot\views\accounts\_viewstart.cshtml", "@using AccountModels");
            fileSystem.AddFile(@"x:\myapproot\views\Shared\_viewstart.cshtml", "@inject SharedHelper Shared");
            fileSystem.AddFile(@"x:\myapproot\views\home\_viewstart.cshtml", "@using MyNamespace");
            fileSystem.AddFile(@"x:\myapproot\views\_viewstart.cshtml", "@inject MyHelper<TModel> Helper" +
                                                                         Environment.NewLine +
                                                                         "@inherits MyBaseType");
            var host = new MvcRazorHost(fileSystem);

            // Act
            var chunks = ChunkInheritanceUtility.GetInheritedChunks(host,
                                                                    fileSystem,
                                                                    @"x:\myapproot\views\home\Index.cshtml",
                                                                    Enumerable.Empty<Chunk>());

            // Assert
            Assert.Equal(6, chunks.Count);
            var usingChunk = Assert.IsType<UsingChunk>(chunks[1]);
            Assert.Equal("MyNamespace", usingChunk.Namespace);

            var injectChunk = Assert.IsType<InjectChunk>(chunks[4]);
            Assert.Equal("MyHelper<TModel>", injectChunk.TypeName);
            Assert.Equal("Helper", injectChunk.MemberName);

            var setBaseTypeChunk = Assert.IsType<SetBaseTypeChunk>(chunks[5]);
            Assert.Equal("MyBaseType", setBaseTypeChunk.TypeName);
        }

        [Fact]
        public void GetInheritedChunks_ReturnsEmptySequenceIfNoViewStartsArePresent()
        {
            // Arrange
            var fileSystem = new TestFileSystem();
            fileSystem.AddFile(@"x:\myapproot\_viewstart.cs", string.Empty);
            fileSystem.AddFile(@"x:\myapproot\views\_Layout.cshtml", string.Empty);
            fileSystem.AddFile(@"x:\myapproot\views\home\_not-viewstart.cshtml", string.Empty);
            var host = new MvcRazorHost(fileSystem);

            // Act
            var chunks = ChunkInheritanceUtility.GetInheritedChunks(host,
                                                                    fileSystem,
                                                                    @"x:\myapproot\views\home\Index.cshtml",
                                                                    Enumerable.Empty<Chunk>());

            // Assert
            Assert.Empty(chunks);
        }

        [Fact]
        public void GetInheritedChunks_ReturnsDefaultInheritedChunks()
        {
            // Arrange

            var fileSystem = new TestFileSystem();
            fileSystem.AddFile(@"x:\myapproot\views\_viewstart.cshtml",
                                "@inject DifferentHelper<TModel> Html" + 
                                Environment.NewLine + 
                                "@using AppNamespace.Models");
            var host = new MvcRazorHost(fileSystem);
            var defaultChunks = new Chunk[]
            {
                new InjectChunk("MyTestHtmlHelper", "Html"),
                new UsingChunk { Namespace = "AppNamespace.Model" },
            };

            // Act
            var chunks = ChunkInheritanceUtility.GetInheritedChunks(host,
                                                                    fileSystem,
                                                                    @"x:\myapproot\views\home\Index.cshtml",
                                                                    defaultChunks);

            // Assert
            var injectChunk = Assert.IsType<InjectChunk>(chunks[1]);
            Assert.Equal("DifferentHelper<TModel>", injectChunk.TypeName);
            Assert.Equal("Html", injectChunk.MemberName);

            var usingChunk = Assert.IsType<UsingChunk>(chunks[2]);
            Assert.Equal("AppNamespace.Models", usingChunk.Namespace);

            injectChunk = Assert.IsType<InjectChunk>(chunks[4]);
            Assert.Equal("MyTestHtmlHelper", injectChunk.TypeName);
            Assert.Equal("Html", injectChunk.MemberName);

            usingChunk = Assert.IsType<UsingChunk>(chunks[5]);
            Assert.Equal("AppNamespace.Model", usingChunk.Namespace);
        }

    }
}