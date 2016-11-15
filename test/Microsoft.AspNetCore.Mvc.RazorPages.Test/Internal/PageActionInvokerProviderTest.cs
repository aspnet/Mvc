// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class PageInvokerProviderTest
    {
        [Fact]
        public void GetOrAddCacheEntry_PopulatesCacheEntry()
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

            var invokerProvider = new PageActionInvokerProvider(
                loader.Object,
                Mock.Of<IPageFactoryProvider>(),
                actionDescriptorProvider.Object,
                new IFilterProvider[0]);
            var context = new ActionInvokerProviderContext(new ActionContext
            {
                ActionDescriptor = descriptor,
            });

            // Act
            var entry = invokerProvider.GetOrAddCacheEntry(context, descriptor);

            // Assert
            Assert.NotNull(entry);
            Assert.Same(descriptor, entry.ActionDescriptor);
            Assert.NotNull(entry.PageFactory);
            Assert.NotNull(entry.ReleasePage);
        }

        [Fact]
        public void GetOrAddCacheEntry_CachesEntries()
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

            var invokerProvider = new PageActionInvokerProvider(
                loader.Object,
                Mock.Of<IPageFactoryProvider>(),
                actionDescriptorProvider.Object,
                new IFilterProvider[0]);
            var context = new ActionInvokerProviderContext(new ActionContext
            {
                ActionDescriptor = descriptor,
            });

            // Act
            var entry1 = invokerProvider.GetOrAddCacheEntry(context, descriptor);
            var entry2 = invokerProvider.GetOrAddCacheEntry(context, descriptor);

            // Assert
            Assert.Same(entry1, entry2);
        }

        [Fact]
        public void GetOrAddCacheEntry_UpdatesEntriesWhenActionDescriptorProviderCollectionIsUpdated()
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
            var invokerProvider = new PageActionInvokerProvider(
                loader.Object,
                Mock.Of<IPageFactoryProvider>(),
                actionDescriptorProvider.Object,
                new IFilterProvider[0]);
            var context = new ActionInvokerProviderContext(new ActionContext
            {
                ActionDescriptor = descriptor,
            });

            // Act
            var entry1 = invokerProvider.GetOrAddCacheEntry(context, descriptor);
            var entry2 = invokerProvider.GetOrAddCacheEntry(context, descriptor);

            // Assert
            Assert.NotSame(entry1, entry2);
        }
    }
}
