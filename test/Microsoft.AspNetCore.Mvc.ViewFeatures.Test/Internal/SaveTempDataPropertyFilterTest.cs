// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;
using System;
using Microsoft.Extensions.Internal;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public class SaveTempDataPropertyFilterTest
    {
        [Fact]
        public void OnTempDataSaving_ControllerUpdatesTempData()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, new NullTempDataProvider())
            {
                ["TempDataProperty-TestString"] = "FirstValue"
            };

            var factory = new Mock<ITempDataDictionaryFactory>();
            factory.Setup(f => f.GetTempData(httpContext))
                .Returns(tempData);
            

            var filter = new SaveTempDataPropertyFilter(factory.Object);
            
            var controller = new TestController();

            var propertyHelper1 = new PropertyHelper(controller.GetType().GetTypeInfo().GetProperty("TestString"));
            var propertyHelper2 = new PropertyHelper(controller.GetType().GetTypeInfo().GetProperty("TestString2"));
            var propertyHelpers = new List<PropertyHelper>
            {
                propertyHelper1,
                propertyHelper2
            };

            filter.PropertyHelpers = propertyHelpers;
            var context = new ActionExecutingContext(
                new ActionContext
                {
                    HttpContext = httpContext,
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor()
                },
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                controller);

            // Act
            filter.OnActionExecuting(context);
            controller.TestString = "SecondValue";
            filter.OnTempDataSaving(tempData);

            // Assert
            Assert.Equal("SecondValue", controller.TestString);
            Assert.Equal("SecondValue", tempData["TempDataProperty-TestString"]);
        }

        [Fact]
        public void OnTempDataSaving_ControllerReadsTempData()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, new NullTempDataProvider())
            {
                ["TempDataProperty-TestString"] = "FirstValue"
            };

            var factory = new Mock<ITempDataDictionaryFactory>();
            factory.Setup(f => f.GetTempData(httpContext))
                .Returns(tempData);

            var filter = new SaveTempDataPropertyFilter(factory.Object);
            var controller = new TestController();
            var propertyHelper1 = new PropertyHelper(controller.GetType().GetTypeInfo().GetProperty("TestString"));
            var propertyHelper2 = new PropertyHelper(controller.GetType().GetTypeInfo().GetProperty("TestString2"));
            var propertyHelpers = new List<PropertyHelper>
            {
                propertyHelper1,
                propertyHelper2
            };

            filter.PropertyHelpers = propertyHelpers;

            var context = new ActionExecutingContext(
                new ActionContext
                {
                    HttpContext = httpContext,
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor()
                },
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                controller);

            // Act
            Assert.Null(controller.TestString);
            filter.OnActionExecuting(context);
            filter.OnTempDataSaving(tempData);

            // Assert
            Assert.Equal("FirstValue", controller.TestString);
        }

        [Fact]
        public void LoadAndTrackChanges_SetsPropertyValue()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, new NullTempDataProvider())
            {
                ["TempDataProperty-TestString"] = "Value"
            };
            var factory = new Mock<ITempDataDictionaryFactory>();
            factory.Setup(f => f.GetTempData(httpContext))
                .Returns(tempData);

            var filter = new SaveTempDataPropertyFilter(factory.Object);
            var controller = new TestController();
            var propertyHelper1 = new PropertyHelper(controller.GetType().GetTypeInfo().GetProperty("TestString"));
            var propertyHelper2 = new PropertyHelper(controller.GetType().GetTypeInfo().GetProperty("TestString2"));
            var propertyHelpers = new List<PropertyHelper>
            {
                propertyHelper1,
                propertyHelper2
            };

            filter.PropertyHelpers = propertyHelpers;

            var context = new ActionExecutingContext(
                new ActionContext
                {
                    HttpContext = httpContext,
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor()
                },
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                controller);

            // Act
            filter.OnActionExecuting(context);
            filter.OnTempDataSaving(tempData);

            // Assert
            Assert.Equal("Value", controller.TestString);
            Assert.Null(controller.TestString2);
        }

        [Fact]
        public void LoadAndTrackChanges_ThrowsInvalidOperationException_PrivateSetter()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, new NullTempDataProvider())
            {
                ["TempDataProperty-TestString"] = "FirstValue"
            };

            var factory = new Mock<ITempDataDictionaryFactory>();
            factory.Setup(f => f.GetTempData(httpContext))
                .Returns(tempData);

            var filter = new SaveTempDataPropertyFilter(factory.Object);
            var controller = new TestController_PrivateSet
            {
                TempData = tempData
            };

            var propertyHelper = new PropertyHelper(controller.GetType().GetTypeInfo().GetProperty("Test"));           
            var propertyHelpers = new List<PropertyHelper>();
            propertyHelpers.Add(propertyHelper);

            filter.PropertyHelpers = propertyHelpers;

            var context = new ActionExecutingContext(
                new ActionContext
                {
                    HttpContext = httpContext,
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor()
                },
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                controller);


            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                filter.OnActionExecuting(context);
                filter.OnTempDataSaving(tempData);
            });

            Assert.Equal("TempData properties must have a public getter and setter.", exception.Message);
        }

        [Fact]
        public void LoadAndTrackChanges_ThrowsInvalidOperationException_NonPrimitiveType()
        {
            // Arrange
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, new NullTempDataProvider())
            {
                ["TempDataProperty-TestString"] = "FirstValue"
            };

            var factory = new Mock<ITempDataDictionaryFactory>();
            factory.Setup(f => f.GetTempData(httpContext))
                .Returns(tempData);

            var filter = new SaveTempDataPropertyFilter(factory.Object);
            var controller = new TestController_NonPrimitiveType
            {
                TempData = tempData
            };

            var propertyHelper = new PropertyHelper(controller.GetType().GetTypeInfo().GetProperty("Test"));
            var propertyHelpers = new List<PropertyHelper>();
            propertyHelpers.Add(propertyHelper);

            filter.PropertyHelpers = propertyHelpers;

            var context = new ActionExecutingContext(
                new ActionContext
                {
                    HttpContext = httpContext,
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor()
                },
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                controller);


            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                filter.OnActionExecuting(context);
                filter.OnTempDataSaving(tempData);
            });

            Assert.Equal("TempData properties must be declared as primitive types or string only.", exception.Message);
        }

        public class TestController : Controller
        {
            [TempData]
            public string TestString { get; set; }

            [TempData]
            public string TestString2 { get; set; }
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
