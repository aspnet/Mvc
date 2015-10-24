// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FiltersWebSite
{
    public class ManagerHandler : AuthorizationHandler<OperationAuthorizationRequirement>
    {
        protected override void Handle(AuthorizationContext context, OperationAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim("Manager", "yes"))
            {
                context.Succeed(requirement);
            }
        }
    }
}
