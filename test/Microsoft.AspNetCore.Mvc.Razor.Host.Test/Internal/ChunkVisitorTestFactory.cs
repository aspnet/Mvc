﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Chunks;
using Microsoft.AspNetCore.Razor.Chunks.Generators;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.Host.Test.Internal
{
    public static class ChunkVisitorTestFactory
    {
        private static string _testClass = "TestClass";
        private static string _testNamespace = "TestNamespace";
        private static string _testFile = "TestFile";

        public static CodeGeneratorContext CreateDummyCodeGeneratorContext()
        {
            var language = new CSharpRazorCodeLanguage();
            var host = new RazorEngineHost(language);
            var chunkGeneratorContext = new ChunkGeneratorContext
            (
                host,
                _testClass,
                _testNamespace,
                _testFile,
                shouldGenerateLinePragmas: false
            );

            var codeGeneratorContext = new CodeGeneratorContext(
                chunkGeneratorContext,
                errorSink: new ErrorSink());
            return codeGeneratorContext;
        }

        public static IList<Chunk> GetTestChunks(bool visitedTagHelperChunks)
        {
            var chunkList = new List<Chunk>();

            var normalTagHelperChunk = GetTagHelperChunk("Baz");
            chunkList.Add(normalTagHelperChunk);

            var nestedViewComponentTagHelperChunk =
                GetNestedViewComponentTagHelperChunk("Foo", visitedTagHelperChunks);
            chunkList.Add(nestedViewComponentTagHelperChunk);

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
                        AssemblyName = $"{name}Assembly",
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
                TypeName = typeof(string).FullName
            };

            var requiredAttribute = new TagHelperRequiredAttributeDescriptor
            {
                Name = "Attribute"
            };

            var tagHelperDescriptor = new TagHelperDescriptor
            {
                AssemblyName = $"{name}Assembly",
                TagName = name.ToLower(),
                TypeName = typeName,
                Attributes = new[]
                {
                    attribute
                },
                RequiredAttributes = new[]
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
                new[]
                {
                    tagHelperDescriptor
                });

            return tagHelperChunk;
        }
    }
}