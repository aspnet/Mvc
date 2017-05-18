// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations.Internal;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.TestCommon;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Testing;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Core.Test
{
    public class ControllerBaseTest
    {
        public static IEnumerable<object[]> PublicNormalMethodsFromControllerBase
        {
            get
            {
                return typeof(ControllerBase).GetTypeInfo()
                    .DeclaredMethods
                    .Where(method => method.IsPublic &&
                    !method.IsSpecialName &&
                    !method.Name.Equals("Dispose", StringComparison.OrdinalIgnoreCase))
                    .Select(method => new[] { method });
            }
        }

        [Fact]
        public void Redirect_WithParameterUrl_SetsRedirectResultSameUrl()
        {
            // Arrange
            var controller = new TestableController();
            var url = "/test/url";

            // Act
            var result = controller.Redirect(url);

            // Assert
            Assert.IsType<RedirectResult>(result);
            Assert.False(result.PreserveMethod);
            Assert.False(result.Permanent);
            Assert.Same(url, result.Url);
        }

        [Fact]
        public void RedirectPermanent_WithParameterUrl_SetsRedirectResultPermanentAndSameUrl()
        {
            // Arrange
            var controller = new TestableController();
            var url = "/test/url";

            // Act
            var result = controller.RedirectPermanent(url);

            // Assert
            Assert.IsType<RedirectResult>(result);
            Assert.False(result.PreserveMethod);
            Assert.True(result.Permanent);
            Assert.Same(url, result.Url);
        }

        [Fact]
        public void RedirectPermanent_WithParameterUrl_SetsRedirectResultPreserveMethodAndSameUrl()
        {
            // Arrange
            var controller = new TestableController();
            var url = "/test/url";

            // Act
            var result = controller.RedirectPreserveMethod(url);

            // Assert
            Assert.IsType<RedirectResult>(result);
            Assert.True(result.PreserveMethod);
            Assert.False(result.Permanent);
            Assert.Same(url, result.Url);
        }

        [Fact]
        public void RedirectPermanent_WithParameterUrl_SetsRedirectResultPermanentPreserveMethodAndSameUrl()
        {
            // Arrange
            var controller = new TestableController();
            var url = "/test/url";

            // Act
            var result = controller.RedirectPermanentPreserveMethod(url);

            // Assert
            Assert.IsType<RedirectResult>(result);
            Assert.True(result.PreserveMethod);
            Assert.True(result.Permanent);
            Assert.Same(url, result.Url);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Redirect_WithParameter_NullOrEmptyUrl_Throws(string url)
        {
            // Arrange
            var controller = new TestableController();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNullOrEmpty(
                () => controller.Redirect(url: url), "url");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void RedirectPreserveMethod_WithParameter_NullOrEmptyUrl_Throws(string url)
        {
            // Arrange
            var controller = new TestableController();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNullOrEmpty(
                () => controller.RedirectPreserveMethod(url: url), "url");
        }

        [Fact]
        public void LocalRedirect_WithParameterUrl_SetsLocalRedirectResultWithSameUrl()
        {
            // Arrange
            var controller = new TestableController();
            var url = "/test/url";

            // Act
            var result = controller.LocalRedirect(url);

            // Assert
            Assert.IsType<LocalRedirectResult>(result);
            Assert.False(result.PreserveMethod);
            Assert.False(result.Permanent);
            Assert.Same(url, result.Url);
        }

        [Fact]
        public void LocalRedirectPermanent_WithParameterUrl_SetsLocalRedirectResultPermanentWithSameUrl()
        {
            // Arrange
            var controller = new TestableController();
            var url = "/test/url";

            // Act
            var result = controller.LocalRedirectPermanent(url);

            // Assert
            Assert.IsType<LocalRedirectResult>(result);
            Assert.False(result.PreserveMethod);
            Assert.True(result.Permanent);
            Assert.Same(url, result.Url);
        }

        [Fact]
        public void LocalRedirectPermanent_WithParameterUrl_SetsLocalRedirectResultPreserveMethodWithSameUrl()
        {
            // Arrange
            var controller = new TestableController();
            var url = "/test/url";

            // Act
            var result = controller.LocalRedirectPreserveMethod(url);

            // Assert
            Assert.IsType<LocalRedirectResult>(result);
            Assert.True(result.PreserveMethod);
            Assert.False(result.Permanent);
            Assert.Same(url, result.Url);
        }

        [Fact]
        public void LocalRedirectPermanent_WithParameterUrl_SetsLocalRedirectResultPermanentPreservesMethodWithSameUrl()
        {
            // Arrange
            var controller = new TestableController();
            var url = "/test/url";

            // Act
            var result = controller.LocalRedirectPermanentPreserveMethod(url);

            // Assert
            Assert.IsType<LocalRedirectResult>(result);
            Assert.True(result.PreserveMethod);
            Assert.True(result.Permanent);
            Assert.Same(url, result.Url);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void LocalRedirect_WithParameter_NullOrEmptyUrl_Throws(string url)
        {
            // Arrange
            var controller = new TestableController();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNullOrEmpty(
                () => controller.LocalRedirect(localUrl: url), "localUrl");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void LocalRedirectPreserveMethod_WithParameter_NullOrEmptyUrl_Throws(string url)
        {
            // Arrange
            var controller = new TestableController();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNullOrEmpty(
                () => controller.LocalRedirectPreserveMethod(localUrl: url), "localUrl");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void LocalRedirectPermanentPreserveMethod_WithParameter_NullOrEmptyUrl_Throws(string url)
        {
            // Arrange
            var controller = new TestableController();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNullOrEmpty(
                () => controller.LocalRedirectPermanentPreserveMethod(localUrl: url), "localUrl");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void RedirectPermanent_WithParameter_NullOrEmptyUrl_Throws(string url)
        {
            // Arrange
            var controller = new TestableController();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNullOrEmpty(
                () => controller.RedirectPermanent(url: url), "url");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void RedirectPermanentPreserveMethod_WithParameter_NullOrEmptyUrl_Throws(string url)
        {
            // Arrange
            var controller = new TestableController();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNullOrEmpty(
                () => controller.RedirectPermanentPreserveMethod(url: url), "url");
        }

        [Fact]
        public void RedirectToAction_WithParameterActionName_SetsResultActionName()
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var resultTemporary = controller.RedirectToAction("SampleAction");

            // Assert
            Assert.IsType<RedirectToActionResult>(resultTemporary);
            Assert.False(resultTemporary.PreserveMethod);
            Assert.False(resultTemporary.Permanent);
            Assert.Equal("SampleAction", resultTemporary.ActionName);
        }

        [Fact]
        public void RedirectToActionPreserveMethod_WithParameterActionName_SetsResultActionName()
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var resultTemporary = controller.RedirectToActionPreserveMethod(actionName: "SampleAction");

            // Assert
            Assert.IsType<RedirectToActionResult>(resultTemporary);
            Assert.True(resultTemporary.PreserveMethod);
            Assert.False(resultTemporary.Permanent);
            Assert.Equal("SampleAction", resultTemporary.ActionName);
        }

        [Fact]
        public void RedirectToActionPermanent_WithParameterActionName_SetsResultActionNameAndPermanent()
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var resultPermanent = controller.RedirectToActionPermanent("SampleAction");

            // Assert
            Assert.IsType<RedirectToActionResult>(resultPermanent);
            Assert.False(resultPermanent.PreserveMethod);
            Assert.True(resultPermanent.Permanent);
            Assert.Equal("SampleAction", resultPermanent.ActionName);
        }

        [Fact]
        public void RedirectToActionPermanentPreserveMethod_WithParameterActionName_SetsResultActionNameAndPermanent()
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var resultPermanent = controller.RedirectToActionPermanentPreserveMethod(actionName: "SampleAction");

            // Assert
            Assert.IsType<RedirectToActionResult>(resultPermanent);
            Assert.True(resultPermanent.PreserveMethod);
            Assert.True(resultPermanent.Permanent);
            Assert.Equal("SampleAction", resultPermanent.ActionName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("SampleController")]
        public void RedirectToAction_WithParameterActionAndControllerName_SetsEqualNames(string controllerName)
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var resultTemporary = controller.RedirectToAction("SampleAction", controllerName);

            // Assert
            Assert.IsType<RedirectToActionResult>(resultTemporary);
            Assert.False(resultTemporary.PreserveMethod);
            Assert.False(resultTemporary.Permanent);
            Assert.Equal("SampleAction", resultTemporary.ActionName);
            Assert.Equal(controllerName, resultTemporary.ControllerName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("SampleController")]
        public void RedirectToActionPreserveMethod_WithParameterActionAndControllerName_SetsEqualNames(string controllerName)
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var resultTemporary = controller.RedirectToActionPreserveMethod(actionName: "SampleAction", controllerName: controllerName);

            // Assert
            Assert.IsType<RedirectToActionResult>(resultTemporary);
            Assert.True(resultTemporary.PreserveMethod);
            Assert.False(resultTemporary.Permanent);
            Assert.Equal("SampleAction", resultTemporary.ActionName);
            Assert.Equal(controllerName, resultTemporary.ControllerName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("SampleController")]
        public void RedirectToActionPermanent_WithParameterActionAndControllerName_SetsEqualNames(string controllerName)
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var resultPermanent = controller.RedirectToActionPermanent("SampleAction", controllerName);

            // Assert
            Assert.IsType<RedirectToActionResult>(resultPermanent);
            Assert.False(resultPermanent.PreserveMethod);
            Assert.True(resultPermanent.Permanent);
            Assert.Equal("SampleAction", resultPermanent.ActionName);
            Assert.Equal(controllerName, resultPermanent.ControllerName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("SampleController")]
        public void RedirectToActionPermanentPreserveMethod_WithParameterActionAndControllerName_SetsEqualNames(string controllerName)
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var resultPermanent = controller.RedirectToActionPermanentPreserveMethod(actionName: "SampleAction", controllerName: controllerName);

            // Assert
            Assert.IsType<RedirectToActionResult>(resultPermanent);
            Assert.True(resultPermanent.PreserveMethod);
            Assert.True(resultPermanent.Permanent);
            Assert.Equal("SampleAction", resultPermanent.ActionName);
            Assert.Equal(controllerName, resultPermanent.ControllerName);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToAction_WithParameterActionControllerRouteValues_SetsResultProperties(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expected)
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var resultTemporary = controller.RedirectToAction("SampleAction", "SampleController", routeValues);

            // Assert
            Assert.IsType<RedirectToActionResult>(resultTemporary);
            Assert.False(resultTemporary.PreserveMethod);
            Assert.False(resultTemporary.Permanent);
            Assert.Equal("SampleAction", resultTemporary.ActionName);
            Assert.Equal("SampleController", resultTemporary.ControllerName);
            Assert.Equal(expected, resultTemporary.RouteValues);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToActionPreserveMethod_WithParameterActionControllerRouteValues_SetsResultProperties(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expected)
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var resultTemporary = controller.RedirectToActionPreserveMethod(
                actionName: "SampleAction",
                controllerName: "SampleController",
                routeValues: routeValues);

            // Assert
            Assert.IsType<RedirectToActionResult>(resultTemporary);
            Assert.True(resultTemporary.PreserveMethod);
            Assert.False(resultTemporary.Permanent);
            Assert.Equal("SampleAction", resultTemporary.ActionName);
            Assert.Equal("SampleController", resultTemporary.ControllerName);
            Assert.Equal(expected, resultTemporary.RouteValues);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToActionPermanent_WithParameterActionControllerRouteValues_SetsResultProperties(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expected)
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var resultPermanent = controller.RedirectToActionPermanent(
                "SampleAction",
                "SampleController",
                routeValues);

            // Assert
            Assert.IsType<RedirectToActionResult>(resultPermanent);
            Assert.False(resultPermanent.PreserveMethod);
            Assert.True(resultPermanent.Permanent);
            Assert.Equal("SampleAction", resultPermanent.ActionName);
            Assert.Equal("SampleController", resultPermanent.ControllerName);
            Assert.Equal(expected, resultPermanent.RouteValues);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToActionPermanentPreserveMethod_WithParameterActionControllerRouteValues_SetsResultProperties(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expected)
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var resultPermanent = controller.RedirectToActionPermanentPreserveMethod(
                actionName: "SampleAction",
                controllerName: "SampleController",
                routeValues: routeValues);

            // Assert
            Assert.IsType<RedirectToActionResult>(resultPermanent);
            Assert.True(resultPermanent.PreserveMethod);
            Assert.True(resultPermanent.Permanent);
            Assert.Equal("SampleAction", resultPermanent.ActionName);
            Assert.Equal("SampleController", resultPermanent.ControllerName);
            Assert.Equal(expected, resultPermanent.RouteValues);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToAction_WithParameterActionAndRouteValues_SetsResultProperties(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expected)
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var resultTemporary = controller.RedirectToAction(actionName: null, routeValues: routeValues);

            // Assert
            Assert.IsType<RedirectToActionResult>(resultTemporary);
            Assert.False(resultTemporary.PreserveMethod);
            Assert.False(resultTemporary.Permanent);
            Assert.Null(resultTemporary.ActionName);
            Assert.Equal(expected, resultTemporary.RouteValues);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToActionPreserveMethod_WithParameterActionAndRouteValues_SetsResultProperties(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expected)
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var resultTemporary = controller.RedirectToActionPreserveMethod(actionName: null, routeValues: routeValues);

            // Assert
            Assert.IsType<RedirectToActionResult>(resultTemporary);
            Assert.True(resultTemporary.PreserveMethod);
            Assert.False(resultTemporary.Permanent);
            Assert.Null(resultTemporary.ActionName);
            Assert.Equal(expected, resultTemporary.RouteValues);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToAction_WithParameterActionAndControllerAndRouteValuesAndFragment_SetsResultProperties(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expectedRouteValues)
        {
            // Arrange
            var controller = new TestableController();
            var expectedAction = "Action";
            var expectedController = "Home";
            var expectedFragment = "test";

            // Act
            var result = controller.RedirectToAction("Action", "Home", routeValues, "test");

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.False(result.PreserveMethod);
            Assert.False(result.Permanent);
            Assert.Equal(expectedAction, result.ActionName);
            Assert.Equal(expectedRouteValues, result.RouteValues);
            Assert.Equal(expectedController, result.ControllerName);
            Assert.Equal(expectedFragment, result.Fragment);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToActionPreserveMethod_WithParameterActionAndControllerAndRouteValuesAndFragment_SetsResultProperties(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expectedRouteValues)
        {
            // Arrange
            var controller = new TestableController();
            var expectedAction = "Action";
            var expectedController = "Home";
            var expectedFragment = "test";

            // Act
            var result = controller.RedirectToActionPreserveMethod("Action", "Home", routeValues, "test");

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.True(result.PreserveMethod);
            Assert.False(result.Permanent);
            Assert.Equal(expectedAction, result.ActionName);
            Assert.Equal(expectedRouteValues, result.RouteValues);
            Assert.Equal(expectedController, result.ControllerName);
            Assert.Equal(expectedFragment, result.Fragment);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToActionPermanent_WithParameterActionAndRouteValues_SetsResultProperties(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expected)
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var resultPermanent = controller.RedirectToActionPermanent(null, routeValues);

            // Assert
            Assert.IsType<RedirectToActionResult>(resultPermanent);
            Assert.False(resultPermanent.PreserveMethod);
            Assert.True(resultPermanent.Permanent);
            Assert.Null(resultPermanent.ActionName);
            Assert.Equal(expected, resultPermanent.RouteValues);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToActionPermanentPreserveMethod_WithParameterActionAndRouteValues_SetsResultProperties(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expected)
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var resultPermanent = controller.RedirectToActionPermanentPreserveMethod(actionName: null, routeValues: routeValues);

            // Assert
            Assert.IsType<RedirectToActionResult>(resultPermanent);
            Assert.True(resultPermanent.PreserveMethod);
            Assert.True(resultPermanent.Permanent);
            Assert.Null(resultPermanent.ActionName);
            Assert.Equal(expected, resultPermanent.RouteValues);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToActionPermanent_WithParameterActionAndControllerAndRouteValuesAndFragment_SetsResultProperties(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expectedRouteValues)
        {
            // Arrange
            var controller = new TestableController();
            var expectedAction = "Action";
            var expectedController = "Home";
            var expectedFragment = "test";

            // Act
            var result = controller.RedirectToActionPermanent("Action", "Home", routeValues, fragment: "test");

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.False(result.PreserveMethod);
            Assert.True(result.Permanent);
            Assert.Equal(expectedAction, result.ActionName);
            Assert.Equal(expectedRouteValues, result.RouteValues);
            Assert.Equal(expectedController, result.ControllerName);
            Assert.Equal(expectedFragment, result.Fragment);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToActionPermanentPreserveMethod_WithParameterActionAndControllerAndRouteValuesAndFragment_SetsResultProperties(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expectedRouteValues)
        {
            // Arrange
            var controller = new TestableController();
            var expectedAction = "Action";
            var expectedController = "Home";
            var expectedFragment = "test";

            // Act
            var result = controller.RedirectToActionPermanentPreserveMethod(
                actionName: "Action",
                controllerName: "Home",
                routeValues: routeValues,
                fragment: "test");

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.True(result.PreserveMethod);
            Assert.True(result.Permanent);
            Assert.Equal(expectedAction, result.ActionName);
            Assert.Equal(expectedRouteValues, result.RouteValues);
            Assert.Equal(expectedController, result.ControllerName);
            Assert.Equal(expectedFragment, result.Fragment);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToRoute_WithParameterRouteValues_SetsResultEqualRouteValues(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expected)
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var resultTemporary = controller.RedirectToRoute(routeValues);

            // Assert
            Assert.IsType<RedirectToRouteResult>(resultTemporary);
            Assert.False(resultTemporary.PreserveMethod);
            Assert.False(resultTemporary.Permanent);
            Assert.Equal(expected, resultTemporary.RouteValues);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToRoutePreserveMethod_WithParameterRouteValues_SetsResultEqualRouteValues(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expected)
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var resultTemporary = controller.RedirectToRoutePreserveMethod(routeValues: routeValues);

            // Assert
            Assert.IsType<RedirectToRouteResult>(resultTemporary);
            Assert.True(resultTemporary.PreserveMethod);
            Assert.False(resultTemporary.Permanent);
            Assert.Equal(expected, resultTemporary.RouteValues);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToRoute_WithParameterRouteNameAndRouteValuesAndFragment_SetsResultProperties(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expectedRouteValues)
        {
            // Arrange
            var controller = new TestableController();
            var expectedRoute = "TestRoute";
            var expectedFragment = "test";

            // Act
            var result = controller.RedirectToRoute("TestRoute", routeValues, "test");

            // Assert
            Assert.IsType<RedirectToRouteResult>(result);
            Assert.False(result.PreserveMethod);
            Assert.False(result.Permanent);
            Assert.Equal(expectedRoute, result.RouteName);
            Assert.Equal(expectedRouteValues, result.RouteValues);
            Assert.Equal(expectedFragment, result.Fragment);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToRoutePreserveMethod_WithParameterRouteNameAndRouteValuesAndFragment_SetsResultProperties(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expectedRouteValues)
        {
            // Arrange
            var controller = new TestableController();
            var expectedRoute = "TestRoute";
            var expectedFragment = "test";

            // Act
            var result = controller.RedirectToRoutePreserveMethod(routeName: "TestRoute", routeValues: routeValues, fragment: "test");

            // Assert
            Assert.IsType<RedirectToRouteResult>(result);
            Assert.True(result.PreserveMethod);
            Assert.False(result.Permanent);
            Assert.Equal(expectedRoute, result.RouteName);
            Assert.Equal(expectedRouteValues, result.RouteValues);
            Assert.Equal(expectedFragment, result.Fragment);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToRoutePermanent_WithParameterRouteValues_SetsResultEqualRouteValuesAndPermanent(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expected)
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var resultPermanent = controller.RedirectToRoutePermanent(routeValues);

            // Assert
            Assert.IsType<RedirectToRouteResult>(resultPermanent);
            Assert.False(resultPermanent.PreserveMethod);
            Assert.True(resultPermanent.Permanent);
            Assert.Equal(expected, resultPermanent.RouteValues);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToRoutePermanentPreserveMethod_WithParameterRouteValues_SetsResultEqualRouteValuesAndPermanent(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expected)
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var resultPermanent = controller.RedirectToRoutePermanentPreserveMethod(routeValues: routeValues);

            // Assert
            Assert.IsType<RedirectToRouteResult>(resultPermanent);
            Assert.True(resultPermanent.PreserveMethod);
            Assert.True(resultPermanent.Permanent);
            Assert.Equal(expected, resultPermanent.RouteValues);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToRoutePermanent_WithParameterRouteNameAndRouteValuesAndFragment_SetsResultProperties(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expectedRouteValues)
        {
            // Arrange
            var controller = new TestableController();
            var expectedRoute = "TestRoute";
            var expectedFragment = "test";

            // Act
            var result = controller.RedirectToRoutePermanent("TestRoute", routeValues, "test");

            // Assert
            Assert.IsType<RedirectToRouteResult>(result);
            Assert.False(result.PreserveMethod);
            Assert.True(result.Permanent);
            Assert.Equal(expectedRoute, result.RouteName);
            Assert.Equal(expectedRouteValues, result.RouteValues);
            Assert.Equal(expectedFragment, result.Fragment);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToRoutePermanentPreserveMethod_WithParameterRouteNameAndRouteValuesAndFragment_SetsResultProperties(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expectedRouteValues)
        {
            // Arrange
            var controller = new TestableController();
            var expectedRoute = "TestRoute";
            var expectedFragment = "test";

            // Act
            var result = controller.RedirectToRoutePermanentPreserveMethod(routeName: "TestRoute", routeValues: routeValues, fragment: "test");

            // Assert
            Assert.IsType<RedirectToRouteResult>(result);
            Assert.True(result.PreserveMethod);
            Assert.True(result.Permanent);
            Assert.Equal(expectedRoute, result.RouteName);
            Assert.Equal(expectedRouteValues, result.RouteValues);
            Assert.Equal(expectedFragment, result.Fragment);
        }

        [Fact]
        public void RedirectToRoute_WithParameterRouteName_SetsResultSameRouteName()
        {
            // Arrange
            var controller = new TestableController();
            var routeName = "CustomRouteName";

            // Act
            var resultTemporary = controller.RedirectToRoute(routeName);

            // Assert
            Assert.IsType<RedirectToRouteResult>(resultTemporary);
            Assert.False(resultTemporary.PreserveMethod);
            Assert.False(resultTemporary.Permanent);
            Assert.Same(routeName, resultTemporary.RouteName);
        }

        [Fact]
        public void RedirectToRoutePreserveMethod_WithParameterRouteName_SetsResultSameRouteName()
        {
            // Arrange
            var controller = new TestableController();
            var routeName = "CustomRouteName";

            // Act;
            var resultTemporary = controller.RedirectToRoutePreserveMethod(routeName: routeName);

            // Assert
            Assert.IsType<RedirectToRouteResult>(resultTemporary);
            Assert.True(resultTemporary.PreserveMethod);
            Assert.False(resultTemporary.Permanent);
            Assert.Same(routeName, resultTemporary.RouteName);
        }

        [Fact]
        public void RedirectToRoutePermanent_WithParameterRouteName_SetsResultSameRouteNameAndPermanent()
        {
            // Arrange
            var controller = new TestableController();
            var routeName = "CustomRouteName";

            // Act
            var resultPermanent = controller.RedirectToRoutePermanent(routeName);

            // Assert
            Assert.IsType<RedirectToRouteResult>(resultPermanent);
            Assert.False(resultPermanent.PreserveMethod);
            Assert.True(resultPermanent.Permanent);
            Assert.Same(routeName, resultPermanent.RouteName);
        }

        [Fact]
        public void RedirectToRoutePermanentPreserveMethod_WithParameterRouteName_SetsResultSameRouteNameAndPermanent()
        {
            // Arrange
            var controller = new TestableController();
            var routeName = "CustomRouteName";

            // Act
            var resultPermanent = controller.RedirectToRoutePermanentPreserveMethod(routeName: routeName);

            // Assert
            Assert.IsType<RedirectToRouteResult>(resultPermanent);
            Assert.True(resultPermanent.PreserveMethod);
            Assert.True(resultPermanent.Permanent);
            Assert.Same(routeName, resultPermanent.RouteName);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToRoute_WithParameterRouteNameAndRouteValues_SetsResultSameRouteNameAndRouteValues(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expected)
        {
            // Arrange
            var controller = new TestableController();
            var routeName = "CustomRouteName";

            // Act
            var resultTemporary = controller.RedirectToRoute(routeName, routeValues);

            // Assert
            Assert.IsType<RedirectToRouteResult>(resultTemporary);
            Assert.False(resultTemporary.PreserveMethod);
            Assert.False(resultTemporary.Permanent);
            Assert.Same(routeName, resultTemporary.RouteName);
            Assert.Equal(expected, resultTemporary.RouteValues);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToRoutePreserveMethod_WithParameterRouteNameAndRouteValues_SetsResultSameRouteNameAndRouteValues(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expected)
        {
            // Arrange
            var controller = new TestableController();
            var routeName = "CustomRouteName";

            // Act
            var resultTemporary = controller.RedirectToRoutePreserveMethod(routeName: routeName, routeValues: routeValues);

            // Assert
            Assert.IsType<RedirectToRouteResult>(resultTemporary);
            Assert.True(resultTemporary.PreserveMethod);
            Assert.False(resultTemporary.Permanent);
            Assert.Same(routeName, resultTemporary.RouteName);
            Assert.Equal(expected, resultTemporary.RouteValues);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToRoutePermanent_WithParameterRouteNameAndRouteValues_SetsResultProperties(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expected)
        {
            // Arrange
            var controller = new TestableController();
            var routeName = "CustomRouteName";

            // Act
            var resultPermanent = controller.RedirectToRoutePermanent(routeName, routeValues);

            // Assert
            Assert.IsType<RedirectToRouteResult>(resultPermanent);
            Assert.False(resultPermanent.PreserveMethod);
            Assert.True(resultPermanent.Permanent);
            Assert.Same(routeName, resultPermanent.RouteName);
            Assert.Equal(expected, resultPermanent.RouteValues);
        }

        [Fact]
        public void RedirectToPagePreserveMethod_WithParameterUrl_SetsRedirectResultPreserveMethod()
        {
            // Arrange
            var pageModel = new TestableController();
            var url = "/test/url";

            // Act
            var result = pageModel.RedirectToPagePreserveMethod(url);

            // Assert
            Assert.IsType<RedirectToPageResult>(result);
            Assert.True(result.PreserveMethod);
            Assert.False(result.Permanent);
            Assert.Same(url, result.PageName);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToPagePreserveMethod_SetsResultProperties(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expected)
        {
            // Arrange
            var pageModel = new TestableController();
            var pageName = "CustomRouteName";

            // Act
            var resultPermanent = pageModel.RedirectToPagePreserveMethod(pageName, routeValues: routeValues);

            // Assert
            Assert.IsType<RedirectToPageResult>(resultPermanent);
            Assert.True(resultPermanent.PreserveMethod);
            Assert.False(resultPermanent.Permanent);
            Assert.Same(pageName, resultPermanent.PageName);
            Assert.Equal(expected, resultPermanent.RouteValues);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToPagePermanentPreserveMethod_SetsResultProperties(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expected)
        {
            // Arrange
            var pageModel = new TestableController();
            var routeName = "CustomRouteName";

            // Act
            var resultPermanent = pageModel.RedirectToPagePermanentPreserveMethod(routeName, routeValues: routeValues);

            // Assert
            Assert.IsType<RedirectToPageResult>(resultPermanent);
            Assert.True(resultPermanent.PreserveMethod);
            Assert.True(resultPermanent.Permanent);
            Assert.Same(routeName, resultPermanent.PageName);
            Assert.Equal(expected, resultPermanent.RouteValues);
        }

        [Theory]
        [MemberData(nameof(RedirectTestData))]
        public void RedirectToRoutePermanentPreserveMethod_WithParameterRouteNameAndRouteValues_SetsResultProperties(
            object routeValues,
            IEnumerable<KeyValuePair<string, object>> expected)
        {
            // Arrange
            var controller = new TestableController();
            var routeName = "CustomRouteName";

            // Act
            var resultPermanent = controller.RedirectToRoutePermanentPreserveMethod(routeName: routeName, routeValues: routeValues);

            // Assert
            Assert.IsType<RedirectToRouteResult>(resultPermanent);
            Assert.True(resultPermanent.PreserveMethod);
            Assert.True(resultPermanent.Permanent);
            Assert.Same(routeName, resultPermanent.RouteName);
            Assert.Equal(expected, resultPermanent.RouteValues);
        }

        [Fact]
        public void Created_WithStringParameter_SetsCreatedLocation()
        {
            // Arrange
            var controller = new TestableController();
            var uri = "http://test/url";

            // Act
            var result = controller.Created(uri, null);

            // Assert
            Assert.IsType<CreatedResult>(result);
            Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
            Assert.Same(uri, result.Location);
        }

        [Fact]
        public void Created_WithAbsoluteUriParameter_SetsCreatedLocation()
        {
            // Arrange
            var controller = new TestableController();
            var uri = new Uri("http://test/url");

            // Act
            var result = controller.Created(uri, null);

            // Assert
            Assert.IsType<CreatedResult>(result);
            Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
            Assert.Equal(uri.OriginalString, result.Location);
        }

        [Fact]
        public void Created_WithRelativeUriParameter_SetsCreatedLocation()
        {
            // Arrange
            var controller = new TestableController();
            var uri = new Uri("/test/url", UriKind.Relative);

            // Act
            var result = controller.Created(uri, null);

            // Assert
            Assert.IsType<CreatedResult>(result);
            Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
            Assert.Equal(uri.OriginalString, result.Location);
        }

        [Fact]
        public void CreatedAtAction_WithParameterActionName_SetsResultActionName()
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var result = controller.CreatedAtAction("SampleAction", null);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
            Assert.Equal("SampleAction", result.ActionName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("SampleController")]
        public void CreatedAtAction_WithActionControllerAndNullRouteValue_SetsSameValue(
            string controllerName)
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var result = controller.CreatedAtAction("SampleAction", controllerName, null, null);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
            Assert.Equal("SampleAction", result.ActionName);
            Assert.Equal(controllerName, result.ControllerName);
        }

        [Fact]
        public void CreatedAtAction_WithActionControllerRouteValues_SetsSameValues()
        {
            // Arrange
            var controller = new TestableController();
            var expected = new Dictionary<string, object>
                {
                    { "test", "case" },
                    { "sample", "route" },
                };

            // Act
            var result = controller.CreatedAtAction(
                "SampleAction",
                "SampleController",
                new RouteValueDictionary(expected), null);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
            Assert.Equal("SampleAction", result.ActionName);
            Assert.Equal("SampleController", result.ControllerName);
            Assert.Equal(expected, result.RouteValues);
        }

        [Fact]
        public void CreatedAtRoute_WithParameterRouteName_SetsResultSameRouteName()
        {
            // Arrange
            var controller = new TestableController();
            var routeName = "SampleRoute";

            // Act
            var result = controller.CreatedAtRoute(routeName, null);

            // Assert
            Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Same(routeName, result.RouteName);
        }

        [Fact]
        public void CreatedAtRoute_WithParameterRouteValues_SetsResultSameRouteValues()
        {
            // Arrange
            var controller = new TestableController();
            var expected = new Dictionary<string, object>
                {
                    { "test", "case" },
                    { "sample", "route" },
                };

            // Act
            var result = controller.CreatedAtRoute(new RouteValueDictionary(expected), null);

            // Assert
            Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
            Assert.Equal(expected, result.RouteValues);
        }

        [Fact]
        public void CreatedAtRoute_WithParameterRouteNameAndValues_SetsResultSameProperties()
        {
            // Arrange
            var controller = new TestableController();
            var routeName = "SampleRoute";
            var expected = new Dictionary<string, object>
                {
                    { "test", "case" },
                    { "sample", "route" },
                };

            // Act
            var result = controller.CreatedAtRoute(routeName, new RouteValueDictionary(expected), null);

            // Assert
            Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
            Assert.Same(routeName, result.RouteName);
            Assert.Equal(expected, result.RouteValues);
        }

        [Fact]
        public void Accepted_SetsStatusCode()
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var result = controller.Accepted();

            // Assert
            Assert.IsType<AcceptedResult>(result);
            Assert.Equal(StatusCodes.Status202Accepted, result.StatusCode);
        }

        [Fact]
        public void Accepted_SetsValue()
        {
            // Arrange
            var controller = new TestableController();
            var value = new object();

            // Act
            var result = controller.Accepted(value);

            // Assert
            Assert.IsType<AcceptedResult>(result);
            Assert.Equal(StatusCodes.Status202Accepted, result.StatusCode);
            Assert.Same(value, result.Value);
        }

        [Fact]
        public void Accepted_StringUri_SetsAcceptedLocation()
        {
            // Arrange
            var controller = new TestableController();
            var uri = "http://test/url";

            // Act
            var result = controller.Accepted(uri);

            // Assert
            Assert.IsType<AcceptedResult>(result);
            Assert.Equal(StatusCodes.Status202Accepted, result.StatusCode);
            Assert.Same(uri, result.Location);
        }

        [Fact]
        public void Accepted_AbsoluteUri_SetsAcceptedLocation()
        {
            // Arrange
            var controller = new TestableController();
            var uri = new Uri("http://test/url");

            // Act
            var result = controller.Accepted(uri);

            // Assert
            Assert.IsType<AcceptedResult>(result);
            Assert.Equal(StatusCodes.Status202Accepted, result.StatusCode);
            Assert.Equal(uri.OriginalString, result.Location);
        }

        [Fact]
        public void Accepted_RelativeUri_SetsAcceptedLocation()
        {
            // Arrange
            var controller = new TestableController();
            var uri = new Uri("/test/url", UriKind.Relative);

            // Act
            var result = controller.Accepted(uri);

            // Assert
            Assert.IsType<AcceptedResult>(result);
            Assert.Equal(StatusCodes.Status202Accepted, result.StatusCode);
            Assert.Equal(uri.OriginalString, result.Location);
        }

        [Fact]
        public void AcceptedAtAction_SetsActionName()
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var result = controller.AcceptedAtAction("SampleAction");

            // Assert
            Assert.IsType<AcceptedAtActionResult>(result);
            Assert.Equal(StatusCodes.Status202Accepted, result.StatusCode);
            Assert.Equal("SampleAction", result.ActionName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("SampleController")]
        public void AcceptedAtAction_SetsActionController(string controllerName)
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var result = controller.AcceptedAtAction("SampleAction", controllerName);

            // Assert
            Assert.IsType<AcceptedAtActionResult>(result);
            Assert.Equal(StatusCodes.Status202Accepted, result.StatusCode);
            Assert.Equal("SampleAction", result.ActionName);
            Assert.Equal(controllerName, result.ControllerName);
        }

        [Fact]
        public void AcceptedAtAction_SetsActionControllerRouteValues()
        {
            // Arrange
            var controller = new TestableController();
            var expected = new Dictionary<string, object>
            {
                { "test", "case" },
                { "sample", "route" },
            };

            // Act
            var result = controller.AcceptedAtAction(
                "SampleAction",
                "SampleController",
                new RouteValueDictionary(expected));

            // Assert
            Assert.IsType<AcceptedAtActionResult>(result);
            Assert.Equal(StatusCodes.Status202Accepted, result.StatusCode);
            Assert.Equal("SampleAction", result.ActionName);
            Assert.Equal("SampleController", result.ControllerName);
            Assert.Equal(expected, result.RouteValues);
        }

        [Fact]
        public void AcceptedAtRoute_SetsRouteValues()
        {
            // Arrange
            var controller = new TestableController();
            var expected = new Dictionary<string, object>
            {
                { "test", "case" },
                { "sample", "route" },
            };

            // Act
            var result = controller.AcceptedAtRoute(new RouteValueDictionary(expected));

            // Assert
            Assert.IsType<AcceptedAtRouteResult>(result);
            Assert.Equal(StatusCodes.Status202Accepted, result.StatusCode);
            Assert.Equal(expected, result.RouteValues);
        }

        [Fact]
        public void AcceptedAtRoute_SetsRouteNameAndValues()
        {
            // Arrange
            var controller = new TestableController();
            var routeName = "SampleRoute";
            var expected = new Dictionary<string, object>
            {
                { "test", "case" },
                { "sample", "route" },
            };

            // Act
            var result = controller.AcceptedAtRoute(routeName, new RouteValueDictionary(expected));

            // Assert
            Assert.IsType<AcceptedAtRouteResult>(result);
            Assert.Equal(StatusCodes.Status202Accepted, result.StatusCode);
            Assert.Same(routeName, result.RouteName);
            Assert.Equal(expected, result.RouteValues);
        }

        [Fact]
        public void File_WithContents()
        {
            // Arrange
            var controller = new TestableController();
            var fileContents = new byte[0];

            // Act
            var result = controller.File(fileContents, "application/pdf");

            // Assert
            Assert.NotNull(result);
            Assert.Same(fileContents, result.FileContents);
            Assert.Equal("application/pdf", result.ContentType.ToString());
            Assert.Equal(string.Empty, result.FileDownloadName);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(null, "\"Etag\"")]
        [InlineData("05/01/2008 +1:00", null)]
        [InlineData("05/01/2008 +1:00", "\"Etag\"")]
        public void File_WithContents_LastModifiedAndEtag(string lastModifiedString, string entityTagString)
        {
            // Arrange
            var controller = new TestableController();
            var fileContents = new byte[0];
            var lastModified = (lastModifiedString == null) ? (DateTimeOffset?)null : DateTimeOffset.Parse(lastModifiedString);
            var entityTag = (entityTagString == null) ? null : new EntityTagHeaderValue(entityTagString);

            // Act
            var result = controller.File(fileContents, "application/pdf", lastModified, entityTag);

            // Assert
            Assert.NotNull(result);
            Assert.Same(fileContents, result.FileContents);
            Assert.Equal("application/pdf", result.ContentType.ToString());
            Assert.Equal(string.Empty, result.FileDownloadName);
            Assert.Equal(lastModified, result.LastModified);
            Assert.Equal(entityTag, result.EntityTag);
        }

        [Fact]
        public void File_WithContentsAndFileDownloadName()
        {
            // Arrange
            var controller = new TestableController();
            var fileContents = new byte[0];

            // Act
            var result = controller.File(fileContents, "application/pdf", "someDownloadName");

            // Assert
            Assert.NotNull(result);
            Assert.Same(fileContents, result.FileContents);
            Assert.Equal("application/pdf", result.ContentType.ToString());
            Assert.Equal("someDownloadName", result.FileDownloadName);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(null, "\"Etag\"")]
        [InlineData("05/01/2008 +1:00", null)]
        [InlineData("05/01/2008 +1:00", "\"Etag\"")]
        public void File_WithContentsAndFileDownloadName_LastModifiedAndEtag(string lastModifiedString, string entityTagString)
        {
            // Arrange
            var controller = new TestableController();
            var fileContents = new byte[0];
            var lastModified = (lastModifiedString == null) ? (DateTimeOffset?)null : DateTimeOffset.Parse(lastModifiedString);
            var entityTag = (entityTagString == null) ? null : new EntityTagHeaderValue(entityTagString);

            // Act
            var result = controller.File(fileContents, "application/pdf", "someDownloadName", lastModified, entityTag);

            // Assert
            Assert.NotNull(result);
            Assert.Same(fileContents, result.FileContents);
            Assert.Equal("application/pdf", result.ContentType.ToString());
            Assert.Equal("someDownloadName", result.FileDownloadName);
            Assert.Equal(lastModified, result.LastModified);
            Assert.Equal(entityTag, result.EntityTag);
        }

        [Fact]
        public void File_WithPath()
        {
            // Arrange
            var controller = new TestableController();
            var path = Path.GetFullPath("somepath");

            // Act
            var result = controller.File(path, "application/pdf");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(path, result.FileName);
            Assert.Equal("application/pdf", result.ContentType.ToString());
            Assert.Equal(string.Empty, result.FileDownloadName);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(null, "\"Etag\"")]
        [InlineData("05/01/2008 +1:00", null)]
        [InlineData("05/01/2008 +1:00", "\"Etag\"")]
        public void File_WithPath_LastModifiedAndEtag(string lastModifiedString, string entityTagString)
        {
            // Arrange
            var controller = new TestableController();
            var path = Path.GetFullPath("somepath");
            var lastModified = (lastModifiedString == null) ? (DateTimeOffset?)null : DateTimeOffset.Parse(lastModifiedString);
            var entityTag = (entityTagString == null) ? null : new EntityTagHeaderValue(entityTagString);

            // Act
            var result = controller.File(path, "application/pdf", lastModified, entityTag);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(path, result.FileName);
            Assert.Equal("application/pdf", result.ContentType.ToString());
            Assert.Equal(string.Empty, result.FileDownloadName);
            Assert.Equal(lastModified, result.LastModified);
            Assert.Equal(entityTag, result.EntityTag);
        }

        [Fact]
        public void File_WithPathAndFileDownloadName()
        {
            // Arrange
            var controller = new TestableController();
            var path = Path.GetFullPath("somepath");

            // Act
            var result = controller.File(path, "application/pdf", "someDownloadName");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(path, result.FileName);
            Assert.Equal("application/pdf", result.ContentType.ToString());
            Assert.Equal("someDownloadName", result.FileDownloadName);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(null, "\"Etag\"")]
        [InlineData("05/01/2008 +1:00", null)]
        [InlineData("05/01/2008 +1:00", "\"Etag\"")]
        public void File_WithPathAndFileDownloadName_LastModifiedAndEtag(string lastModifiedString, string entityTagString)
        {
            // Arrange
            var controller = new TestableController();
            var path = Path.GetFullPath("somepath");
            var lastModified = (lastModifiedString == null) ? (DateTimeOffset?)null : DateTimeOffset.Parse(lastModifiedString);
            var entityTag = (entityTagString == null) ? null : new EntityTagHeaderValue(entityTagString);

            // Act
            var result = controller.File(path, "application/pdf", "someDownloadName", lastModified, entityTag);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(path, result.FileName);
            Assert.Equal("application/pdf", result.ContentType.ToString());
            Assert.Equal("someDownloadName", result.FileDownloadName);
            Assert.Equal(lastModified, result.LastModified);
            Assert.Equal(entityTag, result.EntityTag);
        }

        [Fact]
        public void File_WithStream()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(x => x.Response.RegisterForDispose(It.IsAny<IDisposable>()));

            var controller = new TestableController();
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            var fileStream = Stream.Null;

            // Act
            var result = controller.File(fileStream, "application/pdf");

            // Assert
            Assert.NotNull(result);
            Assert.Same(fileStream, result.FileStream);
            Assert.Equal("application/pdf", result.ContentType.ToString());
            Assert.Equal(string.Empty, result.FileDownloadName);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(null, "\"Etag\"")]
        [InlineData("05/01/2008 +1:00", null)]
        [InlineData("05/01/2008 +1:00", "\"Etag\"")]
        public void File_WithStream_LastModifiedAndEtag(string lastModifiedString, string entityTagString)
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(x => x.Response.RegisterForDispose(It.IsAny<IDisposable>()));

            var controller = new TestableController();
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            var fileStream = Stream.Null;
            var lastModified = (lastModifiedString == null) ? (DateTimeOffset?)null : DateTimeOffset.Parse(lastModifiedString);
            var entityTag = (entityTagString == null) ? null : new EntityTagHeaderValue(entityTagString);

            // Act
            var result = controller.File(fileStream, "application/pdf", lastModified, entityTag);

            // Assert
            Assert.NotNull(result);
            Assert.Same(fileStream, result.FileStream);
            Assert.Equal("application/pdf", result.ContentType.ToString());
            Assert.Equal(string.Empty, result.FileDownloadName);
            Assert.Equal(lastModified, result.LastModified);
            Assert.Equal(entityTag, result.EntityTag);
        }

        [Fact]
        public void File_WithStreamAndFileDownloadName()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();

            var controller = new TestableController();
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            var fileStream = Stream.Null;

            // Act
            var result = controller.File(fileStream, "application/pdf", "someDownloadName");

            // Assert
            Assert.NotNull(result);
            Assert.Same(fileStream, result.FileStream);
            Assert.Equal("application/pdf", result.ContentType.ToString());
            Assert.Equal("someDownloadName", result.FileDownloadName);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(null, "\"Etag\"")]
        [InlineData("05/01/2008 +1:00", null)]
        [InlineData("05/01/2008 +1:00", "\"Etag\"")]
        public void File_WithStreamAndFileDownloadName_LastModifiedAndEtag(string lastModifiedString, string entityTagString)
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();

            var controller = new TestableController();
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            var fileStream = Stream.Null;
            var lastModified = (lastModifiedString == null) ? (DateTimeOffset?)null : DateTimeOffset.Parse(lastModifiedString);
            var entityTag = (entityTagString == null) ? null : new EntityTagHeaderValue(entityTagString);

            // Act
            var result = controller.File(fileStream, "application/pdf", "someDownloadName", lastModified, entityTag);

            // Assert
            Assert.NotNull(result);
            Assert.Same(fileStream, result.FileStream);
            Assert.Equal("application/pdf", result.ContentType.ToString());
            Assert.Equal("someDownloadName", result.FileDownloadName);
            Assert.Equal(lastModified, result.LastModified);
            Assert.Equal(entityTag, result.EntityTag);
        }

        [Fact]
        public void HttpUnauthorized_SetsStatusCode()
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var result = controller.Unauthorized();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
            Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        }

        [Fact]
        public void HttpNotFound_SetsStatusCode()
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var result = controller.NotFound();

            // Assert
            Assert.IsType<NotFoundResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Fact]
        public void HttpNotFound_SetsStatusCodeAndResponseContent()
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var result = controller.NotFound("Test Content");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
            Assert.Equal("Test Content", result.Value);
        }

        [Fact]
        public void Ok_SetsStatusCode()
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var result = controller.Ok();

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public void BadRequest_SetsStatusCode()
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var result = controller.BadRequest();

            // Assert
            Assert.IsType<BadRequestResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [Fact]
        public void BadRequest_SetsStatusCodeAndValue_Object()
        {
            // Arrange
            var controller = new TestableController();
            var obj = new object();

            // Act
            var result = controller.BadRequest(obj);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.Equal(obj, result.Value);
        }

        [Fact]
        public void BadRequest_SetsStatusCodeAndValue_ModelState()
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var result = controller.BadRequest(new ModelStateDictionary());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
            var errors = Assert.IsType<SerializableError>(result.Value);
            Assert.Equal(0, errors.Count);
        }

        [Theory]
        [MemberData(nameof(PublicNormalMethodsFromControllerBase))]
        public void NonActionAttribute_IsOnEveryPublicNormalMethodFromControllerBase(MethodInfo method)
        {
            // Arrange & Act & Assert
            Assert.True(method.IsDefined(typeof(NonActionAttribute)));
        }

        [Fact]
        public void Controller_NoContent()
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var result = controller.NoContent();

            // Assert
            Assert.Equal(StatusCodes.Status204NoContent, result.StatusCode);
        }

        [Fact]
        public void Controller_Content_WithParameterContentString_SetsResultContent()
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var actualContentResult = controller.Content("TestContent");

            // Assert
            Assert.IsType<ContentResult>(actualContentResult);
            Assert.Equal("TestContent", actualContentResult.Content);
            Assert.Null(actualContentResult.ContentType);
        }

        [Fact]
        public void Controller_Content_WithParameterContentStringAndContentType_SetsResultContentAndContentType()
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var actualContentResult = controller.Content("TestContent", "text/plain");

            // Assert
            Assert.IsType<ContentResult>(actualContentResult);
            Assert.Equal("TestContent", actualContentResult.Content);
            Assert.Null(MediaType.GetEncoding(actualContentResult.ContentType));
            Assert.Equal("text/plain", actualContentResult.ContentType.ToString());
        }

        [Fact]
        public void Controller_Content_WithParameterContentAndTypeAndEncoding_SetsResultContentAndTypeAndEncoding()
        {
            // Arrange
            var controller = new TestableController();

            // Act
            var actualContentResult = controller.Content("TestContent", "text/plain", Encoding.UTF8);

            // Assert
            Assert.IsType<ContentResult>(actualContentResult);
            Assert.Equal("TestContent", actualContentResult.Content);
            Assert.Same(Encoding.UTF8, MediaType.GetEncoding(actualContentResult.ContentType));
            Assert.Equal("text/plain; charset=utf-8", actualContentResult.ContentType.ToString());
        }

        [Fact]
        public void Controller_Content_NoContentType_DefaultEncodingIsUsed()
        {
            // Arrange
            var contentController = new ContentController();
            var expected = MediaTypeHeaderValue.Parse("text/plain; charset=utf-8");

            // Act
            var contentResult = (ContentResult)contentController.Content_WithNoEncoding();

            // Assert
            // The default content type of ContentResult is used when the result is executed.
            Assert.Null(contentResult.ContentType);
        }

        [Fact]
        public void Controller_Content_InvalidCharset_DefaultEncodingIsUsed()
        {
            // Arrange
            var contentController = new ContentController();
            var contentType = "text/xml; charset=invalid; p1=p1-value";

            // Act
            var contentResult = (ContentResult)contentController.Content_WithInvalidCharset();

            // Assert
            Assert.NotNull(contentResult.ContentType);
            Assert.Equal(contentType, contentResult.ContentType.ToString());
            // The default encoding of ContentResult is used when this result is executed.
            Assert.Null(MediaType.GetEncoding(contentResult.ContentType));
        }

        [Fact]
        public void Controller_Content_CharsetAndEncodingProvided_EncodingIsUsed()
        {
            // Arrange
            var contentController = new ContentController();
            var contentType = "text/xml; charset=us-ascii; p1=p1-value";

            // Act
            var contentResult = (ContentResult)contentController.Content_WithEncodingInCharset_AndEncodingParameter();

            // Assert
            MediaTypeAssert.Equal(contentType, contentResult.ContentType);
        }

        [Fact]
        public void Controller_Content_CharsetInContentType_IsUsedForEncoding()
        {
            // Arrange
            var contentController = new ContentController();
            var contentType = "text/xml; charset=us-ascii; p1=p1-value";

            // Act
            var contentResult = (ContentResult)contentController.Content_WithEncodingInCharset();

            // Assert
            Assert.Equal(contentType, contentResult.ContentType);
        }

        [Fact]
        public void Controller_StatusCode_SetObject()
        {
            // Arrange
            var statusCode = 204;
            var value = new { Value = 42 };

            var statusCodeController = new StatusCodeController();

            // Act
            var result = (ObjectResult)statusCodeController.StatusCode_Object(statusCode, value);

            // Assert
            Assert.Equal(statusCode, result.StatusCode);
            Assert.Equal(value, result.Value);
        }

        [Fact]
        public void Controller_StatusCode_SetObjectNull()
        {
            // Arrange
            var statusCode = 204;
            object value = null;

            var statusCodeController = new StatusCodeController();

            // Act
            var result = statusCodeController.StatusCode_Object(statusCode, value);

            // Assert
            Assert.Equal(statusCode, result.StatusCode);
            Assert.Equal(value, result.Value);
        }

        [Fact]
        public void Controller_StatusCode_SetsStatusCode()
        {
            // Arrange
            var statusCode = 205;
            var statusCodeController = new StatusCodeController();

            // Act
            var result = statusCodeController.StatusCode_Int(statusCode);

            // Assert
            Assert.Equal(statusCode, result.StatusCode);
        }

        public static IEnumerable<object[]> RedirectTestData
        {
            get
            {
                yield return new object[]
                {
                    null,
                    null,
                };

                yield return new object[]
                {
                    new Dictionary<string, object> { { "hello", "world" } },
                    new RouteValueDictionary() { { "hello", "world" } },
                };

                var expected2 = new Dictionary<string, object>
                {
                    { "test", "case" },
                    { "sample", "route" },
                };

                yield return new object[]
                {
                    new RouteValueDictionary(expected2),
                    new RouteValueDictionary(expected2),
                };
            }
        }

        [Fact]
        public async Task TryUpdateModel_FallsBackOnEmptyPrefix_IfNotSpecified()
        {
            // Arrange
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var valueProvider = Mock.Of<IValueProvider>();
            var binder = new StubModelBinder(context =>
            {
                Assert.Empty(context.ModelName);
                Assert.Same(valueProvider, Assert.IsType<CompositeValueProvider>(context.ValueProvider)[0]);

                // Include and exclude should be null, resulting in property
                // being included.
                Assert.True(context.PropertyFilter(context.ModelMetadata.Properties["Property1"]));
                Assert.True(context.PropertyFilter(context.ModelMetadata.Properties["Property2"]));
            });

            var controller = GetController(binder, valueProvider);
            var model = new MyModel();

            // Act
            var result = await controller.TryUpdateModelAsync(model);

            // Assert
            Assert.NotEqual(0, binder.BindModelCount);
        }

        [Fact]
        public async Task TryUpdateModel_UsesModelTypeNameIfSpecified()
        {
            // Arrange
            var modelName = "mymodel";

            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var valueProvider = Mock.Of<IValueProvider>();
            var binder = new StubModelBinder(context =>
            {
                Assert.Same(valueProvider, Assert.IsType<CompositeValueProvider>(context.ValueProvider)[0]);

                // Include and exclude should be null, resulting in property
                // being included.
                Assert.True(context.PropertyFilter(context.ModelMetadata.Properties["Property1"]));
                Assert.True(context.PropertyFilter(context.ModelMetadata.Properties["Property2"]));
            });

            var controller = GetController(binder, valueProvider);
            var model = new MyModel();

            // Act
            var result = await controller.TryUpdateModelAsync(model, modelName);

            // Assert
            Assert.NotEqual(0, binder.BindModelCount);
        }

        [Fact]
        public async Task TryUpdateModel_UsesModelValueProviderIfSpecified()
        {
            // Arrange
            var modelName = "mymodel";

            var valueProvider = Mock.Of<IValueProvider>();
            var binder = new StubModelBinder(context =>
                  {
                      Assert.Same(valueProvider, context.ValueProvider);

                      // Include and exclude should be null, resulting in property
                      // being included.
                      Assert.True(context.PropertyFilter(context.ModelMetadata.Properties["Property1"]));
                      Assert.True(context.PropertyFilter(context.ModelMetadata.Properties["Property2"]));
                  });

            var controller = GetController(binder, valueProvider: null);
            var model = new MyModel();

            // Act
            var result = await controller.TryUpdateModelAsync(model, modelName, valueProvider);

            // Assert
            Assert.NotEqual(0, binder.BindModelCount);
        }

        [Fact]
        public async Task TryUpdateModel_PropertyFilterOverload_UsesPassedArguments()
        {
            // Arrange
            var modelName = "mymodel";

            Func<ModelMetadata, bool> propertyFilter = (m) =>
                string.Equals(m.PropertyName, "Include1", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(m.PropertyName, "Include2", StringComparison.OrdinalIgnoreCase);

            var valueProvider = Mock.Of<IValueProvider>();
            var binder = new StubModelBinder(context =>
            {
                Assert.Same(valueProvider, Assert.IsType<CompositeValueProvider>(context.ValueProvider)[0]);

                Assert.True(context.PropertyFilter(context.ModelMetadata.Properties["Include1"]));
                Assert.True(context.PropertyFilter(context.ModelMetadata.Properties["Include2"]));

                Assert.False(context.PropertyFilter(context.ModelMetadata.Properties["Exclude1"]));
                Assert.False(context.PropertyFilter(context.ModelMetadata.Properties["Exclude2"]));

            });

            var controller = GetController(binder, valueProvider);
            var model = new MyModel();

            // Act
            await controller.TryUpdateModelAsync(model, modelName, propertyFilter);

            // Assert
            Assert.NotEqual(0, binder.BindModelCount);
        }

        [Fact]
        public async Task TryUpdateModel_PropertyFilterWithValueProviderOverload_UsesPassedArguments()
        {
            // Arrange
            var modelName = "mymodel";

            Func<ModelMetadata, bool> propertyFilter = (m) =>
                string.Equals(m.PropertyName, "Include1", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(m.PropertyName, "Include2", StringComparison.OrdinalIgnoreCase);

            var valueProvider = Mock.Of<IValueProvider>();
            var binder = new StubModelBinder(context =>
            {
                Assert.Same(valueProvider, context.ValueProvider);

                Assert.True(context.PropertyFilter(context.ModelMetadata.Properties["Include1"]));
                Assert.True(context.PropertyFilter(context.ModelMetadata.Properties["Include2"]));

                Assert.False(context.PropertyFilter(context.ModelMetadata.Properties["Exclude1"]));
                Assert.False(context.PropertyFilter(context.ModelMetadata.Properties["Exclude2"]));
            });
            var controller = GetController(binder, valueProvider: null);

            var model = new MyModel();

            // Act
            await controller.TryUpdateModelAsync(model, modelName, valueProvider, propertyFilter);

            // Assert
            Assert.NotEqual(0, binder.BindModelCount);
        }

        [Theory]
        [InlineData("")]
        [InlineData("prefix")]
        public async Task TryUpdateModel_IncludeExpressionOverload_UsesPassedArguments(string prefix)
        {
            // Arrange
            var valueProvider = new Mock<IValueProvider>();
            valueProvider
                .Setup(v => v.ContainsPrefix(prefix))
                .Returns(true);

            var binder = new StubModelBinder(context =>
            {
                Assert.Same(
                    valueProvider.Object,
                    Assert.IsType<CompositeValueProvider>(context.ValueProvider)[0]);

                Assert.True(context.PropertyFilter(context.ModelMetadata.Properties["Property1"]));
                Assert.True(context.PropertyFilter(context.ModelMetadata.Properties["Property2"]));

                Assert.False(context.PropertyFilter(context.ModelMetadata.Properties["Exclude1"]));
                Assert.False(context.PropertyFilter(context.ModelMetadata.Properties["Exclude2"]));
            });


            var controller = GetController(binder, valueProvider.Object);
            var model = new MyModel();

            // Act
            await controller.TryUpdateModelAsync(model, prefix, m => m.Property1, m => m.Property2);

            // Assert
            Assert.NotEqual(0, binder.BindModelCount);
        }

        [Theory]
        [InlineData("")]
        [InlineData("prefix")]
        public async Task
            TryUpdateModel_IncludeExpressionWithValueProviderOverload_UsesPassedArguments(string prefix)
        {
            // Arrange
            var valueProvider = new Mock<IValueProvider>();
            valueProvider
                .Setup(v => v.ContainsPrefix(prefix))
                .Returns(true);

            var binder = new StubModelBinder(context =>
            {
                Assert.Same(valueProvider.Object, context.ValueProvider);

                Assert.True(context.PropertyFilter(context.ModelMetadata.Properties["Property1"]));
                Assert.True(context.PropertyFilter(context.ModelMetadata.Properties["Property2"]));

                Assert.False(context.PropertyFilter(context.ModelMetadata.Properties["Exclude1"]));
                Assert.False(context.PropertyFilter(context.ModelMetadata.Properties["Exclude2"]));
            });

            var controller = GetController(binder, valueProvider: null);
            var model = new MyModel();

            // Act
            await controller.TryUpdateModelAsync(model, prefix, valueProvider.Object, m => m.Property1, m => m.Property2);

            // Assert
            Assert.NotEqual(0, binder.BindModelCount);
        }

        [Fact]
        public async Task TryUpdateModelNonGeneric_PropertyFilterWithValueProviderOverload_UsesPassedArguments()
        {
            // Arrange
            var modelName = "mymodel";

            Func<ModelMetadata, bool> propertyFilter = (m) =>
                string.Equals(m.PropertyName, "Include1", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(m.PropertyName, "Include2", StringComparison.OrdinalIgnoreCase);

            var valueProvider = Mock.Of<IValueProvider>();

            var binder = new StubModelBinder(context =>
            {
                Assert.Same(valueProvider, context.ValueProvider);

                Assert.True(context.PropertyFilter(context.ModelMetadata.Properties["Include1"]));
                Assert.True(context.PropertyFilter(context.ModelMetadata.Properties["Include2"]));

                Assert.False(context.PropertyFilter(context.ModelMetadata.Properties["Exclude1"]));
                Assert.False(context.PropertyFilter(context.ModelMetadata.Properties["Exclude2"]));
            });

            var controller = GetController(binder, valueProvider: null);

            var model = new MyModel();

            // Act
            await controller.TryUpdateModelAsync(model, model.GetType(), modelName, valueProvider, propertyFilter);

            // Assert
            Assert.NotEqual(0, binder.BindModelCount);
        }

        [Fact]
        public async Task TryUpdateModelNonGeneric_ModelTypeOverload_UsesPassedArguments()
        {
            // Arrange
            var modelName = "mymodel";

            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var valueProvider = Mock.Of<IValueProvider>();
            var binder = new StubModelBinder(context =>
            {
                Assert.Same(valueProvider, Assert.IsType<CompositeValueProvider>(context.ValueProvider)[0]);

                // Include and exclude should be null, resulting in property
                // being included.
                Assert.True(context.PropertyFilter(context.ModelMetadata.Properties["Property1"]));
                Assert.True(context.PropertyFilter(context.ModelMetadata.Properties["Property2"]));
            });

            var controller = GetController(binder, valueProvider);
            var model = new MyModel();

            // Act
            var result = await controller.TryUpdateModelAsync(model, model.GetType(), modelName);

            // Assert
            Assert.NotEqual(0, binder.BindModelCount);
        }

        [Fact]
        public async Task TryUpdateModelNonGeneric_BindToBaseDeclaredType_ModelTypeOverload()
        {
            // Arrange
            var modelName = "mymodel";

            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var valueProvider = Mock.Of<IValueProvider>();
            var binder = new StubModelBinder(context =>
            {
                Assert.Same(valueProvider, Assert.IsType<CompositeValueProvider>(context.ValueProvider)[0]);

                // Include and exclude should be null, resulting in property
                // being included.
                Assert.True(context.PropertyFilter(context.ModelMetadata.Properties["Property1"]));
                Assert.True(context.PropertyFilter(context.ModelMetadata.Properties["Property2"]));
            });

            var controller = GetController(binder, valueProvider);
            MyModel model = new MyDerivedModel();

            // Act
            var result = await controller.TryUpdateModelAsync(model, model.GetType(), modelName);

            // Assert
            Assert.NotEqual(0, binder.BindModelCount);
        }

        [Fact]
        public void ControllerExposes_RequestServices()
        {
            // Arrange
            var controller = new TestableController();

            var serviceProvider = Mock.Of<IServiceProvider>();
            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.RequestServices)
                           .Returns(serviceProvider);

            controller.ControllerContext.HttpContext = httpContext.Object;

            // Act
            var innerServiceProvider = controller.HttpContext?.RequestServices;

            // Assert
            Assert.Same(serviceProvider, innerServiceProvider);
        }

        [Fact]
        public void ControllerExposes_Request()
        {
            // Arrange
            var controller = new TestableController();

            var request = Mock.Of<HttpRequest>();
            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.Request)
                           .Returns(request);

            controller.ControllerContext.HttpContext = httpContext.Object;

            // Act
            var innerRequest = controller.Request;

            // Assert
            Assert.Same(request, innerRequest);
        }

        [Fact]
        public void ControllerExposes_Response()
        {
            // Arrange
            var controller = new TestableController();

            var response = Mock.Of<HttpResponse>();
            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.Response)
                           .Returns(response);

            controller.ControllerContext.HttpContext = httpContext.Object;

            // Act
            var innerResponse = controller.Response;

            // Assert
            Assert.Same(response, innerResponse);
        }

        [Fact]
        public void ControllerExposes_RouteData()
        {
            // Arrange
            var controller = new TestableController();

            var routeData = Mock.Of<RouteData>();
            controller.ControllerContext.RouteData = routeData;

            // Act
            var innerRouteData = controller.RouteData;

            // Assert
            Assert.Same(routeData, innerRouteData);
        }

        [Fact]
        public void TryValidateModelWithValidModel_ReturnsTrue()
        {
            // Arrange
            var binder = new StubModelBinder();
            var controller = GetController(binder, valueProvider: null);
            controller.ObjectValidator = new DefaultObjectValidator(
                controller.MetadataProvider,
                new[] { Mock.Of<IModelValidatorProvider>() });

            var model = new TryValidateModelModel();

            // Act
            var result = controller.TryValidateModel(model);

            // Assert
            Assert.True(result);
            Assert.True(controller.ModelState.IsValid);
        }

        [Fact]
        public void TryValidateModelWithInvalidModelWithPrefix_ReturnsFalse()
        {
            // Arrange
            var model = new TryValidateModelModel();
            var validationResult = new[]
            {
                new ModelValidationResult(string.Empty, "Out of range!")
            };

            var validator = new Mock<IModelValidator>();
            validator.Setup(v => v.Validate(It.IsAny<ModelValidationContext>()))
                .Returns(validationResult);
            var validator1 = new ValidatorItem(validator.Object);
            validator1.Validator = validator.Object;

            var provider = new Mock<IModelValidatorProvider>();
            provider.Setup(v => v.CreateValidators(It.IsAny<ModelValidatorProviderContext>()))
                .Callback<ModelValidatorProviderContext>(c => c.Results.Add(validator1));

            var binder = new StubModelBinder();
            var controller = GetController(binder, valueProvider: null);
            controller.ObjectValidator = new DefaultObjectValidator(
                controller.MetadataProvider,
                new[] { provider.Object });

            // Act
            var result = controller.TryValidateModel(model, "Prefix");

            // Assert
            Assert.False(result);
            Assert.Equal(1, controller.ModelState.Count);
            var error = Assert.Single(controller.ModelState["Prefix.IntegerProperty"].Errors);
            Assert.Equal("Out of range!", error.ErrorMessage);
        }

        [Fact]
        public void TryValidateModelWithInvalidModelNoPrefix_ReturnsFalse()
        {
            // Arrange
            var model = new TryValidateModelModel();
            var validationResult = new[]
            {
                new ModelValidationResult(string.Empty, "Out of range!")
            };

            var validator = new Mock<IModelValidator>();
            validator.Setup(v => v.Validate(It.IsAny<ModelValidationContext>()))
                .Returns(validationResult);
            var validator1 = new ValidatorItem(validator.Object);
            validator1.Validator = validator.Object;

            var provider = new Mock<IModelValidatorProvider>();
            provider.Setup(v => v.CreateValidators(It.IsAny<ModelValidatorProviderContext>()))
                .Callback<ModelValidatorProviderContext>(c => c.Results.Add(validator1));

            var binder = new StubModelBinder();
            var controller = GetController(binder, valueProvider: null);
            controller.ObjectValidator = new DefaultObjectValidator(
                controller.MetadataProvider,
                new[] { provider.Object });

            // Act
            var result = controller.TryValidateModel(model);

            // Assert
            Assert.False(result);
            Assert.Equal(1, controller.ModelState.Count);
            var error = Assert.Single(controller.ModelState["IntegerProperty"].Errors);
            Assert.Equal("Out of range!", error.ErrorMessage);
        }

        [Fact]
        public void TryValidateModel_Succeeds_WithoutValidatorMetadata()
        {
            // Arrange
            // Do not add a Mock validator provider to this test. Test is intended to demonstrate ease of unit testing
            // and exercise DataAnnotationsModelValidatorProvider, avoiding #3586 regressions.
            var model = new TryValidateModelModel();
            var controller = GetController(binder: null, valueProvider: null);

            // Act
            var result = controller.TryValidateModel(model);

            // Assert
            Assert.True(controller.ModelState.IsValid);
        }

        private static ControllerBase GetController(IModelBinder binder, IValueProvider valueProvider)
        {
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var httpContext = new DefaultHttpContext();

            var validatorProviders = new[]
            {
                new DataAnnotationsModelValidatorProvider(
                    new ValidationAttributeAdapterProvider(),
                    new TestOptionsManager<MvcDataAnnotationsLocalizationOptions>(),
                    stringLocalizerFactory: null),
            };

            valueProvider = valueProvider ?? new SimpleValueProvider();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
                ValueProviderFactories = new[] { new SimpleValueProviderFactory(valueProvider), },
            };

            var binderFactory = new Mock<IModelBinderFactory>();
            binderFactory
                .Setup(f => f.CreateBinder(It.IsAny<ModelBinderFactoryContext>()))
                .Returns(binder);

            var controller = new TestableController()
            {
                ControllerContext = controllerContext,
                MetadataProvider = metadataProvider,
                ModelBinderFactory = binderFactory.Object,
                ObjectValidator = new DefaultObjectValidator(metadataProvider, validatorProviders),
            };

            return controller;
        }

        private class MyModel
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }

            public string Include1 { get; set; }
            public string Include2 { get; set; }

            public string Exclude1 { get; set; }
            public string Exclude2 { get; set; }
        }

        private class MyDerivedModel : MyModel
        {
            public string Property3 { get; set; }
        }

        private class TryValidateModelModel
        {
            public int IntegerProperty { get; set; }
        }


        private class TestableController : ControllerBase
        {
        }

        private class DisposableObject : IDisposable
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

        private class StatusCodeController : ControllerBase
        {
            public StatusCodeResult StatusCode_Int(int statusCode)
            {
                return StatusCode(statusCode);
            }

            public ObjectResult StatusCode_Object(int statusCode, object value)
            {
                return StatusCode(statusCode, value);
            }
        }

        private class ContentController : ControllerBase
        {
            public IActionResult Content_WithNoEncoding()
            {
                return Content("Hello!!");
            }

            public IActionResult Content_WithEncodingInCharset()
            {
                return Content("Hello!!", "text/xml; charset=us-ascii; p1=p1-value");
            }

            public IActionResult Content_WithInvalidCharset()
            {
                return Content("Hello!!", "text/xml; charset=invalid; p1=p1-value");
            }

            public IActionResult Content_WithEncodingInCharset_AndEncodingParameter()
            {
                return Content("Hello!!", "text/xml; charset=invalid; p1=p1-value", Encoding.ASCII);
            }
        }
    }
}
