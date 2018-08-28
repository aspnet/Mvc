// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using Microsoft.AspNetCore.Testing.xunit;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class XmlDataContractSerializerFormattersWrappingTest : IClassFixture<MvcTestFixture<XmlFormattersWebSite.Startup>>
    {
        public XmlDataContractSerializerFormattersWrappingTest(MvcTestFixture<XmlFormattersWebSite.Startup> fixture)
        {
            Client = fixture.CreateDefaultClient();
        }

        public HttpClient Client { get; }

        [ConditionalTheory]
        // Mono issue - https://github.com/aspnet/External/issues/18
        [FrameworkSkipCondition(RuntimeFrameworks.Mono)]
        [InlineData("http://localhost/IEnumerable/ValueTypes")]
        [InlineData("http://localhost/IQueryable/ValueTypes")]
        public async Task CanWrite_ValueTypes(string url)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml-dcs"));

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal(
                "<ArrayOfint xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                " xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\">" +
                "<int>10</int><int>20</int></ArrayOfint>",
                result);
        }

        [ConditionalTheory]
        // Mono issue - https://github.com/aspnet/External/issues/18
        [FrameworkSkipCondition(RuntimeFrameworks.Mono)]
        [InlineData("http://localhost/IEnumerable/NonWrappedTypes")]
        [InlineData("http://localhost/IQueryable/NonWrappedTypes")]
        public async Task CanWrite_NonWrappedTypes(string url)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml-dcs"));

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal(
                "<ArrayOfstring xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                " xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\">" +
                "<string>value1</string><string>value2</string></ArrayOfstring>",
                result);
        }

        [ConditionalTheory]
        // Mono issue - https://github.com/aspnet/External/issues/18
        [FrameworkSkipCondition(RuntimeFrameworks.Mono)]
        [InlineData("http://localhost/IEnumerable/NonWrappedTypes_Empty")]
        [InlineData("http://localhost/IQueryable/NonWrappedTypes_Empty")]
        public async Task CanWrite_NonWrappedTypes_Empty(string url)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml-dcs"));

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal(
                "<ArrayOfstring xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                " xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\" />",
                result);
        }

        [ConditionalTheory]
        // Mono issue - https://github.com/aspnet/External/issues/18
        [FrameworkSkipCondition(RuntimeFrameworks.Mono)]
        [InlineData("http://localhost/IEnumerable/NonWrappedTypes_NullInstance")]
        [InlineData("http://localhost/IQueryable/NonWrappedTypes_NullInstance")]
        public async Task CanWrite_NonWrappedTypes_NullInstance(string url)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml-dcs"));

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal(
                "<ArrayOfstring i:nil=\"true\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                " xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\" />",
                result);
        }

        [ConditionalTheory]
        // Mono issue - https://github.com/aspnet/External/issues/18
        [FrameworkSkipCondition(RuntimeFrameworks.Mono)]
        [InlineData("http://localhost/IEnumerable/WrappedTypes")]
        [InlineData("http://localhost/IQueryable/WrappedTypes")]
        public async Task CanWrite_WrappedTypes(string url)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml-dcs"));

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal(
                "<ArrayOfPersonWrapper xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                " xmlns=\"http://schemas.datacontract.org/2004/07/XmlFormattersWebSite\"><PersonWrapper>" +
                "<Age>35</Age><Id>10</Id><Name>Mike</Name></PersonWrapper><PersonWrapper><Age>35</Age><Id>" +
                "11</Id><Name>Jimmy</Name></PersonWrapper></ArrayOfPersonWrapper>",
                result);
        }

        [ConditionalTheory]
        // Mono issue - https://github.com/aspnet/External/issues/18
        [FrameworkSkipCondition(RuntimeFrameworks.Mono)]
        [InlineData("http://localhost/IEnumerable/WrappedTypes_Empty")]
        [InlineData("http://localhost/IQueryable/WrappedTypes_Empty")]
        public async Task CanWrite_WrappedTypes_Empty(string url)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml-dcs"));

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal(
                "<ArrayOfPersonWrapper xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                " xmlns=\"http://schemas.datacontract.org/2004/07/XmlFormattersWebSite\" />",
                result);
        }

        [ConditionalTheory]
        // Mono issue - https://github.com/aspnet/External/issues/18
        [FrameworkSkipCondition(RuntimeFrameworks.Mono)]
        [InlineData("http://localhost/IEnumerable/WrappedTypes_NullInstance")]
        [InlineData("http://localhost/IQueryable/WrappedTypes_NullInstance")]
        public async Task CanWrite_WrappedTypes_NullInstance(string url)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml-dcs"));

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal(
                "<ArrayOfPersonWrapper i:nil=\"true\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                " xmlns=\"http://schemas.datacontract.org/2004/07/XmlFormattersWebSite\" />",
                result);
        }

        [ConditionalFact]
        // Mono issue - https://github.com/aspnet/External/issues/18
        [FrameworkSkipCondition(RuntimeFrameworks.Mono)]
        public async Task CanWrite_IEnumerableOf_SerializableErrors()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/IEnumerable/SerializableErrors");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml-dcs"));

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal(
                "<ArrayOfSerializableErrorWrapper xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                " xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.AspNetCore.Mvc.Formatters.Xml\"><SerializableErrorWrapper>" +
                "<key1>key1-error</key1><key2>key2-error</key2></SerializableErrorWrapper><SerializableErrorWrapper>" +
                "<key3>key1-error</key3><key4>key2-error</key4></SerializableErrorWrapper>" +
                "</ArrayOfSerializableErrorWrapper>",
                result);
        }

        [Fact]
        public async Task ProblemDetails_IsSerialized()
        {
            // Arrange
            var expected = @"<ProblemDetails><Status>404</Status><Title>Not Found</Title><Type>https://tools.ietf.org/html/rfc7231#section-6.5.4</Type></ProblemDetails>";

            // Act
            var response = await Client.GetAsync("/api/XmlDataContractApi/ActionReturningClientErrorStatusCodeResult");

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
            var response = await Client.GetAsync("/api/XmlDataContractApi/ActionReturningProblemDetails");

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
            var response = await Client.GetAsync("/api/XmlDataContractApi/ActionReturningValidationProblem");

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
            var response = await Client.GetAsync("/api/XmlDataContractApi/ActionReturningValidationDetailsWithMetadata");

            // Assert
            await response.AssertStatusCodeAsync(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            XmlAssert.Equal(expected, content);
        }
    }
}
