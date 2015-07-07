// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.WebUtilities;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class BadRequestResultTests
    {
        [Fact]
        public void BadRequestResult_InitializesStatusCode()
        {
            // Arrange & act
            var badRequest = new BadRequestResult();

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        }
    }
}