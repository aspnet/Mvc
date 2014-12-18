// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Security;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultAuthorizeRequirement : IAuthorizationRequirement
    {
        // No filter, allow all types through
        public IEnumerable<string> AuthenticationTypesFilter
        {
            get
            {
                return null;
            }
        }

        public Task<bool> CheckAsync(Security.AuthorizationContext context)
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

            var authContext = context.Resources.FirstOrDefault() as AuthorizationContext;
            return Task.FromResult(authContext != null && authContext.Filters.Any(item => item is IAllowAnonymous));
        }
    }

    public class AuthorizeAttribute : AuthorizationFilterAttribute
    {
        public AuthorizeAttribute([NotNull] string policy = "AnyAuthenticated")
        {
            Policy = policy;
        }

        public string Policy { get; set; }
        public string Roles { get; set; }

        public override async Task OnAuthorizationAsync([NotNull] AuthorizationContext context)
        {
            var httpContext = context.HttpContext;
            var user = httpContext.User;

            // Build a policy for the requested roles
            if (Roles != null)
            {
                var rolesPolicy = new AuthorizationPolicy();
                foreach (var role in Roles.Split(','))
                {
                    rolesPolicy.RequiresClaim(ClaimTypes.Role, role);
                }
                if (!await GetAuthService(httpContext).AuthorizeAsync(rolesPolicy, user, context))
                {
                    Fail(context);
                    return;
                }
            }

            if (!await GetAuthService(httpContext).AuthorizeAsync(Policy, user, context))
            {
                Fail(context);
            }
        }

        private IAuthorizationService GetAuthService(HttpContext httpContext)
        {
            var authorizationService = httpContext.RequestServices.GetRequiredService<IAuthorizationService>();
            if (authorizationService == null)
            {
                throw new InvalidOperationException(
                    Resources.AuthorizeAttribute_AuthorizationServiceMustBeDefined);
            }
            return authorizationService;
        }

        public sealed override void OnAuthorization([NotNull] AuthorizationContext context)
        {
            // The async filter will be called by the filter pipeline.
            throw new NotImplementedException(Resources.AuthorizeAttribute_OnAuthorizationNotImplemented);
        }
    }
}
