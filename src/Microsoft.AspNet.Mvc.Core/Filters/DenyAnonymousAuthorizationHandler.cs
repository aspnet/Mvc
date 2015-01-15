// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Security;

namespace Microsoft.AspNet.Mvc
{
    public class DenyAnonymousAuthorizationHandler : AuthorizationHandler<DenyAnonymousAuthorizationRequirement>
    {
        public override Task<bool> CheckAsync(Security.AuthorizationContext context, DenyAnonymousAuthorizationRequirement requirement)
        {
            var user = context.User;
            var userIsAnonymous =
                user == null ||
                user.Identity == null ||
                !user.Identity.IsAuthenticated;

            if (!userIsAnonymous)
            {
                return Task.FromResult(true);
            }
            else
            {
                var authContext = context.Resource as AuthorizationContext;
                if (authContext != null && authContext.Filters.Any(item => item is IAllowAnonymous))
                {
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }
    }
}
