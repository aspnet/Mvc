using System;
using Xunit;

namespace Microsoft.AspNet.Mvc.Test
{
    public class HttpBadRequestResultTest
    {
        [Fact]
        public void HttpBadRequestResult_InitializesStatusCode()
        {
            // Arrange & act
            var notFound = new HttpBadRequestResult();

            // Assert
            Assert.Equal(400, notFound.StatusCode);
        }
    }
}