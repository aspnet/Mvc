// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BasicWebSite;
using BasicWebSite.Models;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class ApiControllerAttributeTests : IClassFixture<MvcTestFixture<BasicWebSite.Startup>>
    {
        public ApiControllerAttributeTests(MvcTestFixture<BasicWebSite.Startup> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        [Fact]
        public async Task ActionsReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var contactModel = new Contact
            {
                Name = "Abc",
                City = "Redmond",
                State = "WA",
                Zip = "Invalid",
            };
            var expected = new ValidationProblemDetails
            {
                Errors =
                {
                    ["Zip"] = new[] { @"The field Zip must match the regular expression '\d{5}'."  },
                    ["Name"] = new[] { "The field Name must be a string with a minimum length of 5 and a maximum length of 30." },
                },
            };
            var contactString = JsonConvert.SerializeObject(contactModel);

            // Act
            var response = await Client.PostAsJsonAsync("/contact", contactModel);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType.MediaType);
            var actual = JsonConvert.DeserializeObject<ValidationProblemDetails>(await response.Content.ReadAsStringAsync());
            Assert.Equal(expected.Errors.Count, actual.Errors.Count);
            foreach (var error in expected.Errors)
            {
                Assert.Equal(error.Value, actual.Errors[error.Key]);
            }
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
            var expected = new[]
            {
                new VndError
                {
                    Path = "Name",
                    Message = "The field Name must be a string with a minimum length of 5 and a maximum length of 30.",
                },
                new VndError
                {
                    Path = "Zip",
                    Message =  @"The field Zip must match the regular expression '\d{5}'.",
                },
            };
            var contactString = JsonConvert.SerializeObject(contactModel);

            // Act
            var response = await Client.PostAsJsonAsync("/contact/PostWithVnd", contactModel);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("application/vnd.error+json", response.Content.Headers.ContentType.MediaType);
            var actual = JsonConvert.DeserializeObject<VndError[]>(await response.Content.ReadAsStringAsync());
            actual = actual.OrderBy(e => e.Path).ToArray();
            Assert.Equal(expected.Length, actual.Length);
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i].Path, expected[i].Path);
                Assert.Equal(expected[i].Message, expected[i].Message);
            }
        }
    }
}
