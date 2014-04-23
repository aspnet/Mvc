﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Security.Authorization;

namespace Microsoft.AspNet.Mvc
{
    public class AuthorizeAttribute : AuthorizationFilterAttribute
    {
        protected Claim[] _claims;

        protected AuthorizeAttribute()
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
            if (_claims.Length == 0)
            {
                throw new InvalidOperationException(Resources.AuthorizeAttribute_ClaimsCantBeEmpty);
            }

            var httpContext = context.HttpContext;
            var user = httpContext.User;

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

        public sealed override void OnAuthorization([NotNull] AuthorizationContext context)
        {
            // The async filter will be called by the filter pipeline.
            throw new NotImplementedException(Resources.AuthorizeAttribute_OnAuthorizationNotImplemented);
        }
    }
}
