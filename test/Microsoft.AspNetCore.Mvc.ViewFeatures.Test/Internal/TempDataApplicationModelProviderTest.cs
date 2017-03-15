// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Internal;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public class TempDataApplicationModelProviderTest
    {
        [Theory]
        [InlineData(typeof(TestController_OneTempDataProperty))]
        [InlineData(typeof(TestController_TwoTempDataProperties))]
        public void AddsTempDataPropertyFilter_ForTempDataAttributeProperties(Type type)
        {
            // Arrange
            var provider = new TempDataApplicationModelProvider();
            var defaultProvider = new DefaultApplicationModelProvider(new TestOptionsManager<MvcOptions>());

            var context = new ApplicationModelProviderContext(new[] { type.GetTypeInfo() });
            defaultProvider.OnProvidersExecuting(context);

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            var controller = Assert.Single(context.Result.Controllers);
            Assert.Single(controller.Filters, f => f is SaveTempDataPropertyFilterFactory);
        }

        [Fact]
        public void InitializeFilterFactory_WithExpectedPropertyHelpers_ForTempDataAttributeProperties()
        {
            // Arrange
            var provider = new TempDataApplicationModelProvider();
            var defaultProvider = new DefaultApplicationModelProvider(new TestOptionsManager<MvcOptions>());

            var context = new ApplicationModelProviderContext(new[] { typeof(TestController_OneTempDataProperty).GetTypeInfo() });
            defaultProvider.OnProvidersExecuting(context);

            // Act
            provider.OnProvidersExecuting(context);
            var controller = context.Result.Controllers.SingleOrDefault();
            var filter = controller.Filters.OfType<SaveTempDataPropertyFilterFactory>();
            var saveTempDataPropertyFilterFactory = filter.SingleOrDefault();

            // Assert
            Assert.NotNull(saveTempDataPropertyFilterFactory);
            var tempDataPropertyHelper = Assert.Single(saveTempDataPropertyFilterFactory.TempDataProperties);
            Assert.Equal("Test2", tempDataPropertyHelper.Name);
            Assert.NotNull(tempDataPropertyHelper.Property.GetCustomAttribute<TempDataAttribute>());
        }

        [Fact]
        public void ThrowsInvalidOperationException_PrivateSetter()
        {
            // Arrange
            var provider = new TempDataApplicationModelProvider();
            var defaultProvider = new DefaultApplicationModelProvider(new TestOptionsManager<MvcOptions>());

            var context = new ApplicationModelProviderContext(new[] { typeof(TestController_PrivateSet).GetTypeInfo() });
            defaultProvider.OnProvidersExecuting(context);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                provider.OnProvidersExecuting(context);
            });

            Assert.Equal($"The '{typeof(TestController_PrivateSet).FullName}.{nameof(TestController_NonPrimitiveType.Test)}' property with TempDataAttribute is invalid. A property using TempDataAttribute must have a public getter and setter.", exception.Message);
        }

        [Fact]
        public void ThrowsInvalidOperationException_NonPrimitiveType()
        {
            // Arrange
            var provider = new TempDataApplicationModelProvider();
            var defaultProvider = new DefaultApplicationModelProvider(new TestOptionsManager<MvcOptions>());

            var context = new ApplicationModelProviderContext(new[] { typeof(TestController_NonPrimitiveType).GetTypeInfo() });
            defaultProvider.OnProvidersExecuting(context);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                provider.OnProvidersExecuting(context);
            });

            Assert.Equal($"The '{typeof(TestController_NonPrimitiveType).FullName}.{nameof(TestController_NonPrimitiveType.Test)}' property with TempDataAttribute is invalid. A property using TempDataAttribute must be of primitive or string type.", exception.Message);
        }

        public class TestController_OneTempDataProperty
        {
            public string Test { get; set; }

            [TempData]
            public string Test2 { get; set; }
        }

        public class TestController_TwoTempDataProperties
        {
            [TempData]
            public string Test { get; set; }

            [TempData]
            public string Test2 { get; set; }
        }

        public class TestController_PrivateSet : Controller
        {
            [TempData]
            public string Test { get; private set; }
        }

        public class TestController_NonPrimitiveType : Controller
        {
            [TempData]
            public object Test { get; set; }
        }
    }
}
