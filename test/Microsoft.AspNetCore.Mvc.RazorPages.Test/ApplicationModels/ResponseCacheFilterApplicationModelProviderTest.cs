﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Filters
{
    public class ResponseCacheFilterApplicationModelProviderTest
    {
        [Fact]
        public void OnProvidersExecuting_DoesNothingIfHandlerHasNoResponseCacheAttributes()
        {
            // Arrange
            var options = Options.Create(new MvcOptions());
            var provider = new ResponseCacheFilterApplicationModelProvider(options, Mock.Of<ILoggerFactory>());
            var typeInfo = typeof(PageWithoutResponseCache).GetTypeInfo();
            var context = GetApplicationProviderContext(typeInfo);

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            Assert.Collection(
                context.PageApplicationModel.Filters,
                f => Assert.IsType<PageHandlerPageFilter>(f),
                f => Assert.IsType<HandleOptionsRequestsPageFilter>(f));
        }

        private class PageWithoutResponseCache : Page
        {
            public ModelWithoutResponseCache Model => null;

            public override Task ExecuteAsync() => throw new NotImplementedException();
        }

        [Authorize]
        public class ModelWithoutResponseCache : PageModel
        {
            public void OnGet()
            {
            }
        }

        [Fact]
        public void OnProvidersExecuting_AddsResponseCacheFilters()
        {
            // Arrange
            var options = Options.Create(new MvcOptions());
            var provider = new ResponseCacheFilterApplicationModelProvider(options, Mock.Of<ILoggerFactory>());
            var typeInfo = typeof(PageWithResponseCache).GetTypeInfo();
            var context = GetApplicationProviderContext(typeInfo);

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            Assert.Collection(
                context.PageApplicationModel.Filters,
                f => { },
                f => Assert.IsType<PageHandlerPageFilter>(f),
                f => Assert.IsType<HandleOptionsRequestsPageFilter>(f),
                f =>
                {
                    var filter = Assert.IsType<PageResponseCacheFilter>(f);
                    Assert.Equal("Abc", filter.VaryByHeader);
                    Assert.Equal(12, filter.Duration);
                    Assert.True(filter.NoStore);
                });
        }

        private class PageWithResponseCache : Page
        {
            public ModelWithResponseCache Model => null;

            public override Task ExecuteAsync() => throw new NotImplementedException();
        }

        [ResponseCache(Duration = 12, NoStore = true, VaryByHeader = "Abc")]
        private class ModelWithResponseCache : PageModel
        {
            public virtual void OnGet()
            {
            }
        }

        [Fact]
        public void OnProvidersExecuting_ReadsCacheProfileFromOptions()
        {
            // Arrange
            var options = Options.Create(new MvcOptions());
            options.Value.CacheProfiles.Add("TestCacheProfile", new CacheProfile
            {
                Duration = 14,
                VaryByQueryKeys = new[] { "A" },
            });
            var provider = new ResponseCacheFilterApplicationModelProvider(options, Mock.Of<ILoggerFactory>());
            var typeInfo = typeof(PageWithResponseCacheProfile).GetTypeInfo();
            var context = GetApplicationProviderContext(typeInfo);

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            Assert.Collection(
                context.PageApplicationModel.Filters,
                f => { },
                f => Assert.IsType<PageHandlerPageFilter>(f),
                f => Assert.IsType<HandleOptionsRequestsPageFilter>(f),
                f =>
                {
                    var filter = Assert.IsType<PageResponseCacheFilter>(f);
                    Assert.Equal(new[] { "A" }, filter.VaryByQueryKeys);
                    Assert.Equal(14, filter.Duration);
                });
        }

        private class PageWithResponseCacheProfile : Page
        {
            public ModelWithResponseCacheProfile Model => null;

            public override Task ExecuteAsync() => throw new NotImplementedException();
        }

        [ResponseCache(CacheProfileName = "TestCacheProfile")]
        private class ModelWithResponseCacheProfile : PageModel
        {
            public virtual void OnGet()
            {
            }
        }

        private static PageApplicationModelProviderContext GetApplicationProviderContext(TypeInfo typeInfo)
        {
            var defaultProvider = new DefaultPageApplicationModelProvider(
                TestModelMetadataProvider.CreateDefaultProvider(),
                Options.Create(new MvcOptions()),
                Options.Create(new RazorPagesOptions { AllowDefaultHandlingForOptionsRequests = true }));
            var context = new PageApplicationModelProviderContext(new PageActionDescriptor(), typeInfo);
            defaultProvider.OnProvidersExecuting(context);
            return context;
        }
    }
}