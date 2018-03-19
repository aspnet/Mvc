﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class RazorPagesWithBasePathTest : IClassFixture<MvcTestFixture<RazorPagesWebSite.StartupWithBasePath>>
    {
        public RazorPagesWithBasePathTest(MvcTestFixture<RazorPagesWebSite.StartupWithBasePath> fixture)
        {
            Client = fixture.CreateDefaultClient();
        }

        public HttpClient Client { get; }

        [Fact]
        public async Task PageOutsideBasePath_IsNotRouteable()
        {
            // Act
            var response = await Client.GetAsync("/HelloWorld");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task IndexAtBasePath_IsRouteableAtRoot()
        {
            // Act
            var response = await Client.GetAsync("/");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello from /Index", content.Trim());
        }

        [Fact]
        public async Task IndexAtBasePath_IsRouteableViaIndex()
        {
            // Act
            var response = await Client.GetAsync("/Index");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello from /Index", content.Trim());
        }

        [Fact]
        public async Task IndexInSubdirectory_IsRouteableViaDirectoryName()
        {
            // Act
            var response = await Client.GetAsync("/Admin/Index");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello from /Admin/Index", content.Trim());
        }

        [Fact]
        public async Task PageWithRouteTemplateInSubdirectory_IsRouteable()
        {
            // Act
            var response = await Client.GetAsync("/Admin/RouteTemplate/1/MyRouteSuffix/");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello from /Admin/RouteTemplate 1", content.Trim());
        }

        [Fact]
        public async Task PageWithRouteTemplateInSubdirectory_IsRouteable_WithOptionalParameters()
        {
            // Act
            var response = await Client.GetAsync("/Admin/RouteTemplate/my-user-id/MyRouteSuffix/4");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello from /Admin/RouteTemplate my-user-id 4", content.Trim());
        }

        [Fact]
        public async Task AuthConvention_IsAppliedOnBasePathRelativePaths_ForFiles()
        {
            // Act
            var response = await Client.GetAsync("/Conventions/Auth");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Login?ReturnUrl=%2FConventions%2FAuth", response.Headers.Location.PathAndQuery);
        }

        [Fact]
        public async Task AuthConvention_IsAppliedOnBasePathRelativePaths_For_Folders()
        {
            // Act
            var response = await Client.GetAsync("/Conventions/AuthFolder");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Login?ReturnUrl=%2FConventions%2FAuthFolder", response.Headers.Location.PathAndQuery);
        }

        [Fact]
        public async Task AuthConvention_AppliedToFolders_CanByOverridenByFiltersOnModel()
        {
            // Act
            var response = await Client.GetStringAsync("/Conventions/AuthFolder/AnonymousViaModel");

            // Assert
            Assert.Equal("Hello from Anonymous", response.Trim());
        }

        [Fact]
        public async Task ViewStart_IsDiscoveredWhenRootDirectoryIsSpecified()
        {
            // Test for https://github.com/aspnet/Mvc/issues/5915
            //Arrange
            var expected = @"Hello from _ViewStart
Hello from /Pages/WithViewStart/Index.cshtml!";

            // Act
            var response = await Client.GetStringAsync("/WithViewStart");

            // Assert
            Assert.Equal(expected, response, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public async Task ViewStart_IsDiscoveredForFilesOutsidePageRoot()
        {
            //Arrange
            var expected = @"Hello from _ViewStart at root
Hello from _ViewStart
Hello from page";

            // Act
            var response = await Client.GetStringAsync("/WithViewStart/ViewStartAtRoot");

            // Assert
            Assert.Equal(expected, response.Trim(), ignoreLineEndingDifferences: true);
        }

        [Fact]
        public async Task ViewImport_IsDiscoveredWhenRootDirectoryIsSpecified()
        {
            // Test for https://github.com/aspnet/Mvc/issues/5915
            //Arrange
            var expected = "Hello from CustomService!";

            // Act
            var response = await Client.GetStringAsync("/WithViewImport");

            // Assert
            Assert.Equal(expected, response.Trim());
        }

        [Fact]
        public async Task FormTagHelper_WithPage_GeneratesLinksToSelf()
        {
            //Arrange
            var expected = "<form method=\"POST\" action=\"/TagHelper/SelfPost/10\">";

            // Act
            var response = await Client.GetStringAsync("/TagHelper/SelfPost");

            // Assert
            Assert.Contains(expected, response.Trim());
        }

        [Fact]
        public async Task FormTagHelper_WithPageHandler_AllowsPostingToSelf()
        {
            //Arrange
            var expected =
@"<form action=""/TagHelper/PostWithHandler/Edit"" method=""post""><input name=""__RequestVerificationToken"" type=""hidden"" value=""{0}"" /></form>
<form method=""post"" action=""/TagHelper/PostWithHandler/Edit""><input name=""__RequestVerificationToken"" type=""hidden"" value=""{0}"" /></form>
<form method=""post"" action=""/TagHelper/PostWithHandler/Edit/10""></form>";

            // Act
            var response = await Client.GetStringAsync("/TagHelper/PostWithHandler");

            // Assert
            var responseContent = response.Trim();
            var forgeryToken = AntiforgeryTestHelper.RetrieveAntiforgeryToken(responseContent, "/TagHelper/PostWithHandler");
            var expectedContent = string.Format(expected, forgeryToken);

            Assert.Equal(expectedContent, responseContent, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public async Task FormTagHelper_WithPage_AllowsPostingToAnotherPage()
        {
            //Arrange
            var expected =
@"<form method=""POST"" action=""/TagHelper/SelfPost/10""></form>
<form method=""POST"" action=""/TagHelper/PostWithHandler/Delete/10""></form>";

            // Act
            var response = await Client.GetStringAsync("/TagHelper/CrossPost");

            // Assert
            Assert.Equal(expected, response.Trim(), ignoreLineEndingDifferences: true);
        }

        [Fact]
        public async Task FormActionTagHelper_WithPage_AllowsPostingToAnotherPage()
        {
            //Arrange
            var expected =
@"<button formaction=""/TagHelper/CrossPost/10"" />
<input type=""submit"" formaction=""/TagHelper/CrossPost/10"" />
<input type=""image"" formaction=""/TagHelper/CrossPost/10"" />
<button formaction=""/TagHelper/PostWithHandler/Edit/11"" />
<input type=""submit"" formaction=""/TagHelper/PostWithHandler/Edit/11"" />
<input type=""image"" formaction=""/TagHelper/PostWithHandler/Delete/11"" />";

            // Act
            var response = await Client.GetStringAsync("/TagHelper/FormAction");

            // Assert
            Assert.Equal(expected, response, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public async Task RedirectFromPage_RedirectsToPathWithoutIndexSegment()
        {
            //Arrange
            var expected = "/Redirects";

            // Act
            var response = await Client.GetAsync("/Redirects/Index");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(expected, response.Headers.Location.ToString());
        }

        [Fact]
        public async Task RedirectFromPage_ToIndex_RedirectsToPathWithoutIndexSegment()
        {
            //Arrange
            var expected = "/Redirects";

            // Act
            var response = await Client.GetAsync("/Redirects/RedirectToIndex");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(expected, response.Headers.Location.ToString());
        }

        [Fact]
        public async Task PageRoute_UsingDefaultPageNameToRoute()
        {
            // Arrange
            var expected = @"<a href=""/Routes/Sibling/10"">Link</a>";

            // Act
            var response = await Client.GetStringAsync("/Routes/RouteUsingDefaultName");

            // Assert
            Assert.Equal(expected, response.Trim());
        }

        [Fact]
        public async Task Pages_ReturnsFromPagesSharedDirectory()
        {
            // Arrange
            var expected = "Hello from /Pages/Shared/";

            // Act
            var response = await Client.GetStringAsync("/SearchInPages");

            // Assert
            Assert.Equal(expected, response.Trim());
        }

        [Fact]
        public async Task PagesInAreas_Work()
        {
            // Arrange
            var expected = "Hello from a page in Accounts area";

            // Act
            var response = await Client.GetStringAsync("/Accounts/About");

            // Assert
            Assert.Equal(expected, response.Trim());
        }

        [Fact]
        public async Task PagesInAreas_CanHaveRouteTemplates()
        {
            // Arrange
            var expected = "The id is 42";

            // Act
            var response = await Client.GetStringAsync("/Accounts/PageWithRouteTemplate/42");

            // Assert
            Assert.Equal(expected, response.Trim());
        }

        [Fact]
        public async Task PagesInAreas_CanGenerateLinksToControllersAndPages()
        {
            // Arrange
            var expected =
@"<a href=""/Accounts/Manage/RenderPartials"">Link inside area</a>
<a href=""/Products/List/old/20"">Link to external area</a>
<a href=""/Accounts"">Link to area action</a>
<a href=""/Admin"">Link to non-area page</a>";

            // Act
            var response = await Client.GetStringAsync("/Accounts/PageWithLinks");

            // Assert
            Assert.Equal(expected, response.Trim());
        }

        [Fact]
        public async Task PagesInAreas_CanGenerateRelativeLinks()
        {
            // Arrange
            var expected =
@"<a href=""/Accounts/PageWithRouteTemplate/1"">Parent directory</a>
<a href=""/Accounts/Manage/RenderPartials"">Sibling directory</a>
<a href=""/Products/List"">Go back to root of different area</a>";

            // Act
            var response = await Client.GetStringAsync("/Accounts/RelativeLinks");

            // Assert
            Assert.Equal(expected, response.Trim());
        }

        [Fact]
        public async Task PagesInAreas_CanDiscoverViewsFromAreaAndSharedDirectories()
        {
            // Arrange
            var expected =
@"Layout in /Views/Shared
Partial in /Areas/Accounts/Pages/Manage/

Partial in /Areas/Accounts/Pages/

Partial in /Areas/Accounts/Pages/

Partial in /Areas/Accounts/Views/Shared/

Hello from /Pages/Shared/";

            // Act
            var response = await Client.GetStringAsync("/Accounts/Manage/RenderPartials");

            // Assert
            Assert.Equal(expected, response.Trim());
        }

        [Fact]
        public async Task AuthorizeFolderConvention_CanBeAppliedToAreaPages()
        {
            // Act
            var response = await Client.GetAsync("/Accounts/RequiresAuth");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Login?ReturnUrl=%2FAccounts%2FRequiresAuth", response.Headers.Location.PathAndQuery);
        }

        [Fact]
        public async Task AllowAnonymouseToPageConvention_CanBeAppliedToAreaPages()
        {
            // Act
            var response = await Client.GetStringAsync("/Accounts/RequiresAuth/AllowAnonymous");

            // Assert
            Assert.Equal("Hello from AllowAnonymous", response.Trim());
        }

        // These test is important as it covers a feature that allows razor pages to use a different
        // model at runtime that wasn't known at compile time. Like a non-generic model used at compile
        // time and overrided at runtime with a closed-generic model that performs the actual implementation.
        // An example of this is how the Identity UI library defines a base page model in their views,
        // like how the Register.cshtml view defines its model as RegisterModel and then, at runtime it replaces
        // that model with RegisterModel<TUser> where TUser is the type of the user used to configure identity.
        [Fact]
        public async Task PageConventions_CanBeUsedToCustomizeTheModelType()
        {
            // Act
            var response = await Client.GetAsync("/CustomModelTypeModel");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("<h2>User</h2>", content);
        }

        [Fact]
        public async Task PageConventions_CustomizedModelCanPostToHandlers()
        {
            // Arrange
            var getPage = await Client.GetAsync("/CustomModelTypeModel");
            var token = AntiforgeryTestHelper.RetrieveAntiforgeryToken(await getPage.Content.ReadAsStringAsync(), "");
            var cookie = AntiforgeryTestHelper.RetrieveAntiforgeryCookie(getPage);

            var message = new HttpRequestMessage(HttpMethod.Post, "/CustomModelTypeModel");
            message.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["ConfirmPassword"] = "",
                ["Password"] = "",
                ["Email"] = ""
            });
            message.Headers.TryAddWithoutValidation("Cookie", $"{cookie.Key}={cookie.Value}");

            // Act
            var response = await Client.SendAsync(message);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("is required.", content);
        }

        [Fact]
        public async Task PageConventions_CustomizedModelCanWorkWithModelState()
        {
            // Arrange
            var getPage = await Client.GetAsync("/CustomModelTypeModel");
            var token = AntiforgeryTestHelper.RetrieveAntiforgeryToken(await getPage.Content.ReadAsStringAsync(), "");
            var cookie = AntiforgeryTestHelper.RetrieveAntiforgeryCookie(getPage);

            var message = new HttpRequestMessage(HttpMethod.Post, "/CustomModelTypeModel");
            message.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Email"] = "javi@example.com",
                ["Password"] = "Password.12$",
                ["ConfirmPassword"] = "Password.12$",
            });
            message.Headers.TryAddWithoutValidation("Cookie", $"{cookie.Key}={cookie.Value}");

            // Act
            var response = await Client.SendAsync(message);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task ValidationAttributes_OnTopLevelProperties()
        {
            // Act
            var response = await Client.GetStringAsync("/Validation/PageWithValidation?age=71");

            // Assert
            Assert.Contains("Name is required", response);
            Assert.Contains("18 &#x2264; Age &#x2264; 60", response);
        }

        [Fact]
        public async Task ValidationAttributes_OnHandlerParameters()
        {
            // Act
            var response = await Client.GetStringAsync("/Validation/PageHandlerWithValidation");

            // Assert
            Assert.Contains("Name is required", response);
        }
    }
}
