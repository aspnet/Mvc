using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class HttpNotFoundResultTests
    {
        [Fact]
        public void HttpNotFoundResult_InitializesStatusCode()
        {
            // Arrange & act
            var notFound = new HttpNotFoundResult();

            // Assert
            Assert.Equal(404, notFound.StatusCode);
            Assert.Null(notFound.StatusDescription);
        }

        [Fact]
        public void HttpNotFoundResult_InitializesStatusCodeAndStatusDescription()
        {
            // Arrange & act
            var notFound = new HttpNotFoundResult("Not found");

            // Assert
            Assert.Equal(404, notFound.StatusCode);
            Assert.Equal("Not found", notFound.StatusDescription);
        }
    }
}