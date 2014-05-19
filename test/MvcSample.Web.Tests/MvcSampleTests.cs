// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Infrastructure;
using Xunit;

namespace MvcSample.Web.Tests
{
    public class MvcSampleTests
    {
        private readonly IServiceProvider _provider;
        private readonly Action<IBuilder> _app = new Startup().Configure;

        public MvcSampleTests()
        {
            var originalProvider = CallContextServiceLocator.Locator.ServiceProvider;

            IApplicationEnvironment appEnvironment = originalProvider.GetService<IApplicationEnvironment>();

            // When an application executes in a regular context, the application base path points to the root
            // directory where the application is located, for example MvcSample.Web. However, when executing
            // an aplication as part of a test, the ApplicationBasePath of the IApplicationEnvironment points
            // to the root folder of the test project.
            // To compensate for this, we need to calculate the original path and override the application
            // environment value so that components like the view engine work properly in the context of the
            // test.
            string appBasePath = CalculateApplicationBasePath(appEnvironment);
            _provider = new ServiceCollection()
                .AddInstance(typeof(IApplicationEnvironment), new MockApplicationEnvironment(appEnvironment, appBasePath))
                .BuildServiceProvider(originalProvider);
        }

        [Theory]
        [InlineData("http://localhost/Link/", "http://localhost/Home/Create#CoolBeans!")]
        [InlineData("http://localhost/Link/Link1", "/")]
        [InlineData("http://localhost/Link/Link2", "/Link/Link2")]
        [InlineData("http://localhost/Link/About", "/Link/About")]
        public async Task LinkController_Works(string url, string result)
        {
            // Arrange
            var testServer = TestServer.Create(_provider, _app);

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
            var testServer = TestServer.Create(_provider, _app);
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
            var testServer = TestServer.Create(_provider, _app);
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
            var testServer = TestServer.Create(_provider, _app);
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
        [InlineData("http://localhost:12345/Home/CreateUser", "{\"Name\":\"My name\",\"Address\":\"My address\",\"Age\":13,\"GPA\":13.37,\"Dependent\":{\"Name\":\"Dependents name\",\"Address\":\"Dependents address\",\"Age\":0,\"GPA\":0.0,\"Dependent\":null,\"Alive\":false,\"Password\":null,\"Profession\":null,\"About\":null,\"Log\":null,\"OwnedAddresses\":[],\"ParentsAges\":[]},\"Alive\":true,\"Password\":\"Secure string\",\"Profession\":\"Software Engineer\",\"About\":\"I like playing Football\",\"Log\":null,\"OwnedAddresses\":[],\"ParentsAges\":[]}")]
        public async Task HomeController_Works_NonViewActions(string url, string result)
        {
            // Arrange
            var testServer = TestServer.Create(_provider, _app);
            var client = testServer.Handler;

            // Act
            var response = await client.GetStringAsync(url);

            // Assert
            Assert.Equal(result, response);
        }

        [Theory]
        [InlineData("http://localhost:12345/Home/Index")]
        public async Task HomeController_Works_ViewActions(string url)
        {
            // Arrange
            var testServer = TestServer.Create(_provider, _app);
            var client = testServer.Handler;

            // Act
            var response = await client.GetAsync(url);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("text/html; charset=utf-8", response.ContentType);
            HomePage page = new HomePage(response.Body);
            page.ValidateContent();
        }

        [Theory]
        [InlineData("http://localhost:12345/Home2/", "Hello World: my namespace is MvcSample.Web.RandomNameSpace")]
        [InlineData("http://localhost:12345/Home2/Something", "Hello World From Content")]
        [InlineData("http://localhost:12345/Home2/Hello", "Hello World")]
        [InlineData("http://localhost:12345/Home2/Raw", "Hello World raw")]
        [InlineData("http://localhost:12345/Home2/UserJson", "{\"Name\":\"User Name\",\"Address\":\"Home Address\",\"Age\":0,\"GPA\":0.0,\"Dependent\":null,\"Alive\":false,\"Password\":null,\"Profession\":null,\"About\":null,\"Log\":null,\"OwnedAddresses\":[],\"ParentsAges\":[]}")]
        [InlineData("http://localhost:12345/Home2/User", "{\"Name\":\"User Name\",\"Address\":\"Home Address\",\"Age\":0,\"GPA\":0.0,\"Dependent\":null,\"Alive\":false,\"Password\":null,\"Profession\":null,\"About\":null,\"Log\":null,\"OwnedAddresses\":[],\"ParentsAges\":[]}")]
        public async Task Home2Controller_Works(string url, string result)
        {
            // Arrange
            var testServer = TestServer.Create(_provider, _app);
            var client = testServer.Handler;

            // Act
            var response = await client.GetStringAsync(url);

            // Assert
            Assert.Equal(result, response);
        }

        // Calculate the path relative to the current application base path.
        private static string CalculateApplicationBasePath(IApplicationEnvironment appEnvironment)
        {
            // Mvc/test/MvcSample.Test
            var appBase = appEnvironment.ApplicationBasePath;

            // Mvc/test
            var test = Path.GetDirectoryName(appBase);

            // Mvc
            var webFxPath = Path.GetDirectoryName(test);

            // Mvc/Samples/MvcSample.Web
            return Path.Combine(webFxPath, "Samples\\MvcSample.Web");
        }

        // Represents an application environment that overrides the base path of the original
        // application environment in order to make it point to the folder of the original web
        // aplication so that components like ViewEngines can find views as if they were executing
        // in a regular context.
        private class MockApplicationEnvironment : IApplicationEnvironment
        {
            private readonly IApplicationEnvironment _originalAppEnvironment;
            private readonly string _applicationBasePath;

            public MockApplicationEnvironment(IApplicationEnvironment originalAppEnvironment, string appBasePath)
            {
                _originalAppEnvironment = originalAppEnvironment;
                _applicationBasePath = appBasePath;
            }

            public string ApplicationName
            {
                get { return _originalAppEnvironment.ApplicationName; }
            }

            public string Version
            {
                get { return _originalAppEnvironment.Version; }
            }

            public string ApplicationBasePath
            {
                get { return _applicationBasePath; }
            }

            public FrameworkName TargetFramework
            {
                get { return _originalAppEnvironment.TargetFramework; }
            }
        }

        // Represents an abstraction over different elements of the rendered page we are interested to check for correctness.
        private class HomePage
        {
            HtmlDocument _document;
            public HomePage(Stream responseBody)
            {
                _document = new HtmlDocument();
                _document.Load(responseBody);
            }

            public HtmlNode NavigationHomeItem
            {
                get
                {
                    return _document.GetElementbyId("test-link-home");
                }
            }

            public HtmlNode HeaderStyleItem
            {
                get
                {
                    return _document.GetElementbyId("test-style-header");
                }
            }

            public HtmlNode FooterScriptItem
            {
                get
                {
                    return _document.GetElementbyId("test-script-footer");
                }
            }

            public HtmlNode AsyncValueItem
            {
                get
                {
                    return _document.GetElementbyId("qux");
                }
            }

            public HtmlNode AsyncPartialItem
            {
                get
                {
                    return _document.GetElementbyId("Bob");
                }
            }

            public HtmlNode LabelFor
            {
                get
                {
                    return _document.GetElementbyId("test-labelfor");
                }
            }

            public HtmlNode DisplayNameFor
            {
                get
                {
                    return _document.GetElementbyId("test-displayname");
                }
            }

            public HtmlNode ValueFor
            {
                get
                {
                    return _document.GetElementbyId("test-valuefor");
                }
            }
            public HtmlNode NameFor
            {
                get
                {
                    return _document.GetElementbyId("test-namefor");
                }
            }

            public HtmlNode Tags
            {
                get
                {
                    return _document.GetElementbyId("test-components");
                }
            }

            // Checks for the presence of multiple elements inside the document contained in the response stream to 
            // ensure that no component failed silently.
            internal void ValidateContent()
            {
                // Check layout elements can be rendered
                Assert.Equal("Home", NavigationHomeItem.InnerText);
                Assert.Equal("text/css", HeaderStyleItem.GetAttributeValue("type", ""));
                Assert.Equal("//ajax.aspnetcdn.com/ajax/jQuery/jquery-2.1.0.min.js", FooterScriptItem.GetAttributeValue("src", ""));

                // Check async values can be retrieved.
                Assert.Equal("This value was retrieved asynchronously: Hello World", AsyncValueItem.InnerText);

                // Check partial views can be rendered.
                Assert.Equal("Create Something!", AsyncPartialItem.InnerText);

                // Check forms can be rendered.
                Assert.Contains("ForModel", LabelFor.Descendants().Select(d => d.InnerText));
                Assert.Contains("''", DisplayNameFor.Descendants().Select(d => d.InnerText.Trim()));
                Assert.Contains("''", NameFor.Descendants().Select(d => d.InnerText.Trim()));
                Assert.Contains("'MvcSample.Web.Models.User'", ValueFor.Descendants().Select(d => d.InnerText.Trim()));

                // Check view components can be rendered.
                Assert.Contains("15 Tags", Tags.InnerText);
            }
        }
    }
}