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

        #pragma warning disable 1998
        public override async Task OnAuthorizationAsync([NotNull] AuthorizationContext context)
        {
            if (_claims.Length == 0)
            {
                throw new InvalidOperationException(Resources.AuthorizeAttribute_ClaimsCantBeEmpty);
            }
            
            if (!base.HasFailed)
            {
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
        }
        #pragma warning restore 1998
    }
}
