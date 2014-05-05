// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Security;

namespace Microsoft.AspNet.Mvc
{
    public class AuthorizeAttribute : AuthorizationFilterAttribute
    {
        protected Claim[] _claims;

        public AuthorizeAttribute()
        {
            _claims = new Claim[0];
        }
        
        public AuthorizeAttribute([NotNull]IEnumerable<Claim> claims) 
        {
            _claims = claims.ToArray();
        }

        public AuthorizeAttribute(string claimType, string claimValue)
        {
            _claims = new [] { new Claim(claimType, claimValue) };
        }

        public AuthorizeAttribute(string claimType, string claimValue, params string[] otherClaimValues)
            : this(claimType, claimValue)
        {
            if (otherClaimValues.Length > 0)
            {
                _claims = _claims.Concat(otherClaimValues.Select(claim => new Claim(claimType, claim))).ToArray();
            }
        }

        public override async Task OnAuthorizationAsync([NotNull] AuthorizationContext context)
        {
            var httpContext = context.HttpContext;
            var user = httpContext.User;

            // when no claims are specified, we just need to ensure the user is authenticated
            if (_claims.Length == 0)
            {
                var userIsAnonymous = 
                    user == null || 
                    user.Identity == null || 
                    !user.Identity.IsAuthenticated;

                    if(userIsAnonymous && !HasAllowAnonymous(context))
                    {
                        base.Fail(context);
                    }
            }
            else 
            {
                var authorizationService = httpContext.RequestServices.GetService<IAuthorizationService>();

                if (authorizationService == null)
                {
                    throw new InvalidOperationException(Resources.AuthorizeAttribute_AuthorizationServiceMustBeDefined);
                }

                var authorized = await authorizationService.AuthorizeAsync(_claims, user);

                if (!authorized)
                {
                    base.Fail(context);
                }
            }
        }

        public sealed override void OnAuthorization([NotNull] AuthorizationContext context)
        {
            // The async filter will be called by the filter pipeline.
            throw new NotImplementedException(Resources.AuthorizeAttribute_OnAuthorizationNotImplemented);
        }
    }
}
