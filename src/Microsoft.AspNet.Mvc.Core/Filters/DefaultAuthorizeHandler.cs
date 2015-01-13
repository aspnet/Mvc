// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Security;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultAuthorizeHandler : IAuthorizationHandler
    {
        public Task HandleAsync(Security.AuthorizationContext context)
        {
            foreach (var req in context.Policy.Requirements)
            {
                if (req is AnyAuthorizedUserRequirement)
                {
                    var user = context.User;
                    var userIsAnonymous =
                        user == null ||
                        user.Identity == null ||
                        !user.Identity.IsAuthenticated;

                    if (!userIsAnonymous)
                    {
                        context.RequirementSucceeded(req);
                    }
                    else
                    {
                        var authContext = context.Resource as AuthorizationContext;
                        if (authContext != null && authContext.Filters.Any(item => item is IAllowAnonymous))
                        {
                            context.RequirementSucceeded(req);
                        }
                    }
                }
            }
            return Task.FromResult(0);
        }
    }
}
