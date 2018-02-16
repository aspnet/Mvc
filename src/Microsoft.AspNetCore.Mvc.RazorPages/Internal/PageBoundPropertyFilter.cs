// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    internal class PageBoundPropertyFilter : IPageFilter, IResultFilter
    {
        private static readonly object ViewDataDictionaryKey = typeof(ViewDataDictionary);
        private readonly ITempDataDictionaryFactory _tempDataFactory;
        private readonly BoundPropertyManager _propertyManager;

        public PageBoundPropertyFilter(
            ITempDataDictionaryFactory tempDataFactory,
            BoundPropertyManager propertyManager)
        {
            _tempDataFactory = tempDataFactory ?? throw new ArgumentNullException(nameof(tempDataFactory));
            _propertyManager = propertyManager;
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var tempData = _tempDataFactory.GetTempData(context.HttpContext);
            // Stash ViewData so we can read it during result execution.
            context.HttpContext.Items[ViewDataDictionaryKey] = context.ViewData;

            var propertyContext = new BoundPropertyContext(context, tempData, context.ViewData);
            _propertyManager.Populate(context.HandlerInstance, propertyContext);
        }

        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var tempData = _tempDataFactory.GetTempData(context.HttpContext);
            var viewData = (ViewDataDictionary)context.HttpContext.Items[ViewDataDictionaryKey];

            var propertyContext = new BoundPropertyContext(context, tempData, viewData);
            _propertyManager.Save(context.Controller, propertyContext);
        }
    }
}
