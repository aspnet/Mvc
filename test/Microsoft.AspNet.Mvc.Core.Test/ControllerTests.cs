// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Testing;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core
{
    public class ControllerTests
    {
        [Fact]
        public void SettingViewData_AlsoUpdatesViewBag()
        {
            // Arrange
            var metadataProvider = new DataAnnotationsModelMetadataProvider();
            var controller = new Controller();
            var originalViewData = controller.ViewData = new ViewDataDictionary<object>(metadataProvider);
            var replacementViewData = new ViewDataDictionary<object>(metadataProvider);

            // Act
            controller.ViewBag.Hello = "goodbye";
            controller.ViewData = replacementViewData;
            controller.ViewBag.Another = "property";

            // Assert
            Assert.NotSame(originalViewData, controller.ViewData);
            Assert.Same(replacementViewData, controller.ViewData);
            Assert.Null(controller.ViewBag.Hello);
            Assert.Equal("property", controller.ViewBag.Another);
            Assert.Equal("property", controller.ViewData["Another"]);
        }

        [Fact]
        public void Redirect_Temporary_SetsSameUrl()
        {
            // Arrange
            var controller = new Controller();

            // Act
            var result = controller.Redirect("sample\\url");

            // Assert
            Assert.False(result.Permanent);
            Assert.Equal("sample\\url", result.Url);
        }

        [Fact]
        public void Redirect_Permanent_SetsSameUrl()
        {
            // Arrange
            var controller = new Controller();

            // Act
            var result = controller.RedirectPermanent("sample\\url");

            // Assert
            Assert.True(result.Permanent);
            Assert.Equal("sample\\url", result.Url);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Redirect_NullOrEmptyUrl_Throws(string url)
        {
            // Arrange
            var controller = new Controller();

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => controller.Redirect(url: url), "url", "The value cannot be null or empty");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void RedirectPermanent_NullOrEmptyUrl_Throws(string url)
        {
            // Arrange
            var controller = new Controller();

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => controller.RedirectPermanent(url: url), "url", "The value cannot be null or empty");
        }

        [Fact]
        public void RedirectToAction_Temporary_Returns_SameAction()
        {
            // Arrange
            var controller = new Controller();

            // Act
            var resultTemporary = controller.RedirectToAction("SampleAction");

            // Assert
            Assert.False(resultTemporary.Permanent);
            Assert.Equal("SampleAction", resultTemporary.ActionName);
        }

        [Fact]
        public void RedirectToAction_Permanent_Returns_SameAction()
        {
            // Arrange
            var controller = new Controller();

            // Act
            var resultPermanent = controller.RedirectToActionPermanent("SampleAction");

            // Assert
            Assert.True(resultPermanent.Permanent);
            Assert.Equal("SampleAction", resultPermanent.ActionName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("SampleController")]
        public void RedirectToAction_Temporary_Returns_SameController(string controllerName)
        {
            // Arrange
            var controller = new Controller();

            // Act
            var resultTemporary = controller.RedirectToAction("SampleAction", controllerName);

            // Assert
            Assert.False(resultTemporary.Permanent);
            Assert.Equal("SampleAction", resultTemporary.ActionName);
            Assert.Equal(controllerName, resultTemporary.ControllerName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("SampleController")]
        public void RedirectToAction_Permanent_Returns_SameController(string controllerName)
        {
            // Arrange
            var controller = new Controller();

            // Act
            var resultPermanent = controller.RedirectToActionPermanent("SampleAction", controllerName);

            // Assert
            Assert.True(resultPermanent.Permanent);
            Assert.Equal("SampleAction", resultPermanent.ActionName);
            Assert.Equal(controllerName, resultPermanent.ControllerName);
        }

        [Theory]
        [MemberData("RedirectTestData")]
        public void RedirectToAction_Temporary_Returns_SameActionControllerAndRouteValues(object routeValues)
        {
            // Arrange
            var controller = new Controller();

            // Act
            var resultTemporary = controller.RedirectToAction("SampleAction", "SampleController", routeValues);

            // Assert
            Assert.False(resultTemporary.Permanent);
            Assert.Equal("SampleAction", resultTemporary.ActionName);
            Assert.Equal("SampleController", resultTemporary.ControllerName);
            Assert.Equal(TypeHelper.ObjectToDictionary(routeValues), resultTemporary.RouteValues);
        }

        [Theory]
        [MemberData("RedirectTestData")]
        public void RedirectToAction_Permanent_Returns_SameActionControllerAndRouteValues(object routeValues)
        {
            // Arrange
            var controller = new Controller();

            // Act
            var resultPermanent = controller.RedirectToActionPermanent("SampleAction", "SampleController", routeValues);

            // Assert
            Assert.True(resultPermanent.Permanent);
            Assert.Equal("SampleAction", resultPermanent.ActionName);
            Assert.Equal("SampleController", resultPermanent.ControllerName);
            Assert.Equal(TypeHelper.ObjectToDictionary(routeValues), resultPermanent.RouteValues);
        }

        [Theory]
        [MemberData("RedirectTestData")]
        public void RedirectToAction_Temporary_Returns_SameActionAndRouteValues(object routeValues)
        {
            // Arrange
            var controller = new Controller();

            // Act
            var resultTemporary = controller.RedirectToAction(actionName: null, routeValues: routeValues);

            // Assert
            Assert.False(resultTemporary.Permanent);
            Assert.Null(resultTemporary.ActionName);
            Assert.Equal(TypeHelper.ObjectToDictionary(routeValues), resultTemporary.RouteValues);
        }

        [Theory]
        [MemberData("RedirectTestData")]
        public void RedirectToAction_Permanent_Returns_SameActionAndRouteValues(object routeValues)
        {
            // Arrange
            var controller = new Controller();

            // Act
            var resultPermanent = controller.RedirectToActionPermanent(null, routeValues);

            // Assert
            Assert.True(resultPermanent.Permanent);
            Assert.Null(resultPermanent.ActionName);
            Assert.Equal(TypeHelper.ObjectToDictionary(routeValues), resultPermanent.RouteValues);
        }

        [Theory]
        [MemberData("RedirectTestData")]
        public void RedirectToRoute_Temporary_Returns_SameRouteValues(object routeValues)
        {
            // Arrange
            var controller = new Controller();

            // Act
            var resultTemporary = controller.RedirectToRoute(routeValues);

            // Assert
            Assert.False(resultTemporary.Permanent);
            Assert.Equal(TypeHelper.ObjectToDictionary(routeValues), resultTemporary.RouteValues);
        }

        [Theory]
        [MemberData("RedirectTestData")]
        public void RedirectToRoute_Permanent_Returns_SameRouteValues(object routeValues)
        {
            // Arrange
            var controller = new Controller();

            // Act
            var resultPermanent = controller.RedirectToRoutePermanent(routeValues);

            // Assert
            Assert.True(resultPermanent.Permanent);
            Assert.Equal(TypeHelper.ObjectToDictionary(routeValues), resultPermanent.RouteValues);
        }

        [Fact]
        public void Controller_View_WithoutParameter_SetsResultNullViewNameAndNullViewDataModel()
        {
            // Arrange
            var controller = new Controller()
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider()),
            };

            // Act
            var actualViewResult = controller.View();

            // Assert
            Assert.IsType<ViewResult>(actualViewResult);
            Assert.Null(actualViewResult.ViewName);
            Assert.Same(controller.ViewData, actualViewResult.ViewData);
            Assert.Null(actualViewResult.ViewData.Model);
        }

        [Fact]
        public void Controller_View_WithParameterViewName_SetsResultViewNameAndNullViewDataModel()
        {
            // Arrange
            var controller = new Controller()
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider()),
            };

            // Act
            var actualViewResult = controller.View("CustomViewName");

            // Assert
            Assert.IsType<ViewResult>(actualViewResult);
            Assert.Equal("CustomViewName", actualViewResult.ViewName);
            Assert.Same(controller.ViewData, actualViewResult.ViewData);
            Assert.Null(actualViewResult.ViewData.Model);
        }

        [Fact]
        public void Controller_View_WithParameterViewModel_SetsResultNullViewNameAndViewDataModel()
        {
            // Arrange
            var controller = new Controller()
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider()),
            };
            var model = new object();

            // Act
            var actualViewResult = controller.View(model);

            // Assert
            Assert.IsType<ViewResult>(actualViewResult);
            Assert.Null(actualViewResult.ViewName);
            Assert.Same(controller.ViewData, actualViewResult.ViewData);
            Assert.Same(model, actualViewResult.ViewData.Model);
        }

        [Fact]
        public void Controller_View_WithParameterViewNameAndViewModel_SetsResultViewNameAndViewDataModel()
        {
            // Arrange
            var controller = new Controller()
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider()),
            };
            var model = new object();

            // Act
            var actualViewResult = controller.View("CustomViewName", model);

            // Assert
            Assert.IsType<ViewResult>(actualViewResult);
            Assert.Equal("CustomViewName", actualViewResult.ViewName);
            Assert.Same(controller.ViewData, actualViewResult.ViewData);
            Assert.Same(model, actualViewResult.ViewData.Model);
        }

        [Fact]
        public void Controller_Content_WithParameterContentString_SetsResultContent()
        {
            // Arrange
            var controller = new Controller();

            // Act
            var actualContentResult = controller.Content("TestContent");

            // Assert
            Assert.IsType<ContentResult>(actualContentResult);
            Assert.Equal("TestContent", actualContentResult.Content);
            Assert.Null(actualContentResult.ContentEncoding);
            Assert.Null(actualContentResult.ContentType);
        }

        [Fact]
        public void Controller_Content_WithParameterContentStringAndContentType_SetsResultContentAndContentType()
        {
            // Arrange
            var controller = new Controller();

            // Act
            var actualContentResult = controller.Content("TestContent", "text/plain");

            // Assert
            Assert.IsType<ContentResult>(actualContentResult);
            Assert.Equal("TestContent", actualContentResult.Content);
            Assert.Null(actualContentResult.ContentEncoding);
            Assert.Equal("text/plain", actualContentResult.ContentType);
        }

        [Fact]
        public void Controller_Content_WithParameterContentAndTypeAndEncoding_SetsResultContentAndTypeAndEncoding()
        {
            // Arrange
            var controller = new Controller();

            // Act
            var actualContentResult = controller.Content("TestContent", "text/plain", Encoding.UTF8);

            // Assert
            Assert.IsType<ContentResult>(actualContentResult);
            Assert.Equal("TestContent", actualContentResult.Content);
            Assert.Same(Encoding.UTF8, actualContentResult.ContentEncoding);
            Assert.Equal("text/plain", actualContentResult.ContentType);
        }

        [Fact]
        public void Controller_Json_WithParameterValue_SetsResultReturnValue()
        {
            // Arrange
            var controller = new Controller();
            var value = new object();

            // Act
            var actualJsonResult = controller.Json(value);

            // Assert
            Assert.IsType<JsonResult>(actualJsonResult);
            Assert.Same(value, actualJsonResult.ReturnValue);
        }

        public static IEnumerable<object[]> RedirectTestData
        {
            get
            {
                yield return new object[] { null };
                yield return
                    new object[] {
                        new Dictionary<string, string>() { { "hello", "world" } }
                    };
                yield return
                    new object[] {
                        new RouteValueDictionary(new Dictionary<string, string>() { 
                                                        { "test", "case" }, { "sample", "route" } })
                    };
            }
        }
    }
}
