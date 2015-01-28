// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Security;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Test
{
    public class AuthorizeFilterTest
    {
        private AuthorizationContext GetAuthorizationContext(Action<ServiceCollection> registerServices, bool anonymous = false)
        {
            var validUser = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewPage"),
                        new Claim(ClaimTypes.Role, "Administrator"),
                        new Claim(ClaimTypes.Role, "User"),
                        new Claim(ClaimTypes.NameIdentifier, "John")},
                        "Basic"));

            validUser.AddIdentity(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CupBearer"),
                        new Claim(ClaimTypes.Role, "Token"),
                        new Claim(ClaimTypes.NameIdentifier, "John Bear")},
                        "Bearer"));

            // ServiceProvider
            var serviceCollection = new ServiceCollection();
            if (registerServices != null)
            {
                registerServices(serviceCollection);
            }

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // HttpContext
            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(c => c.User).Returns(anonymous ? null : validUser);
            httpContext.SetupGet(c => c.RequestServices).Returns(serviceProvider);

            // AuthorizationContext
            var actionContext = new ActionContext(
                httpContext: httpContext.Object,
                routeData: new RouteData(),
                actionDescriptor: null
                );

            var authorizationContext = new AuthorizationContext(
                actionContext,
                Enumerable.Empty<IFilter>().ToList()
            );

            return authorizationContext;
        }

        [Fact]
        public async Task Invoke_ValidClaimShouldNotFail()
        {
            // Arrange
            var authorizeFilter = new AuthorizeFilter { Policy = "CanViewPage" };
            var authorizationContext = GetAuthorizationContext(services =>
            {
                services.AddAuthorization(null, options =>
                {
                    var policy = new AuthorizationPolicyBuilder().RequiresClaim("Permission", "CanViewPage");
                    options.AddPolicy("CanViewPage", policy.Build());
                });
            });

            // Act
            await authorizeFilter.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.Null(authorizationContext.Result);
        }

        [Fact]
        public async Task Invoke_EmptyClaimsShouldRejectAnonymousUser()
        {
            // Arrange
            var authorizationOptions = new AuthorizationOptions();
            var authorizeFilter = new AuthorizeFilter();
            var authorizationContext = GetAuthorizationContext(services =>
                services.AddAuthorization(),
                anonymous: true);

            // Act
            await authorizeFilter.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.NotNull(authorizationContext.Result);
        }

        [Fact]
        public async Task Invoke_EmptyClaimsWithAllowAnonymousAttributeShouldNotRejectAnonymousUser()
        {
            // Arrange
            var authorizeFilter = new AuthorizeFilter();
            var authorizationContext = GetAuthorizationContext(services =>
            {
                services.AddAuthorization();
                services.AddTransient<IAuthorizationHandler, DenyAnonymousAuthorizationHandler>();
            },
                anonymous: true);

            authorizationContext.Filters.Add(new AllowAnonymousAttribute());

            // Act
            await authorizeFilter.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.Null(authorizationContext.Result);
        }

        [Fact]
        public async Task Invoke_EmptyClaimsShouldAuthorizeAuthenticatedUser()
        {
            // Arrange
            var authorizeFilter = new AuthorizeFilter();
            var authorizationContext = GetAuthorizationContext(services =>
            {
                services.AddAuthorization();
                services.AddTransient<IAuthorizationHandler, DenyAnonymousAuthorizationHandler>();
            });

            // Act
            await authorizeFilter.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.Null(authorizationContext.Result);
        }

        [Fact]
        public async Task Invoke_SingleValidClaimShouldSucceed()
        {
            // Arrange
            var authorizeFilter = new AuthorizeFilter { Policy = "CanViewCommentOrPage" };
            var authorizationContext = GetAuthorizationContext(services =>
            {
                services.AddAuthorization(null, options =>
                {
                    var policy = new AuthorizationPolicyBuilder().RequiresClaim("Permission", "CanViewComment", "CanViewPage");
                    options.AddPolicy("CanViewCommentOrPage", policy.Build());
                });
                services.AddTransient<IAuthorizationHandler, DenyAnonymousAuthorizationHandler>();
            });

            // Act
            await authorizeFilter.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.Null(authorizationContext.Result);
        }

        [Fact]
        public async Task Invoke_RequireAdminRoleShouldFailWithNoHandlers()
        {
            // Arrange
            var authorizeFilter = new AuthorizeFilter { Roles = new string[] { "Administrator" } };
            var authorizationContext = GetAuthorizationContext(services =>
            {
                services.AddOptions();
                services.AddTransient<IAuthorizationService, DefaultAuthorizationService>();
            });

            // Act
            await authorizeFilter.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.NotNull(authorizationContext.Result);
        }

        [Fact]
        public async Task Invoke_RequireAdminAndUserRoleWithNoPolicyShouldSucceed()
        {
            // Arrange
            var authorizeFilter = new AuthorizeFilter { Roles = new string[] { "Administrator" } };
            var authorizationContext = GetAuthorizationContext(services =>
            {
                services.AddAuthorization();
                services.AddTransient<IAuthorizationHandler, DenyAnonymousAuthorizationHandler>();
            });

            // Act
            await authorizeFilter.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.Null(authorizationContext.Result);
        }

        [Fact]
        public async Task Invoke_RequireUnknownRoleShouldFail()
        {
            // Arrange
            var authorizeFilter = new AuthorizeFilter { Roles = new string[] { "Wut" } };
            var authorizationContext = GetAuthorizationContext(services =>
            {
                services.AddAuthorization();
                services.AddTransient<IAuthorizationHandler, DenyAnonymousAuthorizationHandler>();
            });

            // Act
            await authorizeFilter.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.NotNull(authorizationContext.Result);
        }

        [Fact]
        public async Task Invoke_RequireAdminRoleButFailPolicyShouldFail()
        {
            // Arrange
            var authorizeFilter = new AuthorizeFilter { Roles = new string[] { "Administrator" }, Policy = "Basic" };
            var authorizationContext = GetAuthorizationContext(services =>
            {
                services.AddAuthorization(null, options =>
                {
                    var policy = new AuthorizationPolicyBuilder().RequiresClaim("Permission", "CanViewComment");
                    options.AddPolicy("CanViewComment", policy.Build());
                });
                services.AddTransient<IAuthorizationHandler, DenyAnonymousAuthorizationHandler>();
            });

            // Act
            await authorizeFilter.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.NotNull(authorizationContext.Result);
        }

        [Fact]
        public async Task Invoke_InvalidClaimShouldFail()
        {
            // Arrange
            var authorizeFilter = new AuthorizeFilter { Policy = "CanViewComment" };
            var authorizationContext = GetAuthorizationContext(services =>
            {
                services.AddAuthorization(null, options =>
                {
                    var policy = new AuthorizationPolicyBuilder().RequiresClaim("Permission", "CanViewComment");
                    options.AddPolicy("CanViewComment", policy.Build());
                });
                services.AddTransient<IAuthorizationHandler, DenyAnonymousAuthorizationHandler>();
            });

            // Act
            await authorizeFilter.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.NotNull(authorizationContext.Result);
        }

        [Fact]
        public async Task Invoke_FailedContextShouldNotCheckPermission()
        {
            // Arrange
            bool authorizationServiceIsCalled = false;
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService
                .Setup(x => x.AuthorizeAsync(null, null, "CanViewComment"))
                .Returns(() =>
                {
                    authorizationServiceIsCalled = true;
                    return Task.FromResult(true);
                });

            var authorizeFilter = new AuthorizeFilter { Policy = "CanViewComment" };
            var authorizationContext = GetAuthorizationContext(services =>
                services.AddInstance(authorizationService.Object)
                );

            authorizationContext.Result = new HttpStatusCodeResult(401);

            // Act
            await authorizeFilter.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.False(authorizationServiceIsCalled);
        }

        [Fact]
        public async Task Invoke_FailWhenLookingForClaimInOtherIdentity()
        {
            // Arrange
            var authorizeFilter = new AuthorizeFilter { Policy = "CanViewComment" };
            var authorizationContext = GetAuthorizationContext(services =>
            {
                services.AddAuthorization(null, options =>
                {
                    var policy = new AuthorizationPolicyBuilder("Bearer").RequiresClaim("Permission", "CanViewComment");
                    options.AddPolicy("CanViewComment", policy.Build());
                });
                services.AddTransient<IAuthorizationHandler, DenyAnonymousAuthorizationHandler>();
            });

            // Act
            await authorizeFilter.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.NotNull(authorizationContext.Result);
        }

        [Fact]
        public async Task Invoke_CanLookingForClaimsInMultipleIdentities()
        {
            // Arrange
            var authorizeFilter = new AuthorizeFilter { Policy = "CanViewCommentCupBearer" };
            var authorizationContext = GetAuthorizationContext(services =>
            {
                services.AddAuthorization(null, options =>
                {
                    var policy = new AuthorizationPolicyBuilder("Basic", "Bearer")
                        .RequiresClaim("Permission", "CanViewComment")
                        .RequiresClaim("Permission", "CupBearer");
                    options.AddPolicy("CanViewComment", policy.Build());
                });
                services.AddTransient<IAuthorizationHandler, DenyAnonymousAuthorizationHandler>();
            });

            // Act
            await authorizeFilter.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.NotNull(authorizationContext.Result);
        }

        public async Task Invoke_NoPoliciesShouldNotFail()
        {
            // Arrange
            var authorizeFilter = new AuthorizeFilter { Policy = "CanViewPage" };
            var authorizationContext = GetAuthorizationContext(services =>
            {
                services.AddAuthorization();
                services.AddTransient<IAuthorizationHandler, DenyAnonymousAuthorizationHandler>();
            });

            // Act
            await authorizeFilter.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.Null(authorizationContext.Result);
        }
    }
}
