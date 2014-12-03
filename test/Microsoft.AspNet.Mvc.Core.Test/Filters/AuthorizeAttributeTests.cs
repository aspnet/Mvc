// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Security;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;
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
            var policy = new AuthorizationPolicy("CanViewPage").Requires("Permission", "CanViewPage");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.Policies["CanViewPage"] = policy;
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var authorizeAttribute = new AuthorizeAttribute("CanViewPage");
            var authorizationContext = GetAuthorizationContext(services =>
                services.AddInstance<IAuthorizationService>(authorizationService)
                );

            // Act
            await authorizeAttribute.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.Null(authorizationContext.Result);
        }

        [Fact]
        public async Task Invoke_EmptyClaimsShouldRejectAnonymousUser()
        {
            // Arrange
            var authorizationOptions = new AuthorizationOptions();
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var authorizeAttribute = new AuthorizeAttribute();
            var authorizationContext = GetAuthorizationContext(services =>
                services.AddInstance<IAuthorizationService>(authorizationService),
                anonymous: true
                );

            // Act
            await authorizeAttribute.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.NotNull(authorizationContext.Result);
        }

        [Fact]
        public async Task Invoke_EmptyClaimsWithAllowAnonymousAttributeShouldNotRejectAnonymousUser()
        {
            // Arrange
            var authorizationOptions = new AuthorizationOptions();
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var authorizeAttribute = new AuthorizeAttribute();
            var authorizationContext = GetAuthorizationContext(services =>
                services.AddInstance<IAuthorizationService>(authorizationService),
                anonymous: true
                );

            authorizationContext.Filters.Add(new AllowAnonymousAttribute());

            // Act
            await authorizeAttribute.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.Null(authorizationContext.Result);
        }

        [Fact]
        public async Task Invoke_EmptyClaimsShouldAuthorizeAuthenticatedUser()
        {
            // Arrange
            var authorizationOptions = new AuthorizationOptions();
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var authorizeAttribute = new AuthorizeAttribute();
            var authorizationContext = GetAuthorizationContext(services =>
                services.AddInstance<IAuthorizationService>(authorizationService)
                );

            // Act
            await authorizeAttribute.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.Null(authorizationContext.Result);
        }

        [Fact]
        public async Task Invoke_SingleValidClaimShouldSucceed()
        {
            // Arrange
            var policy = new AuthorizationPolicy("CanViewCommentOrPage").Requires("Permission", "CanViewComment", "CanViewPage");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.Policies["CanViewCommentOrPage"] = policy;
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var authorizeAttribute = new AuthorizeAttribute("CanViewCommentOrPage");
            var authorizationContext = GetAuthorizationContext(services =>
                services.AddInstance<IAuthorizationService>(authorizationService)
                );

            // Act
            await authorizeAttribute.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.Null(authorizationContext.Result);
        }

        [Fact]
        public async Task Invoke_InvalidClaimShouldFail()
        {
            // Arrange
            var policy = new AuthorizationPolicy("CanViewComment").Requires("Permission", "CanViewComment");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.Policies["CanViewComment"] = policy;
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var authorizeAttribute = new AuthorizeAttribute("CanViewComment");
            var authorizationContext = GetAuthorizationContext(services =>
                services.AddInstance<IAuthorizationService>(authorizationService)
                );

            // Act
            await authorizeAttribute.OnAuthorizationAsync(authorizationContext);

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
                .Setup(x => x.AuthorizeAsync("CanViewComment", null, null))
                .Returns(() =>
                {
                    authorizationServiceIsCalled = true;
                    return Task.FromResult(true);
                });

            var authorizeAttribute = new AuthorizeAttribute("CanViewComment");
            var authorizationContext = GetAuthorizationContext(services =>
                services.AddInstance<IAuthorizationService>(authorizationService.Object)
                );

            authorizationContext.Result = new HttpStatusCodeResult(401);

            // Act
            await authorizeAttribute.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.False(authorizationServiceIsCalled);
        }

        [Fact]
        public async Task Invoke_NullPoliciesShouldNotFail()
        {
            // Arrange
            var authorizationOptions = new AuthorizationOptions();
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var authorizeAttribute = new AuthorizeAttribute("CanViewPage");
            var authorizationContext = GetAuthorizationContext(services =>
                services.AddInstance<IAuthorizationService>(authorizationService)
                );

            // Act
            await authorizeAttribute.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.Null(authorizationContext.Result);
        }
    }
}
