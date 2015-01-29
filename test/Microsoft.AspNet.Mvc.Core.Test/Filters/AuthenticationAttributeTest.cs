// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Security;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Security;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Test
{
    public class AuthenticationAttributeTest
    {
        private AuthenticationContext GetAuthenticationContext(Action<ServiceCollection> registerServices = null)
        {
            var basicId = new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewPage"),
                        new Claim(ClaimTypes.Role, "Administrator"),
                        new Claim(ClaimTypes.Role, "User"),
                        new Claim(ClaimTypes.NameIdentifier, "John")},
                        "Basic");
            var bearerId = new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CupBearer"),
                        new Claim(ClaimTypes.Role, "Token"),
                        new Claim(ClaimTypes.NameIdentifier, "John Bear")},
                        "Bearer");

            // ServiceProvider
            var serviceCollection = new ServiceCollection();
            if (registerServices != null)
            {
                registerServices(serviceCollection);
            }

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // HttpContext
            var httpContext = new Mock<HttpContext>();
            httpContext.SetupProperty(c => c.User);
            httpContext.SetupGet(c => c.RequestServices).Returns(serviceProvider);
            var authResults = new AuthenticationResult[]
            {
                new AuthenticationResult(basicId, new AuthenticationProperties(), new AuthenticationDescription()),
                new AuthenticationResult(bearerId, new AuthenticationProperties(), new AuthenticationDescription())
            };
            httpContext.Setup(c => c.Authenticate(It.IsAny<string[]>()))
                .Returns(authResults);
            httpContext.Setup(c => c.AuthenticateAsync(It.IsAny<string[]>()))
                .ReturnsAsync(authResults);

            // AuthenticationContext
            var actionContext = new ActionContext(
                httpContext: httpContext.Object,
                routeData: new RouteData(),
                actionDescriptor: null
                );

            var authenticationContext = new AuthenticationContext(
                actionContext,
                Enumerable.Empty<IFilter>().ToList()
            );

            return authenticationContext;
        }

        [Fact]
        public async Task Invoke_NoAuthenticationTypesLeavesUserAlone()
        {
            // Arrange
            var authenticationAttribute = new AuthenticateAttribute();
            var authenticationContext = GetAuthenticationContext();

            // Act
            await authenticationAttribute.OnAuthenticationAsync(authenticationContext);

            // Assert
            Assert.Null(authenticationContext.HttpContext.User);
        }

        [Fact]
        public async Task Invoke_ReplacesUserWithPrincipalAuthenticationTypes()
        {
            // Arrange
            var authenticationAttribute = new AuthenticateAttribute() { AuthenticationTypes = "Basic,Bearer" };
            var authenticationContext = GetAuthenticationContext();

            // Act
            await authenticationAttribute.OnAuthenticationAsync(authenticationContext);

            // Assert
            Assert.Equal(2, authenticationContext.HttpContext.User.Identities.Count());
        }
    }
}
