using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Moq;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class AuthorizeAttributeTestsBase
    {
        protected AuthorizationFilterContext GetAuthorizationFilterContext(Action<ServiceCollection> registerServices)
        {
            var validUser = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] { 
                        new Claim("Permission", "CanViewPage"),
                        new Claim(ClaimTypes.Role, "Administrator"), 
                        new Claim(ClaimTypes.NameIdentifier, "John")},
                        "Basic"));

            // ServiceProvider
            var serviceCollection = new ServiceCollection();
            if (registerServices != null)
            {
                registerServices(serviceCollection);
            }

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // HttpContext
            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(c => c.User).Returns(validUser);
            httpContext.SetupGet(c => c.RequestServices).Returns(serviceProvider);

            // AuthorizationFilterContext
            var actionContext = new ActionContext(
                httpContext: httpContext.Object,
                router: null,
                routeValues: null,
                actionDescriptor: null
                );

            var authorizationFilterContext = new AuthorizationFilterContext(
                actionContext,
                new FilterItem[0]
            );

            return authorizationFilterContext;
        }

    }
}