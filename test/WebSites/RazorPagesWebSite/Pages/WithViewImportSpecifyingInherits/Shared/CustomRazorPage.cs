// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.Razor;

namespace RazorPagesWebSite
{
    public abstract class CustomRazorPage : RazorPage
    {
        public string Title
        {
            get => (string)ViewContext.ViewData[nameof(Title)];
            set => ViewContext.ViewData[nameof(Title)] = value;
        }
    }
}
