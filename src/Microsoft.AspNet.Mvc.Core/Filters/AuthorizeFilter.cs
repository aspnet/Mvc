// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Security;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// An implementation of <see cref="IAsyncAuthorizationFilter"/> which corresponds to [Authorize]
    /// </summary>
    public class AuthorizeFilter : IAsyncAuthorizationFilter, IOrderedFilter
    {
        /// <inheritdoc />
        public int Order { get; set; }

        /// <summary>
        /// Policy name used for authorization
        /// </summary>
        public string Policy { get; set; }

        /// <summary>
        /// User must be a member of one of these Roles if any are specified
        /// </summary>
        public IEnumerable<string> Roles { get; set; }

        /// <inheritdoc />
        public virtual async Task OnAuthorizationAsync([NotNull] AuthorizationContext context)
        {
            var httpContext = context.HttpContext;

            // Allow Anonymous skips all authorization
            if (HasAllowAnonymous(context))
            {
                return;
            }

            var authService = httpContext.RequestServices.GetRequiredService<IAuthorizationService>();

            // Build a policy for the requested roles if specified
            if (Roles != null && Roles.Any())
            {
                var rolesPolicy = new AuthorizationPolicyBuilder();
                rolesPolicy.RequiresRole(Roles);
                if (!await authService.AuthorizeAsync(httpContext.User, context, rolesPolicy.Build()))
                {
                    Fail(context);
                    return;
                }
            }

            var authorized = (Policy == null)
                // [Authorize] with no policy just requires any authenticated user
                ? await authService.AuthorizeAsync(httpContext.User, context, BuildAnyAuthorizedUserPolicy())
                : await authService.AuthorizeAsync(httpContext.User, context, Policy);
            if (!authorized)
            {
                Fail(context);
            }
        }

        /// <summary>
        /// Returns true if there is an IAllowAnonymous filter
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual bool HasAllowAnonymous([NotNull] AuthorizationContext context)
        {
            return context.Filters.Any(item => item is IAllowAnonymous);
        }

        /// <inheritdoc />
        protected virtual void Fail([NotNull] AuthorizationContext context)
        {
            context.Result = new HttpStatusCodeResult(401);
        }

        private static AuthorizationPolicy BuildAnyAuthorizedUserPolicy()
        {
            return new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        }
    }
}
