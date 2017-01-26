﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class PageInvokerProviderTest
    {
        [Fact]
        public void OnProvidersExecuting_WithEmptyModel_PopulatesCacheEntry()
        {
            // Arrange
            var descriptor = new PageActionDescriptor
            {
                RelativePath = "Path1",
                FilterDescriptors = new FilterDescriptor[0],
            };
            Func<PageContext, object> factory = _ => null;
            Action<PageContext, object> releaser = (_, __) => { };

            var loader = new Mock<IPageLoader>();
            loader.Setup(l => l.Load(It.IsAny<PageActionDescriptor>()))
                .Returns(typeof(object));
            var descriptorCollection = new ActionDescriptorCollection(new[] { descriptor }, version: 1);
            var actionDescriptorProvider = new Mock<IActionDescriptorCollectionProvider>();
            actionDescriptorProvider.Setup(p => p.ActionDescriptors).Returns(descriptorCollection);
            var pageFactoryProvider = new Mock<IPageFactoryProvider>();
            pageFactoryProvider.Setup(f => f.CreatePageFactory(It.IsAny<CompiledPageActionDescriptor>()))
                .Returns(factory);
            pageFactoryProvider.Setup(f => f.CreatePageDisposer(It.IsAny<CompiledPageActionDescriptor>()))
                .Returns(releaser);

            var invokerProvider = CreateInvokerProvider(
                loader.Object,
                actionDescriptorProvider.Object,
                pageFactoryProvider.Object);
            var context = new ActionInvokerProviderContext(
                new ActionContext(new DefaultHttpContext(), new RouteData(), descriptor));

            // Act
            invokerProvider.OnProvidersExecuting(context);

            // Assert
            Assert.NotNull(context.Result);
            var actionInvoker = Assert.IsType<PageActionInvoker>(context.Result);
            var entry = actionInvoker.CacheEntry;
            var compiledPageActionDescriptor = Assert.IsType<CompiledPageActionDescriptor>(entry.ActionDescriptor);
            Assert.Equal(descriptor.RelativePath, compiledPageActionDescriptor.RelativePath);
            Assert.Same(factory, entry.PageFactory);
            Assert.Same(releaser, entry.ReleasePage);
            Assert.Null(entry.ModelFactory);
            Assert.Null(entry.ReleaseModel);
        }

        [Fact]
        public void OnProvidersExecuting_WithModel_PopulatesCacheEntry()
        {
            // Arrange
            var descriptor = new PageActionDescriptor
            {
                RelativePath = "Path1",
                FilterDescriptors = new FilterDescriptor[0],
            };
            Func<PageContext, object> factory = _ => null;
            Action<PageContext, object> releaser = (_, __) => { };
            Func<PageContext, object> modelFactory = _ => null;
            Action<PageContext, object> modelDisposer = (_, __) => { };

            var loader = new Mock<IPageLoader>();
            loader.Setup(l => l.Load(It.IsAny<PageActionDescriptor>()))
                .Returns(typeof(PageWithModel));
            var descriptorCollection = new ActionDescriptorCollection(new[] { descriptor }, version: 1);
            var actionDescriptorProvider = new Mock<IActionDescriptorCollectionProvider>();
            actionDescriptorProvider.Setup(p => p.ActionDescriptors).Returns(descriptorCollection);
            var pageFactoryProvider = new Mock<IPageFactoryProvider>();
            pageFactoryProvider.Setup(f => f.CreatePageFactory(It.IsAny<CompiledPageActionDescriptor>()))
                .Returns(factory);
            pageFactoryProvider.Setup(f => f.CreatePageDisposer(It.IsAny<CompiledPageActionDescriptor>()))
                .Returns(releaser);

            var modelFactoryProvider = new Mock<IPageModelFactoryProvider>();
            modelFactoryProvider.Setup(f => f.CreateModelFactory(It.IsAny<CompiledPageActionDescriptor>()))
                .Returns(modelFactory);
            modelFactoryProvider.Setup(f => f.CreateModelDisposer(It.IsAny<CompiledPageActionDescriptor>()))
                .Returns(modelDisposer);

            var invokerProvider = CreateInvokerProvider(
                loader.Object,
                actionDescriptorProvider.Object,
                pageFactoryProvider.Object,
                modelFactoryProvider.Object);
            var context = new ActionInvokerProviderContext(
                new ActionContext(new DefaultHttpContext(), new RouteData(), descriptor));

            // Act
            invokerProvider.OnProvidersExecuting(context);

            // Assert
            Assert.NotNull(context.Result);
            var actionInvoker = Assert.IsType<PageActionInvoker>(context.Result);
            var entry = actionInvoker.CacheEntry;
            var compiledPageActionDescriptor = Assert.IsType<CompiledPageActionDescriptor>(entry.ActionDescriptor);
            Assert.Equal(descriptor.RelativePath, compiledPageActionDescriptor.RelativePath);
            Assert.Same(factory, entry.PageFactory);
            Assert.Same(releaser, entry.ReleasePage);
            Assert.Same(modelFactory, entry.ModelFactory);
            Assert.Same(modelDisposer, entry.ReleaseModel);
        }

        [Fact]
        public void OnProvidersExecuting_CachesEntries()
        {
            // Arrange
            var descriptor = new PageActionDescriptor
            {
                RelativePath = "Path1",
                FilterDescriptors = new FilterDescriptor[0],
            };
            var loader = new Mock<IPageLoader>();
            loader.Setup(l => l.Load(It.IsAny<PageActionDescriptor>()))
                .Returns(typeof(object));
            var descriptorCollection = new ActionDescriptorCollection(new[] { descriptor }, version: 1);
            var actionDescriptorProvider = new Mock<IActionDescriptorCollectionProvider>();
            actionDescriptorProvider.Setup(p => p.ActionDescriptors).Returns(descriptorCollection);

            var invokerProvider = CreateInvokerProvider(
                loader.Object,
                actionDescriptorProvider.Object);
            var context = new ActionInvokerProviderContext(
                new ActionContext(new DefaultHttpContext(), new RouteData(), descriptor));

            // Act - 1
            invokerProvider.OnProvidersExecuting(context);

            // Assert - 1
            Assert.NotNull(context.Result);
            var actionInvoker = Assert.IsType<PageActionInvoker>(context.Result);
            var entry1 = actionInvoker.CacheEntry;

            // Act - 2
            invokerProvider.OnProvidersExecuting(context);

            // Assert - 2
            Assert.NotNull(context.Result);
            actionInvoker = Assert.IsType<PageActionInvoker>(context.Result);
            var entry2 = actionInvoker.CacheEntry;
            Assert.Same(entry1, entry2);
        }

        [Fact]
        public void OnProvidersExecuting_UpdatesEntriesWhenActionDescriptorProviderCollectionIsUpdated()
        {
            // Arrange
            var descriptor = new PageActionDescriptor
            {
                RelativePath = "Path1",
                FilterDescriptors = new FilterDescriptor[0],
            };
            var descriptorCollection1 = new ActionDescriptorCollection(new[] { descriptor }, version: 1);
            var descriptorCollection2 = new ActionDescriptorCollection(new[] { descriptor }, version: 2);
            var actionDescriptorProvider = new Mock<IActionDescriptorCollectionProvider>();
            actionDescriptorProvider.SetupSequence(p => p.ActionDescriptors)
                .Returns(descriptorCollection1)
                .Returns(descriptorCollection2);

            var loader = new Mock<IPageLoader>();
            loader.Setup(l => l.Load(It.IsAny<PageActionDescriptor>()))
                .Returns(typeof(object));
            var invokerProvider = CreateInvokerProvider(
                 loader.Object,
                 actionDescriptorProvider.Object);
            var context = new ActionInvokerProviderContext(
                new ActionContext(new DefaultHttpContext(), new RouteData(), descriptor));

            // Act - 1
            invokerProvider.OnProvidersExecuting(context);

            // Assert - 1
            Assert.NotNull(context.Result);
            var actionInvoker = Assert.IsType<PageActionInvoker>(context.Result);
            var entry1 = actionInvoker.CacheEntry;

            // Act - 2
            invokerProvider.OnProvidersExecuting(context);

            // Assert
            Assert.NotNull(context.Result);
            actionInvoker = Assert.IsType<PageActionInvoker>(context.Result);
            var entry2 = actionInvoker.CacheEntry;
            Assert.NotSame(entry1, entry2);
        }

        private static PageActionInvokerProvider CreateInvokerProvider(
            IPageLoader loader,
            IActionDescriptorCollectionProvider actionDescriptorProvider,
            IPageFactoryProvider pageProvider = null,
            IPageModelFactoryProvider modelProvider = null)
        {
            var tempDataFactory = new Mock<ITempDataDictionaryFactory>();
            tempDataFactory.Setup(t => t.GetTempData(It.IsAny<HttpContext>()))
                .Returns((HttpContext context) => new TempDataDictionary(context, Mock.Of<ITempDataProvider>()));

            return new PageActionInvokerProvider(
                loader,
                pageProvider ?? Mock.Of<IPageFactoryProvider>(),
                modelProvider ?? Mock.Of<IPageModelFactoryProvider>(),
                actionDescriptorProvider,
                new IFilterProvider[0],
                new IValueProviderFactory[0],
                new EmptyModelMetadataProvider(),
                tempDataFactory.Object,
                new TestOptionsManager<HtmlHelperOptions>(),
                Mock.Of<IPageHandlerMethodSelector>(),
                new DiagnosticListener("Microsoft.AspNetCore"),
                NullLoggerFactory.Instance);
        }

        private class PageWithModel
        {
            public object Model { get; set; }
        }
    }
}
