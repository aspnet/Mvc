// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Security;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// An implementation of <see cref="IAsyncAuthorizationFilter"/>
    /// </summary>
    public class AuthorizeFilter : IAsyncAuthorizationFilter
    {
        /// <summary>
        /// Authorize filter for the given policy
        /// </summary>
        /// <param name="policy"></param>
        public AuthorizeFilter([NotNull] AuthorizationPolicy policy)
        {
            Policy = policy;
        }

        public AuthorizationPolicy Policy { get; private set; } // Effective Combined Policy

        public virtual async Task OnAuthenticateAsync([NotNull] AuthorizationContext context)
        {
            if (Policy.ActiveAuthenticationTypes != null && Policy.ActiveAuthenticationTypes.Any())
            {
                var results = await context.HttpContext.AuthenticateAsync(Policy.ActiveAuthenticationTypes);
                context.HttpContext.User = new ClaimsPrincipal(results.Where(r => r.Identity != null).Select(r => r.Identity));
            }
        }

        /// <inheritdoc />
        public virtual async Task OnAuthorizationAsync([NotNull] AuthorizationContext context)
        {
            var httpContext = context.HttpContext;

            await OnAuthenticateAsync(context);

            // Allow Anonymous skips all authorization
            if (HasAllowAnonymous(context))
            {
                return;
            }

            // REFER to security for how they compare anonymous
            if (httpContext.User == null || !httpContext.User.Identities.Any(i => i.IsAuthenticated))
            {
                context.Result = new ChallengeResult(Policy.ActiveAuthenticationTypes.ToArray());
                return;
            }

            var authService = httpContext.RequestServices.GetRequiredService<IAuthorizationService>();
            if (!await authService.AuthorizeAsync(httpContext.User, context, Policy))
            {
                context.Result = new HttpStatusCodeResult(403);
                return;
            }
        }

        /// <summary>
        /// Returns true if there is an IAllowAnonymous<see cref="IAllowAnonymous" /> filter
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual bool HasAllowAnonymous([NotNull] AuthorizationContext context)
        {
            return context.Filters.Any(item => item is IAllowAnonymous);
        }
    }
}
