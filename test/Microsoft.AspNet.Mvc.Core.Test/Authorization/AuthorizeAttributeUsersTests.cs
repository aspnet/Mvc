using Microsoft.AspNet.Mvc.Core.Filters;
using Microsoft.AspNet.Security.Authorization;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class AuthorizeAttributeUsersTests : AuthorizeAttributeTestsBase
    {
        protected readonly Func<Task> noop = () => Task.FromResult(true);
        protected readonly IEnumerable<IAuthorizationPolicy> EmptyPolicies = new IAuthorizationPolicy[0];

        [Fact]
        public async void Invoke_ValidUserShouldSucceed()
        {
            // Arrange
            var permissionService = new DefaultAuthorizationService(EmptyPolicies);
            var authorizeAttribute = new AuthorizeAttribute { Users = "John" };
            var authorizationFilterContext = GetAuthorizationFilterContext(sc => 
                sc.AddInstance<IAuthorizationService>(permissionService)
                );

            // Act
            await authorizeAttribute.Invoke(authorizationFilterContext, noop);

            // Assert
            Assert.False(authorizationFilterContext.HasFailed);
        }

        [Fact]
        public async void Invoke_SingleValidUserShouldSucceed()
        {
            // Arrange
            var permissionService = new DefaultAuthorizationService(EmptyPolicies);
            var authorizeAttribute = new AuthorizeAttribute { Users = "John, Jane" };
            var authorizationFilterContext = GetAuthorizationFilterContext(sc => 
                sc.AddInstance<IAuthorizationService>(permissionService)
                );

            // Act
            await authorizeAttribute.Invoke(authorizationFilterContext, noop);

            // Assert
            Assert.False(authorizationFilterContext.HasFailed);
        }

        [Fact]
        public async void Invoke_InvalidUserShouldFail()
        {
            // Arrange
            var permissionService = new DefaultAuthorizationService(EmptyPolicies);
            var authorizeAttribute = new AuthorizeAttribute { Users = "Jane" };

            var authorizationFilterContext = GetAuthorizationFilterContext(sc =>
                sc.AddInstance<IAuthorizationService>(permissionService)
                );

            // Act
            await authorizeAttribute.Invoke(authorizationFilterContext, noop);

            // Assert
            Assert.True(authorizationFilterContext.HasFailed);
        }

        [Fact]
        public async void Invoke_ValidUserCallsNextFilter()
        {
            // Arrange
            var permissionService = new DefaultAuthorizationService(EmptyPolicies);
            var authorizeAttribute = new AuthorizeAttribute { Users = "John" };
            var authorizationFilterContext = GetAuthorizationFilterContext(sc =>
                sc.AddInstance<IAuthorizationService>(permissionService)
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
        public async void Invoke_InvalidUserCallsNextFilter()
        {
            // Arrange
            var permissionService = new DefaultAuthorizationService(EmptyPolicies);
            var authorizeAttribute = new AuthorizeAttribute { Users = "Jane" };
            var authorizationFilterContext = GetAuthorizationFilterContext(sc =>
                sc.AddInstance<IAuthorizationService>(permissionService)
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
            var permissionService = new DefaultAuthorizationService(EmptyPolicies);
            var authorizeAttribute = new AuthorizeAttribute { Users = "Jane" };
            var authorizationFilterContext = GetAuthorizationFilterContext(sc =>
                sc.AddInstance<IAuthorizationService>(permissionService)
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
            bool permissionServiceIsCalled = false;
            var permissionService = new Mock<IAuthorizationService>()
                .Setup(x => x.CheckAsync(null, null))
                .Returns(() =>
                {
                    permissionServiceIsCalled = true;
                    return Task.FromResult(true);
                });

            var authorizeAttribute = new AuthorizeAttribute { Users = "Jane" };
            var authorizationFilterContext = GetAuthorizationFilterContext(sc =>
                sc.AddInstance<IAuthorizationService>(permissionService)
                );
            authorizationFilterContext.Fail();

            // Act
            await authorizeAttribute.Invoke(authorizationFilterContext, noop);

            // Assert
            Assert.False(permissionServiceIsCalled);
        }
    }
}
