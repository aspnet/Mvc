﻿using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Rendering;
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

        [Fact]
        public void FindPartialViewFailureSearchesCorrectLocationsWithAreas()
        {
            // Arrange
            var searchedLocations = new List<string>();
            var viewEngine = CreateSearchLocationViewEngineTester();

            // Act
            var result = viewEngine.FindPartialView(_areaTestContext, "partial");
            
            // Assert
            Assert.False(result.Success);
            Assert.Equal(new[] { 
                "/Areas/foo/Views/bar/partial.cshtml", 
                "/Areas/foo/Views/Shared/partial.cshtml", 
                "/Views/Shared/partial.cshtml",
            }, result.SearchedLocations);
        }

        [Fact]
        public void FindPartialViewFailureSearchesCorrectLocationsWithoutAreas()
        {
            // Arrange
            var viewEngine = CreateSearchLocationViewEngineTester();

            // Act
            var result = viewEngine.FindPartialView(_controllerTestContext, "partialNoArea");

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

            // Act
            var result = viewEngine.FindView(_areaTestContext, "full");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(new[] { 
                "/Areas/foo/Views/bar/full.cshtml", 
                "/Areas/foo/Views/Shared/full.cshtml", 
                "/Views/Shared/full.cshtml",
            }, result.SearchedLocations);
        }

        [Fact]
        public void FindViewFailureSearchesCorrectLocationsWithoutAreas()
        {
            // Arrange
            var viewEngine = CreateSearchLocationViewEngineTester();

            // Act
            var result = viewEngine.FindView(_controllerTestContext, "fullNoArea");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(new[] { 
                "/Views/bar/fullNoArea.cshtml", 
                "/Views/Shared/fullNoArea.cshtml",
            }, result.SearchedLocations);
        }

        private IViewEngine CreateSearchLocationViewEngineTester()
        {
            var virtualPathFactory = new Mock<IVirtualPathViewFactory>();
            virtualPathFactory.Setup(vpf => vpf.CreateInstance(It.IsAny<string>())).Returns<IView>(null);

            var viewEngine = new RazorViewEngine(virtualPathFactory.Object);

            return viewEngine;
        }
    }
}