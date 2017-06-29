// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPagesWebSite
{
    [AllowAnonymous]
    public class AnonymousModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
