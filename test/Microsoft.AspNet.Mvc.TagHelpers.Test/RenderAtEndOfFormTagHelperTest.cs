﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.AspNet.Razor.TagHelpers;
using Xunit;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    public class RenderAtEndOfFormTagHelperTest
    {
        public static TheoryData RenderAtEndOfFormTagHelperData
        {
            get
            {
                // tagBuilderList, expectedOutput
                return new TheoryData<List<TagBuilder>, string>
                {
                    {
                        new List<TagBuilder>
                        {
                            GetTagBuilder("input", "SomeName", "hidden", "false", TagRenderMode.SelfClosing)
                        },
                        @"<input name=""SomeName"" type=""hidden"" value=""false"" />"
                    },
                    {
                        new List<TagBuilder>
                        {
                            GetTagBuilder("input", "SomeName", "hidden", "false", TagRenderMode.SelfClosing),
                            GetTagBuilder("input", "SomeOtherName", "hidden", "false", TagRenderMode.SelfClosing)
                        },
                        @"<input name=""SomeName"" type=""hidden"" value=""false"" />" +
                        @"<input name=""SomeOtherName"" type=""hidden"" value=""false"" />"
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(RenderAtEndOfFormTagHelperData))]
        public async Task Process_AddsHiddenInputTag_FromEndOfFormContent(
            List<TagBuilder> tagBuilderList,
            string expectedOutput)
        {
            // Arrange
            var viewContext = new ViewContext();
            var tagHelperOutput = new TagHelperOutput(
                tagName: "form",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult) =>
                {
                    Assert.True(viewContext.FormContext.CanRenderAtEndOfForm);
                    foreach (var item in tagBuilderList)
                    {
                        viewContext.FormContext.EndOfFormContent.Add(item);
                    }

                    return Task.FromResult<TagHelperContent>(new DefaultTagHelperContent());
                });

            var tagHelperContext = new TagHelperContext(
                Enumerable.Empty<IReadOnlyTagHelperAttribute>(),
                new Dictionary<object, object>(),
                "someId");

            var tagHelper = new RenderAtEndOfFormTagHelper
            {
                ViewContext = viewContext
            };
            tagHelper.Init(tagHelperContext);

            // Act
            await tagHelper.ProcessAsync(context: tagHelperContext, output: tagHelperOutput);

            // Assert
            Assert.Equal(expectedOutput, tagHelperOutput.PostContent.GetContent());
        }

        [Theory]
        [MemberData(nameof(RenderAtEndOfFormTagHelperData))]
        public async Task Process_AddsHiddenInputTag_FromEndOfFormContent_WithCachedBody(
            List<TagBuilder> tagBuilderList,
            string expectedOutput)
        {
            // Arrange
            var viewContext = new ViewContext();
            var runner = new TagHelperRunner();
            var tagHelperExecutionContext = new TagHelperExecutionContext(
                "form",
                TagMode.StartTagAndEndTag,
                items: new Dictionary<object, object>(),
                uniqueId: string.Empty,
                executeChildContentAsync: () =>
                {
                    foreach (var item in tagBuilderList)
                    {
                        viewContext.FormContext.EndOfFormContent.Add(item);
                    }

                    return Task.FromResult(true);
                },
                startTagHelperWritingScope: () => { },
                endTagHelperWritingScope: () => new DefaultTagHelperContent());

            // This TagHelper will pre-execute the child content forcing the body to be cached.
            tagHelperExecutionContext.Add(new ChildContentInvoker());
            tagHelperExecutionContext.Add(new RenderAtEndOfFormTagHelper
            {
                ViewContext = viewContext
            });

            // Act
            var output = await runner.RunAsync(tagHelperExecutionContext);

            // Assert
            Assert.Equal(expectedOutput, output.PostContent.GetContent());
        }

        private static TagBuilder GetTagBuilder(string tag, string name, string type, string value, TagRenderMode mode)
        {
            var tagBuilder = new TagBuilder(tag);
            tagBuilder.MergeAttribute("name", name);
            tagBuilder.MergeAttribute("type", type);
            tagBuilder.MergeAttribute("value", value);
            tagBuilder.TagRenderMode = mode;

            return tagBuilder;
        }

        private class ChildContentInvoker : TagHelper
        {
            public override int Order
            {
                get
                {
                    return int.MinValue;
                }
            }

            public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
            {
                await output.GetChildContentAsync();
            }
        }
    }
}
