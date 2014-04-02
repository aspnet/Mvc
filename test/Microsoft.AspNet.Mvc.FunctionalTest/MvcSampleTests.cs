using System.Threading.Tasks;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.AspNet.Mvc.FunctionalTest.Testing;
using Microsoft.AspNet.RequestContainer;
using Microsoft.AspNet.Routing;
using MvcSample.Web;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTest
{
    public class MvcSampleTests
    {
        [Theory]
        [InlineData("http://localhost/Link/", "Home/Details")]
        [InlineData("http://localhost/Link/Link1", "")]
        [InlineData("http://localhost/Link/Link2", "Link/Link2")]
        [InlineData("http://localhost/Link/About", "Link/About")]
        public async Task LinkController_Works(string url, string result)
        {
            // Arrange
            var testServer = MvcTestServer.Create<Startup>();
            var request = new TestHttpRequest("GET", url);

            // Act
            var response = await testServer.SendRequest(request);

            // Assert
            Assert.Equal(result, response.ReadAsString());
        }

        [Theory]
        [InlineData("http://localhost/Overload/", "Get()")]
        [InlineData("http://localhost/Overload?id=1", "Get(id)")]
        [InlineData("http://localhost/Overload?id=1&name=Ryan", "Get(id, name)")]
        public async Task OverloadController_Works(string url, string responseContent)
        {
            // Arrange
            var testServer = MvcTestServer.Create<Startup>();
            var request = new TestHttpRequest("GET", url);

            // Act
            var response = await testServer.SendRequest(request);

            // Assert
            Assert.Equal(responseContent, response.ReadAsString());
        }

        [Fact]
        public async Task SimplePocoController_Works()
        {
            // Arrange
            var testServer = MvcTestServer.Create<Startup>();
            var request = new TestHttpRequest("GET", "http://localhost:12345/SimplePoco/");

            // Act
            var response = await testServer.SendRequest(request);

            // Assert
            Assert.Equal("Hello world", response.ReadAsString());
        }

        [Fact]
        public async Task SimpleRest_Works()
        {
            // Arrange
            var testServer = MvcTestServer.Create<Startup>();
            var request = new TestHttpRequest("GET", "http://localhost:12345/SimpleRest/");

            // Act
            var response = await testServer.SendRequest(request);

            // Assert
            Assert.Equal("Get method", response.ReadAsString());
        }

        [Theory]
        [InlineData("http://localhost:12345/Home/Something", "Hello World From Content")]
        [InlineData("http://localhost:12345/Home/Hello", "Hello World")]
        [InlineData("http://localhost:12345/Home/Raw", "Hello World raw")]
        [InlineData("http://localhost:12345/Home/User", "{\"Name\":\"My name\",\"Address\":\"My address\",\"Age\":0}")]
        public async Task HomeController_Works_NonViewActions(string url, string result)
        {
            // Arrange
            var testServer = MvcTestServer.Create<Startup>();
            var request = new TestHttpRequest("GET", url);

            // Act
            var response = await testServer.SendRequest(request);

            // Assert
            Assert.Equal(result, response.ReadAsString());
        }

        [Theory]
        [InlineData("http://localhost:12345/Home2/", "Hello World: my namespace is MvcSample.Web.RandomNameSpace")]
        [InlineData("http://localhost:12345/Home2/Something", "Hello World From Content")]
        [InlineData("http://localhost:12345/Home2/Hello", "Hello World")]
        [InlineData("http://localhost:12345/Home2/Raw", "Hello World raw")]
        [InlineData("http://localhost:12345/Home2/UserJson", "{\"Name\":\"User Name\",\"Address\":\"Home Address\",\"Age\":0}")]
        [InlineData("http://localhost:12345/Home2/User", "{\"Name\":\"User Name\",\"Address\":\"Home Address\",\"Age\":0}")]
        public async Task Home2Controller_Works(string url, string result)
        {
            // Arrange
            var testServer = MvcTestServer.Create<Startup>();
            var request = new TestHttpRequest("GET", url);

            // Act
            var response = await testServer.SendRequest(request);

            // Assert
            Assert.Equal(result, response.ReadAsString());
        }
    }
}
