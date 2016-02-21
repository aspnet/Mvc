// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.Filters;

namespace FiltersWebSite
{
    public class BlockAnonymous : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!HasAllowAnonymous(context))
            {
                var user = context.HttpContext.User;
                var userIsAnonymous =
                    user == null ||
                    user.Identity == null ||
                    !user.Identity.IsAuthenticated;

                if (userIsAnonymous)
                {
                    base.Fail(context);
                }
            }
        }
    }
}