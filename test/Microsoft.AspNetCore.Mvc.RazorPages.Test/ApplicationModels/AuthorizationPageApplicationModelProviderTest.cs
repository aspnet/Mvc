﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    public class AuthorizationPageApplicationModelProviderTest
    {
        [Fact]
        public void OnProvidersExecuting_IgnoresAttributesOnHandlerMethods()
        {
            // Arrange
            var policyProvider = new DefaultAuthorizationPolicyProvider(Options.Create(new AuthorizationOptions()));
            var authorizationProvider = new AuthorizationPageApplicationModelProvider(policyProvider);
            var typeInfo = typeof(PageWithAuthorizeHandlers).GetTypeInfo();
            var context = GetApplicationProviderContext(typeInfo);

            // Act
            authorizationProvider.OnProvidersExecuting(context);

            // Assert
            Assert.Collection(
                context.PageApplicationModel.Filters,
                f => Assert.IsType<PageHandlerPageFilter>(f),
                f => Assert.IsType<HandleOptionsRequestsPageFilter>(f));
        }

        private class PageWithAuthorizeHandlers : Page
        {
            public ModelWithAuthorizeHandlers Model => null;

            public override Task ExecuteAsync() => throw new NotImplementedException();
        }

        public class ModelWithAuthorizeHandlers : PageModel
        {
            [Authorize]
            public void OnGet()
            {
            }
        }

        [Fact]
        public void OnProvidersExecuting_AddsAuthorizeFilter_IfModelHasAuthorizationAttributes()
        {
            // Arrange
            var policyProvider = new DefaultAuthorizationPolicyProvider(Options.Create(new AuthorizationOptions()));
            var authorizationProvider = new AuthorizationPageApplicationModelProvider(policyProvider);
            var context = GetApplicationProviderContext(typeof(TestPage).GetTypeInfo());

            // Act
            authorizationProvider.OnProvidersExecuting(context);

            // Assert
            Assert.Collection(
                context.PageApplicationModel.Filters,
                f => Assert.IsType<PageHandlerPageFilter>(f),
                f => Assert.IsType<HandleOptionsRequestsPageFilter>(f),
                f => Assert.IsType<AuthorizeFilter>(f));
        }

        private class TestPage : Page
        {
            public TestModel Model => null;

            public override Task ExecuteAsync() => throw new NotImplementedException();
        }

        [Authorize]
        private class TestModel : PageModel
        {
            public virtual void OnGet()
            {
            }
        }

        [Fact]
        public void OnProvidersExecuting_CollatesAttributesFromInheritedTypes()
        {
            // Arrange
            var options = Options.Create(new AuthorizationOptions());
            options.Value.AddPolicy("Base", policy => policy.RequireClaim("Basic").RequireClaim("Basic2"));
            options.Value.AddPolicy("Derived", policy => policy.RequireClaim("Derived"));

            var policyProvider = new DefaultAuthorizationPolicyProvider(options);
            var authorizationProvider = new AuthorizationPageApplicationModelProvider(policyProvider);

            var context = GetApplicationProviderContext(typeof(TestPageWithDerivedModel).GetTypeInfo());

            // Act
            authorizationProvider.OnProvidersExecuting(context);

            // Assert
            AuthorizeFilter authorizeFilter = null;
            Assert.Collection(
                context.PageApplicationModel.Filters,
                f => Assert.IsType<PageHandlerPageFilter>(f),
                f => Assert.IsType<HandleOptionsRequestsPageFilter>(f),
                f => authorizeFilter = Assert.IsType<AuthorizeFilter>(f));

            // Basic + Basic2 + Derived authorize
            Assert.Equal(3, authorizeFilter.Policy.Requirements.Count);
        }

        private class TestPageWithDerivedModel : Page
        {
            public DerivedModel Model => null;

            public override Task ExecuteAsync() =>throw new NotImplementedException();
        }

        [Authorize(Policy = "Base")]
        public class BaseModel : PageModel
        {
        }

        [Authorize(Policy = "Derived")]
        private class DerivedModel : BaseModel
        {
            public virtual void OnGet()
            {
            }
        }

        [Fact]
        public void OnProvidersExecuting_AddsAllowAnonymousFilter()
        {
            // Arrange
            var policyProvider = new DefaultAuthorizationPolicyProvider(Options.Create(new AuthorizationOptions()));
            var authorizationProvider = new AuthorizationPageApplicationModelProvider(policyProvider);
            var context = GetApplicationProviderContext(typeof(PageWithAnonymousModel).GetTypeInfo());

            // Act
            authorizationProvider.OnProvidersExecuting(context);

            // Assert
            Assert.Collection(
                context.PageApplicationModel.Filters,
                f => Assert.IsType<PageHandlerPageFilter>(f),
                f => Assert.IsType<HandleOptionsRequestsPageFilter>(f),
                f => Assert.IsType<AllowAnonymousFilter>(f));
        }

        private class PageWithAnonymousModel : Page
        {
            public AnonymousModel Model => null;

            public override Task ExecuteAsync() => throw new NotImplementedException();
        }

        [AllowAnonymous]
        public class AnonymousModel : PageModel
        {
            public void OnGet() { }
        }

        private static PageApplicationModelProviderContext GetApplicationProviderContext(TypeInfo typeInfo)
        {
            var defaultProvider = new DefaultPageApplicationModelProvider(
                TestModelMetadataProvider.CreateDefaultProvider(),
                Options.Create(new MvcOptions { AllowValidatingTopLevelNodes = true }),
                Options.Create(new RazorPagesOptions { AllowDefaultHandlingForOptionsRequests = true }));
            var context = new PageApplicationModelProviderContext(new PageActionDescriptor(), typeInfo);
            defaultProvider.OnProvidersExecuting(context);
            return context;
        }
    }
}