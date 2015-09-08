// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.ActionResults;
using Microsoft.AspNet.Mvc.Actions;
using Microsoft.AspNet.Mvc.Filters;
using Newtonsoft.Json;

namespace WebApiCompatShimWebSite
{
    public class ActionSelectionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var action = (ControllerActionDescriptor)context.ActionDescriptor;
            context.HttpContext.Response.Headers.Add(
                "ActionSelection",
                new string[]
                {
                    JsonConvert.SerializeObject(new
                    {
                        ActionName = action.Name,
                        ControllerName = action.ControllerName
                    })
                });

            context.Result = new HttpStatusCodeResult(StatusCodes.Status200OK);
        }
    }
}