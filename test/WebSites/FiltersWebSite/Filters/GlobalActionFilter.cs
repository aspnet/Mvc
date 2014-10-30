// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Mvc;

namespace FiltersWebSite
{
    public class GlobalActionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.ActionDescriptor.DisplayName == "FiltersWebSite.ActionFilterController.GetHelloWorld")
            {
                context.Result = Helpers.GetContentResult(context.Result, "GlobalActionFilter.OnActionExecuted");
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if(context.ActionDescriptor.DisplayName == "FiltersWebSite.ActionFilterController.GetHelloWorld")
            {
                (context.ActionArguments["fromGlobalActionFilter"] as List<ContentResult>)
                    .Add(Helpers.GetContentResult(null, "GlobalActionFilter.OnActionExecuting"));
            }
        }
    }
}