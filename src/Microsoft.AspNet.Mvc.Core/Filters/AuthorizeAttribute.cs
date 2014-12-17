// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Security;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class AuthorizeAttribute : AuthorizationFilterAttribute
    {
        protected string _policy;

        public AuthorizeAttribute([NotNull] string policy = "AnyAuthenticated")
        {
            _policy = policy;
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
                    rolesPolicy.Requires(ClaimTypes.Role, role);
                }
                if (!await GetAuthService(httpContext).AuthorizeAsync(rolesPolicy, user, context))
                {
                    Fail(context);
                    return;
                }
            }

            if (!await GetAuthService(httpContext).AuthorizeAsync(_policy, user))
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
