// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class XmlSerializerFormattersWrappingTest : IClassFixture<MvcTestFixture<XmlFormattersWebSite.Startup>>
    {
        public XmlSerializerFormattersWrappingTest(MvcTestFixture<XmlFormattersWebSite.Startup> fixture)
        {
            Client = fixture.CreateDefaultClient();
        }

        public HttpClient Client { get; }

        [Theory]
        [InlineData("http://localhost/IEnumerable/ValueTypes")]
        [InlineData("http://localhost/IQueryable/ValueTypes")]
        public async Task CanWrite_ValueTypes(string url)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml-xmlser"));

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal("<ArrayOfInt xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                         "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><int>10</int>" +
                         "<int>20</int></ArrayOfInt>",
                         result);
        }

        [Theory]
        [InlineData("http://localhost/IEnumerable/NonWrappedTypes")]
        [InlineData("http://localhost/IQueryable/NonWrappedTypes")]
        public async Task CanWrite_NonWrappedTypes(string url)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml-xmlser"));

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal("<ArrayOfString xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                         "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><string>value1</string>" +
                         "<string>value2</string></ArrayOfString>",
                         result);
        }

        [Theory]
        [InlineData("http://localhost/IEnumerable/NonWrappedTypes_NullInstance")]
        [InlineData("http://localhost/IQueryable/NonWrappedTypes_NullInstance")]
        public async Task CanWrite_NonWrappedTypes_NullInstance(string url)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml-xmlser"));

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal("<ArrayOfString xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                " xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xsi:nil=\"true\" />",
                result);
        }

        [Theory]
        [InlineData("http://localhost/IEnumerable/NonWrappedTypes_Empty")]
        [InlineData("http://localhost/IQueryable/NonWrappedTypes_Empty")]
        public async Task CanWrite_NonWrappedTypes_Empty(string url)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml-xmlser"));

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal("<ArrayOfString xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                " xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" />",
                result);
        }

        [Theory]
        [InlineData("http://localhost/IEnumerable/WrappedTypes")]
        [InlineData("http://localhost/IQueryable/WrappedTypes")]
        public async Task CanWrite_WrappedTypes(string url)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml-xmlser"));

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal("<ArrayOfPersonWrapper xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                         "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><PersonWrapper><Id>10</Id>" +
                         "<Name>Mike</Name><Age>35</Age></PersonWrapper><PersonWrapper><Id>11</Id>" +
                         "<Name>Jimmy</Name><Age>35</Age></PersonWrapper></ArrayOfPersonWrapper>",
                         result);
        }

        [Theory]
        [InlineData("http://localhost/IEnumerable/WrappedTypes_Empty")]
        [InlineData("http://localhost/IQueryable/WrappedTypes_Empty")]
        public async Task CanWrite_WrappedTypes_Empty(string url)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml-xmlser"));

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal("<ArrayOfPersonWrapper xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                " xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" />",
                result);
        }


        [Theory]
        [InlineData("http://localhost/IEnumerable/WrappedTypes_NullInstance")]
        [InlineData("http://localhost/IQueryable/WrappedTypes_NullInstance")]
        public async Task CanWrite_WrappedTypes_NullInstance(string url)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml-xmlser"));

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal("<ArrayOfPersonWrapper xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                " xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xsi:nil=\"true\" />",
                result);
        }

        [Fact]
        public async Task CanWrite_IEnumerableOf_SerializableErrors()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/IEnumerable/SerializableErrors");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml-xmlser"));

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal("<ArrayOfSerializableErrorWrapper xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                " xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><SerializableErrorWrapper><key1>key1-error</key1>" +
                "<key2>key2-error</key2></SerializableErrorWrapper><SerializableErrorWrapper><key3>key1-error</key3>" +
                "<key4>key2-error</key4></SerializableErrorWrapper></ArrayOfSerializableErrorWrapper>",
                result);
        }

        [Fact]
        public async Task ProblemDetails_IsSerialized()
        {
            // Arrange
            var expected = @"<ProblemDetails><Status>404</Status><Title>Not Found</Title><Type>https://tools.ietf.org/html/rfc7231#section-6.5.4</Type></ProblemDetails>";

            // Act
            var response = await Client.GetAsync("/api/XmlSerializerApi/ActionReturningClientErrorStatusCodeResult");

            // Assert
            await response.AssertStatusCodeAsync(HttpStatusCode.NotFound);
            var content = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal(expected, content);
        }

        [Fact]
        public async Task ProblemDetails_WithExtensionMembers_IsSerialized()
        {
            // Arrange
            var expected = @"<ProblemDetails><Instance>instance</Instance><Status>404</Status><Title>title</Title>
<Correlation>correlation</Correlation><Accounts>Account1 Account2</Accounts></ProblemDetails>";

            // Act
            var response = await Client.GetAsync("/api/XmlSerializerApi/ActionReturningProblemDetails");

            // Assert
            await response.AssertStatusCodeAsync(HttpStatusCode.NotFound);
            var content = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal(expected, content);
        }

        [Fact]
        public async Task ValidationProblemDetails_IsSerialized()
        {
            // Arrange
            var expected = @"<ValidationProblemDetails><Status>400</Status><Title>One or more validation errors occurred.</Title>
<MVC-Errors><State>The State field is required.</State></MVC-Errors></ValidationProblemDetails>";

            // Act
            var response = await Client.GetAsync("/api/XmlSerializerApi/ActionReturningValidationProblem");

            // Assert
            await response.AssertStatusCodeAsync(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal(expected, content);
        }

        [Fact]
        public async Task ValidationProblemDetails_WithExtensionMembers_IsSerialized()
        {
            // Arrange
            var expected = @"<ValidationProblemDetails><Detail>some detail</Detail><Status>400</Status><Title>One or more validation errors occurred.</Title>
<Type>some type</Type><CorrelationId>correlation</CorrelationId><MVC-Errors><Error1>ErrorValue</Error1></MVC-Errors></ValidationProblemDetails>";

            // Act
            var response = await Client.GetAsync("/api/XmlSerializerApi/ActionReturningValidationDetailsWithMetadata");

            // Assert
            await response.AssertStatusCodeAsync(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal(expected, content);
        }
    }
}
