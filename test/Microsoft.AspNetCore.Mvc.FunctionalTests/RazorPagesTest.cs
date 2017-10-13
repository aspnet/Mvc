﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class RazorPagesTest : IClassFixture<MvcTestFixture<RazorPagesWebSite.Startup>>
    {
        private static readonly Assembly _resourcesAssembly = typeof(RazorPagesTest).GetTypeInfo().Assembly;

        public RazorPagesTest(MvcTestFixture<RazorPagesWebSite.Startup> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        [Fact]
        public async Task Page_SimpleForms_RenderAntiforgery()
        {
            // Arrange
            var expectedMediaType = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");
            var outputFile = "compiler/resources/RazorPagesWebSite.SimpleForms.html";
            var expectedContent = await ResourceFile.ReadResourceAsync(_resourcesAssembly, outputFile, sourceFile: false);

            // Act
            var response = await Client.GetAsync("http://localhost/SimpleForms");
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedMediaType, response.Content.Headers.ContentType);

            responseContent = responseContent.Trim();

            var forgeryToken = AntiforgeryTestHelper.RetrieveAntiforgeryToken(responseContent, "SimpleForms");
#if GENERATE_BASELINES
            // Reverse usual substitution and insert a format item into the new file content.
            responseContent = responseContent.Replace(forgeryToken, "{0}");
            ResourceFile.UpdateFile(_resourcesAssembly, outputFile, expectedContent, responseContent);
#else
            expectedContent = string.Format(expectedContent, forgeryToken);
            Assert.Equal(expectedContent.Trim(), responseContent, ignoreLineEndingDifferences: true);
#endif
        }

        [Fact]
        public async Task Page_Handler_HandlerFromQueryString()
        {
            // Arrange & Act
            var content = await Client.GetStringAsync("http://localhost/HandlerTestPage?handler=Customer");

            // Assert
            Assert.StartsWith("Method: OnGetCustomer", content.Trim());
        }

        [Fact]
        public async Task Page_Handler_HandlerRouteDataChosenOverQueryString()
        {
            // Arrange & Act
            var content = await Client.GetStringAsync("http://localhost/HandlerTestPage/Customer?handler=ViewCustomer");

            // Assert
            Assert.StartsWith("Method: OnGetCustomer", content.Trim());
        }

        [Fact]
        public async Task Page_Handler_Handler()
        {
            // Arrange & Act
            var content = await Client.GetStringAsync("http://localhost/HandlerTestPage/Customer");

            // Assert
            Assert.StartsWith("Method: OnGetCustomer", content.Trim());
        }

        [Fact]
        public async Task Page_Handler_Async()
        {
            // Arrange
            var getResponse = await Client.GetAsync("http://localhost/HandlerTestPage");
            var getResponseBody = await getResponse.Content.ReadAsStringAsync();
            var formToken = AntiforgeryTestHelper.RetrieveAntiforgeryToken(getResponseBody, "/ModelHandlerTestPage");
            var cookie = AntiforgeryTestHelper.RetrieveAntiforgeryCookie(getResponse);

            var postRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost/HandlerTestPage");
            postRequest.Headers.Add("Cookie", cookie.Key + "=" + cookie.Value);
            postRequest.Headers.Add("RequestVerificationToken", formToken);

            // Act
            var response = await Client.SendAsync(postRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.StartsWith("Method: OnPostAsync", content.Trim());
        }

        [Fact]
        public async Task Page_Handler_AsyncHandler()
        {
            // Arrange & Act
            var content = await Client.GetStringAsync("http://localhost/HandlerTestPage/ViewCustomer");

            // Assert
            Assert.StartsWith("Method: OnGetViewCustomerAsync", content.Trim());
        }

        [Fact]
        public async Task Page_Handler_ReturnTypeImplementsIActionResult()
        {
            // Arrange
            var getResponse = await Client.GetAsync("http://localhost/HandlerTestPage");
            var getResponseBody = await getResponse.Content.ReadAsStringAsync();
            var formToken = AntiforgeryTestHelper.RetrieveAntiforgeryToken(getResponseBody, "/ModelHandlerTestPage");
            var cookie = AntiforgeryTestHelper.RetrieveAntiforgeryCookie(getResponse);

            var postRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost/HandlerTestPage/CustomActionResult");
            postRequest.Headers.Add("Cookie", cookie.Key + "=" + cookie.Value);
            postRequest.Headers.Add("RequestVerificationToken", formToken);
            // Act
            var response = await Client.SendAsync(postRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("CustomActionResult", content);
        }

        [Fact]
        public async Task Page_Handler_AsyncReturnTypeImplementsIActionResult()
        {
            // Arrange & Act
            var content = await Client.GetStringAsync("http://localhost/HandlerTestPage/CustomActionResult");

            // Assert
            Assert.Equal("CustomActionResult", content);
        }

        [Fact]
        public async Task PageModel_Handler_Handler()
        {
            // Arrange & Act
            var content = await Client.GetStringAsync("http://localhost/ModelHandlerTestPage/Customer");

            // Assert
            Assert.StartsWith("Method: OnGetCustomer", content.Trim());
        }

        [Fact]
        public async Task PageModel_Handler_Async()
        {
            // Arrange
            var getResponse = await Client.GetAsync("http://localhost/ModelHandlerTestPage");
            var getResponseBody = await getResponse.Content.ReadAsStringAsync();
            var formToken = AntiforgeryTestHelper.RetrieveAntiforgeryToken(getResponseBody, "/ModelHandlerTestPage");
            var cookie = AntiforgeryTestHelper.RetrieveAntiforgeryCookie(getResponse);

            var postRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost/ModelHandlerTestPage");
            postRequest.Headers.Add("Cookie", cookie.Key + "=" + cookie.Value);
            postRequest.Headers.Add("RequestVerificationToken", formToken);

            // Act
            var response = await Client.SendAsync(postRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.StartsWith("Method: OnPostAsync", content.Trim());
        }

        [Fact]
        public async Task PageModel_Handler_AsyncHandler()
        {
            // Arrange & Act
            var content = await Client.GetStringAsync("http://localhost/ModelHandlerTestPage/ViewCustomer");

            // Assert
            Assert.StartsWith("Method: OnGetViewCustomerAsync", content.Trim());
        }

        [Fact]
        public async Task PageModel_Handler_ReturnTypeImplementsIActionResult()
        {
            // Arrange
            var getResponse = await Client.GetAsync("http://localhost/ModelHandlerTestPage");
            var getResponseBody = await getResponse.Content.ReadAsStringAsync();
            var formToken = AntiforgeryTestHelper.RetrieveAntiforgeryToken(getResponseBody, "/ModelHandlerTestPage");
            var cookie = AntiforgeryTestHelper.RetrieveAntiforgeryCookie(getResponse);

            var postRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost/ModelHandlerTestPage/CustomActionResult");
            postRequest.Headers.Add("Cookie", cookie.Key + "=" + cookie.Value);
            postRequest.Headers.Add("RequestVerificationToken", formToken);
            // Act
            var response = await Client.SendAsync(postRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("CustomActionResult", content);
        }

        [Fact]
        public async Task PageModel_Handler_AsyncReturnTypeImplementsIActionResult()
        {
            // Arrange & Act
            var content = await Client.GetStringAsync("http://localhost/ModelHandlerTestPage/CustomActionResult");

            // Assert
            Assert.Equal("CustomActionResult", content);
        }

        [Fact]
        public async Task RouteData_StringValueOnIntProp_ExpectsNotFound()
        {
            // Arrange
            var routeRequest = new HttpRequestMessage(HttpMethod.Get, "http://localhost/RouteData/pizza");

            // Act
            var routeResponse = await Client.SendAsync(routeRequest);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, routeResponse.StatusCode);
        }

        [Fact]
        public async Task RouteData_IntProperty_IsCoerced()
        {
            // Arrange
            var routeRequest = new HttpRequestMessage(HttpMethod.Get, "http://localhost/RouteData/5");

            // Act
            var routeResponse = await Client.SendAsync(routeRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, routeResponse.StatusCode);

            var content = await routeResponse.Content.ReadAsStringAsync();
            Assert.Equal("From RouteData: 5", content.Trim());
        }

        [Fact]
        public async Task Page_SetsPath()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/PathSet");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Path: /PathSet.cshtml", content.Trim());
        }

        // Tests that RazorPage includes InvalidTagHelperIndexerAssignment which is called when the page has an indexer
        // Issue https://github.com/aspnet/Mvc/issues/5920
        [Fact]
        public async Task TagHelper_InvalidIndexerDoesNotFail()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/TagHelpers");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.StartsWith("<a href=\"/Show?id=2\">Post title</a>", content.Trim());
        }

        [Fact]
        public async Task NoPage_NotFound()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/NoPage");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task HelloWorld_CanGetContent()
        {
            // Arrange
            // Note: If the route in this test case ever changes, the negative test case
            // RazorPagesWithBasePathTest.PageOutsideBasePath_IsNotRouteable needs to be updated too.
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/HelloWorld");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello, World!", content.Trim());
        }

        [Fact]
        public async Task HelloWorldWithRoute_CanGetContent()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/HelloWorldWithRoute/Some/Path/route");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello, route!", content.Trim());
        }

        [Fact]
        public async Task HelloWorldWithHandler_CanGetContent()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/HelloWorldWithHandler?message=handler");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello, handler!", content.Trim());
        }

        [Fact]
        public async Task HelloWorldWithPageModelHandler_CanPostContent()
        {
            // Arrange
            var getRequest = new HttpRequestMessage(HttpMethod.Get, "http://localhost/HelloWorldWithPageModelHandler?message=message");
            var getResponse = await Client.SendAsync(getRequest);
            var getResponseBody = await getResponse.Content.ReadAsStringAsync();
            var formToken = AntiforgeryTestHelper.RetrieveAntiforgeryToken(getResponseBody, "/HelloWorlWithPageModelHandler");
            var cookie = AntiforgeryTestHelper.RetrieveAntiforgeryCookie(getResponse);


            var postRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost/HelloWorldWithPageModelHandler");
            postRequest.Headers.Add("Cookie", cookie.Key + "=" + cookie.Value);
            postRequest.Headers.Add("RequestVerificationToken", formToken);

            // Act
            var response = await Client.SendAsync(postRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.StartsWith("Hello, You posted!", content.Trim());
        }

        [Fact]
        public async Task HelloWorldWithPageModelHandler_CanGetContent()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/HelloWorldWithPageModelHandler?message=pagemodel");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.StartsWith("Hello, pagemodel!", content.Trim());
        }

        [Fact]
        public async Task HelloWorldWithPageModelAttributeHandler()
        {
            // Arrange
            var url = "HelloWorldWithPageModelAttributeModel?message=DecoratedModel";

            // Act
            var content = await Client.GetStringAsync(url);

            // Assert
            Assert.Equal("Hello, DecoratedModel!", content.Trim());
        }

        [Fact]
        public async Task PageWithoutContent()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/PageWithoutContent/No/Content/Path");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("", content);
        }

        [Fact]
        public async Task ViewReturnsPage()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/OnGetView");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("The message: From OnGet", content.Trim());
        }

        [Fact]
        public async Task TempData_SetTempDataInPage_CanReadValue()
        {
            // Arrange 1
            var url = "http://localhost/TempData/SetTempDataOnPageAndRedirect?message=Hi1";
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Act 1
            var response = await Client.SendAsync(request);

            // Assert 1
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            // Arrange 2
            request = new HttpRequestMessage(HttpMethod.Get, response.Headers.Location);
            request.Headers.Add("Cookie", GetCookie(response));

            // Act2
            response = await Client.SendAsync(request);

            // Assert 2
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hi1", content.Trim());
        }

        [Fact]
        public async Task TempData_SetTempDataInPageModel_CanReadValue()
        {
            // Arrange 1
            var url = "http://localhost/TempData/SetTempDataOnPageModelAndRedirect?message=Hi2";
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Act 1
            var response = await Client.SendAsync(request);

            // Assert 1
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            // Arrange 2
            request = new HttpRequestMessage(HttpMethod.Get, response.Headers.Location);
            request.Headers.Add("Cookie", GetCookie(response));

            // Act 2
            response = await Client.SendAsync(request);

            // Assert 2
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hi2", content.Trim());
        }

        [Fact]
        public async Task TempData_TempDataPropertyOnPageModel_IsPopulatedFromTempData()
        {
            // Arrange 1
            var url = "http://localhost/TempData/SetMessageAndRedirect";
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Act 1
            var response = await Client.SendAsync(request);

            // Assert 1
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            // Act 2
            request = new HttpRequestMessage(HttpMethod.Get, response.Headers.Location);
            request.Headers.Add("Cookie", GetCookie(response));
            response = await Client.SendAsync(request);

            // Assert 2
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.StartsWith("Message: Secret Message", content.Trim());
            Assert.EndsWith("TempData: Secret Message", content.Trim());
        }

        [Fact]
        public async Task TempData_TempDataPropertyOnPageModel_PopulatesTempData()
        {
            // Arrange 1
            var getRequest = new HttpRequestMessage(HttpMethod.Get, "http://localhost/TempData/TempDataPageModelProperty");
            var getResponse = await Client.SendAsync(getRequest);
            var getResponseBody = await getResponse.Content.ReadAsStringAsync();
            var formToken = AntiforgeryTestHelper.RetrieveAntiforgeryToken(getResponseBody, "/TempData/TempDataPageModelProperty");
            var cookie = AntiforgeryTestHelper.RetrieveAntiforgeryCookie(getResponse);

            var url = "http://localhost/TempData/TempDataPageModelProperty";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Cookie", cookie.Key + "=" + cookie.Value);
            request.Headers.Add("RequestVerificationToken", formToken);

            // Act 1
            var response = await Client.SendAsync(request);

            // Assert 1
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.StartsWith("Message: Secret post", content.Trim());
            Assert.EndsWith("TempData:", content.Trim());

            // Arrange 2
            request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/TempData/TempDataPageModelProperty");
            request.Headers.Add("Cookie", GetCookie(response));

            // Act 2
            response = await Client.SendAsync(request);

            // Assert 2
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            content = await response.Content.ReadAsStringAsync();
            Assert.StartsWith("Message: Secret post", content.Trim());
            Assert.EndsWith("TempData: Secret post", content.Trim());
        }

        [Fact]
        public async Task AuthorizePage_AddsAuthorizationForSpecificPages()
        {
            // Arrange
            var url = "/HelloWorldWithAuth";

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Login?ReturnUrl=%2FHelloWorldWithAuth", response.Headers.Location.PathAndQuery);
        }

        [Fact]
        public async Task AuthorizePage_AllowAnonymousForSpecificPages()
        {
            // Arrange
            var url = "/Pages/Admin/Login";

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Login Page", content);
        }

        [Fact]
        public async Task ViewStart_IsDiscoveredWhenRootDirectoryIsNotSpecified()
        {
            // Test for https://github.com/aspnet/Mvc/issues/5915
            //Arrange
            var expected = $"Hello from _ViewStart{Environment.NewLine}Hello from /Pages/WithViewStart/Index.cshtml!";

            // Act
            var response = await Client.GetStringAsync("/Pages/WithViewStart");

            // Assert
            Assert.Equal(expected, response.Trim());
        }

        [Fact]
        public async Task ViewImport_IsDiscoveredWhenRootDirectoryIsNotSpecified()
        {
            // Test for https://github.com/aspnet/Mvc/issues/5915
            // Arrange
            var expected = "Hello from CustomService!";

            // Act
            var response = await Client.GetStringAsync("/Pages/WithViewImport");

            // Assert
            Assert.Equal(expected, response.Trim());
        }

        [Fact]
        public async Task PropertiesOnPageAreBound()
        {
            // Arrange
            var expected = "Id = 10, Name = Foo, Age = 25";
            var request = new HttpRequestMessage(HttpMethod.Post, "Pages/PropertyBinding/PagePropertyBinding/10")
            {
                Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Name", "Foo"),
                    new KeyValuePair<string, string>("Age", "25"),
                }),
            };
            await AddAntiforgeryHeaders(request);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.StartsWith(expected, content.Trim());
        }

        [Fact]
        public async Task PropertiesOnPageAreValidated()
        {
            // Arrange
            var expected = new[]
            {
                "Id = 27, Name = , Age = 325",
                "The Name field is required.",
                "The field Age must be between 0 and 99.",
            };
            var request = new HttpRequestMessage(HttpMethod.Post, "Pages/PropertyBinding/PagePropertyBinding/27")
            {
                Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Age", "325"),
                }),
            };
            await AddAntiforgeryHeaders(request);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            foreach (var item in expected)
            {
                Assert.Contains(item, content);
            }
        }

        [Fact]
        public async Task PropertiesOnPageModelAreBound()
        {
            // Arrange
            var expected = "Id = 10, Name = Foo, Age = 25";
            var request = new HttpRequestMessage(HttpMethod.Post, "Pages/PropertyBinding/PageModelWithPropertyBinding/10")
            {
                Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Name", "Foo"),
                    new KeyValuePair<string, string>("Age", "25"),
                }),
            };
            await AddAntiforgeryHeaders(request);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.StartsWith(expected, content.Trim());
        }

        [Fact]
        public async Task PropertiesOnPageModelAreValidated()
        {
            // Arrange
            var url = "Pages/PropertyBinding/PageModelWithPropertyBinding/27";
            var expected = new[]
            {
                "Id = 27, Name = , Age = 325",
                "The Name field is required.",
                "The field Age must be between 0 and 99.",
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Age", "325"),
                }),
            };

            await AddAntiforgeryHeaders(request);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            foreach (var item in expected)
            {
                Assert.Contains(item, content);
            }
        }

        [Fact]
        public async Task HandlerMethodArgumentsAndPropertiesAreModelBound()
        {
            // Arrange
            var expected = "Id = 11, Name = Test-Name, Age = 32";
            var request = new HttpRequestMessage(HttpMethod.Post, "Pages/PropertyBinding/PageWithPropertyAndArgumentBinding?id=11")
            {
                Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Name", "Test-Name"),
                    new KeyValuePair<string, string>("Age", "32"),
                }),
            };
            await AddAntiforgeryHeaders(request);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.StartsWith(expected, content.Trim());
        }

        [Fact]
        public async Task PagePropertiesAreNotBoundInGetRequests()
        {
            // Arrange
            var expected = "Id = 11, Name = Test-Name, Age =";
            var validationError = "The Name field is required.";
            var request = new HttpRequestMessage(HttpMethod.Get, "Pages/PropertyBinding/PageWithPropertyAndArgumentBinding?id=11")
            {
                Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Name", "Test-Name"),
                    new KeyValuePair<string, string>("Age", "32"),
                }),
            };

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.StartsWith(expected, content.Trim());
            Assert.DoesNotContain(validationError, content);
        }

        [Fact]
        public async Task PageProperty_WithSupportsGet_BoundInGet()
        {
            // Arrange
            var expected = "<p>11</p>";
            var request = new HttpRequestMessage(HttpMethod.Get, "Pages/PropertyBinding/BindPropertyWithGet?value=11");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.StartsWith(expected, content.Trim());
        }

        [Fact]
        public async Task PagePropertiesAreInjected()
        {
            // Arrange
            var expected =
@"Microsoft.AspNetCore.Mvc.Routing.UrlHelper
Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper`1[AspNetCore._InjectedPageProperties_cshtml]
Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary`1[AspNetCore._InjectedPageProperties_cshtml]";

            // Act
            var response = await Client.GetStringAsync("InjectedPageProperties");

            // Assert
            Assert.Equal(expected, response.Trim());
        }

        [Fact]
        public async Task RedirectFromPageWorks()
        {
            // Arrange
            var expected = "/Pages/Redirects/Redirect/10";

            // Act
            var response = await Client.GetAsync("/Pages/Redirects/RedirectFromPage");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(expected, response.Headers.Location.ToString());
        }

        [Fact]
        public async Task RedirectFromPageModelWorks()
        {
            // Arrange
            var expected = "/Pages/Redirects/Redirect/12";

            // Act
            var response = await Client.GetAsync("/Pages/Redirects/RedirectFromModel");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(expected, response.Headers.Location.ToString());
        }


        [Fact]
        public async Task RedirectToSelfWorks()
        {
            // Arrange
            var expected = "/Pages/Redirects/RedirectToSelf?user=37";
            var request = new HttpRequestMessage(HttpMethod.Post, "/Pages/Redirects/RedirectToSelf")
            {
                Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("value", "37"),
                }),
            };

            // Act
            await AddAntiforgeryHeaders(request);
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(expected, response.Headers.Location.ToString());
        }

        [Fact]
        public async Task RedirectDoesNotIncludeHandlerByDefault()
        {
            // Arrange
            var expected = "/Pages/Redirects/RedirectFromHandler";

            // Act
            var response = await Client.GetAsync("/Pages/Redirects/RedirectFromHandler/RedirectToPage/10");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(expected, response.Headers.Location.ToString());
        }

        [Fact]
        public async Task RedirectToOtherHandlersWorks()
        {
            // Arrange
            var expected = "/Pages/Redirects/RedirectFromHandler/RedirectToPage/11";

            // Act
            var response = await Client.GetAsync("/Pages/Redirects/RedirectFromHandler/RedirectToAnotherHandler/11");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(expected, response.Headers.Location.ToString());
        }

        [Fact]
        public async Task Controller_RedirectToPage()
        {
            // Arrange
            var expected = "/RedirectToController?param=17";

            // Act
            var response = await Client.GetAsync("/RedirectToPage");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(expected, response.Headers.Location.ToString());
        }

        [Fact]
        public async Task Page_RedirectToController()
        {
            // Arrange
            var expected = "/RedirectToPage?param=92";

            // Act
            var response = await Client.GetAsync("/RedirectToController");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(expected, response.Headers.Location.ToString());
        }

        [Fact]
        public async Task RedirectToSibling_Works()
        {
            // Arrange
            var expected = "/Pages/Redirects/Redirect/10";
            var response = await Client.GetAsync("/Pages/Redirects/RedirectToSibling/RedirectToRedirect");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(expected, response.Headers.Location.ToString());
        }

        [Fact]
        public async Task RedirectToSibling_RedirectsToIndexPage_WithoutIndexSegment()
        {
            // Arrange
            var expected = "/Pages/Redirects";
            var response = await Client.GetAsync("/Pages/Redirects/RedirectToSibling/RedirectToIndex");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(expected, response.Headers.Location.ToString());
        }

        [Fact]
        public async Task RedirectToSibling_RedirectsToSubDirectory()
        {
            // Arrange
            var expected = "/Pages/Redirects/SubDir/SubDirPage";
            var response = await Client.GetAsync("/Pages/Redirects/RedirectToSibling/RedirectToSubDir");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(expected, response.Headers.Location.ToString());
        }

        [Fact]
        public async Task RedirectToSibling_RedirectsToDotSlash()
        {
            // Arrange
            var expected = "/Pages/Redirects/SubDir/SubDirPage";

            // Act
            var response = await Client.GetAsync("/Pages/Redirects/RedirectToSibling/RedirectToDotSlash");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(expected, response.Headers.Location.ToString());
        }

        [Fact]
        public async Task RedirectToSibling_RedirectsToParentDirectory()
        {
            // Arrange
            var expected = "/Pages/Conventions/AuthFolder";
            var response = await Client.GetAsync("/Pages/Redirects/RedirectToSibling/RedirectToParent");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(expected, response.Headers.Location.ToString());
        }

        [Fact]
        public async Task TagHelpers_SupportSiblingRoutes()
        {
            // Arrange
            var expected =
            @"<form method=""post"" action=""/Pages/TagHelper/CrossPost""></form>
<a href=""/Pages/TagHelper/SelfPost/12"" />
<input type=""image"" formaction=""/Pages/TagHelper/CrossPost#my-fragment"" />";

            // Act
            var response = await Client.GetStringAsync("/Pages/TagHelper/SiblingLinks");

            // Assert
            Assert.Equal(expected, response.Trim());
        }

        [Fact]
        public async Task TagHelpers_SupportSubDirectoryRoutes()
        {
            // Arrange
            var expected =
            @"<form method=""post"" action=""/Pages/TagHelper/SubDir/SubDirPage""></form>
<a href=""/Pages/TagHelper/SubDir/SubDirPage/12"" />
<input type=""image"" formaction=""/Pages/TagHelper/SubDir/SubDirPage#my-fragment"" />";

            // Act
            var response = await Client.GetStringAsync("/Pages/TagHelper/SubDirectoryLinks");

            // Assert
            Assert.Equal(expected, response.Trim());
        }

        [Fact]
        public async Task TagHelpers_SupportsPathNavigation()
        {
            // Arrange
            var expected =
            @"<form method=""post"" action=""/HelloWorld""></form>
<a href=""/Pages/Redirects/RedirectToIndex"" />
<input type=""image"" formaction=""/Pages/Admin#my-fragment"" />";

            // Act
            var response = await Client.GetStringAsync("/Pages/TagHelper/PathTraversalLinks");

            // Assert
            Assert.EndsWith(expected, response.Trim());
        }

        [Fact]
        public async Task TagHelpers_SupportsRelativeNavigation()
        {
            // Arrange
            var expected = @"<form method=""post"" action=""/Pages/TagHelper/SubDirectoryLinks""></form>";

            // Act
            var response = await Client.GetStringAsync("/Pages/TagHelper/PathTraversalLinks");

            // Assert
            Assert.StartsWith(expected, response.Trim());
        }

        [Fact]
        public async Task Page_WithSection_CanAccessModel()
        {
            // Arrange
            var expected = "Value is 17";

            // Act
            var response = await Client.GetStringAsync("/Pages/Section");

            // Assert
            Assert.StartsWith(expected, response.Trim());
        }

        [Fact]
        public async Task PagesCanByRoutedViaRoute_AddedViaAddPageRoute()
        {
            // Arrange
            var expected = "Hello, test!";

            // Act
            var response = await Client.GetStringAsync("/Different-Route/test");

            // Assert
            Assert.StartsWith(expected, response.Trim());
        }

        [Fact]
        public async Task PagesCanByRoutedToApplicationRoot_ViaAddPageRoute()
        {
            // Arrange
            var expected = "Hello from NotTheRoot";

            // Act
            var response = await Client.GetStringAsync("");

            // Assert
            Assert.StartsWith(expected, response.Trim());
        }

        [Fact]
        public async Task AuthFiltersAppliedToPageModel_AreExecuted()
        {
            // Act
            var response = await Client.GetAsync("/ModelWithAuthFilter");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Login?ReturnUrl=%2FModelWithAuthFilter", response.Headers.Location.PathAndQuery);
        }

        [Fact]
        public async Task AuthorizeAttributeIsExecutedPriorToAutoAntiforgeryFilter()
        {
            // Act
            var response = await Client.PostAsync("/Pages/Admin/Edit", new StringContent(""));

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Login?ReturnUrl=%2FPages%2FAdmin%2FEdit", response.Headers.Location.PathAndQuery);
        }

        [Fact]
        public async Task PageFiltersAppliedToPageModel_AreExecuted()
        {
            // Arrange
            var expected = "Hello from OnGetEdit";

            // Act
            var response = await Client.GetStringAsync("/ModelWithPageFilter");

            // Assert
            Assert.Equal(expected, response.Trim());
        }

        [Fact]
        public async Task ResponseCacheAttributes_AreApplied()
        {
            // Arrange
            var expected = "Hello from ModelWithResponseCache.OnGet";

            // Act
            var response = await Client.GetAsync("/ModelWithResponseCache");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var cacheControl = response.Headers.CacheControl;
            Assert.Equal(TimeSpan.FromSeconds(10), cacheControl.MaxAge.Value);
            Assert.True(cacheControl.Private);
            Assert.Equal(expected, (await response.Content.ReadAsStringAsync()).Trim());
        }

        [Fact]
        public async Task ViewLocalizer_WorksForPagesWithoutModel()
        {
            // Arrange
            var expected = "Bon Jour from Page";

            // Act
            var response = await Client.GetStringAsync("/Pages/Localized/Page?culture=fr-FR");

            Assert.Equal(expected, response.Trim());
        }

        [Fact]
        public async Task ViewLocalizer_WorksForPagesWithModel()
        {
            // Arrange
            var expected = "Bon Jour from PageWithModel";

            // Act
            var response = await Client.GetStringAsync("/Pages/Localized/PageWithModel?culture=fr-FR");

            // Assert
            Assert.Equal(expected, response.Trim());
        }

        [Fact]
        public async Task BindPropertyAttribute_CanBeAppliedToModelType()
        {
            // Arrange
            var expected = "Property1 = 123, Property2 = 25,";
            var request = new HttpRequestMessage(HttpMethod.Post, "/Pages/PropertyBinding/BindPropertyOnModel?Property1=123")
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("Property2", "25"),
                }),
            };
            await AddAntiforgeryHeaders(request);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.StartsWith(expected, responseContent.Trim());
        }

        [Fact]
        public async Task BindingInfoOnPropertiesIsPreferredToBindingInfoOnType()
        {
            // Arrange
            var expected = "Property1 = 123, Property2 = 25,";
            var request = new HttpRequestMessage(HttpMethod.Post, "/Pages/PropertyBinding/BindPropertyOnModel?Property1=123")
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    // FormValueProvider appears before QueryStringValueProvider. However, the FromQuery explicitly listed
                    // on the property should cause it to use the latter.
                    new KeyValuePair<string, string>("Property1", "345"),
                    new KeyValuePair<string, string>("Property2", "25"),
                }),
            };
            await AddAntiforgeryHeaders(request);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.StartsWith(expected, responseContent.Trim());
        }

        [Fact]
        public Task InheritsOnViewImportsWorksForPagesWithoutModel()
            => InheritsOnViewImportsWorks("Pages/CustomBaseType/Page");

        [Fact]
        public Task InheritsOnViewImportsWorksForPagesWithModel()
            => InheritsOnViewImportsWorks("Pages/CustomBaseType/PageWithModel");

        private async Task InheritsOnViewImportsWorks(string path)
        {
            // Arrange
            var expected = "<custom-base-type-layout>RazorPagesWebSite.CustomPageBase</custom-base-type-layout>";

            // Act
            var response = await Client.GetStringAsync(path);

            // Assert
            Assert.Equal(expected, response.Trim());
        }

        private async Task AddAntiforgeryHeaders(HttpRequestMessage request)
        {
            var getResponse = await Client.GetAsync(request.RequestUri);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var getResponseBody = await getResponse.Content.ReadAsStringAsync();
            var formToken = AntiforgeryTestHelper.RetrieveAntiforgeryToken(getResponseBody, "");
            var cookie = AntiforgeryTestHelper.RetrieveAntiforgeryCookie(getResponse);

            request.Headers.Add("Cookie", cookie.Key + "=" + cookie.Value);
            request.Headers.Add("RequestVerificationToken", formToken);
        }

        private static string GetCookie(HttpResponseMessage response)
        {
            var setCookie = response.Headers.GetValues("Set-Cookie").ToArray();
            return setCookie[0].Split(';').First();
        }

        public class CookieMetadata
        {
            public string Key { get; set; }

            public string Value { get; set; }
        }
    }
}
