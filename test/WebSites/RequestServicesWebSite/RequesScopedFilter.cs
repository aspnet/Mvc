﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc;

namespace RequestServicesWebSite
{
    public class RequestScopedFilter : IActionFilter
    {
        private readonly RequestIdService _requestIdService;

        public RequestScopedFilter(RequestIdService requestIdService)
        {
            _requestIdService = requestIdService;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            throw new NotImplementedException();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            context.Result = new ObjectResult(_requestIdService.RequestId);
        }
    }
}