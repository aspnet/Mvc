// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor.Test
{
    public class RazorViewEngineTest
    {
        private static readonly Dictionary<string, object> _areaTestContext = new Dictionary<string, object>()
        {
            {"area", "foo"},
            {"controller", "bar"},
        };

        private static readonly Dictionary<string, object> _controllerTestContext = new Dictionary<string, object>()
        {
            {"controller", "bar"},
        };

        public static IEnumerable<string[]> InvalidViewNameValues
        {
            get
            {
                yield return new[] { "~/foo/bar" };
                yield return new[] { "/foo/bar" };
                yield return new[] { "~/foo/bar.txt" };
                yield return new[] { "/foo/bar.txt" };
            }
        }

        [Theory]
        [MemberData("InvalidViewNameValues")]
        public void FindViewFullPathFailsWithNoCshtmlEnding(string viewName)
        {
            // Arrange
            var viewEngine = CreateSearchLocationViewEngineTester();
            var context = GetActionContext(_controllerTestContext);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                viewEngine.FindView(context, viewName));
        }

        [Theory]
        [MemberData("InvalidViewNameValues")]
        public void FindViewFullPathSucceedsWithCshtmlEnding(string viewName)
        {
            // Arrange
            var viewEngine = CreateSearchLocationViewEngineTester();
            // Append .cshtml so the viewname is no longer invalid
            viewName += ".cshtml";
            var context = GetActionContext(_controllerTestContext);

            // Act & Assert
            // If this throws then our test case fails
            var result = viewEngine.FindPartialView(context, viewName);

            Assert.False(result.Success);
        }

        [Theory]
        [MemberData("InvalidViewNameValues")]
        public void FindPartialViewFullPathFailsWithNoCshtmlEnding(string partialViewName)
        {
            // Arrange
            var viewEngine = CreateSearchLocationViewEngineTester();
            var context = GetActionContext(_controllerTestContext);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                viewEngine.FindPartialView(context, partialViewName));
        }

        [Theory]
        [MemberData("InvalidViewNameValues")]
        public void FindPartialViewFullPathSucceedsWithCshtmlEnding(string partialViewName)
        {
            // Arrange
            var viewEngine = CreateSearchLocationViewEngineTester();
            // Append .cshtml so the viewname is no longer invalid
            partialViewName += ".cshtml";
            var context = GetActionContext(_controllerTestContext);

            // Act & Assert
            // If this throws then our test case fails
            var result = viewEngine.FindPartialView(context, partialViewName);

            Assert.False(result.Success);
        }

        [Fact]
        public void FindPartialViewFailureSearchesCorrectLocationsWithAreas()
        {
            // Arrange
            var viewEngine = CreateSearchLocationViewEngineTester();
            var context = GetActionContext(_areaTestContext);

            // Act
            var result = viewEngine.FindPartialView(context, "partial");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(new[] {
                "/Areas/foo/Views/bar/partial.cshtml",
                "/Areas/foo/Views/Shared/partial.cshtml",
                "/Views/Shared/partial.cshtml",
            }, result.SearchedLocations);
        }

        [Fact]
        public void FindPartialView_UsesViewExtensionSuffix()
        {
            // Arrange
            var expected = new[]
            {
                "/Areas/foo/Views/bar/partial.test-view-extension",
                "/Areas/foo/Views/Shared/partial.test-view-extension",
                "/Views/Shared/partial.test-view-extension"
            };
            var options = new MvcOptions();
            options.ViewEngineOptions.ViewExtension = ".test-view-extension";
            var viewEngine = CreateSearchLocationViewEngineTester(options);
            var context = GetActionContext(_areaTestContext);

            // Act
            var result = viewEngine.FindPartialView(context, "partial");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(expected, result.SearchedLocations);
        }

        [Fact]
        public void FindPartialViewFailureSearchesCorrectLocationsWithoutAreas()
        {
            // Arrange
            var viewEngine = CreateSearchLocationViewEngineTester();
            var context = GetActionContext(_controllerTestContext);

            // Act
            var result = viewEngine.FindPartialView(context, "partialNoArea");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(new[] {
                "/Views/bar/partialNoArea.cshtml",
                "/Views/Shared/partialNoArea.cshtml",
            }, result.SearchedLocations);
        }

        [Fact]
        public void FindViewFailureSearchesCorrectLocationsWithAreas()
        {
            // Arrange
            var viewEngine = CreateSearchLocationViewEngineTester();
            var context = GetActionContext(_areaTestContext);

            // Act
            var result = viewEngine.FindView(context, "full");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(new[] {
                "/Areas/foo/Views/bar/full.cshtml",
                "/Areas/foo/Views/Shared/full.cshtml",
                "/Views/Shared/full.cshtml",
            }, result.SearchedLocations);
        }

        [Fact]
        public void FindView_UsesViewExtensionSuffix()
        {
            // Arrange
            var expected = new[]
            {
                "/Views/bar/viewname.test-extension",
                "/Views/Shared/viewname.test-extension",
            };
            var options = new MvcOptions();
            options.ViewEngineOptions.ViewExtension = ".test-extension";
            var viewEngine = CreateSearchLocationViewEngineTester(options);
            var context = GetActionContext(_controllerTestContext);

            // Act
            var result = viewEngine.FindPartialView(context, "viewname");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(expected, result.SearchedLocations);
        }

        [Fact]
        public void FindViewFailureSearchesCorrectLocationsWithoutAreas()
        {
            // Arrange
            var viewEngine = CreateSearchLocationViewEngineTester();
            var context = GetActionContext(_controllerTestContext);

            // Act
            var result = viewEngine.FindView(context, "fullNoArea");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(new[] {
                "/Views/bar/fullNoArea.cshtml",
                "/Views/Shared/fullNoArea.cshtml",
            }, result.SearchedLocations);
        }

        [Fact]
        public void FindView_ReturnsRazorView_IfLookupWasSuccessful()
        {
            // Arrange
            var pageFactory = new Mock<IRazorPageFactory>();
            var page = Mock.Of<IRazorPage>();
            pageFactory.Setup(p => p.CreateInstance(It.IsAny<string>()))
                       .Returns(Mock.Of<IRazorPage>());

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(p => p.GetService(typeof(IRazorPageFactory)))
                           .Returns(pageFactory.Object);
            serviceProvider.Setup(p => p.GetService(typeof(IRazorPageActivator)))
                           .Returns(Mock.Of<IRazorPageActivator>());
            serviceProvider.Setup(p => p.GetService(typeof(IViewStartProvider)))
                           .Returns(Mock.Of<IViewStartProvider>());
            var viewEngine = new RazorViewEngine(pageFactory.Object,
                                                 new TypeActivator(),
                                                 serviceProvider.Object,
                                                 GetOptionsAccessor(new MvcOptions()));
            var context = GetActionContext(_controllerTestContext);

            // Act
            var result = viewEngine.FindView(context, "test-view");

            // Assert
            Assert.True(result.Success);
            Assert.IsType<RazorView>(result.View);
            Assert.Equal("/Views/bar/test-view.cshtml", result.ViewName);
        }

        private IViewEngine CreateSearchLocationViewEngineTester(MvcOptions options = null)
        {
            options = options ?? new MvcOptions();
            var pageFactory = new Mock<IRazorPageFactory>();
            pageFactory.Setup(vpf => vpf.CreateInstance(It.IsAny<string>()))
                       .Returns<RazorPage>(null);

            var viewEngine = new RazorViewEngine(pageFactory.Object,
                                                 Mock.Of<ITypeActivator>(),
                                                 Mock.Of<IServiceProvider>(),
                                                 GetOptionsAccessor(options));

            return viewEngine;
        }

        private static ActionContext GetActionContext(IDictionary<string, object> routeValues)
        {
            var httpContext = Mock.Of<HttpContext>();
            var routeData = new RouteData { Values = routeValues };
            return new ActionContext(httpContext, routeData, new ActionDescriptor());
        }

        private static IOptionsAccessor<MvcOptions> GetOptionsAccessor(MvcOptions options)
        {
            var accessor = new Mock<IOptionsAccessor<MvcOptions>>();
            accessor.SetupGet(a => a.Options)
                    .Returns(options);

            return accessor.Object;
        }
    }
}
