// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BasicWebSite.Models;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class ApiBehaviorTest : IClassFixture<MvcTestFixture<BasicWebSite.Startup>>
    {
        public ApiBehaviorTest(MvcTestFixture<BasicWebSite.Startup> fixture)
        {
            Client = fixture.CreateDefaultClient();

            var factory = fixture.WithWebHostBuilder(ConfigureWebHostBuilder);
            CustomInvalidModelStateClient = factory.CreateDefaultClient();
        }

        private static void ConfigureWebHostBuilder(IWebHostBuilder builder) =>
            builder.UseStartup<BasicWebSite.StartupWithCustomInvalidModelStateFactory>();

        public HttpClient Client { get; }
        public HttpClient CustomInvalidModelStateClient { get; }

        [Fact]
        public async Task ActionsReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            using (new ActivityReplacer())
            {
                var contactModel = new Contact
                {
                    Name = "Abc",
                    City = "Redmond",
                    State = "WA",
                    Zip = "Invalid",
                };
                var contactString = JsonConvert.SerializeObject(contactModel);

                // Act
                var response = await Client.PostAsJsonAsync("/contact", contactModel);

                // Assert
                await response.AssertStatusCodeAsync(HttpStatusCode.BadRequest);
                Assert.Equal("application/problem+json", response.Content.Headers.ContentType.MediaType);
                var problemDetails = JsonConvert.DeserializeObject<ValidationProblemDetails>(await response.Content.ReadAsStringAsync());
                Assert.Collection(
                    problemDetails.Errors.OrderBy(kvp => kvp.Key),
                    kvp =>
                    {
                        Assert.Equal("Name", kvp.Key);
                        var error = Assert.Single(kvp.Value);
                        Assert.Equal("The field Name must be a string with a minimum length of 5 and a maximum length of 30.", error);
                    },
                    kvp =>
                    {
                        Assert.Equal("Zip", kvp.Key);
                        var error = Assert.Single(kvp.Value);
                        Assert.Equal("The field Zip must match the regular expression '\\d{5}'.", error);
                    }
                );

                Assert.Collection(
                    problemDetails.Extensions,
                    kvp =>
                    {
                        Assert.Equal("traceId", kvp.Key);
                        Assert.Equal(Activity.Current.Id, kvp.Value);
                    });
            }
        }

        [Fact]
        public async Task ActionsReturnUnsupportedMediaType_WhenMediaTypeIsNotSupported()
        {
            // Arrange
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/contact")
            {
                Content = new StringContent("some content", Encoding.UTF8, "text/css"),
            };

            // Act
            var response = await Client.SendAsync(requestMessage);

            // Assert
            await response.AssertStatusCodeAsync(HttpStatusCode.UnsupportedMediaType);
        }

        [Fact]
        public async Task ActionsReturnUnsupportedMediaType_WhenEncodingIsUnsupported()
        {
            // Arrange
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/contact")
            {
                Content = new StringContent("some content", Encoding.UTF7, "application/json"),
            };

            // Act
            var response = await Client.SendAsync(requestMessage);

            // Assert
            await response.AssertStatusCodeAsync(HttpStatusCode.UnsupportedMediaType);
            var content = await response.Content.ReadAsStringAsync();
            var problemDetails = JsonConvert.DeserializeObject<ProblemDetails>(content);
            Assert.Equal((int)HttpStatusCode.UnsupportedMediaType, problemDetails.Status);
            Assert.Equal("Unsupported Media Type", problemDetails.Title);
        }

        [Fact]
        public async Task ActionsReturnBadRequest_UsesProblemDescriptionProviderAndApiConventionsToConfigureErrorResponse()
        {
            // Arrange
            var contactModel = new Contact
            {
                Name = "Abc",
                City = "Redmond",
                State = "WA",
                Zip = "Invalid",
            };
            var expected = new Dictionary<string, string[]>
            {
                {"Name", new[] {"The field Name must be a string with a minimum length of 5 and a maximum length of 30."}},
                {"Zip", new[] { @"The field Zip must match the regular expression '\d{5}'."}}
            };
            var contactString = JsonConvert.SerializeObject(contactModel);

            // Act
            var response = await CustomInvalidModelStateClient.PostAsJsonAsync("/contact/PostWithVnd", contactModel);

            // Assert
            await response.AssertStatusCodeAsync(HttpStatusCode.BadRequest);
            Assert.Equal("application/vnd.error+json", response.Content.Headers.ContentType.MediaType);
            var content = await response.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(content);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public Task ActionsWithApiBehavior_InferFromBodyParameters()
            => ActionsWithApiBehaviorInferFromBodyParameters("ActionWithInferredFromBodyParameter");

        [Fact]
        public Task ActionsWithApiBehavior_InferFromBodyParameters_DoNotConsiderCancellationTokenSourceParameter()
            => ActionsWithApiBehaviorInferFromBodyParameters("ActionWithInferredFromBodyParameterAndCancellationToken");

        private async Task ActionsWithApiBehaviorInferFromBodyParameters(string action)
        {
            // Arrange
            var input = new Contact
            {
                ContactId = 13,
                Name = "Test123",
            };

            // Act
            var response = await Client.PostAsJsonAsync($"/contact/{action}", input);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = JsonConvert.DeserializeObject<Contact>(await response.Content.ReadAsStringAsync());
            Assert.Equal(input.ContactId, result.ContactId);
            Assert.Equal(input.Name, result.Name);
        }

        [Fact]
        public async Task ActionsWithApiBehavior_InferQueryAndRouteParameters()
        {
            // Arrange
            var id = 31;
            var name = "test";
            var email = "email@test.com";
            var url = $"/contact/ActionWithInferredRouteAndQueryParameters/{name}/{id}?email={email}";
            var response = await Client.PostAsync(url, new StringContent(string.Empty));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = JsonConvert.DeserializeObject<Contact>(await response.Content.ReadAsStringAsync());
            Assert.Equal(id, result.ContactId);
            Assert.Equal(name, result.Name);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task ActionsWithApiBehavior_InferEmptyPrefixForComplexValueProviderModel_Success()
        {
            // Arrange
            var id = 31;
            var name = "test_user";
            var email = "email@test.com";
            var url = $"/contact/ActionWithInferredEmptyPrefix?name={name}&contactid={id}&email={email}";

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsAsync<Contact>();
            Assert.Equal(id, result.ContactId);
            Assert.Equal(name, result.Name);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task ActionsWithApiBehavior_InferEmptyPrefixForComplexValueProviderModel_Ignored()
        {
            // Arrange
            var id = 31;
            var name = "test_user";
            var email = "email@test.com";
            var url = $"/contact/ActionWithInferredEmptyPrefix?contact.name={name}&contact.contactid={id}&contact.email={email}";

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsAsync<Contact>();
            Assert.Equal(id, result.ContactId);
            Assert.Equal(name, result.Name);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task ActionsWithApiBehavior_InferModelBinderType()
        {
            // Arrange
            var expected = "From TestModelBinder: Hello!";

            // Act
            var response = await Client.GetAsync("/contact/ActionWithInferredModelBinderType?foo=Hello!");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ActionsWithApiBehavior_InferModelBinderTypeWithExplicitModelName()
        {
            // Arrange
            var expected = "From TestModelBinder: Hello!";

            // Act
            var response = await Client.GetAsync("/contact/ActionWithInferredModelBinderTypeWithExplicitModelName?bar=Hello!");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ClientErrorResultFilterExecutesForStatusCodeResults()
        {
            using (new ActivityReplacer())
            {
                // Act
                var response = await Client.GetAsync("/contact/ActionReturningStatusCodeResult");

                // Assert
                await response.AssertStatusCodeAsync(HttpStatusCode.NotFound);
                var content = await response.Content.ReadAsStringAsync();
                var problemDetails = JsonConvert.DeserializeObject<ProblemDetails>(content);
                Assert.Equal(404, problemDetails.Status);
                Assert.Collection(
                    problemDetails.Extensions,
                    kvp =>
                    {
                        Assert.Equal("traceId", kvp.Key);
                        Assert.Equal(Activity.Current.Id, kvp.Value);
                    });
            }
        }

        [Fact]
        public async Task SerializingProblemDetails_IgnoresNullValuedProperties()
        {
            // Arrange
            var expected = new[] { "status", "title", "traceId", "type" };

            // Act
            var response = await Client.GetAsync("/contact/ActionReturningStatusCodeResult");

            // Assert
            await response.AssertStatusCodeAsync(HttpStatusCode.NotFound);
            var content = await response.Content.ReadAsStringAsync();

            // Verify that null-valued properties on ProblemDetails are not serialized.
            var json = JObject.Parse(content);
            Assert.Equal(expected, json.Properties().OrderBy(p => p.Name).Select(p => p.Name));
        }

        [Fact]
        public async Task SerializingProblemDetails_WithAllValuesSpecified()
        {
            // Arrange
            var expected = new[] { "detail", "instance", "status", "title", "tracking-id", "type" };

            // Act
            var response = await Client.GetAsync("/contact/ActionReturningProblemDetails");

            // Assert
            await response.AssertStatusCodeAsync(HttpStatusCode.NotFound);
            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            Assert.Equal(expected, json.Properties().OrderBy(p => p.Name).Select(p => p.Name));
        }

        [Fact]
        public async Task SerializingValidationProblemDetails_WithExtensionData()
        {
            // Act
            var response = await Client.GetAsync("/contact/ActionReturningValidationProblemDetails");

            // Assert
            await response.AssertStatusCodeAsync(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            var validationProblemDetails = JsonConvert.DeserializeObject<ValidationProblemDetails>(content);

            Assert.Equal("Error", validationProblemDetails.Title);
            Assert.Equal(400, validationProblemDetails.Status);
            Assert.Collection(
                validationProblemDetails.Extensions,
                kvp =>
                {
                    Assert.Equal("tracking-id", kvp.Key);
                    Assert.Equal("27", kvp.Value);
                });

            Assert.Collection(
                validationProblemDetails.Errors,
                kvp =>
                {
                    Assert.Equal("Error1", kvp.Key);
                    Assert.Equal(new[] { "Error Message" }, kvp.Value);
                });
        }
    }
}
