// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor.Host.Internal;
using Microsoft.AspNetCore.Mvc.Razor.Host.Test.Internal;
using Microsoft.AspNetCore.Razor.Chunks;
using Microsoft.AspNetCore.Razor.Parser.SyntaxTree;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Host.Test
{
    public class TagHelperChunkDecoratorTest
    {
        [Fact]
        public void Accept_CorrectlyDecoratesViewComponentChunks()
        {
            // Arrange
            var codeGeneratorContext = ChunkVisitorTestFactory.CreateDummyCodeGeneratorContext();
            var syntaxTreeNode = new Mock<Span>(new SpanBuilder());
            foreach (var chunk in ChunkVisitorTestFactory.GetTestChunks(visitedTagHelperChunks: false))
            {
                codeGeneratorContext.ChunkTreeBuilder.AddChunk(chunk, syntaxTreeNode.Object);
            }

            var tagHelperChunkVisitor = new TagHelperChunkDecorator(codeGeneratorContext);
            var expectedChunks = ChunkVisitorTestFactory.GetTestChunks(visitedTagHelperChunks: true);

            // Act
            var resultChunks = codeGeneratorContext.ChunkTreeBuilder.Root.Children;
            tagHelperChunkVisitor.Accept(resultChunks);

            // Assert
            // Test the normal tag helper chunk, Baz.
            Assert.Equal(expectedChunks.Count(), resultChunks.Count());

            var expectedTagHelperChunk = expectedChunks.ElementAt(0) as TagHelperChunk;
            var resultTagHelperChunk = resultChunks.ElementAt(0) as TagHelperChunk;
            Assert.NotNull(resultTagHelperChunk);

            Assert.Equal(expectedTagHelperChunk.Descriptors.First().TypeName,
                         resultTagHelperChunk.Descriptors.First().TypeName,
                         StringComparer.Ordinal);

            // Test the parent chunk with view component tag helper inside, Foo.
            var expectedParentChunk = expectedChunks.ElementAt(1) as ParentChunk;
            var resultParentChunk = expectedChunks.ElementAt(1) as ParentChunk;
            Assert.NotNull(resultParentChunk);
            Assert.Single(resultParentChunk.Children);

            expectedTagHelperChunk = expectedParentChunk.Children.First() as TagHelperChunk;
            resultTagHelperChunk = resultParentChunk.Children.First() as TagHelperChunk;

            Assert.Equal(expectedTagHelperChunk.Descriptors.First().TypeName,
                resultTagHelperChunk.Descriptors.First().TypeName,
                StringComparer.Ordinal);

            // Test the view component tag helper, Bar.
            expectedTagHelperChunk = expectedChunks.ElementAt(2) as TagHelperChunk;
            resultTagHelperChunk = resultChunks.ElementAt(2) as TagHelperChunk;
            Assert.NotNull(resultTagHelperChunk);

            Assert.Equal(expectedTagHelperChunk.Descriptors.First().TypeName,
                 resultTagHelperChunk.Descriptors.First().TypeName,
                 StringComparer.Ordinal);
        }
    }
}
