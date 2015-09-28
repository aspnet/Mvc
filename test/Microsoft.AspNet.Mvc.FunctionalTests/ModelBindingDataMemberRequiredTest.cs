// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ModelBindingWebSite;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class ModelBindingDataMemberRequiredTest : IClassFixture<MvcTestFixture<ModelBindingWebSite.Startup>>
    {
        public ModelBindingDataMemberRequiredTest(MvcTestFixture<ModelBindingWebSite.Startup> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        [Fact]
        public async Task DataMember_MissingRequiredProperty_ValidationError()
        {
            // Arrange
            var url = "http://localhost/DataMemberRequired/EchoModelValues";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("model.ExplicitlyOptionalProperty", "Hi"),
            };

            request.Content = new FormUrlEncodedContent(formData);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var errors = JsonConvert.DeserializeObject<SerializableError>(body);

            Assert.Equal(1, errors.Count);

            var error = Assert.Single(errors, kvp => kvp.Key == "model.RequiredProperty");
            Assert.Equal(
                "A value for the 'model.RequiredProperty' property was not provided.",
                ((JArray)error.Value)[0].Value<string>());
        }

        [Fact]
        public async Task DataMember_RequiredPropertyProvided_Success()
        {
            // Arrange
            var url = "http://localhost/DataMemberRequired/EchoModelValues";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("model.ImplicitlyOptionalProperty", "Hello"),
                new KeyValuePair<string, string>("model.ExplicitlyOptionalProperty", "World!"),
                new KeyValuePair<string, string>("model.RequiredProperty", "Required!"),
            };

            request.Content = new FormUrlEncodedContent(formData);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<DataMemberRequiredModel>(body);

            Assert.Equal("Hello", model.ImplicitlyOptionalProperty);
            Assert.Equal("World!", model.ExplicitlyOptionalProperty);
            Assert.Equal("Required!", model.RequiredProperty);
        }
    }
}