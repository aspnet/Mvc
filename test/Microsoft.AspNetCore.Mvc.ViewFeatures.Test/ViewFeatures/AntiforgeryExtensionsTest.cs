﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc.TestCommon;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    /// <summary>
    /// Extension methods for <see cref="IAntiforgery"/>.
    /// </summary>
    public class AntiforgeryExtensionsTest
    {
        [Fact]
        public void GetHtml_ExistingInvalidCookieToken_GeneratesANewCookieAndAnAntiforgeryToken()
        {
            // Arrange
            var antiforgery = new Mock<IAntiforgery>(MockBehavior.Strict);
            var tokenSet = new AntiforgeryTokenSet("request-token", "cookie-token", "form-field", "header");
            antiforgery
                .Setup(a => a.GetAndStoreTokens(It.IsAny<HttpContext>()))
                .Returns(tokenSet);

            // Act
            var inputElement = antiforgery.Object.GetHtml(new DefaultHttpContext());

            // Assert
            Assert.Equal(
                @"<input name=""HtmlEncode[[form-field]]"" type=""hidden"" ""value=""HtmlEncode[[request-token]]"" />",
                HtmlContentUtilities.HtmlContentToString(inputElement));
        }
    }
}
