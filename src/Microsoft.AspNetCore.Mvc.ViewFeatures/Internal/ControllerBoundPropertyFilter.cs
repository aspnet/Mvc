// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    internal class ControllerBoundPropertyFilter : IActionFilter, IResultFilter
    {
        private readonly ITempDataDictionaryFactory _tempDataFactory;
        private readonly ControllerViewDataDictionaryFactory _viewDataFactory;
        private readonly BoundPropertyManager _propertyManager;

        public ControllerBoundPropertyFilter(
            ITempDataDictionaryFactory tempDataFactory,
            ControllerViewDataDictionaryFactory viewDataFactory,
            BoundPropertyManager propertyManager)
        {
            _tempDataFactory = tempDataFactory ?? throw new ArgumentNullException(nameof(tempDataFactory));
            _viewDataFactory = viewDataFactory ?? throw new ArgumentNullException(nameof(viewDataFactory));
            _propertyManager = propertyManager;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var tempData = _tempDataFactory.GetTempData(context.HttpContext);
            var viewData = _viewDataFactory.GetViewDataDictionary(context);

            var propertySourceContext = new BoundPropertyContext(context, tempData, viewData);
            _propertyManager.Populate(context.Controller, propertySourceContext);
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
            var viewData = _viewDataFactory.GetViewDataDictionary(context);

            var propertySourceContext = new BoundPropertyContext(context, tempData, viewData);
            _propertyManager.Save(context.Controller, propertySourceContext);
        }
    }
}
