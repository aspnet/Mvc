// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Razor.Generator.Compiler;
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
            fileSystem.AddFile(@"Views\accounts\_ViewStart.cshtml", "@using AccountModels");
            fileSystem.AddFile(@"Views\Shared\_ViewStart.cshtml", "@inject SharedHelper Shared");
            fileSystem.AddFile(@"Views\home\_ViewStart.cshtml", "@using MyNamespace");
            fileSystem.AddFile(@"Views\_ViewStart.cshtml",
@"@inject MyHelper<TModel> Helper
@inherits MyBaseType

@{
    Layout = ""test.cshtml"";
}

");
            var host = new MvcRazorHost(fileSystem);
            var utility = new ChunkInheritanceUtility(host, fileSystem, new Chunk[0]);

            // Act
            var chunks = utility.GetInheritedChunks(@"Views\home\Index.cshtml");

            // Assert
            Assert.Equal(2, chunks.Count);

            var viewStartChunks = chunks[@"x:\myapproot\views\home\_viewstart.cshtml"];
            Assert.Equal(3, viewStartChunks.Count);

            Assert.IsType<LiteralChunk>(viewStartChunks[0]);
            var usingChunk = Assert.IsType<UsingChunk>(viewStartChunks[1]);
            Assert.Equal("MyNamespace", usingChunk.Namespace);
            Assert.IsType<LiteralChunk>(viewStartChunks[2]);

            viewStartChunks = chunks[@"x:\myapproot\views\_viewstart.cshtml"];
            Assert.Equal(5, viewStartChunks.Count);

            Assert.IsType<LiteralChunk>(viewStartChunks[0]);

            var injectChunk = Assert.IsType<InjectChunk>(viewStartChunks[1]);
            Assert.Equal("MyHelper<TModel>", injectChunk.TypeName);
            Assert.Equal("Helper", injectChunk.MemberName);

            var setBaseTypeChunk = Assert.IsType<SetBaseTypeChunk>(viewStartChunks[2]);
            Assert.Equal("MyBaseType", setBaseTypeChunk.TypeName);

            Assert.IsType<StatementChunk>(viewStartChunks[3]);
            Assert.IsType<LiteralChunk>(viewStartChunks[4]);
        }

        [Fact]
        public void GetInheritedChunks_ReturnsEmptySequenceIfNoViewStartsArePresent()
        {
            // Arrange
            var fileSystem = new TestFileSystem();
            fileSystem.AddFile(@"_ViewStart.cs", string.Empty);
            fileSystem.AddFile(@"Views\_Layout.cshtml", string.Empty);
            fileSystem.AddFile(@"Views\home\_not-viewstart.cshtml", string.Empty);
            var host = new MvcRazorHost(fileSystem);
            var utility = new ChunkInheritanceUtility(host, fileSystem, new Chunk[0]);

            // Act
            var chunks = utility.GetInheritedChunks(@"Views\home\Index.cshtml");

            // Assert
            Assert.Empty(chunks);
        }

        [Fact]
        public void GetInheritedChunks_ReturnsDefaultInheritedChunks()
        {
            // Arrange
            var fileSystem = new TestFileSystem();
            fileSystem.AddFile(@"Views\_ViewStart.cshtml",
                               "@inject DifferentHelper<TModel> Html");
            var host = new MvcRazorHost(fileSystem);
            var defaultChunks = new Chunk[]
            {
                new InjectChunk("MyTestHtmlHelper", "Html"),
                new UsingChunk { Namespace = "AppNamespace.Model" },
            };
            var utility = new ChunkInheritanceUtility(host, fileSystem, defaultChunks);

            // Act
            var chunks = utility.GetInheritedChunks(@"Views\Home\Index.cshtml");

            // Assert
            Assert.Equal(2, chunks.Count);
            var viewStartChunks = chunks[@"x:\myapproot\views\_viewstart.cshtml"];
            Assert.Equal(2, viewStartChunks.Count);
            Assert.IsType<LiteralChunk>(viewStartChunks[0]);
            var injectChunk = Assert.IsType<InjectChunk>(viewStartChunks[1]);
            Assert.Equal("DifferentHelper<TModel>", injectChunk.TypeName);
            Assert.Equal("Html", injectChunk.MemberName);

            Assert.Equal(defaultChunks, chunks[""]);
        }
    }
}