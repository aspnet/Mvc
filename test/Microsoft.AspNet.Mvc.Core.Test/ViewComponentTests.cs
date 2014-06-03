// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.ModelBinding;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class ViewComponentTests
    {
        [Fact]
        public void ViewComponent_ViewBag_UsesViewData()
        {
            // Arrange
            var viewComponent = new TestViewComponent()
            {
                ViewData = new ViewDataDictionary(metadataProvider: null),
            };

            // Act
            viewComponent.ViewBag.A = "Alice";
            viewComponent.ViewBag.B = "Bob";

            // Assert
            Assert.Equal(2, viewComponent.ViewData.Count);
            Assert.Equal("Alice", viewComponent.ViewData["A"]);
            Assert.Equal("Bob", viewComponent.ViewData["B"]);
        }

        [Fact]
        public void ViewComponent_ViewData_StoresDataForViewBag()
        {
            // Arrange
            var viewComponent = new TestViewComponent()
            {
                ViewData = new ViewDataDictionary(metadataProvider: null),
            };

            // Act
            viewComponent.ViewData["A"] = "Alice";
            viewComponent.ViewData["B"] = "Bob";

            // Assert
            Assert.Equal(2, viewComponent.ViewData.Count);
            Assert.Equal("Alice", viewComponent.ViewBag.A);
            Assert.Equal("Bob", viewComponent.ViewBag.B);
        }

        [Fact]
        public void ViewComponent_Content_SetsResultEncodedContent()
        {
            // Arrange
            var viewComponent = new TestViewComponent();

            // Act
            var actualResult = viewComponent.Content("TestContent");

            // Assert
            Assert.Equal(typeof(ContentViewComponentResult), actualResult.GetType());
            Assert.Same("TestContent", actualResult.EncodedContent.ToString());
        }

        [Fact]
        public void ViewComponent_Json_SetsResultValue()
        {
            // Arrange
            var viewComponent = new TestViewComponent();
            var testValue = new object();

            // Act
            var actualResult = viewComponent.Json(testValue);

            // Assert
            Assert.Equal(typeof(JsonViewComponentResult), actualResult.GetType());
            Assert.Same(testValue, actualResult.Value);
        }

        [Fact]
        public void ViewComponent_View_WithEmptyParameter_SetsResultViewWithDefaultViewName()
        {
            // Arrange
            var viewComponent = new TestViewComponent()
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider()),
            };

            // Act
            var actualResult = viewComponent.View();

            // Assert
            Assert.Equal(typeof(ViewViewComponentResult), actualResult.GetType());
            Assert.Equal(new ViewDataDictionary<object>(viewComponent.ViewData), actualResult.ViewData);
            Assert.Null(actualResult.ViewData.Model);
            Assert.Equal("Default", actualResult.ViewName);
            Assert.Same(viewComponent.ViewEngine, actualResult.ViewEngine);
        }

        [Fact]
        public void ViewComponent_View_WithViewNameParameter_SetsResultViewWithCustomViewName()
        {
            // Arrange
            var viewComponent = new TestViewComponent()
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider()),
            };

            // Act
            var actualResult = viewComponent.View("CustomViewName");

            // Assert
            Assert.Equal(typeof(ViewViewComponentResult), actualResult.GetType());
            Assert.Equal(typeof(ViewDataDictionary<object>), actualResult.ViewData.GetType());
            Assert.Null(actualResult.ViewData.Model);
            Assert.Equal("CustomViewName", actualResult.ViewName);
            Assert.Same(viewComponent.ViewEngine, actualResult.ViewEngine);
        }

        [Fact]
        public void ViewComponent_View_WithModelParameter_SetsResultViewWithDefaultViewNameAndModel()
        {
            // Arrange
            var viewComponent = new TestViewComponent()
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider()),
            };
            var model = new object();

            // Act
            var actualResult = viewComponent.View(model);

            // Assert
            Assert.Equal(typeof(ViewViewComponentResult), actualResult.GetType());
            Assert.Equal(typeof(ViewDataDictionary<object>), actualResult.ViewData.GetType());
            Assert.Same(model, actualResult.ViewData.Model);
            Assert.Equal("Default", actualResult.ViewName);
            Assert.Same(viewComponent.ViewEngine, actualResult.ViewEngine);
        }

        [Fact]
        public void ViewComponent_View_WithViewNameAndModelParameters_SetsResultViewWithCustomViewNameAndModel()
        {
            // Arrange
            var viewComponent = new TestViewComponent()
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider()),
            };
            var model = new object();

            // Act
            var actualResult = viewComponent.View("CustomViewName", model);

            // Assert
            Assert.Equal(typeof(ViewViewComponentResult), actualResult.GetType());
            Assert.Equal(typeof(ViewDataDictionary<object>), actualResult.ViewData.GetType());
            Assert.Same(model, actualResult.ViewData.Model);
            Assert.Equal("CustomViewName", actualResult.ViewName);
            Assert.Same(viewComponent.ViewEngine, actualResult.ViewEngine);
        }

        private class TestViewComponent : ViewComponent
        {
        }
    }
}
