// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Chunks;
using Microsoft.AspNetCore.Razor.Chunks.Generators;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.Parser.SyntaxTree;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;

namespace Microsoft.AspNetCore.Mvc.Razor.Host.Test.Internal
{
    public static class ChunkVisitorTestFactory
    {
        private static string _testClass = "TestClass";
        private static string _testNamespace = "TestNamespace";
        private static string _testFile = "TestFile";

        public static IList<Chunk> GetTestChunks(bool visitedTagHelperChunks)
        {
            var chunkList = new List<Chunk>();

            // Add a normal tag helper chunk.
            var normalTagHelperChunk = GetTagHelperChunk("Baz");
            chunkList.Add(normalTagHelperChunk);

            var nestedViewComponentTagHelperChunk =
                GetNestedViewComponentTagHelperChunk("Foo", visitedTagHelperChunks);
            chunkList.Add(nestedViewComponentTagHelperChunk);

            // Add a view component tag helper chunk.
            var viewComponentTagHelperChunk = GetViewComponentTagHelperChunk("Bar", visitedTagHelperChunks);
            chunkList.Add(viewComponentTagHelperChunk);

            return chunkList;
        }

        private static TagHelperChunk GetTagHelperChunk(string name)
        {
            var tagHelperChunk = new TagHelperChunk(
                name.ToLower(),
                TagMode.SelfClosing,
                new List<TagHelperAttributeTracker>(),
                new List<TagHelperDescriptor>
                {
                    new TagHelperDescriptor
                    {
                        TagName = name.ToLower(),
                        TypeName = $"{name}Type",
                    }
                });

            return tagHelperChunk;
        }

        private static ParentChunk GetNestedViewComponentTagHelperChunk(string name, bool visitedTagHelperChunks)
        {
            var parentChunk = new ParentChunk();
            var tagHelperChunk = GetViewComponentTagHelperChunk(name, visitedTagHelperChunks);

            if (visitedTagHelperChunks)
            {
                var tagHelperDescriptor = tagHelperChunk.Descriptors.First();
                tagHelperDescriptor.TypeName = $"{_testNamespace}.{_testClass}.{tagHelperDescriptor.TypeName}";
            }

            parentChunk.Children.Add(tagHelperChunk);
            return parentChunk;
        }

        private static TagHelperChunk GetViewComponentTagHelperChunk(string name, bool visitedTagHelperChunks)
        {
            // Add a view component tag helper.
            var typeName = visitedTagHelperChunks ? $"{_testNamespace}.{_testClass}.{name}Type" : $"{name}Type";

            var attribute = new TagHelperAttributeDescriptor
            {
                Name = "attribute",
                PropertyName = "Attribute",
                TypeName = "string"
            };

            var requiredAttribute = new TagHelperRequiredAttributeDescriptor
            {
                Name = "Attribute"
            };

            var tagHelperDescriptor = new TagHelperDescriptor
            {
                TagName = name.ToLower(),
                TypeName = typeName,
                Attributes = new List<TagHelperAttributeDescriptor>
                {
                    attribute
                },
                RequiredAttributes = new List<TagHelperRequiredAttributeDescriptor>
                {
                    requiredAttribute
                }
            };

            tagHelperDescriptor.PropertyBag.Add(
                ViewComponentTagHelperDescriptorConventions.ViewComponentNameKey,
                name);

            var tagHelperChunk = new TagHelperChunk(
                $"vc:{name.ToLower()}",
                TagMode.SelfClosing,
                new List<TagHelperAttributeTracker>(),
                new List<TagHelperDescriptor>
                {
                    tagHelperDescriptor
                });

            return tagHelperChunk;
        }

        public static CodeGeneratorContext CreateDummyCodeGeneratorContext()
        {
            var syntaxTreeNode = new Mock<Span>(new SpanBuilder());
            var language = new CSharpRazorCodeLanguage();
            var host = new RazorEngineHost(language);
            var chunkGeneratorContext = new ChunkGeneratorContext
            (
                host,
                _testClass,
                _testNamespace,
                _testFile,
                false
            );

            var codeGeneratorContext = new CodeGeneratorContext(
                chunkGeneratorContext,
                errorSink: new ErrorSink());
            return codeGeneratorContext;
        }
    }
}