// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace ViewComponentWebSite
{
    // The full name is different here from the other view component with the same short name.
    [ViewComponent(Name = "ViewComponentWebSite.Explicit.FullName")]
    public class FullName1ViewComponent : ViewComponent
    {
        public string Invoke()
        {
            return "ViewComponentWebSite.Explicit.FullName";
        }
    }
}