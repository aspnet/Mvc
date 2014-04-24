﻿using Moq;
using System;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class ClaimUidExtractorTest
    {
        [Fact]
        public void ExtractClaimUid_NullIdentity()
        {
            // Arrange
            IClaimUidExtractor extractor = new DefaultClaimUidExtractor();

            // Act
            var retVal = extractor.ExtractClaimUid(null);

            // Assert
            Assert.Null(retVal);
        }

        [Fact]
        public void ExtractClaimUid_Unauthenticated()
        {
            // Arrange
            IClaimUidExtractor extractor = new DefaultClaimUidExtractor();

            var mockIdentity = new Mock<ClaimsIdentity>();
            mockIdentity.Setup(o => o.IsAuthenticated)
                        .Returns(false);

            // Act
            var retVal = extractor.ExtractClaimUid(mockIdentity.Object);

            // Assert
            Assert.Null(retVal);
        }

        [Fact]
        public void ExtractClaimUid_ClaimsIdentity()
        {
            // Arrange
            var mockIdentity = new Mock<ClaimsIdentity>();
            mockIdentity.Setup(o => o.IsAuthenticated)
                        .Returns(true);

            IClaimUidExtractor extractor = new DefaultClaimUidExtractor();

            // Act
            var retVal = extractor.ExtractClaimUid(mockIdentity.Object);

            // Assert
            Assert.NotNull(retVal);
            Assert.Equal("47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=", retVal);
        }

        [Fact]
        public void DefaultUniqueClaimTypes_NotPresent_SerializesAllClaimTypes()
        {
            var identity = new MockClaimsIdentity();
            identity.AddClaim(ClaimTypes.Email, "someone@antifrogery.com");
            identity.AddClaim(ClaimTypes.GivenName, "some");
            identity.AddClaim(ClaimTypes.Surname, "one");
            identity.AddClaim(ClaimTypes.NameIdentifier, String.Empty);

            // Arrange
            var claimsIdentity = (ClaimsIdentity)identity;

            // Act
            var identiferParameters = DefaultClaimUidExtractor.GetUniqueIdentifierParameters(claimsIdentity)
                                                              .ToArray();
            var claims = claimsIdentity.Claims.ToList();
            claims.Sort((a, b) => string.Compare(a.Type, b.Type, StringComparison.Ordinal));

            // Assert
            int index = 0;
            foreach (var claim in claims)
            {
                Assert.True(String.Equals(identiferParameters[index++], claim.Type, StringComparison.Ordinal));
                Assert.True(String.Equals(identiferParameters[index++], claim.Value, StringComparison.Ordinal));
            }
        }

        [Fact]
        public void DefaultUniqueClaimTypes_Present()
        {
            // Arrange
            var identity = new MockClaimsIdentity();
            identity.AddClaim("fooClaim", "fooClaimValue");
            identity.AddClaim(ClaimTypes.NameIdentifier, "nameIdentifierValue");

            // Act
            var retVal = DefaultClaimUidExtractor.GetUniqueIdentifierParameters(identity);

            // Assert
            Assert.Equal(new string[]
            {
                ClaimTypes.NameIdentifier,
                "nameIdentifierValue",
            }, retVal);
        }
    }
}