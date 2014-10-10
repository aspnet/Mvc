﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Mvc.WebApiCompatShim;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.NestedProviders;
using Microsoft.Framework.OptionsModel;
using Moq;
using Xunit;

namespace System.Web.Http
{
    public class ApiControllerActionDiscoveryTest
    {
        [Fact]
        public void GetActions_ApiControllerWithControllerSuffix_IsController()
        {
            // Arrange
            var provider = CreateProvider();

            // Act
            var context = new ActionDescriptorProviderContext();
            provider.Invoke(context);

            var results = context.Results.Cast<ControllerActionDescriptor>();

            // Assert
            var controllerType = typeof(TestControllers.ProductsController).GetTypeInfo();
            var actions = results.Where(ad => ad.ControllerDescriptor.ControllerTypeInfo == controllerType).ToArray();

            Assert.NotEmpty(actions);
        }

        [Fact]
        public void GetActions_ApiControllerWithoutControllerSuffix_IsNotController()
        {
            // Arrange
            var provider = CreateProvider();

            // Act
            var context = new ActionDescriptorProviderContext();
            provider.Invoke(context);

            var results = context.Results.Cast<ControllerActionDescriptor>();

            // Assert
            var controllerType = typeof(TestControllers.Blog).GetTypeInfo();
            var actions = results.Where(ad => ad.ControllerDescriptor.ControllerTypeInfo == controllerType).ToArray();

            Assert.Empty(actions);
        }

        [Fact]
        public void GetActions_CreatesNamedAndUnnamedAction()
        {
            // Arrange
            var provider = CreateProvider();

            // Act
            var context = new ActionDescriptorProviderContext();
            provider.Invoke(context);

            var results = context.Results.Cast<ControllerActionDescriptor>();

            // Assert
            var controllerType = typeof(TestControllers.StoreController).GetTypeInfo();
            var actions = results
                .Where(ad => ad.ControllerDescriptor.ControllerTypeInfo == controllerType)
                .Where(ad => ad.MethodInfo.Name == "GetAll")
                .ToArray();

            Assert.Equal(2, actions.Length);

            var action = Assert.Single(
                actions,
                a => a.RouteConstraints.Any(rc => rc.RouteKey == "action" && rc.RouteValue == "GetAll"));
            Assert.Equal(
                new string[] { "GET" },
                Assert.Single(action.ActionConstraints.OfType<HttpMethodConstraint>()).HttpMethods);

            action = Assert.Single(
                actions,
                a => a.RouteConstraints.Any(rc => rc.RouteKey == "action" && rc.RouteValue == ""));
            Assert.Equal(
                new string[] { "GET" },
                Assert.Single(action.ActionConstraints.OfType<HttpMethodConstraint>()).HttpMethods);
        }

        [Fact]
        public void GetActions_CreatesNamedAndUnnamedAction_DefaultVerbIsPost()
        {
            // Arrange
            var provider = CreateProvider();

            // Act
            var context = new ActionDescriptorProviderContext();
            provider.Invoke(context);

            var results = context.Results.Cast<ControllerActionDescriptor>();

            // Assert
            var controllerType = typeof(TestControllers.StoreController).GetTypeInfo();
            var actions = results
                .Where(ad => ad.ControllerDescriptor.ControllerTypeInfo == controllerType)
                .Where(ad => ad.MethodInfo.Name == "Edit")
                .ToArray();

            Assert.Equal(2, actions.Length);

            var action = Assert.Single(
                actions,
                a => a.RouteConstraints.Any(rc => rc.RouteKey == "action" && rc.RouteValue == "Edit"));
            Assert.Equal(
                new string[] { "POST" },
                Assert.Single(action.ActionConstraints.OfType<HttpMethodConstraint>()).HttpMethods);

            action = Assert.Single(
                actions,
                a => a.RouteConstraints.Any(rc => rc.RouteKey == "action" && rc.RouteValue == ""));
            Assert.Equal(
                new string[] { "POST" },
                Assert.Single(action.ActionConstraints.OfType<HttpMethodConstraint>()).HttpMethods);
        }

        [Fact]
        public void GetActions_CreatesNamedAndUnnamedAction_RespectsVerbAttribute()
        {
            // Arrange
            var provider = CreateProvider();

            // Act
            var context = new ActionDescriptorProviderContext();
            provider.Invoke(context);

            var results = context.Results.Cast<ControllerActionDescriptor>();

            // Assert
            var controllerType = typeof(TestControllers.StoreController).GetTypeInfo();
            var actions = results
                .Where(ad => ad.ControllerDescriptor.ControllerTypeInfo == controllerType)
                .Where(ad => ad.MethodInfo.Name == "Delete")
                .ToArray();

            Assert.Equal(2, actions.Length);

            var action = Assert.Single(
                actions,
                a => a.RouteConstraints.Any(rc => rc.RouteKey == "action" && rc.RouteValue == "Delete"));
            Assert.Equal(
                new string[] { "PUT" },
                Assert.Single(action.ActionConstraints.OfType<HttpMethodConstraint>()).HttpMethods);

            action = Assert.Single(
                actions,
                a => a.RouteConstraints.Any(rc => rc.RouteKey == "action" && rc.RouteValue == ""));
            Assert.Equal(
                new string[] { "PUT" },
                Assert.Single(action.ActionConstraints.OfType<HttpMethodConstraint>()).HttpMethods);
        }

        // The method name is used to infer a verb, not the action name
        [Fact]
        public void GetActions_CreatesNamedAndUnnamedAction_VerbBasedOnMethodName()
        {
            // Arrange
            var provider = CreateProvider();

            // Act
            var context = new ActionDescriptorProviderContext();
            provider.Invoke(context);

            var results = context.Results.Cast<ControllerActionDescriptor>();

            // Assert
            var controllerType = typeof(TestControllers.StoreController).GetTypeInfo();
            var actions = results
                .Where(ad => ad.ControllerDescriptor.ControllerTypeInfo == controllerType)
                .Where(ad => ad.MethodInfo.Name == "Options")
                .ToArray();

            Assert.Equal(2, actions.Length);

            var action = Assert.Single(
                actions,
                a => a.RouteConstraints.Any(rc => rc.RouteKey == "action" && rc.RouteValue == "GetOptions"));
            Assert.Equal(
                new string[] { "OPTIONS" },
                Assert.Single(action.ActionConstraints.OfType<HttpMethodConstraint>()).HttpMethods);

            action = Assert.Single(
                actions,
                a => a.RouteConstraints.Any(rc => rc.RouteKey == "action" && rc.RouteValue == ""));
            Assert.Equal(
                new string[] { "OPTIONS" },
                Assert.Single(action.ActionConstraints.OfType<HttpMethodConstraint>()).HttpMethods);
        }

        [Fact]
        public void GetActions_AllWebApiActionsAreOverloaded()
        {
            // Arrange
            var provider = CreateProvider();

            // Act
            var context = new ActionDescriptorProviderContext();
            provider.Invoke(context);

            var results = context.Results.Cast<ControllerActionDescriptor>();

            // Assert
            var controllerType = typeof(TestControllers.StoreController).GetTypeInfo();
            var actions = results
                .Where(ad => ad.ControllerDescriptor.ControllerTypeInfo == controllerType)
                .ToArray();

            Assert.NotEmpty(actions);
            foreach (var action in actions)
            {
                Assert.Single(action.ActionConstraints, c => c is OverloadActionConstraint);
            }
        }

        [Fact]
        public void GetActions_AllWebApiActionsAreInWebApiArea()
        {
            // Arrange
            var provider = CreateProvider();

            // Act
            var context = new ActionDescriptorProviderContext();
            provider.Invoke(context);

            var results = context.Results.Cast<ControllerActionDescriptor>();

            // Assert
            var controllerType = typeof(TestControllers.StoreController).GetTypeInfo();
            var actions = results
                .Where(ad => ad.ControllerDescriptor.ControllerTypeInfo == controllerType)
                .ToArray();

            Assert.NotEmpty(actions);
            foreach (var action in actions)
            {
                Assert.Single(action.RouteConstraints, c => c.RouteKey == "area" && c.RouteValue == "api");
            }
        }

        private INestedProviderManager<ActionDescriptorProviderContext> CreateProvider()
        {
            var assemblyProvider = new Mock<IControllerAssemblyProvider>();
            assemblyProvider
                .SetupGet(ap => ap.CandidateAssemblies)
                .Returns(new Assembly[] { typeof(ApiControllerActionDiscoveryTest).Assembly });

            var filterProvider = new Mock<IGlobalFilterProvider>();
            filterProvider
                .SetupGet(fp => fp.Filters)
                .Returns(new List<IFilter>());

            var conventions = new NamespaceLimitedActionDiscoveryConventions();

            var options = new MvcOptions();

            var setup = new WebApiCompatShimOptionsSetup();
            setup.Invoke(options);

            var optionsAccessor = new Mock<IOptionsAccessor<MvcOptions>>();
            optionsAccessor
                .SetupGet(o => o.Options)
                .Returns(options);

            var provider = new ControllerActionDescriptorProvider(
                assemblyProvider.Object,
                conventions,
                filterProvider.Object,
                optionsAccessor.Object);

            return new NestedProviderManager<ActionDescriptorProviderContext>(
                new INestedProvider<ActionDescriptorProviderContext>[]
                {
                    provider
                });
        }

        private class NamespaceLimitedActionDiscoveryConventions : DefaultActionDiscoveryConventions
        {
            public override bool IsController(TypeInfo typeInfo)
            {
                return
                    typeInfo.Namespace == "System.Web.Http.TestControllers" &&
                    base.IsController(typeInfo);
            }
        }
    }
}

// These need to be public top-level classes to test discovery end-to-end. Don't reuse
// these outside of this test.
namespace System.Web.Http.TestControllers
{
    public class ProductsController : ApiController
    {
        public IActionResult GetAll()
        {
            return null;
        }
    }

    // Not a controller, because there's no controller suffix
    public class Blog : ApiController
    {
        public IActionResult GetBlogPosts()
        {
            return null;
        }
    }

    public class StoreController : ApiController
    {
        public IActionResult GetAll()
        {
            return null;
        }

        public IActionResult Edit(int id)
        {
            return null;
        }

        [HttpPut]
        public IActionResult Delete(int id)
        {
            return null;
        }

        [ActionName("GetOptions")]
        public IActionResult Options()
        {
            return null;
        }
    }
}