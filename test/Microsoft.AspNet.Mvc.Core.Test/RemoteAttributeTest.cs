// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http.Core;
using Microsoft.AspNet.Mvc.Internal;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class RemoteAttributeTest
    {
        // Action, controller, and route names containing just whitespace are not legal.
        public static TheoryData<string> InvalidNames
        {
            get
            {
                return new TheoryData<string>
                {
                    null,
                    string.Empty,
                    "   ",
                    "\t \t",
                    " \r\n"
                };
            }
        }

        // Property names containing just whitespace are legal.
        public static TheoryData<string> NullOrEmptyNames
        {
            get
            {
                return new TheoryData<string>
                {
                    null,
                    string.Empty,
                };
            }
        }

        [Fact]
        public void IsValidAlwaysReturnsTrue()
        {
            // Act & Assert
            Assert.True(new RemoteAttribute("RouteName", "ParameterName").IsValid(null));
            Assert.True(new RemoteAttribute("ActionName", "ControllerName", "ParameterName").IsValid(null));
        }

        [Theory]
        [MemberData(nameof(InvalidNames))]
        public void Constructor_WithInvalidRouteName_Throws(string routeName)
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>("routeName", () => new RemoteAttribute(routeName));
        }

        [Theory]
        [MemberData(nameof(InvalidNames))]
        public void Constructor_WithInvalidActionName_Throws(string action)
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>("action", () => new RemoteAttribute(action, "Controller"));
        }

        [Theory]
        [MemberData(nameof(InvalidNames))]
        public void Constructor_WithInvalidControllerName_Throws(string controller)
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>("controller", () => new RemoteAttribute("Action", controller));
        }

        [Theory]
        [MemberData(nameof(NullOrEmptyNames))]
        public void FormatAdditionalFieldsForClientValidation_WithInvalidPropertyName_Throws(string property)
        {
            // Arrange
            var attribute = new RemoteAttribute(routeName: "default");

            // Act & Assert
            Assert.Throws<ArgumentException>(
                "property",
                () => attribute.FormatAdditionalFieldsForClientValidation(property));
        }

        [Theory]
        [MemberData(nameof(NullOrEmptyNames))]
        public void FormatPropertyForClientValidation_WithInvalidPropertyName_Throws(string property)
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(
                "property",
                () => RemoteAttribute.FormatPropertyForClientValidation(property));
        }

        [Fact]
        public void GetClientValidationRules_WithInvalidContext_Throws()
        {
            // Arrange
            var attribute = new RemoteAttribute(routeName: "default");
            var metadataProvider = new EmptyModelMetadataProvider();
            var metadata = metadataProvider.GetMetadataForProperty(
                modelAccessor: null,
                containerType: typeof(string),
                propertyName: "Length");
            var context = new ClientModelValidationContext(metadata, metadataProvider);

            // Act & Assert
            Assert.Throws<ArgumentException>("context", () => attribute.GetClientValidationRules(context));
        }

        [Fact]
        public void GetClientValidationRules_WithBadRouteName_Throws()
        {
            // Arrange
            var attribute = new RemoteAttribute("RouteName");
            var context = GetValidationContext();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => attribute.GetClientValidationRules(context));
            Assert.Equal("No URL for remote validation could be found.", exception.Message);
        }

        [Fact]
        public void GetClientValidationRules_WithActionController_NoController_Throws()
        {
            // Arrange
            var attribute = new RemoteAttribute("Action", "Controller");
            var context = GetValidationContextWithNoController();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => attribute.GetClientValidationRules(context));
            Assert.Equal("No URL for remote validation could be found.", exception.Message);
        }

        [Fact]
        public void GetClientValidationRules_WithRoute_CallsUrlHelperWithExpectedValues()
        {
            // Arrange
            var attribute = new RemoteAttribute("RouteName");
            var url = "/my/URL";
            var context = GetValidationContext(
                url,
                expectedRoute: "RouteName",
                expectedAction: null,
                expectedController: null);

            // Act & Assert
            var rule = Assert.Single(attribute.GetClientValidationRules(context));
            Assert.Equal("remote", rule.ValidationType);
            Assert.Equal("'Length' is invalid.", rule.ErrorMessage);

            Assert.Equal(2, rule.ValidationParameters.Count);
            Assert.Equal("*.Length", rule.ValidationParameters["additionalfields"]);
            Assert.Equal(url, rule.ValidationParameters["url"]);
        }

        [Fact]
        public void GetClientValidationRules_WithActionController_CallsUrlHelperWithExpectedValues()
        {
            // Arrange
            var attribute = new RemoteAttribute("Action", "Controller");
            var url = "/Controller/Action";
            var context = GetValidationContext(
                url,
                expectedRoute: null,
                expectedAction: "Action",
                expectedController: "Controller");

            // Act & Assert
            var rule = Assert.Single(attribute.GetClientValidationRules(context));
            Assert.Equal("remote", rule.ValidationType);
            Assert.Equal("'Length' is invalid.", rule.ErrorMessage);

            Assert.Equal(2, rule.ValidationParameters.Count);
            Assert.Equal("*.Length", rule.ValidationParameters["additionalfields"]);
            Assert.Equal(url, rule.ValidationParameters["url"]);
        }

        [Fact]
        public void GetClientValidationRules_WithActionController_PropertiesSet_CallsUrlHelperWithExpectedValues()
        {
            // Arrange
            var attribute = new RemoteAttribute("Action", "Controller")
            {
                HttpMethod = "POST",
                AdditionalFields = "Password,ConfirmPassword",
            };
            var url = "/Controller/Action";
            var context = GetValidationContext(
                url,
                expectedRoute: null,
                expectedAction: "Action",
                expectedController: "Controller");

            // Act & Assert
            var rule = Assert.Single(attribute.GetClientValidationRules(context));
            Assert.Equal("remote", rule.ValidationType);
            Assert.Equal("'Length' is invalid.", rule.ErrorMessage);

            Assert.Equal(3, rule.ValidationParameters.Count);
            Assert.Equal("*.Length,*.Password,*.ConfirmPassword", rule.ValidationParameters["additionalfields"]);
            Assert.Equal("POST", rule.ValidationParameters["type"]);
            Assert.Equal(url, rule.ValidationParameters["url"]);
        }

        [Fact]
        public void GetClientValidationRules_WithActionControllerArea_CallsUrlHelperWithExpectedValues()
        {
            // Arrange
            var attribute = new RemoteAttribute("Action", "Controller", "Test")
            {
                HttpMethod = "POST",
            };
            var url = "/Test/Controller/Action";
            var context = GetValidationContext(
                url,
                expectedRoute: null,
                expectedAction: "Action",
                expectedController: "Controller",
                expectedArea: "Test");

            // Act & Assert
            var rule = Assert.Single(attribute.GetClientValidationRules(context));
            Assert.Equal("remote", rule.ValidationType);
            Assert.Equal("'Length' is invalid.", rule.ErrorMessage);

            Assert.Equal(3, rule.ValidationParameters.Count);
            Assert.Equal("*.Length", rule.ValidationParameters["additionalfields"]);
            Assert.Equal("POST", rule.ValidationParameters["type"]);
            Assert.Equal(url, rule.ValidationParameters["url"]);
        }

        // Root area is current in this case.
        [Fact]
        public void GetClientValidationRules_WithActionController_FindsControllerInCurrentArea()
        {
            // Arrange XYZ
            var attribute = new RemoteAttribute("Action", "Controller");
            var context = GetValidationContextWithArea(currentArea: null);

            // Act & Assert
            var rule = Assert.Single(attribute.GetClientValidationRules(context));
            Assert.Equal("remote", rule.ValidationType);
            Assert.Equal("'Length' is invalid.", rule.ErrorMessage);

            Assert.Equal(2, rule.ValidationParameters.Count);
            Assert.Equal("*.Length", rule.ValidationParameters["additionalfields"]);
            Assert.Equal("/Controller/Action", rule.ValidationParameters["url"]);
        }

        // Test area is current in this case.
        [Fact]
        public void GetClientValidationRules_WithActionControllerInArea_FindsControllerInCurrentArea()
        {
            // Arrange XYZ
            var attribute = new RemoteAttribute("Action", "Controller");
            var context = GetValidationContextWithArea(currentArea: "Test");

            // Act & Assert
            var rule = Assert.Single(attribute.GetClientValidationRules(context));
            Assert.Equal("remote", rule.ValidationType);
            Assert.Equal("'Length' is invalid.", rule.ErrorMessage);

            Assert.Equal(2, rule.ValidationParameters.Count);
            Assert.Equal("*.Length", rule.ValidationParameters["additionalfields"]);
            Assert.Equal("/Test/Controller/Action", rule.ValidationParameters["url"]);
        }

        // Explicit reference to the (current) root area.
        [Theory]
        [MemberData(nameof(NullOrEmptyNames))]
        public void GetClientValidationRules_WithActionControllerArea_FindsControllerInRootArea(string areaName)
        {
            // Arrange
            var attribute = new RemoteAttribute("Action", "Controller", areaName);
            var context = GetValidationContextWithArea(currentArea: null);

            // Act & Assert
            var rule = Assert.Single(attribute.GetClientValidationRules(context));
            Assert.Equal("remote", rule.ValidationType);
            Assert.Equal("'Length' is invalid.", rule.ErrorMessage);

            Assert.Equal(2, rule.ValidationParameters.Count);
            Assert.Equal("*.Length", rule.ValidationParameters["additionalfields"]);
            Assert.Equal("/Controller/Action", rule.ValidationParameters["url"]);
        }

        // Test area is current in this case.
        [Theory]
        [MemberData(nameof(NullOrEmptyNames))]
        public void GetClientValidationRules_WithActionControllerAreaInArea_FindsControllerInRootArea(string areaName)
        {
            // Arrange
            var attribute = new RemoteAttribute("Action", "Controller", areaName);
            var context = GetValidationContextWithArea(currentArea: "Test");

            // Act & Assert
            var rule = Assert.Single(attribute.GetClientValidationRules(context));
            Assert.Equal("remote", rule.ValidationType);
            Assert.Equal("'Length' is invalid.", rule.ErrorMessage);

            Assert.Equal(2, rule.ValidationParameters.Count);
            Assert.Equal("*.Length", rule.ValidationParameters["additionalfields"]);
            Assert.Equal("/Controller/Action", rule.ValidationParameters["url"]);
        }

        // Root area is current in this case.
        [Fact]
        public void GetClientValidationRules_WithActionControllerArea_FindsControllerInNamedArea()
        {
            // Arrange XYZ
            var attribute = new RemoteAttribute("Action", "Controller", "Test");
            var context = GetValidationContextWithArea(currentArea: null);

            // Act & Assert
            var rule = Assert.Single(attribute.GetClientValidationRules(context));
            Assert.Equal("remote", rule.ValidationType);
            Assert.Equal("'Length' is invalid.", rule.ErrorMessage);

            Assert.Equal(2, rule.ValidationParameters.Count);
            Assert.Equal("*.Length", rule.ValidationParameters["additionalfields"]);
            Assert.Equal("/Test/Controller/Action", rule.ValidationParameters["url"]);
        }

        // Explicit reference to the current (Test) area.
        [Fact]
        public void GetClientValidationRules_WithActionControllerAreaInArea_FindsControllerInNamedArea()
        {
            // Arrange XYZ
            var attribute = new RemoteAttribute("Action", "Controller", "Test");
            var context = GetValidationContextWithArea(currentArea: "Test");

            // Act & Assert
            var rule = Assert.Single(attribute.GetClientValidationRules(context));
            Assert.Equal("remote", rule.ValidationType);
            Assert.Equal("'Length' is invalid.", rule.ErrorMessage);

            Assert.Equal(2, rule.ValidationParameters.Count);
            Assert.Equal("*.Length", rule.ValidationParameters["additionalfields"]);
            Assert.Equal("/Test/Controller/Action", rule.ValidationParameters["url"]);
        }

        // Test area is current in this case.
        [Fact]
        public void GetClientValidationRules_WithActionControllerAreaInArea_FindsControllerInDifferentArea()
        {
            // Arrange XYZ
            var attribute = new RemoteAttribute("Action", "Controller", "AnotherArea");
            var context = GetValidationContextWithArea(currentArea: "Test");

            // Act & Assert
            var rule = Assert.Single(attribute.GetClientValidationRules(context));
            Assert.Equal("remote", rule.ValidationType);
            Assert.Equal("'Length' is invalid.", rule.ErrorMessage);

            Assert.Equal(2, rule.ValidationParameters.Count);
            Assert.Equal("*.Length", rule.ValidationParameters["additionalfields"]);
            Assert.Equal("/AnotherArea/Controller/Action", rule.ValidationParameters["url"]);
        }

        private static ClientModelValidationContext GetValidationContext()
        {
            var metadataProvider = new EmptyModelMetadataProvider();
            var metadata = metadataProvider.GetMetadataForProperty(
                modelAccessor: null,
                containerType: typeof(string),
                propertyName: "Length");

            var urlHelper = UrlHelperTest.CreateUrlHelperWithRouteCollection("/app");

            return new MvcClientModelValidationContext(metadata, metadataProvider, urlHelper);
        }

        private static ClientModelValidationContext GetValidationContext(string url)
        {
            var metadataProvider = new EmptyModelMetadataProvider();
            var metadata = metadataProvider.GetMetadataForProperty(
                modelAccessor: null,
                containerType: typeof(string),
                propertyName: "Length");

            var serviceProvider = GetServiceProvider();
            var urlHelper = GetUrlHelper(serviceProvider, url);
            urlHelper
                .Setup(helper => helper.RouteUrl(
                    It.IsAny<string>(), // routeName
                    It.IsAny<object>(), // values
                    null,               // protocol
                    null,               // host
                    null))              // fragment
                .Returns(url);

            return new MvcClientModelValidationContext(metadata, metadataProvider, urlHelper.Object);
        }

        private static ClientModelValidationContext GetValidationContext(
            string url,
            string expectedRoute,
            string expectedAction,
            string expectedController)
        {
            var metadataProvider = new EmptyModelMetadataProvider();
            var metadata = metadataProvider.GetMetadataForProperty(
                modelAccessor: null,
                containerType: typeof(string),
                propertyName: "Length");

            var serviceProvider = GetServiceProvider();
            var urlHelper = GetUrlHelper(serviceProvider, url);
            urlHelper
                .Setup(helper => helper.RouteUrl(
                    expectedRoute,      // routeName
                    It.IsAny<object>(), // values
                    null,               // protocol
                    null,               // host
                    null))              // fragment
                .Callback((string routeName, object values, string protocol, string host, string fragment) =>
                {
                    var routeDictionary = Assert.IsType<RouteValueDictionary>(values);
                    Assert.Equal(expectedAction, (string)routeDictionary["action"]);
                    Assert.Equal(expectedController, (string)routeDictionary["controller"]);
                    Assert.False(routeDictionary.ContainsKey("area")); // Ensure no value is present.
                })
                .Returns(url);

            return new MvcClientModelValidationContext(metadata, metadataProvider, urlHelper.Object);
        }

        private static ClientModelValidationContext GetValidationContext(
            string url,
            string expectedRoute,
            string expectedAction,
            string expectedController,
            string expectedArea)
        {
            var metadataProvider = new EmptyModelMetadataProvider();
            var metadata = metadataProvider.GetMetadataForProperty(
                modelAccessor: null,
                containerType: typeof(string),
                propertyName: "Length");

            var serviceProvider = GetServiceProvider();
            var urlHelper = GetUrlHelper(serviceProvider, url);
            urlHelper
                .Setup(helper => helper.RouteUrl(
                    expectedRoute,      // routeName
                    It.IsAny<object>(), // values
                    null,               // protocol
                    null,               // host
                    null))              // fragment
                .Callback((string routeName, object values, string protocol, string host, string fragment) =>
                {
                    var routeDictionary = Assert.IsType<RouteValueDictionary>(values);
                    Assert.Equal(expectedAction, (string)routeDictionary["action"]);
                    Assert.Equal(expectedController, (string)routeDictionary["controller"]);

                    Assert.True(routeDictionary.ContainsKey("area")); // Ensure value is present, even if expecting null.
                    Assert.Equal(expectedArea, (string)routeDictionary["area"]);
                })
                .Returns(url);

            return new MvcClientModelValidationContext(metadata, metadataProvider, urlHelper.Object);
        }

        private static ClientModelValidationContext GetValidationContextWithArea(string currentArea)
        {
            var serviceProvider = GetServiceProvider();
            var routeCollection = GetRouteCollectionWithArea(serviceProvider);
            var routeData = new RouteData
            {
                Routers =
                {
                    routeCollection,
                },
                Values =
                {
                    { "action", "Index" },
                    { "controller", "Home" },
                },
            };
            if (!string.IsNullOrEmpty(currentArea))
            {
                routeData.Values["area"] = currentArea;
            }

            var contextAccessor = GetContextAccessor(serviceProvider, routeData);
            var actionSelector = new Mock<IActionSelector>(MockBehavior.Strict);
            var urlHelper = new UrlHelper(contextAccessor, actionSelector.Object);
            var metadataProvider = new EmptyModelMetadataProvider();
            var metadata = metadataProvider.GetMetadataForProperty(
                modelAccessor: null,
                containerType: typeof(string),
                propertyName: "Length");

            return new MvcClientModelValidationContext(metadata, metadataProvider, urlHelper);
        }

        private static ClientModelValidationContext GetValidationContextWithNoController()
        {
            var serviceProvider = GetServiceProvider();
            var routeCollection = GetRouteCollectionWithNoController(serviceProvider);
            var routeData = new RouteData
            {
                Routers =
                {
                    routeCollection,
                },
            };

            var contextAccessor = GetContextAccessor(serviceProvider, routeData);
            var actionSelector = new Mock<IActionSelector>(MockBehavior.Strict);
            var urlHelper = new UrlHelper(contextAccessor, actionSelector.Object);
            var metadataProvider = new EmptyModelMetadataProvider();
            var metadata = metadataProvider.GetMetadataForProperty(
                modelAccessor: null,
                containerType: typeof(string),
                propertyName: "Length");

            return new MvcClientModelValidationContext(metadata, metadataProvider, urlHelper);
        }

        private static IRouter GetRouteCollectionWithArea(IServiceProvider serviceProvider)
        {
            var builder = new RouteBuilder
            {
                ServiceProvider = serviceProvider,
            };

            var handler = new Mock<IRouter>(MockBehavior.Strict);
            handler
                .Setup(e => e.GetVirtualPath(It.IsAny<VirtualPathContext>()))
                .Callback<VirtualPathContext>(context =>
                {
                    builder.ToString();

                    // This makes order of the routes below matter. Normal selection is more complicated and does not
                    // rely on route ordering.
                    context.IsBound = true;
                })
                .Returns<VirtualPathContext>(context => null);
            builder.DefaultHandler = handler.Object;

            // First try the route that requires the area value.
            builder.MapRoute("areaRoute", "{area:exists}/{controller}/{action}");
            builder.MapRoute("default", "{controller}/{action}", new { controller = "Home", action = "Index" });

            return builder.Build();
        }

        private static IRouter GetRouteCollectionWithNoController(IServiceProvider serviceProvider)
        {
            var builder = new RouteBuilder
            {
                ServiceProvider = serviceProvider,
            };

            var handler = new Mock<IRouter>(MockBehavior.Strict);
            handler
                .Setup(e => e.GetVirtualPath(It.IsAny<VirtualPathContext>()))
                .Callback<VirtualPathContext>(context =>
                {
                    builder.ToString();
                    context.IsBound = false;
                })
                .Returns<VirtualPathContext>(context => null);
            builder.DefaultHandler = handler.Object;

            builder.MapRoute("default", "static/route");

            return builder.Build();
        }

        private static Mock<UrlHelper> GetUrlHelper(IServiceProvider serviceProvider, string url)
        {
            var contextAccessor = GetContextAccessor(serviceProvider);
            var actionSelector = Mock.Of<IActionSelector>();
            var urlHelper = new Mock<UrlHelper>(MockBehavior.Strict, contextAccessor, actionSelector);

            return urlHelper;
        }

        private static IScopedInstance<ActionContext> GetContextAccessor(
            IServiceProvider serviceProvider,
            RouteData routeData = null)
        {
            var httpContext = new DefaultHttpContext
            {
                ApplicationServices = serviceProvider,
                RequestServices = serviceProvider,
            };

            if (routeData == null)
            {
                routeData = new RouteData
                {
                    Routers = { Mock.Of<IRouter>(), },
                };
            }

            var actionContext = new ActionContext(httpContext, routeData, new ActionDescriptor());
            var contextAccessor = new Mock<IScopedInstance<ActionContext>>();
            contextAccessor
                .SetupGet(accessor => accessor.Value)
                .Returns(actionContext);

            return contextAccessor.Object;
        }

        private static IServiceProvider GetServiceProvider()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(services => services.GetService(typeof(ILoggerFactory)))
                .Returns(new NullLoggerFactory());

            var descriptors = new List<ActionDescriptor>
            {
                new ActionDescriptor
                {
                    RouteConstraints = new List<RouteDataActionConstraint>
                    {
                        new RouteDataActionConstraint("area", null),
                    },
                },
                new ActionDescriptor
                {
                    RouteConstraints = new List<RouteDataActionConstraint>
                    {
                        new RouteDataActionConstraint("area", "Test"),
                    },
                },
                new ActionDescriptor
                {
                    RouteConstraints = new List<RouteDataActionConstraint>
                    {
                        new RouteDataActionConstraint("area", "AnotherArea"),
                    },
                },
            };

            var collection = new ActionDescriptorsCollection(descriptors, version: 1);
            var collectionProvider = new Mock<IActionDescriptorsCollectionProvider>(MockBehavior.Strict);
            collectionProvider
                .SetupGet(provider => provider.ActionDescriptors)
                .Returns(collection);
            serviceProvider
                .Setup(services => services.GetService(typeof(IActionDescriptorsCollectionProvider)))
                .Returns(collectionProvider.Object);

            var routeOptions = new RouteOptions
            {
                ConstraintMap =
                {
                    { "exists", typeof(KnownRouteValueConstraint) },
                },
            };
            var accessor = new Mock<IOptions<RouteOptions>>();
            accessor
                .SetupGet(options => options.Options)
                .Returns(routeOptions);
            serviceProvider
                .Setup(services => services.GetService(typeof(IInlineConstraintResolver)))
                .Returns(new DefaultInlineConstraintResolver(serviceProvider.Object, accessor.Object));

            return serviceProvider.Object;
        }
    }
}
