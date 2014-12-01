// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.Parser;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.TagHelpers;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class MvcRazorCodeParserTest
    {
        [Fact]
        public void GetTagHelperDescriptors_ReturnsDescriptorsFromViewStart()
        {
            // Arrange
            var builder = new BlockBuilder { Type = BlockType.Comment };
            var block = new Block(builder);
            var chunks = new Dictionary<string, IList<Chunk>>(StringComparer.Ordinal)
            {
                {
                    "_ViewStart.cshtml",
                    new[]
                    {
                        new RemoveTagHelperChunk { LookupText = "Remove Tag Helper" },
                    }
                },
                {
                    "Views/_ViewStart.cshtml",
                    new Chunk[]
                    {
                        new LiteralChunk { Text = "Hello world" },
                        new AddTagHelperChunk { LookupText = "Add Tag Helper" },
                    }
                }
            };

            IList<TagHelperDirectiveDescriptor> descriptors = null;
            var resolver = new Mock<ITagHelperDescriptorResolver>();
            resolver.Setup(r => r.Resolve(It.IsAny<TagHelperDescriptorResolutionContext>()))
                    .Callback((TagHelperDescriptorResolutionContext context) =>
                    {
                        descriptors = context.DirectiveDescriptors;
                    })
                    .Returns(Enumerable.Empty<TagHelperDescriptor>())
                    .Verifiable();

            var baseParser = new RazorParser(new CSharpCodeParser(),
                                             new HtmlMarkupParser(),
                                             resolver.Object);
            var parser = new TestableMvcRazorParser(baseParser, chunks);
            var sink = new ParserErrorSink();

            // Act
            var result = parser.GetTagHelperDescriptorsPublic(block, sink).ToArray();

            // Assert
            Assert.NotNull(descriptors);
            Assert.Equal(2, descriptors.Count);

            Assert.Equal("Remove Tag Helper", descriptors[0].LookupText);
            Assert.Equal(TagHelperDirectiveType.RemoveTagHelper, descriptors[0].DirectiveType);

            Assert.Equal("Add Tag Helper", descriptors[1].LookupText);
            Assert.Equal(TagHelperDirectiveType.AddTagHelper, descriptors[1].DirectiveType);
        }

        private class TestableMvcRazorParser : MvcRazorParser
        {
            public TestableMvcRazorParser(RazorParser parser, IDictionary<string, IList<Chunk>> viewStartChunks)
                : base(parser, viewStartChunks)
            {
            }

            public IEnumerable<TagHelperDescriptor> GetTagHelperDescriptorsPublic(
                Block documentRoot,
                ParserErrorSink errorSink)
            {
                return GetTagHelperDescriptors(documentRoot, errorSink);
            }
        }
    }
}