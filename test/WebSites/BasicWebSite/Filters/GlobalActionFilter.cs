// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace BasicWebSite
{
    public class GlobalActionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.ActionDescriptor.DisplayName ==
                "BasicWebSite.Controllers.HomeController.GetTextFromFilterAddedByOptions")
            {
                context.Result = new ContentResult()
                {
                    Content = "This was added by filter.",
                    ContentType = "text/plain"
                };
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }
    }
}