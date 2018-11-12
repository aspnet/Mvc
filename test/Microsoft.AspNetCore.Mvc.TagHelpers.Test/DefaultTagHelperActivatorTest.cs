﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    // Tests to verify that script, link and image tag helper use the size limited instance of MemoryCache.
    public class DefaultTagHelperActivatorTest
    {
        private readonly TagHelperMemoryCacheProvider CacheProvider = new TagHelperMemoryCacheProvider();
        private readonly IMemoryCache MemoryCache = new MemoryCache(new MemoryCacheOptions());
        private readonly IHostingEnvironment HostingEnvironment = Mock.Of<IHostingEnvironment>();
        private readonly IFileVersionProvider FileVersionProvider = Mock.Of<IFileVersionProvider>();

        [Fact]
        public void ScriptTagHelper_DoesNotUseMemoryCacheInstanceFromDI()
        {
            // Arrange
            var activator = new DefaultTagHelperActivator(new TypeActivatorCache());
            var viewContext = CreateViewContext();

            var scriptTagHelper = activator.Create<ScriptTagHelper>(viewContext);

            Assert.Same(CacheProvider.Cache, scriptTagHelper.Cache);
            Assert.Same(HostingEnvironment, scriptTagHelper.HostingEnvironment);
            Assert.Same(FileVersionProvider, scriptTagHelper.FileVersionProvider);
        }

        [Fact]
        public void LinkTagHelper_DoesNotUseMemoryCacheInstanceFromDI()
        {
            // Arrange
            var activator = new DefaultTagHelperActivator(new TypeActivatorCache());
            var viewContext = CreateViewContext();

            var linkTagHelper = activator.Create<LinkTagHelper>(viewContext);

            Assert.Same(CacheProvider.Cache, linkTagHelper.Cache);
            Assert.Same(HostingEnvironment, linkTagHelper.HostingEnvironment);
            Assert.Same(FileVersionProvider, linkTagHelper.FileVersionProvider);
        }

        [Fact]
        public void ImageTagHelper_DoesNotUseMemoryCacheInstanceFromDI()
        {
            // Arrange
            var activator = new DefaultTagHelperActivator(new TypeActivatorCache());
            var viewContext = CreateViewContext();

            var imageTagHelper = activator.Create<ImageTagHelper>(viewContext);

            Assert.Same(CacheProvider.Cache, imageTagHelper.Cache);
            Assert.Same(HostingEnvironment, imageTagHelper.HostingEnvironment);
            Assert.Same(FileVersionProvider, imageTagHelper.FileVersionProvider);
        }

        private ViewContext CreateViewContext()
        {
            var services = new ServiceCollection()
                .AddSingleton(HostingEnvironment)
                .AddSingleton(MemoryCache)
                .AddSingleton(CacheProvider)
                .AddSingleton(HtmlEncoder.Default)
                .AddSingleton(JavaScriptEncoder.Default)
                .AddSingleton(Mock.Of<IUrlHelperFactory>())
                .AddSingleton(FileVersionProvider)
                .BuildServiceProvider();

            var viewContext = new ViewContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = services,
                }
            };

            return viewContext;
        }
    }
}
