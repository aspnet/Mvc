using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class CustomUrlHelperTests
    {
        private readonly IServiceProvider _services = TestHelper.CreateServices("UrlHelperWebSite",
                                                       applicationPath: Path.Combine("..", "WebSites"));
        private readonly Action<IApplicationBuilder> _app = new UrlHelperWebSite.Startup().Configure;
        private const string _cdnServerBaseUrl = "http://testcdn.com";

        [Theory]
        [InlineData("http://localhost/Home/UrlContent")]
        [InlineData("http://localhost/SimplePoco/UrlContent")]
        public async Task CustomUrlHelperGeneratesUrlFromController(string url)
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(_cdnServerBaseUrl + "/bootstrap.min.css", await response.Content.ReadAsStringAsync(),
                        ignoreCase: false);
        }

        [Theory]
        [InlineData("http://localhost/Home/Index")]
        [InlineData("http://localhost/SimplePoco/Index")]
        public async Task CustomUrlHelperGeneratesUrlFromView(string url)
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(_cdnServerBaseUrl + "/bootstrap.min.css",
                            await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("http://localhost/Home/LinkByUrlRouteUrl", "/api/simplepoco/10")]
        [InlineData("http://localhost/Home/LinkByUrlAction", "/home/urlcontent")]
        public async Task LowercaseUrlsLinkGeneration(string url, string expectedLink)
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedLink, await response.Content.ReadAsStringAsync(),
                        ignoreCase: false);
        }
    }
}