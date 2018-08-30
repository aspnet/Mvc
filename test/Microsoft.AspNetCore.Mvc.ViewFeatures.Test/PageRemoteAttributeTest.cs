﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Resources = Microsoft.AspNetCore.Mvc.ViewFeatures.Resources;

namespace Microsoft.AspNetCore.Mvc
{
    public class PageRemoteAttributeTest
    {
        [Fact]
        public void GetUrl_CallsUrlHelperWithExpectedValues()
        {
            // Arrange
            var testableAttribute = new TestablePageRemoteAttribute
            {
                PageName = "Foo",
                PageHandler = "Bar"
            };

            var ambientValues = new RouteValueDictionary()
            {
                ["page"] = "/Foo"
            };

            var routeData = new RouteData(ambientValues)
            {
                Routers = { Mock.Of<IRouter>() }
            };

            var urlHelper = new MockUrlHelper(url: "/Foo?handler=Bar")
            {
                ActionContext = GetActionContext(new ServiceCollection().BuildServiceProvider(), routeData)
            };

            var validationContext = GetValidationContext(urlHelper);

            // Act
            testableAttribute.InvokeGetUrl(validationContext);

            // Assert
            var routeDictionary = Assert.IsType<RouteValueDictionary>(urlHelper.RouteValues);

            Assert.Equal(2, routeDictionary.Count);
            Assert.Equal("/Foo", routeDictionary["page"] as string);
            Assert.Equal("Bar", routeDictionary["handler"] as string);
        }

        [Fact]
        public void GetUrl_WhenUrlHelperReturnsNull_Throws()
        {
            // Arrange
            var testableAttribute = new TestablePageRemoteAttribute
            {
                PageName = "Foo",
                PageHandler = "Bar"
            };

            var ambientValues = new RouteValueDictionary
            {
                { "page", "/Page" }
            };

            var routeData = new RouteData(ambientValues)
            {
                Routers = { Mock.Of<IRouter>() }
            };
            
            var urlHelper = new MockUrlHelper(url: null)
            {
                ActionContext = GetActionContext(new ServiceCollection().BuildServiceProvider(), routeData)
            };

            var validationContext = GetValidationContext(urlHelper);

            // Act && Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => testableAttribute.InvokeGetUrl(validationContext),
                Resources.RemoteAttribute_NoUrlFound);
        }
        
        private static ClientModelValidationContext GetValidationContext(IUrlHelper urlHelper, RouteData routeData = null)
        {
            var serviceCollection = GetServiceCollection();
            var factory = new Mock<IUrlHelperFactory>(MockBehavior.Strict);
            serviceCollection.AddSingleton<IUrlHelperFactory>(factory.Object);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var actionContext = GetActionContext(serviceProvider, routeData);

            factory
                .Setup(f => f.GetUrlHelper(actionContext))
                .Returns(urlHelper);

            var metadataProvider = new EmptyModelMetadataProvider();
            var metadata = metadataProvider.GetMetadataForProperty(
                containerType: typeof(string),
                propertyName: nameof(string.Length));

            return new ClientModelValidationContext(
                actionContext,
                metadata,
                metadataProvider,
                new AttributeDictionary());
        }

        private static ServiceCollection GetServiceCollection()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddSingleton<ILoggerFactory>(new NullLoggerFactory());

            serviceCollection.AddOptions();
            serviceCollection.AddRouting();

            serviceCollection.AddSingleton<IInlineConstraintResolver>(
                provider => new DefaultInlineConstraintResolver(provider.GetRequiredService<IOptions<RouteOptions>>()));

            return serviceCollection;
        }

        private static ActionContext GetActionContext(IServiceProvider serviceProvider, RouteData routeData)
        {
            // Set IServiceProvider properties because TemplateRoute gets services (e.g. an ILoggerFactory instance)
            // through the HttpContext.
            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider,
            };

            if (routeData == null)
            {
                routeData = new RouteData
                {
                    Routers = { Mock.Of<IRouter>(), },
                };
            }

            return new ActionContext(httpContext, routeData, new ActionDescriptor());
        }

        private class TestablePageRemoteAttribute : PageRemoteAttribute
        {
            public string InvokeGetUrl(ClientModelValidationContext context)
            {
                return base.GetUrl(context);
            }
        }

        private class MockUrlHelper : IUrlHelper
        {
            private readonly string _url;

            public MockUrlHelper(string url)
            {
                _url = url;
            }

            public ActionContext ActionContext { get; set; }

            public object RouteValues { get; private set; }

            public string Action(UrlActionContext actionContext)
            {
                throw new NotImplementedException();
            }

            public string Content(string contentPath)
            {
                throw new NotImplementedException();
            }

            public bool IsLocalUrl(string url)
            {
                throw new NotImplementedException();
            }

            public string Link(string routeName, object values)
            {
                throw new NotImplementedException();
            }

            public string RouteUrl(UrlRouteContext routeContext)
            {
                RouteValues = routeContext.Values;

                return _url;
            }
        }
    }
}
