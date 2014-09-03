// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.PipelineCore;
using Microsoft.AspNet.Routing;
using Xunit;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class LanguageViewLocationExpanderTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void PopulateValues_DoesNotAddToDictionary_IfValueFactoryReturnsNullOrEmpty(string value)
        {
            // Arrange
            var expander = new LanguageViewLocationExpander(c => value);
            var context = new ViewLocationExpanderContext(GetActionContext(), "view-name")
            {
                Values = new Dictionary<string, string>(StringComparer.Ordinal)
            };

            // Act
            expander.PopulateValues(context);

            // Assert
            Assert.Empty(context.Values);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ExpandViewLocations_ReturnsOriginalSequence_IfalueFactoryReturnsNullOrEmpty(string value)
        {
            // Arrange
            var expander = new LanguageViewLocationExpander(c => value);
            var context = new ViewLocationExpanderContext(GetActionContext(), "view-name")
            {
                Values = new Dictionary<string, string>(StringComparer.Ordinal)
            };
            var seed = new[] { "some-seed" };

            // Act
            expander.PopulateValues(context);
            var result = expander.ExpandViewLocations(context, seed);

            // Assert
            Assert.Same(result, seed);
        }

        [Fact]
        public void ExpandViewLocations_AddsValueFromValueFactoryAsViewDirectory()
        {
            // Arrange
            var expected = new[]
            {
                "/views/{1}/fr/{0}.cshtml",
                "/views/{1}/{0}.cshtml",
                "/areas/{2}/views/{1}/fr/{0}.cshtml",
                "/areas/{2}/views/{1}/{0}.cshtml",
                "/areas/{2}/views/shared/fr/{0}.cshtml",
                "/areas/{2}/views/shared/{0}.cshtml",
                "/views/shared/fr/{0}.cshtml",
                "/views/shared/{0}.cshtml"
            };
            var expander = new LanguageViewLocationExpander(c =>
            {
                Assert.NotNull(c.HttpContext);
                return "fr";
            });
            var context = new ViewLocationExpanderContext(GetActionContext(), "view-name")
            {
                Values = new Dictionary<string, string>(StringComparer.Ordinal)
            };
            var seed = new[]
            {
                "/views/{1}/{0}.cshtml",
                "/areas/{2}/views/{1}/{0}.cshtml",
                "/areas/{2}/views/shared/{0}.cshtml",
                "/views/shared/{0}.cshtml"
            };

            // Act
            expander.PopulateValues(context);
            var result = expander.ExpandViewLocations(context, seed);

            // Assert

            Assert.Equal(expected, result);
        }

        private static ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext();
            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }
    }
}