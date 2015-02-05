﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace FiltersWebSite
{
    [ControllerResultFilter]
    public class ResultFilterController : Controller, IResultFilter
    {
        [ChangeContentResultFilter]
        public IActionResult GetHelloWorld()
        {
            return Helpers.GetContentResult(null, "Hello World");
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            context.Result = Helpers.GetContentResult(context.Result, "Controller Override");
        }
    }
}