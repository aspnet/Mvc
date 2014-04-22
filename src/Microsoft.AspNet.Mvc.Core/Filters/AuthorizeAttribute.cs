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
        private readonly Claim[] _claims;

        public AuthorizeAttribute()
        {
            _claims = new Claim[0];
        }
        
        public AuthorizeAttribute([NotNull]IEnumerable<Claim> claims) 
        {
            _claims = claims.ToArray();

            if (_claims.Length == 0)
            {
                throw new ArgumentException(Resources.AuthorizeAttribute_ClaimsCantBeEmpty);
            }
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

        public override async Task OnAuthorizationAsync([NotNull] AuthorizationContext context)
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

        public override void OnAuthorization([NotNull] AuthorizationContext context)
        {
            // The async filter will be called by the filter pipeline.
            throw new NotImplementedException(Resources.AuthorizeAttribute_OnAuthorizationNotImplemented);
        }
    }
}
