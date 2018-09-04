﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public abstract class RoutingTestsBase<TStartup> : IClassFixture<MvcTestFixture<TStartup>> where TStartup : class
    {
        protected RoutingTestsBase(MvcTestFixture<TStartup> fixture)
        {
            var factory = fixture.Factories.FirstOrDefault() ?? fixture.WithWebHostBuilder(ConfigureWebHostBuilder);
            Client = factory.CreateDefaultClient();
        }

        private static void ConfigureWebHostBuilder(IWebHostBuilder builder) =>
            builder.UseStartup<TStartup>();

        public HttpClient Client { get; }

        [Fact]
        public abstract Task HasEndpointMatch();

        [Fact]
        public abstract Task RouteData_Routers_ConventionalRoute();

        [Fact]
        public abstract Task RouteData_Routers_AttributeRoute();

        // Verifies that components in the MVC pipeline can modify datatokens
        // without impacting any static data.
        //
        // This does two request, to verify that the data in the route is not modified
        [Fact]
        public async Task RouteData_DataTokens_FilterCanSetDataTokens()
        {
            // Arrange
            var response = await Client.GetAsync("http://localhost/RouteData/DataTokens");

            // Guard
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResultData>(body);
            Assert.Single(result.DataTokens);
            Assert.Single(result.DataTokens, kvp => kvp.Key == "actionName" && ((string)kvp.Value) == "DataTokens");

            // Act
            response = await Client.GetAsync("http://localhost/RouteData/Conventional");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            body = await response.Content.ReadAsStringAsync();
            result = JsonConvert.DeserializeObject<ResultData>(body);

            Assert.Single(result.DataTokens);
            Assert.Single(result.DataTokens, kvp => kvp.Key == "actionName" && ((string)kvp.Value) == "Conventional");
        }

        protected class ResultData
        {
            public Dictionary<string, object> DataTokens { get; set; }

            public string[] Routers { get; set; }
        }

        [Fact]
        public async Task DataTokens_ReturnsDataTokensForRoute()
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/DataTokensRoute/DataTokens/Index");

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(body);
            Assert.Single(result, kvp => kvp.Key == "hasDataTokens" && ((bool)kvp.Value) == true);
        }

        [Fact]
        public async Task DataTokens_ReturnsNoDataTokensForRoute()
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/DataTokens/Index");

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(body);
            Assert.Empty(result);
        }

        [Fact]
        public virtual async Task ConventionalRoutedController_ActionIsReachable()
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/Home/Index");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Contains("/Home/Index", result.ExpectedUrls);
            Assert.Equal("Home", result.Controller);
            Assert.Equal("Index", result.Action);
            Assert.Equal(
                new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    { "controller", "Home" },
                    { "action", "Index" },
                },
                result.RouteValues);
        }

        [Fact]
        public virtual async Task ConventionalRoutedController_ActionIsReachable_WithDefaults()
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Contains("/", result.ExpectedUrls);
            Assert.Equal("Home", result.Controller);
            Assert.Equal("Index", result.Action);
            Assert.Equal(
                new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    { "controller", "Home" },
                    { "action", "Index" },
                },
                result.RouteValues);
        }

        [Fact]
        public virtual async Task ConventionalRoutedController_NonActionIsNotReachable()
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/Home/NotAnAction");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public virtual async Task ConventionalRoutedController_InArea_ActionIsReachable()
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/Travel/Flight/Index");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Contains("/Travel/Flight/Index", result.ExpectedUrls);
            Assert.Equal("Flight", result.Controller);
            Assert.Equal("Index", result.Action);
            Assert.Equal(
                new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    { "area", "Travel" },
                    { "controller", "Flight" },
                    { "action", "Index" },
                },
                result.RouteValues);
        }

        [Fact]
        public virtual async Task ConventionalRoutedController_InArea_ActionBlockedByHttpMethod()
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/Travel/Flight/BuyTickets");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("", "/Home/OptionalPath/default")]
        [InlineData("CustomPath", "/Home/OptionalPath/CustomPath")]
        public virtual async Task ConventionalRoutedController_WithOptionalSegment(string optionalSegment, string expected)
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/Home/OptionalPath/" + optionalSegment);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Single(result.ExpectedUrls, expected);
        }

        [Fact]
        public async Task AttributeRoutedAction_IsReachable()
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/Store/Shop/Products");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Contains("/Store/Shop/Products", result.ExpectedUrls);
            Assert.Equal("Store", result.Controller);
            Assert.Equal("ListProducts", result.Action);

            Assert.Contains(
                new KeyValuePair<string, object>("controller", "Store"),
                result.RouteValues);

            Assert.Contains(
                new KeyValuePair<string, object>("action", "ListProducts"),
                result.RouteValues);
        }

        [Theory]
        [InlineData("Get", "/Friends")]
        [InlineData("Get", "/Friends/Peter")]
        [InlineData("Delete", "/Friends")]
        public async Task AttributeRoutedAction_AcceptRequestsWithValidMethods_InRoutesWithoutExtraTemplateSegmentsOnTheAction(
            string method,
            string url)
        {
            // Arrange
            var request = new HttpRequestMessage(new HttpMethod(method), $"http://localhost{url}");

            // Assert
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Contains(url, result.ExpectedUrls);
            Assert.Equal("Friends", result.Controller);
            Assert.Equal(method, result.Action);

            Assert.Contains(
                new KeyValuePair<string, object>("controller", "Friends"),
                result.RouteValues);

            Assert.Contains(
                new KeyValuePair<string, object>("action", method),
                result.RouteValues);

            if (result.RouteValues.ContainsKey("id"))
            {
                Assert.Contains(
                    new KeyValuePair<string, object>("id", "Peter"),
                    result.RouteValues);
            }
        }

        public static TheoryData<string, string> AttributeRoutedAction_RejectsRequestsWithWrongMethods_InRoutesWithoutExtraTemplateSegmentsOnTheActionData
        {
            get
            {
                return new TheoryData<string, string>
                {
                    { "Post", "/Friends" },
                    { "Put", "/Friends" },
                    { "Patch", "/Friends" },
                    { "Options", "/Friends" },
                    { "Head", "/Friends" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(AttributeRoutedAction_RejectsRequestsWithWrongMethods_InRoutesWithoutExtraTemplateSegmentsOnTheActionData))]
        public virtual async Task AttributeRoutedAction_RejectsRequestsWithWrongMethods_InRoutesWithoutExtraTemplateSegmentsOnTheAction(
            string method,
            string url)
        {
            // Arrange
            var request = new HttpRequestMessage(new HttpMethod(method), $"http://localhost{url}");

            // Assert
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("http://localhost/api/v1/Maps")]
        [InlineData("http://localhost/api/v2/Maps")]
        public virtual async Task AttributeRoutedAction_MultipleRouteAttributes_WorksWithNameAndOrder(string url)
        {
            // Arrange & Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Maps", result.Controller);
            Assert.Equal("Get", result.Action);

            Assert.Equal(new string[]
            {
                    "/api/v2/Maps",
                    "/api/v1/Maps",
                    "/api/v2/Maps"
            },
            result.ExpectedUrls);
        }

        [Fact]
        public virtual async Task AttributeRoutedAction_MultipleRouteAttributes_WorksWithOverrideRoutes()
        {
            // Arrange
            var url = "http://localhost/api/v2/Maps";

            // Act
            var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, url));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Maps", result.Controller);
            Assert.Equal("Post", result.Action);

            Assert.Equal(new string[]
            {
                    "/api/v2/Maps",
                    "/api/v2/Maps"
            },
            result.ExpectedUrls);
        }

        [Fact]
        public virtual async Task AttributeRoutedAction_MultipleRouteAttributes_RouteAttributeTemplatesIgnoredForOverrideActions()
        {
            // Arrange
            var url = "http://localhost/api/v1/Maps";

            // Act
            var response = await Client.SendAsync(new HttpRequestMessage(new HttpMethod("POST"), url));

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Theory]
        [InlineData("http://localhost/api/v1/Maps/5", "PUT")]
        [InlineData("http://localhost/api/v2/Maps/5", "PUT")]
        [InlineData("http://localhost/api/v1/Maps/PartialUpdate/5", "PATCH")]
        [InlineData("http://localhost/api/v2/Maps/PartialUpdate/5", "PATCH")]
        public virtual async Task AttributeRoutedAction_MultipleRouteAttributes_CombinesWithMultipleHttpAttributes(
            string url,
            string method)
        {
            // Arrange & Act
            var response = await Client.SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Maps", result.Controller);
            Assert.Equal("Update", result.Action);

            Assert.Equal(new string[]
            {
                    "/api/v2/Maps/PartialUpdate/5",
                    "/api/v2/Maps/PartialUpdate/5"
            },
            result.ExpectedUrls);
        }

        [Theory]
        [InlineData("http://localhost/Banks/Get/5")]
        [InlineData("http://localhost/Bank/Get/5")]
        public virtual async Task AttributeRoutedAction_MultipleHttpAttributesAndTokenReplacement(string url)
        {
            // Arrange
            var expectedUrl = new Uri(url).AbsolutePath;

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Banks", result.Controller);
            Assert.Equal("Get", result.Action);

            Assert.Equal(new string[]
            {
                    "/Bank/Get/5",
                    "/Bank/Get/5"
            },
            result.ExpectedUrls);
        }

        public static TheoryData<string, string> AttributeRoutedAction_MultipleRouteAttributes_WithMultipleHttpAttributes_RespectsConstraintsData
        {
            get
            {
                return new TheoryData<string, string>
                {
                    { "http://localhost/api/v1/Maps/5", "PATCH" },
                    { "http://localhost/api/v2/Maps/5", "PATCH" },
                    { "http://localhost/api/v1/Maps/PartialUpdate/5", "PUT" },
                    { "http://localhost/api/v2/Maps/PartialUpdate/5", "PUT" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(AttributeRoutedAction_MultipleRouteAttributes_WithMultipleHttpAttributes_RespectsConstraintsData))]
        public virtual async Task AttributeRoutedAction_MultipleRouteAttributes_WithMultipleHttpAttributes_RespectsConstraints(
            string url,
            string method)
        {
            // Arrange
            var expectedUrl = new Uri(url).AbsolutePath;

            // Act
            var response = await Client.SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // The url would be /Store/ListProducts with conventional routes
        [Fact]
        public async Task AttributeRoutedAction_IsNotReachableWithTraditionalRoute()
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/Store/ListProducts");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // There's two actions at this URL - but attribute routes go in the route table
        // first.
        [Fact]
        public async Task AttributeRoutedAction_TriedBeforeConventionalRouting()
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/Home/About");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Contains("/Home/About", result.ExpectedUrls);
            Assert.Equal("Store", result.Controller);
            Assert.Equal("About", result.Action);
        }

        [Fact]
        public async Task AttributeRoutedAction_ControllerLevelRoute_WithActionParameter_IsReachable()
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/Blog/Edit/5");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Contains("/Blog/Edit/5", result.ExpectedUrls);
            Assert.Equal("Blog", result.Controller);
            Assert.Equal("Edit", result.Action);

            Assert.Contains(
                new KeyValuePair<string, object>("controller", "Blog"),
                result.RouteValues);

            Assert.Contains(
                new KeyValuePair<string, object>("action", "Edit"),
                result.RouteValues);

            Assert.Contains(
                new KeyValuePair<string, object>("postId", "5"),
                result.RouteValues);
        }

        // There's no [HttpGet] on the action here.
        [Fact]
        public async Task AttributeRoutedAction_ControllerLevelRoute_IsReachable()
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/api/Employee");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Contains("/api/Employee", result.ExpectedUrls);
            Assert.Equal("Employee", result.Controller);
            Assert.Equal("List", result.Action);
        }

        // We are intentionally skipping GET because we have another method with [HttpGet] on the same controller
        // and a test that verifies that if you define another action with a specific verb we'll route to that
        // more specific action.
        [Theory]
        [InlineData("PUT")]
        [InlineData("POST")]
        [InlineData("PATCH")]
        [InlineData("DELETE")]
        public async Task AttributeRoutedAction_RouteAttributeOnAction_IsReachable(string method)
        {
            // Arrange
            var message = new HttpRequestMessage(new HttpMethod(method), "http://localhost/Store/Shop/Orders");

            // Act
            var response = await Client.SendAsync(message);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Contains("/Store/Shop/Orders", result.ExpectedUrls);
            Assert.Equal("Store", result.Controller);
            Assert.Equal("Orders", result.Action);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("PATCH")]
        [InlineData("DELETE")]
        public async Task AttributeRoutedAction_RouteAttributeOnActionAndController_IsReachable(string method)
        {
            // Arrange
            var message = new HttpRequestMessage(new HttpMethod(method), "http://localhost/api/Employee/5/Salary");

            // Act
            var response = await Client.SendAsync(message);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Contains("/api/Employee/5/Salary", result.ExpectedUrls);
            Assert.Equal("Employee", result.Controller);
            Assert.Equal("Salary", result.Action);
        }

        [Fact]
        public async Task AttributeRoutedAction_RouteAttributeOnActionAndHttpGetOnDifferentAction_ReachesHttpGetAction()
        {
            // Arrange
            var message = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Store/Shop/Orders");

            // Act
            var response = await Client.SendAsync(message);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Contains("/Store/Shop/Orders", result.ExpectedUrls);
            Assert.Equal("Store", result.Controller);
            Assert.Equal("GetOrders", result.Action);
        }

        // There's no [HttpGet] on the action here.
        [Theory]
        [InlineData("PUT")]
        [InlineData("PATCH")]
        public async Task AttributeRoutedAction_ControllerLevelRoute_WithAcceptVerbs_IsReachable(string verb)
        {
            // Arrange
            var message = new HttpRequestMessage(new HttpMethod(verb), "http://localhost/api/Employee");

            // Act
            var response = await Client.SendAsync(message);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Contains("/api/Employee", result.ExpectedUrls);
            Assert.Equal("Employee", result.Controller);
            Assert.Equal("UpdateEmployee", result.Action);
        }

        [Theory]
        [InlineData("PUT")]
        [InlineData("PATCH")]
        public async Task AttributeRoutedAction_ControllerLevelRoute_WithAcceptVerbsAndRouteTemplate_IsReachable(string verb)
        {
            // Arrange
            var message = new HttpRequestMessage(new HttpMethod(verb), "http://localhost/api/Employee/Manager");

            // Act
            var response = await Client.SendAsync(message);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Contains("/api/Employee/Manager", result.ExpectedUrls);
            Assert.Equal("Employee", result.Controller);
            Assert.Equal("UpdateManager", result.Action);
        }

        [Theory]
        [InlineData("PUT", "Bank")]
        [InlineData("PATCH", "Bank")]
        [InlineData("PUT", "Bank/Update")]
        [InlineData("PATCH", "Bank/Update")]
        public virtual async Task AttributeRoutedAction_AcceptVerbsAndRouteTemplate_IsReachable(string verb, string path)
        {
            // Arrange
            var expectedUrl = "/Bank/Update";
            var message = new HttpRequestMessage(new HttpMethod(verb), "http://localhost/" + path);

            // Act
            var response = await Client.SendAsync(message);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal(new string[] { expectedUrl, expectedUrl }, result.ExpectedUrls);
            Assert.Equal("Banks", result.Controller);
            Assert.Equal("UpdateBank", result.Action);
        }

        [Fact]
        public async Task AttributeRoutedAction_WithCustomHttpAttributes_IsReachable()
        {
            // Arrange
            var message = new HttpRequestMessage(new HttpMethod("MERGE"), "http://localhost/api/Employee/5");

            // Act
            var response = await Client.SendAsync(message);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Contains("/api/Employee/5", result.ExpectedUrls);
            Assert.Equal("Employee", result.Controller);
            Assert.Equal("MergeEmployee", result.Action);
        }

        // There's an [HttpGet] with its own template on the action here.
        [Theory]
        [InlineData("GET", "GetAdministrator")]
        [InlineData("DELETE", "DeleteAdministrator")]
        public async Task AttributeRoutedAction_ControllerLevelRoute_CombinedWithActionRoute_IsReachable(string verb, string action)
        {
            // Arrange
            var message = new HttpRequestMessage(new HttpMethod(verb), "http://localhost/api/Employee/5/Administrator");

            // Act
            var response = await Client.SendAsync(message);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Contains("/api/Employee/5/Administrator", result.ExpectedUrls);
            Assert.Equal("Employee", result.Controller);
            Assert.Equal(action, result.Action);

            Assert.Contains(
                new KeyValuePair<string, object>("id", "5"),
                result.RouteValues);
        }

        [Fact]
        public async Task AttributeRoutedAction_ActionLevelRouteWithTildeSlash_OverridesControllerLevelRoute()
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/Manager/5");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Contains("/Manager/5", result.ExpectedUrls);
            Assert.Equal("Employee", result.Controller);
            Assert.Equal("GetManager", result.Action);

            Assert.Contains(
                new KeyValuePair<string, object>("id", "5"),
                result.RouteValues);
        }

        [Fact]
        public async Task AttributeRoutedAction_OverrideActionOverridesOrderOnController()
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/Team/5");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Contains("/Team/5", result.ExpectedUrls);
            Assert.Equal("Team", result.Controller);
            Assert.Equal("GetOrganization", result.Action);

            Assert.Contains(
                new KeyValuePair<string, object>("teamId", "5"),
                result.RouteValues);
        }

        [Fact]
        public async Task AttributeRoutedAction_OrderOnActionOverridesOrderOnController()
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/Teams");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Contains("/Teams", result.ExpectedUrls);
            Assert.Equal("Team", result.Controller);
            Assert.Equal("GetOrganizations", result.Action);
        }

        [Fact]
        public virtual async Task AttributeRoutedAction_LinkGeneration_OverrideActionOverridesOrderOnController()
        {
            // Arrange & Act
            var response = await Client.GetStringAsync("http://localhost/Organization/5");

            // Assert
            Assert.NotNull(response);
            Assert.Equal("/Club/5", response);
        }

        [Fact]
        public virtual async Task AttributeRoutedAction_LinkGeneration_OrderOnActionOverridesOrderOnController()
        {
            // Arrange & Act
            var response = await Client.GetStringAsync("http://localhost/Teams/AllTeams");

            // Assert
            Assert.NotNull(response);
            Assert.Equal("/Teams/AllOrganizations", response);
        }

        [Theory]
        [InlineData("", "/TeamName/DefaultName")]
        [InlineData("CustomName", "/TeamName/CustomName")]
        public virtual async Task AttributeRoutedAction_PreservesDefaultValue_IfRouteValueIsNull(string teamName, string expected)
        {
            // Arrange & Act
            var body = await Client.GetStringAsync("http://localhost/TeamName/" + teamName);

            // Assert
            Assert.NotNull(body);
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);
            Assert.Single(result.ExpectedUrls, expected);
        }

        [Fact]
        public virtual async Task AttributeRoutedAction_LinkToSelf()
        {
            // Arrange
            var url = LinkFrom("http://localhost/api/Employee").To(new { });

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Employee", result.Controller);
            Assert.Equal("List", result.Action);

            Assert.Equal("/api/Employee", result.Link);
        }

        [Fact]
        public virtual async Task AttributeRoutedAction_LinkWithAmbientController()
        {
            // Arrange
            var url = LinkFrom("http://localhost/api/Employee").To(new { action = "Get", id = 5 });

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Employee", result.Controller);
            Assert.Equal("List", result.Action);

            Assert.Equal("/api/Employee/5", result.Link);
        }

        [Fact]
        public virtual async Task AttributeRoutedAction_LinkToAttributeRoutedController()
        {
            // Arrange
            var url = LinkFrom("http://localhost/api/Employee").To(new { action = "ShowPosts", controller = "Blog" });

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Employee", result.Controller);
            Assert.Equal("List", result.Action);

            Assert.Equal("/Blog/ShowPosts", result.Link);
        }

        [Fact]
        public virtual async Task AttributeRoutedAction_LinkToConventionalController()
        {
            // Arrange
            var url = LinkFrom("http://localhost/api/Employee").To(new { action = "Index", controller = "Home" });

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Employee", result.Controller);
            Assert.Equal("List", result.Action);

            Assert.Equal("/", result.Link);
        }

        [Theory]
        [InlineData("GET", "Get")]
        [InlineData("PUT", "Put")]
        public virtual async Task AttributeRoutedAction_LinkWithName_WithNameInheritedFromControllerRoute(
            string method,
            string actionName)
        {
            // Arrange
            var message = new HttpRequestMessage(new HttpMethod(method), "http://localhost/api/Company/5");

            // Act
            var response = await Client.SendAsync(message);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Company", result.Controller);
            Assert.Equal(actionName, result.Action);

            Assert.Equal("/api/Company/5", result.ExpectedUrls.Single());
            Assert.Equal("Company", result.RouteName);
        }

        [Fact]
        public virtual async Task AttributeRoutedAction_LinkWithName_WithNameOverridenFromController()
        {
            // Arrange & Act
            var response = await Client.DeleteAsync("http://localhost/api/Company/5");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Company", result.Controller);
            Assert.Equal("Delete", result.Action);

            Assert.Equal("/api/Company/5", result.ExpectedUrls.Single());
            Assert.Equal("RemoveCompany", result.RouteName);
        }

        [Fact]
        public virtual async Task AttributeRoutedAction_Link_WithNonEmptyActionRouteTemplateAndNoActionRouteName()
        {
            // Arrange
            var url = LinkFrom("http://localhost")
                .To(new { id = 5 });

            // Act
            var response = await Client.GetAsync("http://localhost/api/Company/5/Employees");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Company", result.Controller);
            Assert.Equal("GetEmployees", result.Action);

            Assert.Equal("/api/Company/5/Employees", result.ExpectedUrls.Single());
            Assert.Null(result.RouteName);
        }

        [Fact]
        public virtual async Task AttributeRoutedAction_LinkWithName_WithNonEmptyActionRouteTemplateAndActionRouteName()
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/api/Company/5/Departments");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Company", result.Controller);
            Assert.Equal("GetDepartments", result.Action);

            Assert.Equal("/api/Company/5/Departments", result.ExpectedUrls.Single());
            Assert.Equal("Departments", result.RouteName);
        }

        [Fact]
        public virtual async Task ConventionalRoutedAction_LinkToArea()
        {
            // Arrange
            var url = LinkFrom("http://localhost/")
                .To(new { action = "BuyTickets", controller = "Flight", area = "Travel" });

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Home", result.Controller);
            Assert.Equal("Index", result.Action);

            Assert.Equal("/Travel/Flight/BuyTickets", result.Link);
        }

        [Fact]
        public virtual async Task ConventionalRoutedAction_InArea_ImplicitLinkToArea()
        {
            // Arrange
            var url = LinkFrom("http://localhost/Travel/Flight").To(new { action = "BuyTickets" });

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Flight", result.Controller);
            Assert.Equal("Index", result.Action);

            Assert.Equal("/Travel/Flight/BuyTickets", result.Link);
        }

        [Fact]
        public virtual async Task ConventionalRoutedAction_InArea_ExplicitLeaveArea()
        {
            // Arrange
            var url = LinkFrom("http://localhost/Travel/Flight")
                .To(new { action = "Index", controller = "Home", area = "" });

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Flight", result.Controller);
            Assert.Equal("Index", result.Action);

            Assert.Equal("/", result.Link);
        }

        [Fact]
        public virtual async Task AttributeRoutedAction_LinkToArea()
        {
            // Arrange
            var url = LinkFrom("http://localhost/api/Employee")
                .To(new { action = "Schedule", controller = "Rail", area = "Travel" });

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Employee", result.Controller);
            Assert.Equal("List", result.Action);

            Assert.Equal("/ContosoCorp/Trains/CheckSchedule", result.Link);
        }

        [Fact]
        public virtual async Task AttributeRoutedAction_InArea_ImplicitLinkToArea()
        {
            // Arrange
            var url = LinkFrom("http://localhost/ContosoCorp/Trains/CheckSchedule").To(new { action = "Index" });

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Rail", result.Controller);
            Assert.Equal("Schedule", result.Action);

            Assert.Equal("/ContosoCorp/Trains", result.Link);
        }

        [Fact]
        public virtual async Task AttributeRoutedAction_InArea_ExplicitLeaveArea()
        {
            // Arrange
            var url = LinkFrom("http://localhost/ContosoCorp/Trains/CheckSchedule")
                .To(new { action = "Index", controller = "Home", area = "" });

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Rail", result.Controller);
            Assert.Equal("Schedule", result.Action);

            Assert.Equal("/", result.Link);
        }

        

        [Fact]
        public virtual async Task AttributeRoutedAction_InArea_LinkToConventionalRoutedActionInArea()
        {
            // Arrange
            var url = LinkFrom("http://localhost/ContosoCorp/Trains")
                .To(new { action = "Index", controller = "Flight", });

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Rail", result.Controller);
            Assert.Equal("Index", result.Action);

            Assert.Equal("/Travel/Flight", result.Link);
        }

        [Fact]
        public virtual async Task ConventionalRoutedAction_InArea_LinkToAttributeRoutedActionInArea()
        {
            // Arrange
            var url = LinkFrom("http://localhost/Travel/Flight")
                .To(new { action = "Index", controller = "Rail", });

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Flight", result.Controller);
            Assert.Equal("Index", result.Action);

            Assert.Equal("/ContosoCorp/Trains", result.Link);
        }

        [Fact]
        public virtual async Task ConventionalRoutedAction_InArea_LinkToAnotherArea()
        {
            // Arrange
            var url = LinkFrom("http://localhost/Travel/Flight")
                .To(new { action = "ListUsers", controller = "UserManagement", area = "Admin" });

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Flight", result.Controller);
            Assert.Equal("Index", result.Action);

            Assert.Equal("/Admin/Users/All", result.Link);
        }

        [Fact]
        public virtual async Task AttributeRoutedAction_InArea_LinkToAnotherArea()
        {
            // Arrange
            var url = LinkFrom("http://localhost/ContosoCorp/Trains")
                .To(new { action = "ListUsers", controller = "UserManagement", area = "Admin" });

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Equal("Rail", result.Controller);
            Assert.Equal("Index", result.Action);

            Assert.Equal("/Admin/Users/All", result.Link);
        }

        [Theory]
        [InlineData("/Bank/Deposit", "PUT", "Deposit")]
        [InlineData("/Bank/Deposit", "POST", "Deposit")]
        [InlineData("/Bank/Deposit/5", "PUT", "Deposit")]
        [InlineData("/Bank/Deposit/5", "POST", "Deposit")]
        [InlineData("/Bank/Withdraw/5", "POST", "Withdraw")]
        public async Task AttributeRouting_MixedAcceptVerbsAndRoute_Reachable(string path, string verb, string actionName)
        {
            // Arrange
            var request = new HttpRequestMessage(new HttpMethod(verb), "http://localhost" + path);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Contains(path, result.ExpectedUrls);
            Assert.Equal("Banks", result.Controller);
            Assert.Equal(actionName, result.Action);
        }

        // These verbs don't match
        public static TheoryData<string, string> AttributeRouting_MixedAcceptVerbsAndRoute_UnreachableData
        {
            get
            {
                return new TheoryData<string, string>
                {
                    { "/Bank/Deposit", "GET" },
                    { "/Bank/Deposit/5", "DELETE" },
                    { "/Bank/Withdraw/5", "GET" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(AttributeRouting_MixedAcceptVerbsAndRoute_UnreachableData))]
        public virtual async Task AttributeRouting_MixedAcceptVerbsAndRoute_Unreachable(string path, string verb)
        {
            // Arrange
            var request = new HttpRequestMessage(new HttpMethod(verb), "http://localhost" + path);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("/Order/Add/1", "GET", "Add")]
        [InlineData("/Order/Add", "POST", "Add")]
        [InlineData("/Order/Edit/1", "PUT", "Edit")]
        [InlineData("/Order/GetOrder", "GET", "GetOrder")]
        public async Task AttributeRouting_RouteNameTokenReplace_Reachable(string path, string verb, string actionName)
        {
            // Arrange
            var request = new HttpRequestMessage(new HttpMethod(verb), "http://localhost" + path);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoutingResult>(body);

            Assert.Contains(path, result.ExpectedUrls);
            Assert.Equal("Order", result.Controller);
            Assert.Equal(actionName, result.Action);
        }

        [Fact]
        public async Task CanRunMiddlewareAfterRouting()
        {
            // Act
            var response = await Client.GetAsync("/afterrouting");

            // Assert
            await response.AssertStatusCodeAsync(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello from middleware after routing", content);
        }


        protected static LinkBuilder LinkFrom(string url)
        {
            return new LinkBuilder(url);
        }

        // See TestResponseGenerator in RoutingWebSite for the code that generates this data.
        protected class RoutingResult
        {
            public string[] ExpectedUrls { get; set; }

            public string ActualUrl { get; set; }

            public Dictionary<string, object> RouteValues { get; set; }

            public string RouteName { get; set; }

            public string Action { get; set; }

            public string Controller { get; set; }

            public string Link { get; set; }
        }

        protected class LinkBuilder
        {
            public LinkBuilder(string url)
            {
                Url = url;

                Values = new Dictionary<string, object>
                {
                    { "link", string.Empty }
                };
            }

            public string Url { get; set; }

            public Dictionary<string, object> Values { get; set; }

            public LinkBuilder To(object values)
            {
                var dictionary = new RouteValueDictionary(values);
                foreach (var kvp in dictionary)
                {
                    Values.Add("link_" + kvp.Key, kvp.Value);
                }

                return this;
            }

            public override string ToString()
            {
                return Url + "?" + string.Join("&", Values.Select(kvp => kvp.Key + "=" + kvp.Value));
            }

            public static implicit operator string(LinkBuilder builder)
            {
                return builder.ToString();
            }
        }
    }
}
