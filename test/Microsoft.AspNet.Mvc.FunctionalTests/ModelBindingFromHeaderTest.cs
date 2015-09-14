// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class ModelBindingFromHeaderTest : IClassFixture<MvcTestFixture<ModelBindingWebSite.Startup>>
    {
        public ModelBindingFromHeaderTest(MvcTestFixture<ModelBindingWebSite.Startup> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        // The action that this test hits will echo back the model-bound value
        [Theory]
        [InlineData("transactionId", "1e331f25-0869-4c87-8a94-64e6e40cb5a0")]
        [InlineData("TransaCtionId", "1e331f25-0869-4c87-8a94-64e6e40cb5a0")] // Case-Insensitive
        [InlineData("TransaCtionId", "1e331f25-0869-4c87-8a94-64e6e40cb5a0,abcd")] // Binding to string doesn't split values
        public async Task FromHeader_BindHeader_ToString_OnParameter(string headerName, string headerValue)
        {
            // Arrange
            var expected = headerValue;
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Blog/BindToStringParameter");
            request.Headers.TryAddWithoutValidation(headerName, headerValue);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Result>(body);
            Assert.Equal(expected, result.HeaderValue);
        }

        [Fact]
        public async Task FromHeader_BindHeader_ToString_OnProperty_CustomName()
        {
            // Arrange
            var title = "How to make really really good soup.";
            var tags = new string[] { "Cooking", "Recipes", "Awesome" };
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Blog/BindToProperty/CustomName");
            request.Headers.TryAddWithoutValidation("BlogTitle", title);
            request.Headers.TryAddWithoutValidation("BlogTags", string.Join(", ", tags));

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Result>(body);
            Assert.Equal(title, result.HeaderValue);
            Assert.Equal<string>(tags, result.HeaderValues);
            Assert.Empty(result.ModelStateErrors);
        }

        [Fact]
        public async Task FromHeader_NonExistingHeaderAddsValidationErrors_OnProperty_CustomName()
        {
            // Arrange
            var tags = new string[] { "Cooking", "Recipes", "Awesome" };
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Blog/BindToProperty/CustomName");
            request.Headers.TryAddWithoutValidation("BlogTags", string.Join(", ", tags));

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Result>(body);
            Assert.Equal<string>(tags, result.HeaderValues);
            var error = Assert.Single(result.ModelStateErrors);
            Assert.Equal("BlogTitle", error);
        }

        [Fact]
        public async Task FromHeader_NonExistingHeaderAddsValidationErrors_OnCollectionProperty_CustomName()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Blog/BindToProperty/CustomName");
            request.Headers.TryAddWithoutValidation("BlogTitle", "Cooking Receipes.");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Result>(body);
            Assert.Equal<string>("Cooking Receipes.", result.HeaderValue);
            var error = Assert.Single(result.ModelStateErrors);
            Assert.Equal("BlogTags", error);
        }

        // The action that this test hits will echo back the model-bound value
        [Fact]
        public async Task FromHeader_BindHeader_ToString_OnParameter_CustomName()
        {
            // Arrange
            var expected = "1e331f25-0869-4c87-8a94-64e6e40cb5a0";
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Blog/BindToStringParameter/CustomName");
            request.Headers.TryAddWithoutValidation("tId", "1e331f25-0869-4c87-8a94-64e6e40cb5a0");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Result>(body);

            Assert.Equal(expected, result.HeaderValue);
            Assert.Null(result.HeaderValues);
            Assert.Empty(result.ModelStateErrors);
        }

        // There should be no model state error for a top-level object
        [Theory]
        [InlineData("transactionId1234", "1e331f25-0869-4c87-8a94-64e6e40cb5a0")]
        public async Task FromHeader_BindHeader_ToString_OnParameter_NoValues(string headerName, string headerValue)
        {
            // Arrange
            var expected = headerValue;
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Blog/BindToStringParameter");
            request.Headers.TryAddWithoutValidation(headerName, headerValue);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Result>(body);

            Assert.Null(result.HeaderValue);
            Assert.Null(result.HeaderValues);
            Assert.Empty(result.ModelStateErrors);
        }

        // There should be no model state error for a top-level object
        [Theory]
        [InlineData("transactionId1234", "1e331f25-0869-4c87-8a94-64e6e40cb5a0")]
        public async Task FromHeader_BindHeader_ToString_OnParameter_NoValues_DefaultValue(
            string headerName,
            string headerValue)
        {
            // Arrange
            // Intentionally not setting a header value
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "http://localhost/Blog/BindToStringParameterDefaultValue");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Result>(body);

            Assert.Equal("default-value", result.HeaderValue);
            Assert.Null(result.HeaderValues);
            Assert.Empty(result.ModelStateErrors);
        }

        // The action that this test hits will echo back the model-bound values
        [Theory]
        [InlineData("transactionIds", "1e331f25-0869-4c87-8a94-64e6e40cb5a0")]
        [InlineData("transactionIds", "1e331f25-0869-4c87-8a94-64e6e40cb5a0,abcd,efg")]
        public async Task FromHeader_BindHeader_ToStringArray_OnParameter(string headerName, string headerValue)
        {
            // Arrange
            var expected = headerValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Blog/BindToStringArrayParameter");
            request.Headers.TryAddWithoutValidation(headerName, headerValue);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Result>(body);

            Assert.Null(result.HeaderValue);
            Assert.Equal<string>(expected, result.HeaderValues);
            Assert.Empty(result.ModelStateErrors);
        }

        // The action that this test hits will echo back the model-bound values
        [Fact]
        public async Task FromHeader_BindHeader_ToModel()
        {
            // Arrange
            var title = "How to make really really good soup.";
            var tags = new string[] { "Cooking", "Recipes", "Awesome" };
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Blog/BindToModel?author=Marvin");
            request.Headers.TryAddWithoutValidation("title", title);
            request.Headers.TryAddWithoutValidation("tags", string.Join(", ", tags));

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Result>(body);

            Assert.Equal(title, result.HeaderValue);
            Assert.Equal<string>(tags, result.HeaderValues);
            Assert.Empty(result.ModelStateErrors);
        }

        // Title on the model has [Required] so it will have a validation error
        // Tags does not, so no error.
        [Fact]
        public async Task FromHeader_BindHeader_ToModel_NoValues_ValidationError()
        {
            // Arrange
            // Intentionally not setting a title or tags
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Blog/BindToModel?author=Marvin");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Result>(body);

            Assert.Null(result.HeaderValue);
            Assert.Null(result.HeaderValues);

            var error = Assert.Single(result.ModelStateErrors);
            Assert.Equal("Title", error);
        }

        // This model sets a value for 'Title', and the model binder won't trounce it.
        //
        // There's no validation error because we validate the value on the model.
        [Fact]
        public async Task FromHeader_BindHeader_ToModel_NoValues_InitializedValue_NoValidationError()
        {
            // Arrange
            // Intentionally not setting a title or tags
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "http://localhost/Blog/BindToModelWithInitializedValue?author=Marvin");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Result>(body);

            Assert.Equal("How to Make Soup", result.HeaderValue);
            Assert.Equal<string>(new[] { "Cooking" }, result.HeaderValues);

            Assert.Empty(result.ModelStateErrors);
        }

        private class Result
        {
            public string HeaderValue { get; set; }

            public string[] HeaderValues { get; set; }

            public string[] ModelStateErrors { get; set; }
        }
    }
}