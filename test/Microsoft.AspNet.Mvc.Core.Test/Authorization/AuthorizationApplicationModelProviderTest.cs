﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Authorization;
using Xunit;

namespace Microsoft.AspNet.Mvc.ApplicationModels
{
    public class AuthorizationApplicationModelProviderTest
    {
        [Fact]
        public void CreateControllerModel_AuthorizeAttributeAddsAuthorizeFilter()
        {
            // Arrange
            var provider = new AuthorizationApplicationModelProvider(new TestOptionsManager<AuthorizationOptions>());
            var defaultProvider = new DefaultApplicationModelProvider(new MockMvcOptionsAccessor());

            var context = new ApplicationModelProviderContext(new[] { typeof(AccountController).GetTypeInfo() });
            defaultProvider.OnProvidersExecuting(context);

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            var controller = Assert.Single(context.Result.Controllers);
            Assert.Single(controller.Filters, f => f is AuthorizeFilter);
        }

        [Fact]
        public void BuildActionModels_BaseAuthorizeFiltersAreStillValidWhenOverriden()
        {
            // Arrange
            var options = new TestOptionsManager<AuthorizationOptions>();
            options.Options.AddPolicy("Base", policy => policy.RequireClaim("Basic").RequireClaim("Basic2"));
            options.Options.AddPolicy("Derived", policy => policy.RequireClaim("Derived"));

            var provider = new AuthorizationApplicationModelProvider(options);
            var defaultProvider = new DefaultApplicationModelProvider(new MockMvcOptionsAccessor());

            var context = new ApplicationModelProviderContext(new[] { typeof(DerivedController).GetTypeInfo() });
            defaultProvider.OnProvidersExecuting(context);

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            var controller = Assert.Single(context.Result.Controllers);
            var action = Assert.Single(controller.Actions);
            Assert.Equal("Authorize", action.ActionName);
            Assert.Null(action.AttributeRouteModel);
            var authorizeFilters = action.Filters.OfType<AuthorizeFilter>();
            Assert.Single(authorizeFilters);
            Assert.Equal(3, authorizeFilters.First().Policy.Requirements.Count);
        }

        private class BaseController
        {
            [Authorize(Policy = "Base")]
            public virtual void Authorize()
            {
            }
        }

        private class DerivedController : BaseController
        {
            [Authorize(Policy = "Derived")]
            public override void Authorize()
            {
            }
        }

        [Authorize]
        public class AccountController
        {
        }
    }
}