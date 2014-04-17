using System;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.TestHost;
using Microsoft.Net.Runtime;
using Xunit;
using System.Collections.Generic;

namespace MvcSample.Web.Tests
{
    public class MvcSampleTests
    {
        private readonly IServiceProvider _provider;

        public MvcSampleTests()
        {
            _provider = new ServiceCollection()
                .AddSingleton<IApplicationEnvironment, MockApplicationEnvironment>()
                .BuildServiceProvider();
        }

        Action<IBuilder> app = app =>
            {
                var startup = new Startup();
                var mockAssemblyProvider = new ControllerAssemblyProvider(typeof(Startup).GetTypeInfo().Assembly);

                var services = startup.CreateServices();
                services.AddInstance<IControllerAssemblyProvider>(mockAssemblyProvider);
                startup.ConfigurationCore(app, services.BuildServiceProvider(app.ServiceProvider));
            };

        [Theory]
        [InlineData("http://localhost/Link/", "http://localhost/?action=Details#CoolBeans!")]
        [InlineData("http://localhost/Link/Link1", "/")]
        [InlineData("http://localhost/Link/Link2", "/Link/Link2")]
        [InlineData("http://localhost/Link/About", "/Link/About")]
        public async Task LinkController_Works(string url, string result)
        {
            // Arrange
            var testServer = TestServer.Create(_provider, app);

            // When we have HttpClient we'll only need to substitute this line for new HttpClient(testServer.Handler)
            // the rest of the API will be equivalent.
            var client = testServer.Handler;

            // Act
            var response = await client.GetStringAsync(url);

            // Assert
            Assert.Equal(result, response);
        }

        [Theory]
        [InlineData("http://localhost/Overload/", "Get()")]
        [InlineData("http://localhost/Overload?id=1", "Get(id)")]
        [InlineData("http://localhost/Overload?id=1&name=Ryan", "Get(id, name)")]
        public async Task OverloadController_Works(string url, string responseContent)
        {
            // Arrange
            var testServer = TestServer.Create(_provider, app);
            var client = testServer.Handler;

            // Act
            var response = await client.GetStringAsync(url);

            // Assert
            Assert.Equal(responseContent, response);
        }

        [Fact]
        public async Task SimplePocoController_Works()
        {
            // Arrange
            var testServer = TestServer.Create(_provider, app);
            var client = testServer.Handler;

            // Act
            var response = await client.GetStringAsync("http://localhost:12345/SimplePoco/");

            // Assert
            Assert.Equal("Hello world", response);
        }

        [Fact]
        public async Task SimpleRest_Works()
        {
            // Arrange
            var testServer = TestServer.Create(_provider, app);
            var client = testServer.Handler;

            // Act
            var response = await client.GetStringAsync("http://localhost:12345/SimpleRest/");

            // Assert
            Assert.Equal("Get method", response);
        }

        [Theory]
        [InlineData("http://localhost:12345/Home/Something", "Hello World From Content")]
        [InlineData("http://localhost:12345/Home/Hello", "Hello World")]
        [InlineData("http://localhost:12345/Home/Raw", "Hello World raw")]
        [InlineData("http://localhost:12345/Home/User", @"{""Name"":""My name"",""Address"":""My address"",""Age"":13,""GPA"":13.37,""Dependent"":{""Name"":""Dependents name"",""Address"":""Dependents address"",""Age"":0,""GPA"":0.0,""Dependent"":null,""Alive"":false,""Password"":null},""Alive"":true,""Password"":""Secure string""}")]
        public async Task HomeController_Works_NonViewActions(string url, string result)
        {
            // Arrange
            var testServer = TestServer.Create(_provider, app);
            var client = testServer.Handler;

            // Act
            var response = await client.GetStringAsync(url);

            // Assert
            Assert.Equal(result, response);
        }

        [Theory]
        [InlineData("http://localhost:12345/Home2/", "Hello World: my namespace is MvcSample.Web.RandomNameSpace")]
        [InlineData("http://localhost:12345/Home2/Something", "Hello World From Content")]
        [InlineData("http://localhost:12345/Home2/Hello", "Hello World")]
        [InlineData("http://localhost:12345/Home2/Raw", "Hello World raw")]
        [InlineData("http://localhost:12345/Home2/UserJson", @"{""Name"":""User Name"",""Address"":""Home Address"",""Age"":0,""GPA"":0.0,""Dependent"":null,""Alive"":false,""Password"":null}")]
        [InlineData("http://localhost:12345/Home2/User", @"{""Name"":""User Name"",""Address"":""Home Address"",""Age"":0,""GPA"":0.0,""Dependent"":null,""Alive"":false,""Password"":null}")]
        public async Task Home2Controller_Works(string url, string result)
        {
            // Arrange
            var testServer = TestServer.Create(_provider, app);
            var client = testServer.Handler;

            // Act
            var response = await client.GetStringAsync(url);

            // Assert
            Assert.Equal(result, response);
        }

        public class ControllerAssemblyProvider : IControllerAssemblyProvider
        {
            private Assembly _assembly;
            public ControllerAssemblyProvider([NotNull]Assembly assembly)
            {
                _assembly = assembly;
            }
            public IEnumerable<Assembly> CandidateAssemblies
            {
                get { yield return _assembly; }
            }
        }

        private class MockApplicationEnvironment : IApplicationEnvironment
        {
            public string ApplicationName
            {
                get { return "Test"; }
            }

            public string Version
            {
                get { return "1.0.0.0"; }
            }

            public string ApplicationBasePath
            {
                get { return "."; }
            }

            public FrameworkName TargetFramework
            {
                get { return new FrameworkName(".NETFramework", new Version(4, 5)); }
            }
        }
    }
}
