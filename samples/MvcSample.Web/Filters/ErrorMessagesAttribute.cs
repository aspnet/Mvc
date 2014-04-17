﻿using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;

namespace MvcSample.Web
{
    public class ErrorMessagesAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null && !context.ExceptionHandled)
            {
                context.ExceptionHandled = true;

                context.Result = new ContentResult
                {
                    ContentType = "text/plain",
                    Content = "Boom " + context.Exception.Message
                };
            }
        }
    }
}