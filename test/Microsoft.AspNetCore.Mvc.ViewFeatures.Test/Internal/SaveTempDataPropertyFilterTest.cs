// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Internal;
using Moq;
using Xunit;

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
            Assert.Null(controller.TestString2);
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
            filter.OnActionExecuting(context);
            filter.OnTempDataSaving(tempData);

            // Assert
            Assert.Equal("FirstValue", controller.TestString);
            Assert.Null(controller.TestString2);
        }

        public class TestController : Controller
        {
            [TempData]
            public string TestString { get; set; }

            [TempData]
            public string TestString2 { get; set; }
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
