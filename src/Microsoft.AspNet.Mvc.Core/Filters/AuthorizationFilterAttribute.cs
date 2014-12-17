// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Security;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultMvcAuthorizationPolicy : IAuthorizationPolicy
    {
        public DefaultMvcAuthorizationPolicy()
        {
            var handlers = new List<IAuthorizationPolicyHandler>();
            handlers.Add(new DefaultAuthoriziationPolicyHandler());
            Handlers = handlers;
        }

        public IEnumerable<string> AuthenticationTypes { get; } = Enumerable.Empty<string>();

        public IEnumerable<AuthorizationClaimRequirement> Requirements { get; } = Enumerable.Empty<AuthorizationClaimRequirement>();

        public IEnumerable<IAuthorizationPolicyHandler> Handlers { get; private set; }
    }

    public class DefaultMvcAuthorizationPolicyHandler : IAuthorizationPolicyHandler
    {
        public Task<bool> AuthorizeAsync(Security.AuthorizationContext context)
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

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class AuthorizationFilterAttribute :
        Attribute, IAsyncAuthorizationFilter, IAuthorizationFilter, IOrderedFilter
    {
        public int Order { get; set; }

#pragma warning disable 1998
        public virtual async Task OnAuthorizationAsync([NotNull] AuthorizationContext context)
        {
            OnAuthorization(context);
        }
#pragma warning restore 1998

        public virtual void OnAuthorization([NotNull] AuthorizationContext context)
        {
        }

        protected virtual bool HasAllowAnonymous([NotNull] AuthorizationContext context)
        {
            return context.Filters.Any(item => item is IAllowAnonymous);
        }

        protected virtual void Fail([NotNull] AuthorizationContext context)
        {
            context.Result = new HttpStatusCodeResult(401);
        }
    }
}
