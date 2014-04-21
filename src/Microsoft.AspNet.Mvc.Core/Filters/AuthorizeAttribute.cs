using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Security.Authorization;

namespace Microsoft.AspNet.Mvc.Core.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizeAttribute : AuthorizationFilterAttribute
    {
        private Claim[] _claims;

        public AuthorizeAttribute()
        {
            _claims = new Claim[0];
        }
        
        public AuthorizeAttribute(IEnumerable<Claim> claims) 
        {
            _claims = claims.ToArray();
        }

        public AuthorizeAttribute(string type, string value)
        {
            _claims = new [] { new Claim(type, value) };
        }

        public AuthorizeAttribute(string type, string value, params string[] other)
            : this(type, value)
        {
            if (other.Length > 0)
            {
                _claims = _claims.Concat(other.Select(c => new Claim(type, c))).ToArray();
            }
        }

        public override async Task Invoke(AuthorizationFilterContext context, Func<Task> next)
        {
            if (_claims == null || _claims.Length == 0)
            {
                throw new InvalidOperationException("Claims can't be empty");
            }

            // there is no reason to check claims if the context has already failed
            if (!context.HasFailed)
            {
                var httpContext = context.ActionContext.HttpContext;
                var user = httpContext.User;

                var permissionService = httpContext.RequestServices.GetService<IAuthorizationService>();

                if (permissionService == null)
                {
                    throw new InvalidOperationException("Permission service is not defined");
                }

                var hasClaims = await permissionService.CheckAsync(_claims, user);

                if (!hasClaims)
                {
                    context.Fail();
                }
            }

            await next();
        }
    }
}
