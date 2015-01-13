// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Security;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class AuthorizeAttribute : AuthorizationFilterAttribute
    {
        public AuthorizeAttribute(string policy = null)
        {
            Policy = policy;
        }

        public string Policy { get; set; }
        public string Roles { get; set; }

        public override async Task OnAuthorizationAsync([NotNull] AuthorizationContext context)
        {
            var httpContext = context.HttpContext;

            var authService = GetAuthService(httpContext);

            // Build a policy for the requested roles if specified
            if (Roles != null)
            {
                var rolesPolicy = new AuthorizationPolicyBuilder();
                rolesPolicy.RequiresRole(Roles.Split(','));
                if (!await authService.AuthorizeAsync(rolesPolicy.Build(), httpContext, context))
                {
                    Fail(context);
                    return;
                }
            }

            var authorized = (Policy == null)
                // [Authorize] with no policy just requires any authenticated user
                ? await authService.AuthorizeAsync(BuildAnyAuthorizedUserPolicy(), httpContext, context)
                : await authService.AuthorizeAsync(Policy, httpContext, context);
            if (!authorized)
            {
                Fail(context);
            }
        }

        private static AuthorizationPolicy BuildAnyAuthorizedUserPolicy()
        {
            var policyBuilder = new AuthorizationPolicyBuilder();
            policyBuilder.Requirements.Add(new AnyAuthorizedUserRequirement());
            return policyBuilder.Build();
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
