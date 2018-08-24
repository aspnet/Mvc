// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Resources = Microsoft.AspNetCore.Mvc.ViewFeatures.Resources;

namespace Microsoft.AspNetCore.Mvc
{
    public class PageRemoteAttributeTest
    {
        private static readonly IModelMetadataProvider _metadataProvider = new EmptyModelMetadataProvider();
        private static readonly ModelMetadata _metadata = _metadataProvider.GetMetadataForProperty(
            typeof(string),
            nameof(string.Length));

        [Fact]
        public void GetUrl_WhenUrlHelperReturnsNull_Throws()
        {
            // Arrange
            var testablePageRemoteAttribute = new TestablePageRemoteAttribute("pageHandler", "pageName");
            var actionContext = GetActionContextForPage("/pageName");
            
            var mockUrlHelper = CreateMockUrlHelper(actionContext, returnUrl: null);
            var validationContext = new ClientModelValidationContext(
                actionContext, 
                _metadata, 
                _metadataProvider, 
                new AttributeDictionary());

            var serviceCollection = new ServiceCollection();
            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            urlHelperFactory.Setup(f => f.GetUrlHelper(actionContext))
                .Returns(mockUrlHelper.Object);
            serviceCollection.AddSingleton<IUrlHelperFactory>(urlHelperFactory.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            actionContext.HttpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };
            
            // Act && Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => testablePageRemoteAttribute.InvokeGetUrl(validationContext),
                Resources.RemoteAttribute_NoUrlFound);
        }


        private static ActionContext GetActionContextForPage(string page)
        {
            return new ActionContext
            {
                ActionDescriptor = new ActionDescriptor
                {
                    RouteValues = new Dictionary<string, string>
                    {
                        { "page", page },
                    },
                },
                RouteData = new RouteData
                {
                    Routers = { Mock.Of<IRouter>() },
                    Values = { ["page"] = page },
                },
            };
        }

        private static Mock<IUrlHelper> CreateMockUrlHelper(ActionContext context, string returnUrl)
        {
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.SetupGet(h => h.ActionContext)
                .Returns(context);
            urlHelper.Setup(h => h.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns(returnUrl);
            return urlHelper;
        }
        
        private class TestablePageRemoteAttribute : PageRemoteAttribute
        {
            public TestablePageRemoteAttribute(string pageHandler)
                : base(pageHandler)
            {
            }

            public TestablePageRemoteAttribute(string pageHandler, string pageName)
                : base(pageHandler, pageName)
            {
            }
            
            public new string RouteName
            {
                get
                {
                    return base.RouteName;
                }
            }

            public new RouteValueDictionary RouteData
            {
                get
                {
                    return base.RouteData;
                }
            }

            public string InvokeGetUrl(ClientModelValidationContext context)
            {
                return base.GetUrl(context);
            }
        }
    }
}
