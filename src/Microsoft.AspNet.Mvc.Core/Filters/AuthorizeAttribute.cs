using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Security.Authorization;

namespace Microsoft.AspNet.Mvc.Core.Filters
{
    public class AuthorizeAttribute : AuthorizationFilterAttribute
    {
        private Claim[] _claims;

        public AuthorizeAttribute()
        {
            _claims = new Claim[0];
        }
        
        public AuthorizeAttribute([NotNull]IEnumerable<Claim> claims) 
        {
            _claims = claims.ToArray();
        }

        public AuthorizeAttribute(string type, string value)
        {
            _claims = new [] { new Claim(type, value) };
        }

        public AuthorizeAttribute(string type, string value, params string[] otherValues)
            : this(type, value)
        {
            if (otherValues.Length > 0)
            {
                _claims = _claims.Concat(otherValues.Select(claim => new Claim(type, claim))).ToArray();
            }
        }

        public override async Task Invoke(AuthorizationFilterContext context, Func<Task> next)
        {
            if (_claims.Length == 0)
            {
                throw new InvalidOperationException(Resources.AuthorizeAttribute_ClaimsCantBeEmpty);
            }

            // there is no reason to check claims if the context has already failed
            if (!context.HasFailed)
            {
                var httpContext = context.ActionContext.HttpContext;
                var user = httpContext.User;

                var authorizationService = httpContext.RequestServices.GetService<IAuthorizationService>();

                if (authorizationService == null)
                {
                    throw new InvalidOperationException(Resources.AuthorizeAttribute_AuthorizationServiceMustBeDefined);
                }

                var hasClaims = await authorizationService.CheckAsync(_claims, user);

                if (!hasClaims)
                {
                    context.Fail();
                }
            }

            await next();
        }
    }
}
