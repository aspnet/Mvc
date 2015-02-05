﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace ApplicationModelWebSite
{
    public class HomeController : Controller
    {
        public string GetCommonDescription()
        {
            var actionDescriptor = (ControllerActionDescriptor)ActionContext.ActionDescriptor;
            return actionDescriptor.Properties["description"].ToString();
        }
    }
}