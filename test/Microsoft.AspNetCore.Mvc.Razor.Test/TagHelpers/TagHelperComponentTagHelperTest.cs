// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.TagHelpers
{
    public class TagHelperComponentTagHelperTest
    {
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

            var testTagHelperComponentTagHelper = new TestTagHelperComponentTagHelper(new List<ITagHelperComponent>()
            {
                new TestHeadTagHelperComponent()
            }, loggerFactory);

            // Act
            testTagHelperComponentTagHelper.Init(tagHelperContext);

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

            var testTagHelperComponentTagHelper = new TestTagHelperComponentTagHelper(new List<ITagHelperComponent>()
            {
                new TestHeadTagHelperComponent()
            }, loggerFactory);

            // Act
            await testTagHelperComponentTagHelper.ProcessAsync(tagHelperContext, output);

            // Assert           
            Assert.Equal($"Tag helper component '{typeof(TestHeadTagHelperComponent)}' processed.", sink.Writes[0].State.ToString(), StringComparer.Ordinal);
        }

        private class TestHeadTagHelperComponent : ITagHelperComponent
        {
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

        private class TestTagHelperComponentTagHelper : TagHelperComponentTagHelper
        {            
            public TestTagHelperComponentTagHelper(
                IEnumerable<ITagHelperComponent> components,
                ILoggerFactory loggerFactory                )
                : base(components, loggerFactory)
            {               
            }           
        }

        private class CallbackTagHelperComponentTagHelper : TagHelperComponentTagHelper
        {
            private readonly Action _initCallback;
            private readonly Action _processAsyncCallback;

            public CallbackTagHelperComponentTagHelper(
                IEnumerable<ITagHelperComponent> components,
                ILoggerFactory loggerFactory,
                Action initCallback,
                Action processAsyncCallback)
                : base(components, loggerFactory)
            {
                _initCallback = initCallback;
                _processAsyncCallback = processAsyncCallback;
            }

            public override void Init(TagHelperContext context)
            {
                _initCallback();
            }

            public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
            {
                _processAsyncCallback();

                return base.ProcessAsync(context, output);
            }
        }
    }
}
