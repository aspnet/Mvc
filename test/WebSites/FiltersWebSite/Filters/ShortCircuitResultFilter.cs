// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.ActionResults;
using Microsoft.AspNet.Mvc.Filters;

namespace FiltersWebSite
{
    public class ShortCircuitResultFilter : ResultFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            context.Result = new ContentResult
            {
                Content = "The Result was never executed"
            };
        }
    }
}