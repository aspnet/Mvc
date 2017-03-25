// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.TagHelpers
{
    public class BodyHeadTagHelperTest
    {
        [Fact]
        public async Task ProcessAsync_AppliesToHTMLElement_GeneratesExpectedOutput()
        {
            // Arrange
            var tagHelperContext = new TagHelperContext(
                "head",
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            var output = new TagHelperOutput(
                "head",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(
                    new DefaultTagHelperContent()));

            var headTagHelper = new HeadTagHelper(new List<ITagHelperComponent>()
            {
                new TestHeadTagHelperComponent()
            }, NullLoggerFactory.Instance);

            // Act
            headTagHelper.Init(tagHelperContext);
            await headTagHelper.ProcessAsync(tagHelperContext, output);

            // Assert
            Assert.Equal("<script>'This was injected!!'</script>", output.PostContent.GetContent(), StringComparer.Ordinal);
        }

        [Fact]
        public async Task ProcessAsync_DoesNotApplyToHTMLElement_GeneratesExpectedOutput()
        {
            // Arrange
            var tagHelperContext = new TagHelperContext(
                "body",
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            var output = new TagHelperOutput(
                "body",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(
                    new DefaultTagHelperContent()));

            var bodyTagHelper = new BodyTagHelper(new List<ITagHelperComponent>()
            {
                new TestHeadTagHelperComponent()
            }, NullLoggerFactory.Instance);

            // Act
            bodyTagHelper.Init(tagHelperContext);
            await bodyTagHelper.ProcessAsync(tagHelperContext, output);

            // Assert
            Assert.Empty(output.PostContent.GetContent());
        }

        [Fact]
        public void Init_LogsTagHelperComponentInitialized()
        {
            // Arrange
            var sink = new TestSink();
            var loggerFactory = new TestLoggerFactory(sink, enabled: true);
            var tagHelperContext = new TagHelperContext(
                "head",
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            var output = new TagHelperOutput(
                "head",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(
                    new DefaultTagHelperContent()));

            var bodyHeadTagHelper = new HeadTagHelper(new List<ITagHelperComponent>()
            {
                new TestHeadTagHelperComponent()
            }, loggerFactory);

            // Act
            bodyHeadTagHelper.Init(tagHelperContext);

            // Assert
            Assert.Equal($"Tag helper component '{typeof(TestHeadTagHelperComponent)}' initialized.", sink.Writes[0].State.ToString(), StringComparer.Ordinal);
        }

        [Fact]
        public async Task ProcessAsync_LogsComponentProcessed()
        {
            // Arrange
            var sink = new TestSink();
            var loggerFactory = new TestLoggerFactory(sink, enabled: true);
            var tagHelperContext = new TagHelperContext(
                "head",
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            var output = new TagHelperOutput(
                "head",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(
                    new DefaultTagHelperContent()));

            var headTagHelper = new HeadTagHelper(new List<ITagHelperComponent>()
            {
                new TestHeadTagHelperComponent()
            }, loggerFactory);

            // Act
            await headTagHelper.ProcessAsync(tagHelperContext, output);

            // Assert           
            Assert.Equal($"Tag helper component '{typeof(TestHeadTagHelperComponent)}' processed.", sink.Writes[0].State.ToString(), StringComparer.Ordinal);
        }


        private class TestHeadTagHelperComponent : ITagHelperComponent
        {
            public TestHeadTagHelperComponent()
            {
            }

            public int Order => 1;

            public void Init(TagHelperContext context)
            {
            }

            public Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
            {
                if (context.TagName == "head")
                {
                    output.PostContent.AppendHtml("<script>'This was injected!!'</script>");
                }

                return Task.FromResult(0);
            }
        }
    }
}
