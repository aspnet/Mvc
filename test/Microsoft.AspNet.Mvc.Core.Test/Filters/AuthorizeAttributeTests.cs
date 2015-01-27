// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.Security;
using Microsoft.AspNet.WebUtilities;
using Microsoft.Framework.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class AuthorizeAttributeTests : AuthorizeAttributeTestsBase
    {
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

            authorizationContext.Result = new HttpStatusCodeResult(StatusCodes.Status401Unauthorized);

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
