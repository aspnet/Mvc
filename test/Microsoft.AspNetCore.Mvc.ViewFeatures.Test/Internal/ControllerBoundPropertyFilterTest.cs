// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public class ControllerBoundPropertyFilterTest
    {
        [Fact]
        public void OnActionExecuting_PopulatesPropertiesWithAttributes()
        {
            // Arrange
            var tempDataFactory = GetTempDataFactory(new Dictionary<string, object>
            {
                { "TempDataProperty", "ValueFromRequest" },
            });

            var handler = new TestController();
            var actionContext = CreateActionContext();
            var filter = CreateFilter(tempDataFactory, handler);
            var actionExecutingContext = CreateActionExecutingContext(handler, actionContext);

            // Act
            filter.OnActionExecuting(actionExecutingContext);

            // Assert
            Assert.Equal("ValueFromRequest", handler.TempDataProperty);
            Assert.Null(handler.ViewDataProperty);
        }

        [Fact]
        public void OnResultExecuting_WritesValuesToCollections()
        {
            // Arrange
            var tempDataFactory = GetTempDataFactory(new Dictionary<string, object>
            {
                { "TempDataProperty", "ValueFromRequest" },
            });
            var viewDataFactory = new ControllerViewDataDictionaryFactory(new EmptyModelMetadataProvider());

            var handler = new TestController
            {
                TempDataProperty = "New-TempDataValue",
                ViewDataProperty = "New-ViewDataValue",
            };
            var actionContext = CreateActionContext();
            var filter = CreateFilter(tempDataFactory, handler, viewDataFactory);
            var resultExecutingContext = CreateResultExecutingContext(handler, actionContext);

            // Act
            filter.OnResultExecuting(resultExecutingContext);

            // Assert
            var tempData = (TempDataDictionary)tempDataFactory.GetTempData(actionContext.HttpContext);
            Assert.Equal("New-TempDataValue", tempData["TempDataProperty"]);
            var viewData = viewDataFactory.GetViewDataDictionary(actionContext);
            Assert.Equal("New-ViewDataValue", viewData["ViewDataProperty"]);

            // Ensure the key is retained
            Assert.Equal(new[] { "TempDataProperty" }, tempData.RetainedKeys);
        }

        [Fact]
        public void OnResultExecuting_SkipsWritingToDictionary_IfValueIsUnchaned()
        {
            // Arrange
            var tempDataFactory = GetTempDataFactory(new Dictionary<string, object>
            {
                { "TempDataProperty", "ValueFromRequest" },
            });
            var viewDataFactory = new ControllerViewDataDictionaryFactory(new EmptyModelMetadataProvider());

            var handler = new TestController
            {
                TempDataProperty = "ValueFromRequest",
                ViewDataProperty = null,
            };
            var actionContext = CreateActionContext();
            var filter = CreateFilter(tempDataFactory, handler, viewDataFactory);
            var resultExecutingContext = CreateResultExecutingContext(handler, actionContext);

            // Act
            filter.OnResultExecuting(resultExecutingContext);

            // Assert
            var tempData = (TempDataDictionary)tempDataFactory.GetTempData(actionContext.HttpContext);
            Assert.Equal("ValueFromRequest", tempData["TempDataProperty"]);
            var viewData = viewDataFactory.GetViewDataDictionary(actionContext);
            Assert.Null(viewData["ViewDataProperty"]);

            // Ensure the key is retained
            Assert.Empty(tempData.RetainedKeys);
        }

        private static ActionExecutingContext CreateActionExecutingContext(TestController handler, ActionContext actionContext)
        {
            return new ActionExecutingContext(
                actionContext,
                Array.Empty<IFilterMetadata>(),
                new Dictionary<string, object>(),
                handler);
        }

        private ResultExecutingContext CreateResultExecutingContext(TestController handler, ActionContext actionContext)
        {
            return new ResultExecutingContext(
                actionContext,
                Array.Empty<IFilterMetadata>(),
                new ViewResult(),
                handler);
        }

        private static ControllerBoundPropertyFilter CreateFilter(
            TempDataDictionaryFactory tempDataFactory,
            object controller,
            ControllerViewDataDictionaryFactory viewDataDictionaryFactory = null)
        {
            var viewOptions = new MvcViewOptions
            {
                SuppressTempDataPropertyPrefix = true,
            };
            var manager = BoundPropertyManager.Create(viewOptions, controller.GetType());
            viewDataDictionaryFactory = viewDataDictionaryFactory ??
                new ControllerViewDataDictionaryFactory(new EmptyModelMetadataProvider());
            var filter = new ControllerBoundPropertyFilter(tempDataFactory, viewDataDictionaryFactory, manager);
            return filter;
        }

        private static TempDataDictionaryFactory GetTempDataFactory(Dictionary<string, object> tempDataValues)
        {
            var provider = new TestTempDataProvider(tempDataValues);
            var tempDataFactory = new TempDataDictionaryFactory(provider);
            return tempDataFactory;
        }

        private static ActionContext CreateActionContext()
        {
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());
            return actionContext;
        }

        public class TestController : Controller
        {
            [TempData]
            public string TempDataProperty { get; set; }

            [ViewData]
            public string ViewDataProperty { get; set; }
        }
    }
}
