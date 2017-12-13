// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace FiltersWebSite
{
    [Authorize]
    public class CombineAuthorizeController : Controller
    {
        [Authorize("Api")]
        [Authorize(Roles = "Administrator")]
        public string Api()
        {
            if (User.Identities.Count() != 1)
            {
                return "Expected 1 identities.";
            }
            return "Hello World!";
        }
    }
}