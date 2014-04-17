using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.DependencyInjection;

namespace Microsoft.AspNet.Mvc.Core.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizeAttribute : AuthorizationFilterAttribute
    {
        private string _roles;
        private string _users;

        protected Claim[] Claims;

        public AuthorizeAttribute()
        {
            Claims = new Claim[0];
        }
        
        public AuthorizeAttribute(string type, string value)
        {
            Claims = new [] { new Claim(type, value) };
        }

        public AuthorizeAttribute(string type, string value, params string[] other)
            : this(type, value)
        {
            if (other.Length > 0)
            {
                Claims = Claims.Concat(other.Select(c => new Claim(type, c))).ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the authorized roles.
        /// </summary>
        /// <value>
        /// The roles string.
        /// </value>
        /// <remarks>Multiple role names can be specified using the comma character as a separator.</remarks>
        public string Roles
        {
            get { return _roles ?? String.Empty; }
            set
            {
                _roles = value;
                var roleValues = SplitString(value);
                Claims = roleValues.Select(c => new Claim(System.Security.Claims.ClaimTypes.Role, c)).ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the authorized users.
        /// </summary>
        /// <value>
        /// The users string.
        /// </value>
        /// <remarks>Multiple role names can be specified using the comma character as a separator.</remarks>
        public string Users
        {
            get { return _users ?? String.Empty; }
            set
            {
                _users = value;
                var userValues = SplitString(value);
                Claims = userValues.Select(c => new Claim(System.Security.Claims.ClaimTypes.NameIdentifier, c)).ToArray();
            }
        }

        public override async Task Invoke(AuthorizationFilterContext context, Func<Task> next)
        {
            if (Claims == null || Claims.Length == 0)
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

                var hasClaims = await permissionService.CheckAsync(Claims, user);

                if (!hasClaims)
                {
                    context.Fail();
                }
            }

            await next();
        }

        private static IEnumerable<string> SplitString(string original)
        {
            if (String.IsNullOrWhiteSpace(original))
            {
                return new string[0];
            }

            return original.Split(',')
                .Where(piece => !String.IsNullOrWhiteSpace(piece))
                .ToArray();
        }
    }
}
