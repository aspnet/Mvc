// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.Abstractions;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class IdParameterTest
    {
        [Theory]
        [InlineData("id")]
        [InlineData("personId")]
        [InlineData("üId")]
        public void IsIdParameter_ParameterNameMatchesConvention_ReturnsTrue(string name)
        {
            var parameter = new ParameterDescriptor()
            {
                Name = name,
            };

            // Act
            var result = IdParameter.IsIdParameter(parameter);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("i")]
        [InlineData("Id")]
        [InlineData("iD")]
        [InlineData("persoNId")]
        [InlineData("personid")]
        [InlineData("ü Id")]
        [InlineData("ÜId")]
        public void IsIdParameter_ParameterNameDoesNotMatchConvention_ReturnsFalse(string name)
        {
            var parameter = new ParameterDescriptor()
            {
                Name = name,
            };

            // Act
            var result = IdParameter.IsIdParameter(parameter);

            // Assert
            Assert.False(result);
        }
    }
}
