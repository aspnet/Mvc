using System;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Testing;
using Xunit;
using System.Security.Claims;

namespace Microsoft.AspNet.Mvc.Core
{
    public class DefaultPermissionAuthorizationServiceTests
    {
        [Fact]
        public void Check_ShouldAllowIfClaimIsPresent()
        {
            // Arrange
            var authorizationService = new DefaultAuthorizationService();
            var user = new ClaimsPrincipal(
                new ClaimsIdentity( new Claim[] { new Claim("Permission", "CanViewPage") }, "Basic")
                );

            // Act
            var allowed = authorizationService.CheckAsync(new Claim[] { new Claim("Permission", "CanViewPage") }, user).Result;

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public void Check_ShouldAllowIfClaimIsAmongValues()
        {
            // Arrange
            var permissionService = new DefaultAuthorizationService();
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] { 
                        new Claim("Permission", "CanViewPage"), 
                        new Claim("Permission", "CanViewAnything")
                    }, 
                    "Basic")
                );

            // Act
            var allowed = permissionService.CheckAsync(new Claim[] { new Claim("Permission", "CanViewPage") }, user).Result;

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public void Check_ShouldNotAllowIfClaimTypeIsNotPresent()
        {
            // Arrange
            var authorizationService = new DefaultAuthorizationService();
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] { 
                        new Claim("SomethingElse", "CanViewPage"), 
                    },
                    "Basic")
                );

            // Act
            var allowed = authorizationService.CheckAsync(new Claim[] { new Claim("Permission", "CanViewPage") }, user).Result;

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public void Check_ShouldNotAllowIfClaimValueIsNotPresent()
        {
            // Arrange
            var authorizationService = new DefaultAuthorizationService();
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] { 
                        new Claim("Permission", "CanViewComment"), 
                    },
                    "Basic")
                );

            // Act
            var allowed = authorizationService.CheckAsync(new Claim[] { new Claim("Permission", "CanViewPage") }, user).Result;

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public void Check_ShouldNotAllowIfNoClaims()
        {
            // Arrange
            var permissionService = new DefaultAuthorizationService();
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[0],
                    "Basic")
                );

            // Act
            var allowed = permissionService.CheckAsync(new Claim[] { new Claim("Permission", "CanViewPage") }, user).Result;

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public void Check_ShouldNotAllowIfUserIsNull()
        {
            // Arrange
            var permissionService = new DefaultAuthorizationService();
            ClaimsPrincipal user = null;

            // Act
            var allowed = permissionService.CheckAsync(new Claim[] { new Claim("Permission", "CanViewPage") }, user).Result;

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public void Check_ShouldNotAllowIfUserIsNotAuthenticated()
        {
            // Arrange
            var authorizationService = new DefaultAuthorizationService();
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] { 
                        new Claim("Permission", "CanViewComment"), 
                    },
                    null)
                );

            // Act
            var allowed = authorizationService.CheckAsync(new Claim[] { new Claim("Permission", "CanViewPage") }, user).Result;

            // Assert
            Assert.False(allowed);
        }
    }
}
