﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace FiltersWebSite
{
    public class ControllerAuthorizationFilter : AuthorizeUserAttribute
    {
        public override void OnAuthorization(AuthorizationContext context)
        {
            context.HttpContext.Response.Headers.Append("filters", "On Controller Authorization Filter - OnAuthorization");
        }
    }
}