﻿// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Linq;
using System.Security.Claims;
using Moq;
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
            var claimUid = extractor.ExtractClaimUid(null);

            // Assert
            Assert.Null(claimUid);
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
            var claimUid = extractor.ExtractClaimUid(mockIdentity.Object);

            // Assert
            Assert.Null(claimUid);
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
            var claimUid = extractor.ExtractClaimUid(mockIdentity.Object);

            // Assert
            Assert.NotNull(claimUid);
            Assert.Equal("47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=", claimUid);
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
            var uniqueIdentifierParameters = DefaultClaimUidExtractor.GetUniqueIdentifierParameters(identity);

            // Assert
            Assert.Equal(new string[]
            {
                ClaimTypes.NameIdentifier,
                "nameIdentifierValue",
            }, uniqueIdentifierParameters);
        }
    }
}