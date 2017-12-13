// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace FiltersWebSite
{
    [Authorize("Api")]
    public class AuthorizeUserController : Controller
    {
        [Authorize("Api-Manager")]
        public string ApiManagers()
        {
            if (User.Identities.Count() != 1)
            {
                return "Expected 1 identities.";
            }
            return "Hello World!";
        }

        [Authorize(Roles = "Administrator")]
        public string AdminRole()
        {
            if (User.Identities.Count() != 1)
            {
                return "Expected 1 identities.";
            }
            return "Hello World!";
        }

        [Authorize("Interactive")]
        public string InteractiveUsers()
        {
            if (User.Identities.Count() != 2)
            {
                return "Expected 2 identities.";
            }
            return "Hello World!";
        }

        [Authorize("Impossible")]
        [AllowAnonymous]
        public string AlwaysCanCallAllowAnonymous()
        {
            return "Hello World!";
        }

        [Authorize("Impossible")]
        public string Impossible()
        {
            return "Hello World!";
        }
    }
}