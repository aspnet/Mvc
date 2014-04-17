using Microsoft.AspNet.Mvc.Core.Filters;
using Microsoft.AspNet.Security.Authorization;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class AuthorizeAttributeTests : AuthorizeAttributeTestsBase
    {
        protected readonly Func<Task> noop = () => Task.FromResult(true);

        [Fact]
        public async void Invoke_ValidClaimShouldNotFail()
        {
            // Arrange
            var authorizationService = new DefaultAuthorizationService();
            var authorizeAttribute = new AuthorizeAttribute("Permission", "CanViewPage");
            var authorizationFilterContext = GetAuthorizationFilterContext(sc => 
                sc.AddInstance<IAuthorizationService>(authorizationService)
                );

            // Act
            await authorizeAttribute.Invoke(authorizationFilterContext, noop);

            // Assert
            Assert.False(authorizationFilterContext.HasFailed);
        }

        [Fact]
        public async void Invoke_SingleValidClaimShouldSucceed()
        {
            // Arrange
            var authorizationService = new DefaultAuthorizationService();
            var authorizeAttribute = new AuthorizeAttribute("Permission", "CanViewComment", "CanViewPage");
            var authorizationFilterContext = GetAuthorizationFilterContext(sc => 
                sc.AddInstance<IAuthorizationService>(authorizationService)
                );

            // Act
            await authorizeAttribute.Invoke(authorizationFilterContext, noop);

            // Assert
            Assert.False(authorizationFilterContext.HasFailed);
        }

        [Fact]
        public async void Invoke_InvalidClaimShouldFail()
        {
            // Arrange
            var authorizationService = new DefaultAuthorizationService();
            var authorizeAttribute = new AuthorizeAttribute("Permission", "CanViewComment");
            var authorizationFilterContext = GetAuthorizationFilterContext(sc =>
                sc.AddInstance<IAuthorizationService>(authorizationService)
                );

            // Act
            await authorizeAttribute.Invoke(authorizationFilterContext, noop);

            // Assert
            Assert.True(authorizationFilterContext.HasFailed);
        }

        [Fact]
        public async void Invoke_ValidClaimCallsNextFilter()
        {
            // Arrange
            var authorizationService = new DefaultAuthorizationService();
            var authorizeAttribute = new AuthorizeAttribute("Permission", "CanViewPage");
            var authorizationFilterContext = GetAuthorizationFilterContext(sc =>
                sc.AddInstance<IAuthorizationService>(authorizationService)
                );

            bool nextIsCalled = false;
            Func<Task> next = () =>
            {
                nextIsCalled = true;
                return Task.FromResult(true);
            };

            // Act
            await authorizeAttribute.Invoke(authorizationFilterContext, next);

            // Assert
            Assert.True(nextIsCalled);
        }

        [Fact]
        public async void Invoke_InvalidClaimCallsNextFilter()
        {
            // Arrange
            var authorizationService = new DefaultAuthorizationService();
            var authorizeAttribute = new AuthorizeAttribute("Permission", "CanViewComment");
            var authorizationFilterContext = GetAuthorizationFilterContext(sc =>
                sc.AddInstance<IAuthorizationService>(authorizationService)
                );

            bool nextIsCalled = false;
            Func<Task> next = () =>
            {
                nextIsCalled = true;
                return Task.FromResult(true);
            };

            // Act
            await authorizeAttribute.Invoke(authorizationFilterContext, next);

            // Assert
            Assert.True(nextIsCalled);
        }

        [Fact]
        public async void Invoke_FailedContextStillCallsNextFilter()
        {
            // Arrange
            var authorizationService = new DefaultAuthorizationService();
            var authorizeAttribute = new AuthorizeAttribute("Permission", "CanViewComment");
            var authorizationFilterContext = GetAuthorizationFilterContext(sc =>
                sc.AddInstance<IAuthorizationService>(authorizationService)
                );
            authorizationFilterContext.Fail();

            bool nextIsCalled = false;
            Func<Task> next = () =>
            {
                nextIsCalled = true;
                return Task.FromResult(true);
            };

            // Act
            await authorizeAttribute.Invoke(authorizationFilterContext, next);

            // Assert
            Assert.True(nextIsCalled);
        }

        [Fact]
        public async void Invoke_FailedContextShouldNotCheckPermission()
        {
            // Arrange
            bool authorizationServiceIsCalled = false;
            var authorizationService = new Mock<IAuthorizationService>()
                .Setup(x => x.CheckAsync(null, null))
                .Returns(() =>
                {
                    authorizationServiceIsCalled = true;
                    return Task.FromResult(true);
                });

            var authorizeAttribute = new AuthorizeAttribute("Permission", "CanViewComment");
            var authorizationFilterContext = GetAuthorizationFilterContext(sc =>
                sc.AddInstance<IAuthorizationService>(authorizationService)
                );
            authorizationFilterContext.Fail();

            // Act
            await authorizeAttribute.Invoke(authorizationFilterContext, noop);

            // Assert
            Assert.False(authorizationServiceIsCalled);
        }
    }
}
