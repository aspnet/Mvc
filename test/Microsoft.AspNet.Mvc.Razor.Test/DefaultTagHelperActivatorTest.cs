// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Core;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor.Test
{
    public class DefaultTagHelperActivatorTest
    {
        [Theory]
        [InlineData("test", 100)]
        [InlineData(null, -1)]
        public void Activate_InitializesTagHelpers(string name, int number)
        {
            // Arrange
            var services = new ServiceCollection();
            services.InitializeTagHelper<TestTagHelper>((h, vc) =>
            {
                h.Name = name;
                h.Number = number;
                h.ViewDataValue = vc.ViewData["TestData"];
            });
            var httpContext = MakeHttpContext(services.BuildServiceProvider());
            var viewContext = MakeViewContext(httpContext);
            var viewDataValue = new object();
            viewContext.ViewData.Add("TestData", viewDataValue);
            var activator = new DefaultTagHelperActivator();
            var helper = new TestTagHelper();

            // Act
            activator.Activate(helper, viewContext);

            // Assert
            Assert.Equal(name, helper.Name);
            Assert.Equal(number, helper.Number);
            Assert.Same(viewDataValue, helper.ViewDataValue);
        }

        [Fact]
        public void Activate_InitializesTagHelpersAfterActivatingProperties()
        {
            // Arrange
            var services = new ServiceCollection();
            services.InitializeTagHelper<TestTagHelper>((h, _) => h.ViewContext = MakeViewContext(MakeHttpContext()));
            var httpContext = MakeHttpContext(services.BuildServiceProvider());
            var viewContext = MakeViewContext(httpContext);
            var activator = new DefaultTagHelperActivator();
            var helper = new TestTagHelper();

            // Act
            activator.Activate(helper, viewContext);

            // Assert
            Assert.NotSame(viewContext, helper.ViewContext);
        }

        private static HttpContext MakeHttpContext(IServiceProvider services = null)
        {
            var httpContext = new Mock<DefaultHttpContext>();
            httpContext.CallBase = true;
            if (services != null)
            {
                httpContext.Setup(c => c.RequestServices).Returns(services);
            }
            return httpContext.Object;
        }

        private static ViewContext MakeViewContext(HttpContext httpContext)
        {
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var metadataProvider = new EmptyModelMetadataProvider();
            var viewData = new ViewDataDictionary(metadataProvider);
            var viewContext = new ViewContext(
                actionContext,
                Mock.Of<IView>(),
                viewData,
                Mock.Of<ITempDataDictionary>(),
                TextWriter.Null);

            return viewContext;
        }

        private class TestTagHelper : TagHelper
        {
            public string Name { get; set; } = "Initial Name";

            public int Number { get; set; } = 1000;

            public object ViewDataValue { get; set; } = new object();

            [Activate]
            public ViewContext ViewContext { get; set; }
        }
    }
}
