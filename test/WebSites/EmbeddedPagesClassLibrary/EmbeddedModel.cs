// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EmbeddedPagesClassLibrary
{
    public class EmbeddedModel : PageModel
    {
        public string Message { get; set; }

        public void OnGet()
        {
            Message = "Hello from Embedded Page with model";
        }
    }
}
