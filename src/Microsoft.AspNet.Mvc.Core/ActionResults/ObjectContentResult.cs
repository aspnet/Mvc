// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.Core
{
    public class ObjectContentResult : ActionResult
    {
        public object Value { get; set; }

        public ObjectContentResult(object value)
        {
            Value = value;
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            ActionResult result;
            var actionReturnString = Value as string;
            if (actionReturnString != null)
            {
                result  = new ContentResult
                {
                    ContentType = "text/plain",
                    Content = actionReturnString,
                };
            }
            else
            {
                result = new JsonResult(Value);
            }

            return result.ExecuteResultAsync(context);
        }
    }
}