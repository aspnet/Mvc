// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public class TempDataApplicationModelProviderTest
    {
        [Fact]
        public void CreateControllerModelWithOneTempDataProperty_TempDataAttributeAddsTempDataFilter()
        {
            // Arrange
            var provider = new TempDataApplicationModelProvider();
            var defaultProvider = new DefaultApplicationModelProvider(new TestOptionsManager<MvcOptions>());

            var context = new ApplicationModelProviderContext(new[] { typeof(TestController_OneTempDataProperty).GetTypeInfo() });
            defaultProvider.OnProvidersExecuting(context);

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            var controller = Assert.Single(context.Result.Controllers);
            Assert.Single(controller.Filters, f => f is SaveTempDataPropertyFilterFactory);
        }

        [Fact]
        public void CreateControllerModelWithTwoTempDataProperties_TempDataAttributeAddsTempDataFilter()
        {
            // Arrange
            var provider = new TempDataApplicationModelProvider();
            var defaultProvider = new DefaultApplicationModelProvider(new TestOptionsManager<MvcOptions>());

            var context = new ApplicationModelProviderContext(new[] { typeof(TestController_TwoTempDataProperties).GetTypeInfo() });
            defaultProvider.OnProvidersExecuting(context);

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            var controller = Assert.Single(context.Result.Controllers);
            Assert.Single(controller.Filters, f => f is SaveTempDataPropertyFilterFactory);
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

            Assert.Equal("TempData property Test does not have a public getter or setter.", exception.Message);
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

            Assert.Equal("TempData property Test is not declared as a primitive type or string.", exception.Message);
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

        private class NullTempDataProvider : ITempDataProvider
        {
            public IDictionary<string, object> LoadTempData(HttpContext context)
            {
                return null;
            }

            public void SaveTempData(HttpContext context, IDictionary<string, object> values)
            {
            }
        }
    }
}
