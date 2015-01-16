﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace CustomRouteWebSite
{
    public class LocaleAttribute : RouteConstraintAttribute
    {
        public LocaleAttribute(string locale)
            : base("locale", routeValue: locale, blockNonAttributedActions: true)
        {
        }
    }
}