// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Microsoft.AspNet.Http.Core;
using Microsoft.AspNet.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Filters
{
    public class DefaultFilterProviderTest
    {
        [Fact]
        public void DefaultFilterProvider_Orders_CorsFilters_First()
        {
            // Arrange
            var filter1 = Mock.Of<IFilter>();
            var filter2 = Mock.Of<IFilter>();
            var corsFilter1 = Mock.Of<ICorsAuthorizationFilter>();
            var corsFilter2 = Mock.Of<ICorsAuthorizationFilter>();
            var context = CreateFilterContext(new List<FilterItem>()
            {
                new FilterItem(new FilterDescriptor(filter1, FilterScope.Global)),
                new FilterItem(new FilterDescriptor(corsFilter1, FilterScope.Global)),
                new FilterItem(new FilterDescriptor(filter2, FilterScope.Action)),
                new FilterItem(new FilterDescriptor(corsFilter2, FilterScope.Action)),
            });

            var provider = CreateProvider();

            // Act
            provider.OnProvidersExecuting(context);
            provider.OnProvidersExecuted(context);
            var results = context.Results;

            // Assert
            Assert.Equal(4, results.Count);
            Assert.Same(corsFilter1, results[0].Filter);
            Assert.Same(corsFilter2, results[1].Filter);
            Assert.Same(filter1, results[2].Filter);
            Assert.Same(filter2, results[3].Filter);
        }

        [Fact]
        public void DefaultFilterProvider_UsesFilter_WhenItsNotIFilterFactory()
        {
            // Arrange
            var filter = Mock.Of<IFilter>();

            var context = CreateFilterContext(new List<FilterItem>()
            {
                new FilterItem(new FilterDescriptor(filter, FilterScope.Global)),
            });

            var provider = CreateProvider();

            // Act
            provider.OnProvidersExecuting(context);
            provider.OnProvidersExecuted(context);

            var results = context.Results;

            // Assert
            var item = Assert.Single(results);
            Assert.Same(filter, item.Filter);
            Assert.Same(filter, item.Descriptor.Filter);
            Assert.Equal(0, item.Descriptor.Order);
        }

        [Fact]
        public void DefaultFilterProvider_UsesFilterFactory()
        {
            // Arrange
            var filter = Mock.Of<IFilter>();

            var filterFactory = new Mock<IFilterFactory>();
            filterFactory
                .Setup(ff => ff.CreateInstance(It.IsAny<IServiceProvider>()))
                .Returns(filter);

            var context = CreateFilterContext(new List<FilterItem>()
            {
                new FilterItem(new FilterDescriptor(filterFactory.Object, FilterScope.Global)),
            });

            var provider = CreateProvider();

            // Act
            provider.OnProvidersExecuting(context);
            provider.OnProvidersExecuted(context);

            var results = context.Results;

            // Assert
            var item = Assert.Single(results);
            Assert.Same(filter, item.Filter);
            Assert.Same(filterFactory.Object, item.Descriptor.Filter);
            Assert.Equal(0, item.Descriptor.Order);
        }

        [Fact]
        public void DefaultFilterProvider_UsesFilterFactory_WithOrder()
        {
            // Arrange
            var filter = Mock.Of<IFilter>();

            var filterFactory = new Mock<IFilterFactory>();
            filterFactory
                .Setup(ff => ff.CreateInstance(It.IsAny<IServiceProvider>()))
                .Returns(filter);

            filterFactory.As<IOrderedFilter>().SetupGet(ff => ff.Order).Returns(17);

            var context = CreateFilterContext(new List<FilterItem>()
            {
                new FilterItem(new FilterDescriptor(filterFactory.Object, FilterScope.Global)),
            });

            var provider = CreateProvider();

            // Act
            provider.OnProvidersExecuting(context);
            provider.OnProvidersExecuted(context);
            var results = context.Results;

            // Assert
            var item = Assert.Single(results);
            Assert.Same(filter, item.Filter);
            Assert.Same(filterFactory.Object, item.Descriptor.Filter);
            Assert.Equal(17, item.Descriptor.Order);
        }

        [Fact]
        public void DefaultFilterProvider_UsesFilterFactory_WithIFilterContainer()
        {
            // Arrange
            var filter = new Mock<IFilterContainer>();
            filter.SetupAllProperties();

            var filterFactory = new Mock<IFilterFactory>();
            filterFactory
                .Setup(ff => ff.CreateInstance(It.IsAny<IServiceProvider>()))
                .Returns(filter.As<IFilter>().Object);

            var context = CreateFilterContext(new List<FilterItem>()
            {
                new FilterItem(new FilterDescriptor(filterFactory.Object, FilterScope.Global)),
            });

            var provider = CreateProvider();

            // Act
            provider.OnProvidersExecuting(context);
            provider.OnProvidersExecuted(context);
            var results = context.Results;

            // Assert
            var item = Assert.Single(results);
            Assert.Same(filter.Object, item.Filter);
            Assert.Same(filterFactory.Object, ((IFilterContainer)item.Filter).FilterDefinition);
            Assert.Same(filterFactory.Object, item.Descriptor.Filter);
            Assert.Equal(0, item.Descriptor.Order);
        }

        private DefaultFilterProvider CreateProvider()
        {
            var services = new ServiceContainer();

            return new DefaultFilterProvider(services);
        }

        private FilterProviderContext CreateFilterContext(List<FilterItem> items)
        {
            var actionContext = CreateActionContext();
            actionContext.ActionDescriptor.FilterDescriptors = new List<FilterDescriptor>(
                items.Select(item => item.Descriptor));

            return new FilterProviderContext(actionContext, items);
        }

        private ActionContext CreateActionContext()
        {
            return new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        }
    }
}