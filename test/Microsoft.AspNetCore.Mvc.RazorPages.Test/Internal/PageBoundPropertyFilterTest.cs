// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class PageBoundPropertyFilterTest
    {
        [Fact]
        public void OnPageHandlerExecuting_PopulatesPropertiesWithAttributes()
        {
            // Arrange
            var tempDataFactory = GetTempDataFactory(new Dictionary<string, object>
            {
                { "TempData", "ValueFromRequest" },
            });

            var handler = new TestPageModel();
            var pageContext = CreatePageContext();
            var filter = CreateFilter(tempDataFactory, handler);
            var handlerExecutingContext = CreateHandlerExecutingContext(handler, pageContext);

            // Act
            filter.OnPageHandlerExecuting(handlerExecutingContext);

            // Assert
            Assert.Equal("ValueFromRequest", handler.TempData);
            Assert.Null(handler.ViewData);
        }

        [Fact]
        public void OnResultExecuting_WritesValuesToCollections()
        {
            // Arrange
            var tempDataFactory = GetTempDataFactory(new Dictionary<string, object>
            {
                { "TempData", "ValueFromRequest" },
            });

            var handler = new TestPageModel
            {
                TempData = "New-TempDataValue",
                ViewData = "New-ViewDataValue",
            };
            var pageContext = CreatePageContext();
            var filter = CreateFilter(tempDataFactory, handler);
            var resultExecutingContext = CreateResultExecutingContext(handler, pageContext);
            pageContext.HttpContext.Items[typeof(ViewDataDictionary)] = pageContext.ViewData;

            // Act
            filter.OnResultExecuting(resultExecutingContext);

            // Assert
            var tempData = (TempDataDictionary)tempDataFactory.GetTempData(pageContext.HttpContext);
            Assert.Equal("New-TempDataValue", tempData["TempData"]);
            Assert.Equal("New-ViewDataValue", pageContext.ViewData["ViewData"]);

            // Ensure the key is retained
            Assert.Equal(new[] { "TempData" }, tempData.RetainedKeys);
        }

        [Fact]
        public void OnResultExecuting_SkipsWritingToDictionary_IfValueIsUnchaned()
        {
            // Arrange
            var tempDataFactory = GetTempDataFactory(new Dictionary<string, object>
            {
                { "TempData", "ValueFromRequest" },
            });

            var handler = new TestPageModel
            {
                TempData = "ValueFromRequest",
                ViewData = null,
            };
            var pageContext = CreatePageContext();
            var filter = CreateFilter(tempDataFactory, handler);
            var resultExecutingContext = CreateResultExecutingContext(handler, pageContext);
            pageContext.HttpContext.Items[typeof(ViewDataDictionary)] = pageContext.ViewData;

            // Act
            filter.OnResultExecuting(resultExecutingContext);

            // Assert
            var tempData = (TempDataDictionary)tempDataFactory.GetTempData(pageContext.HttpContext);
            Assert.Equal("ValueFromRequest", tempData["TempData"]);
            Assert.Null(pageContext.ViewData["ViewData"]);

            // Ensure the key is not retained
            Assert.Empty(tempData.RetainedKeys);
        }

        private static PageHandlerExecutingContext CreateHandlerExecutingContext(TestPageModel handler, PageContext pageContext)
        {
            return new PageHandlerExecutingContext(
                pageContext,
                Array.Empty<IFilterMetadata>(),
                new HandlerMethodDescriptor(),
                new Dictionary<string, object>(),
                handler);
        }

        private ResultExecutingContext CreateResultExecutingContext(TestPageModel handler, PageContext pageContext)
        {
            return new ResultExecutingContext(
                pageContext,
                Array.Empty<IFilterMetadata>(),
                new PageResult(),
                handler);
        }

        private static PageBoundPropertyFilter CreateFilter(TempDataDictionaryFactory tempDataFactory, object handlerInstance)
        {
            var viewOptions = new MvcViewOptions
            {
                SuppressTempDataPropertyPrefix = true,
            };
            var manager = BoundPropertyManager.Create(viewOptions, handlerInstance.GetType());
            var filter = new PageBoundPropertyFilter(tempDataFactory, manager);
            return filter;
        }

        private static TempDataDictionaryFactory GetTempDataFactory(Dictionary<string, object> tempDataValues)
        {
            var provider = new TestTempDataProvider(tempDataValues);
            var tempDataFactory = new TempDataDictionaryFactory(provider);
            return tempDataFactory;
        }

        private static PageContext CreatePageContext()
        {
            var httpContext = new DefaultHttpContext();
            var pageContext = new PageContext(new ActionContext(httpContext, new RouteData(), new PageActionDescriptor()))
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()),
            };
            return pageContext;
        }

        public class TestPageModel
        {
            [TempData]
            public string TempData { get; set; }

            [ViewData]
            public string ViewData { get; set; }
        }
    }
}
