using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.PipelineCore;
using Microsoft.AspNet.Routing;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class HttpStatusCodeResultTests
    {
        [Fact]
        public void HttpStatusCodeResult_InitializesStatusCodeAndStatusDescription()
        {
            // Arrange & act
            var result = new HttpStatusCodeResult(402, "Bad request");

            // Assert
            Assert.Equal(402, result.StatusCode);
            Assert.Equal("Bad request", result.StatusDescription);
        }

        [Fact]
        public async Task HttpStatusCodeResult_ExecuteResultAsyncSetsResponseStatusCode()
        {
            // Arrange
            var result = new HttpStatusCodeResult(404);

            var httpContext = new DefaultHttpContext();

            var routeData = new RouteData();

            var actionDescriptor = new ActionDescriptor();

            var context = new ActionContext(httpContext, routeData, actionDescriptor);

            // Act
            await result.ExecuteResultAsync(context);

            // Assert
            Assert.Equal(404, httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task HttpStatusCodeResult_ExecuteResultAsyncWritesTheStatusDescription()
        {
            // Arrange
            var result = new HttpStatusCodeResult(404, "This is the status description");

            var responseStream = new MemoryStream();

            var httpContext = new DefaultHttpContext();

            var httpResponse = httpContext.Response;
            httpResponse.Body = responseStream;

            var routeData = new RouteData();

            var actionDescriptor = new ActionDescriptor();

            var context = new ActionContext(httpContext, routeData, actionDescriptor);

            // Act
            await result.ExecuteResultAsync(context);

            // Assert
            Assert.Equal(404, httpResponse.StatusCode);

            Assert.Equal("text/plain; charset=utf-8", httpResponse.ContentType);

            responseStream.Seek(0, SeekOrigin.Begin);
            var responseText = new StreamReader(responseStream).ReadToEnd();
            Assert.Equal("This is the status description", responseText);
        }
    }
}