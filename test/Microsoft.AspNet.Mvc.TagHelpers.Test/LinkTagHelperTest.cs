// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http.Core;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    public class LinkTagHelperTest
    {
        [Fact]
        public void RunsWhenRequiredAttributesArePresent()
        {
            // Arrange
            var context = MakeTagHelperContext(
                attributes: new Dictionary<string, object>
                {
                    ["asp-fallback-href"] = "test.css",
                    ["asp-fallback-test-class"] = "hidden",
                    ["asp-fallback-test-property"] = "visibility",
                    ["asp-fallback-test-value"] = "hidden"
                });
            var output = MakeTagHelperOutput("link");
            var logger = new Mock<ILogger<LinkTagHelper>>();
            var hostingEnvironment = new Mock<IHostingEnvironment>();
            var viewContext = MakeViewContext();
            var helper = new LinkTagHelper
            {
                Logger = logger.Object,
                HostingEnvironment = hostingEnvironment.Object,
                ViewContext = viewContext,
                FallbackHref = "test.css",
                FallbackTestClass = "hidden",
                FallbackTestProperty = "visibility",
                FallbackTestValue = "hidden"
            };

            // Act
            helper.Process(context, output);

            // Assert
            Assert.Null(output.TagName);
            Assert.NotNull(output.Content);
            Assert.True(output.ContentSet);
        }

        [Fact]
        public void PreservesOrderOfSourceAttributesWhenRun()
        {
            // Arrange
            var context = MakeTagHelperContext(
                attributes: new Dictionary<string, object>
                {
                    ["rel"] = "stylesheet",
                    ["data-extra"] = "something",
                    ["href"] = "test.css",
                    ["asp-fallback-href"] = "test.css",
                    ["asp-fallback-test-class"] = "hidden",
                    ["asp-fallback-test-property"] = "visibility",
                    ["asp-fallback-test-value"] = "hidden"
                });
            var output = MakeTagHelperOutput("link",
                attributes: new Dictionary<string, string>
                {
                    ["rel"] = "stylesheet",
                    ["data-extra"] = "something",
                    ["href"] = "test.css"
                });
            var logger = new Mock<ILogger<LinkTagHelper>>();
            var hostingEnvironment = new Mock<IHostingEnvironment>();
            var viewContext = MakeViewContext();
            var helper = new LinkTagHelper
            {
                Logger = logger.Object,
                HostingEnvironment = hostingEnvironment.Object,
                ViewContext = viewContext,
                FallbackHref = "test.css",
                FallbackTestClass = "hidden",
                FallbackTestProperty = "visibility",
                FallbackTestValue = "hidden"
            };

            // Act
            helper.Process(context, output);

            // Assert
            Assert.StartsWith("<link rel=\"stylesheet\" data-extra=\"something\" href=\"test.css\"", output.Content);
        }

        [Theory]
        [MemberData(nameof(DoesNotRunWhenARequiredAttributeIsMissing_Data))]
        public void DoesNotRunWhenARequiredAttributeIsMissing(IDictionary<string, object> attributes)
        {
            // Arrange
            var context = MakeTagHelperContext(attributes);
            var output = MakeTagHelperOutput("link");
            var logger = new Mock<ILogger<LinkTagHelper>>();
            var hostingEnvironment = new Mock<IHostingEnvironment>();
            var viewContext = MakeViewContext();
            var helper = new LinkTagHelper
            {
                Logger = logger.Object,
                HostingEnvironment = hostingEnvironment.Object,
                ViewContext = viewContext,
                // This is commented out on purpose: FallbackHref = "test.css",
                FallbackTestClass = "hidden",
                FallbackTestProperty = "visibility",
                FallbackTestValue = "hidden"
            };

            // Act
            helper.Process(context, output);

            // Assert
            Assert.NotNull(output.TagName);
            Assert.False(output.ContentSet);
        }

        public static TheoryData DoesNotRunWhenARequiredAttributeIsMissing_Data
        {
            get
            {
                return new TheoryData<IDictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        // This is commented out on purpose: ["asp-fallback-href"] = "test.css",
                        ["asp-fallback-test-class"] = "hidden",
                        ["asp-fallback-test-property"] = "visibility",
                        ["asp-fallback-test-value"] = "hidden"
                    },
                    new Dictionary<string, object>
                    {
                        ["asp-fallback-href"] = "test.css",
                        ["asp-fallback-test-class"] = "hidden",
                        // This is commented out on purpose: ["asp-fallback-test-property"] = "visibility",
                        ["asp-fallback-test-value"] = "hidden"
                    },
                    new Dictionary<string, object>
                    {
                        // This is commented out on purpose: ["asp-fallback-href-include"], "test.css",
                        ["asp-fallback-href-exclude"] = "**/*.min.css",
                        ["asp-fallback-test-class"] = "hidden",
                        ["asp-fallback-test-property"] = "visibility",
                        ["asp-fallback-test-value"] = "hidden"
                    }
                };
            }
        }

        [Fact]
        public void DoesNotRunWhenAllRequiredAttributesAreMissing()
        {
            // Arrange
            var context = MakeTagHelperContext();
            var output = MakeTagHelperOutput("link");
            var logger = new Mock<ILogger<LinkTagHelper>>();
            var hostingEnvironment = new Mock<IHostingEnvironment>();
            var viewContext = MakeViewContext();
            var helper = new LinkTagHelper
            {
                Logger = logger.Object,
                HostingEnvironment = hostingEnvironment.Object,
                ViewContext = viewContext
            };

            // Act
            helper.Process(context, output);

            // Assert
            Assert.NotNull(output.TagName);
            Assert.False(output.ContentSet);
        }

        private static ViewContext MakeViewContext()
        {
            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
            var metadataProvider = new EmptyModelMetadataProvider();
            var viewData = new ViewDataDictionary(metadataProvider);
            var viewContext = new ViewContext(actionContext, Mock.Of<IView>(), viewData, TextWriter.Null);

            return viewContext;
        }

        private static TagHelperContext MakeTagHelperContext(
            IDictionary<string, object> attributes = null,
            string content = null)
        {
            attributes = attributes ?? new Dictionary<string, object>();

            return new TagHelperContext(attributes, Guid.NewGuid().ToString("N"), () => Task.FromResult(content));
        }

        private static TagHelperOutput MakeTagHelperOutput(string tagName, IDictionary<string, string> attributes = null)
        {
            attributes = attributes ?? new Dictionary<string, string>();

            return new TagHelperOutput(tagName, attributes);
        }
    }
}