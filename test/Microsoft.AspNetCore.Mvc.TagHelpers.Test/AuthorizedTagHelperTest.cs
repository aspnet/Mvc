// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.TagHelpers.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Razor.TagHelpers;
    using Moq;
    using Xunit;

    public class AuthorizedTagHelperTest
    {
        [Theory]
        [InlineData("Admin", "Admin")]
        [InlineData("admin", "Admin")]
        [InlineData("Admin", "admin")]
        [InlineData(" admin", "Admin")]
        [InlineData("admin ", "Admin")]
        [InlineData(" admin ", "Admin")]
        [InlineData("admin,somethingelse", "Admin")]
        [InlineData("admin ,somethingelse", "admin")]
        [InlineData("admin\t,somethingelse", "admin")]
        public void ShowsContentWhenAuthenticatedAndRoleMatchesForNonInvertedRoleCheck(string rolesAttribute, string userHasRoles)
        {
            ShouldShowContent(rolesAttribute, userHasRoles, isInverted: false, isAuthenticated: true);
        }

        [Theory]
        [InlineData("", "Admin")]
        [InlineData(null, "Admin")]
        [InlineData("  ", "Admin")]
        [InlineData(", ", "Admin")]
        [InlineData("   , ", "Admin")]
        [InlineData("\t,\t", "Admin")]
        [InlineData(",", "Admin")]
        [InlineData(",,", "Admin")]
        [InlineData(",,,", "Admin")]
        [InlineData(",,, ", "Admin")]
        public void ShowsContentWhenAuthenticatedAndNoRoleIsSpecifiedForNonInvertedRoleCheck(string rolesAttribute, string userHasRoles)
        {
            ShouldShowContent(rolesAttribute, userHasRoles, isInverted: false, isAuthenticated: true);
        }

        [Theory]
        [InlineData("Admin", null)]
        public void DoesNotShowContentWhenNotAuthenticatedForNonInvertedRoleCheck(string rolesAttribute, string userHasRoles)
        {
            ShouldNotShowContent(rolesAttribute, userHasRoles, isInverted: false, isAuthenticated: false);
        }

        [Theory]
        [InlineData("Admin", null)]
        public void ShowsContentWhenNotAuthenticatedForInvertedRoleCheck(string rolesAttribute, string userHasRoles)
        {
            ShouldShowContent(rolesAttribute, userHasRoles, isInverted: true, isAuthenticated: false);
        }

        [Theory]
        [InlineData(null, "Admin")]
        public void DoesNotShowContentWhenAuthenticatedForInvertedRoleCheck(string rolesAttribute, string userHasRoles)
        {
            ShouldNotShowContent(rolesAttribute, userHasRoles, isInverted: true, isAuthenticated: true);
        }


        [Theory]
        [InlineData("NotAdmin", "Admin")]
        [InlineData(" NotAdmin", "Admin")]
        [InlineData("NotAdmin ", "Admin")]
        [InlineData(" NotAdmin ", "Admin")]
        [InlineData("notadmin", "Admin")]
        [InlineData(" notadmin", "Admin")]
        [InlineData("notadmin ", "Admin")]
        [InlineData("foo,notadmin", "Admin")]
        [InlineData("foo, notadmin", "Admin")]
        [InlineData("foo,\tnotadmin", "Admin")]
        [InlineData("foo,notadmin\t", "Admin")]
        [InlineData("foo,\tnotadmin\t", "Admin")]
        public void ShowsContentWhenAuthenticatedAndNoRoleMatchesForInvertedRoleCheck(string rolesAttribute, string userHasRoles)
        {
            ShouldShowContent(rolesAttribute, userHasRoles, isInverted: true, isAuthenticated: true);
        }

        [Theory]
        [InlineData("Admin", "Admin")]
        [InlineData(" Admin", "Admin")]
        [InlineData("Admin ", "Admin")]
        [InlineData(" Admin ", "Admin")]
        [InlineData("admin", "Admin")]
        [InlineData(" admin", "Admin")]
        [InlineData("admin ", "Admin")]
        [InlineData("foo,admin", "Admin")]
        [InlineData("foo, admin", "Admin")]
        [InlineData("foo,\tadmin", "Admin")]
        [InlineData("foo,admin\t", "Admin")]
        [InlineData("foo,\tadmin\t", "Admin")]
        public void DoesNotShowContentWhenAuthenticatedAndRoleMatchesForInvertedRoleCheck(string rolesAttribute, string userHasRoles)
        {
            ShouldNotShowContent(rolesAttribute, userHasRoles, isInverted: true, isAuthenticated: true);
        }

        [Theory]
        [InlineData()]
        public void ShouldNotFailForNullUser()
        {
            // Arrange
            var content = "content";
            var context = MakeTagHelperContext(
                attributes: new TagHelperAttributeList { { "roles", string.Empty }, { "is-inverted", false } });
            var output = MakeTagHelperOutput("authorized", childContent: content);
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.SetupGet(x => x.HttpContext.User).Returns((ClaimsPrincipal)null);

            // Act
            var helper = new AuthorizedTagHelper(httpContextAccessor.Object)
            {
                Roles = string.Empty,
                IsInverted = false
            };

            helper.Process(context, output);

            // Assert
            // Not throwing is good enough
        }

        private void ShouldShowContent(string rolesAttribute, string userHasRoles, bool isInverted, bool isAuthenticated)
        {
            // Arrange
            var content = "content";
            var context = MakeTagHelperContext(
                attributes: new TagHelperAttributeList { { "roles", rolesAttribute }, { "is-inverted", isInverted } });
            var output = MakeTagHelperOutput("authorized", childContent: content);
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(h => h.HttpContext.User.IsInRole(It.IsAny<string>())).Returns<string>(role => userHasRoles?.Split(',').Any(userRole => string.Compare(role, userRole, StringComparison.OrdinalIgnoreCase) == 0) ?? false);
            httpContextAccessor.SetupGet(h => h.HttpContext.User.Identity.IsAuthenticated).Returns(isAuthenticated);

            // Act
            var helper = new AuthorizedTagHelper(httpContextAccessor.Object)
            {
                Roles = rolesAttribute,
                IsInverted = isInverted
            };
            helper.Process(context, output);

            // Assert
            Assert.Null(output.TagName);
            Assert.False(output.IsContentModified);
        }

        private void ShouldNotShowContent(string rolesAttribute, string userHasRoles, bool isInverted, bool isAuthenticated)
        {
            // Arrange
            var content = "content";
            var context = MakeTagHelperContext(
                attributes: new TagHelperAttributeList { { "roles", rolesAttribute }, { "is-inverted", isInverted } });
            var output = MakeTagHelperOutput("authorized", childContent: content);
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(h => h.HttpContext.User.IsInRole(It.IsAny<string>())).Returns<string>(role => userHasRoles?.Split(',').Any(userRole => string.Compare(role, userRole, StringComparison.OrdinalIgnoreCase) == 0) ?? false);
            httpContextAccessor.SetupGet(h => h.HttpContext.User.Identity.IsAuthenticated).Returns(isAuthenticated);

            // Act
            var helper = new AuthorizedTagHelper(httpContextAccessor.Object)
            {
                Roles = rolesAttribute,
                IsInverted = isInverted
            };
            helper.Process(context, output);

            // Assert
            Assert.Null(output.TagName);
            Assert.Empty(output.PreContent.GetContent());
            Assert.True(output.Content.GetContent().Length == 0);
            Assert.Empty(output.PostContent.GetContent());
            Assert.True(output.IsContentModified);
        }

        private TagHelperContext MakeTagHelperContext(TagHelperAttributeList attributes = null)
        {
            attributes = attributes ?? new TagHelperAttributeList();

            return new TagHelperContext(
                attributes,
                items: new Dictionary<object, object>(),
                uniqueId: Guid.NewGuid().ToString("N"));
        }

        private TagHelperOutput MakeTagHelperOutput(
            string tagName,
            TagHelperAttributeList attributes = null,
            string childContent = null)
        {
            attributes = attributes ?? new TagHelperAttributeList();

            return new TagHelperOutput(
                tagName,
                attributes,
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent(childContent);
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
        }
    }
}