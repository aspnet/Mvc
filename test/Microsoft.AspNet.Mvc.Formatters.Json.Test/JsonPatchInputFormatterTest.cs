// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.JsonPatch;
using Microsoft.AspNet.Mvc.ModelBinding;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class JsonPatchInputFormatterTest
    {
        [Fact]
        public async Task JsonPatchInputFormatter_ReadsOneOperation_Successfully()
        {
            // Arrange
            var formatter = new JsonPatchInputFormatter();
            var content = "[{\"op\":\"add\",\"path\":\"Customer/Name\",\"value\":\"John\"}]";
            var contentBytes = Encoding.UTF8.GetBytes(content);

            var modelState = new ModelStateDictionary();
            var httpContext = GetHttpContext(contentBytes);
            var context = new InputFormatterContext(httpContext, modelState, typeof(JsonPatchDocument<Customer>));

            // Act
            var model = await formatter.ReadAsync(context);

            // Assert
            var patchDoc = Assert.IsType<JsonPatchDocument<Customer>>(model);
            Assert.Equal("add", patchDoc.Operations[0].op);
            Assert.Equal("Customer/Name", patchDoc.Operations[0].path);
            Assert.Equal("John", patchDoc.Operations[0].value);
        }

        [Fact]
        public async Task JsonPatchInputFormatter_ReadsMultipleOperations_Successfully()
        {
            // Arrange
            var formatter = new JsonPatchInputFormatter();
            var content = "[{\"op\": \"add\", \"path\" : \"Customer/Name\", \"value\":\"John\"}," +
                "{\"op\": \"remove\", \"path\" : \"Customer/Name\"}]";
            var contentBytes = Encoding.UTF8.GetBytes(content);

            var modelState = new ModelStateDictionary();
            var httpContext = GetHttpContext(contentBytes);
            var context = new InputFormatterContext(httpContext, modelState, typeof(JsonPatchDocument<Customer>));

            // Act
            var model = await formatter.ReadAsync(context);

            // Assert
            var patchDoc = Assert.IsType<JsonPatchDocument<Customer>>(model);
            Assert.Equal("add", patchDoc.Operations[0].op);
            Assert.Equal("Customer/Name", patchDoc.Operations[0].path);
            Assert.Equal("John", patchDoc.Operations[0].value);
            Assert.Equal("remove", patchDoc.Operations[1].op);
            Assert.Equal("Customer/Name", patchDoc.Operations[1].path);
        }

        [Theory]
        [InlineData("application/json-patch+json", true)]
        [InlineData("application/json", false)]
        [InlineData("application/*", true)]
        [InlineData("*/*", true)]
        public void CanRead_ReturnsTrueOnlyForJsonPatchContentType(string requestContentType, bool expectedCanRead)
        {
            // Arrange
            var formatter = new JsonPatchInputFormatter();
            var content = "[{\"op\": \"add\", \"path\" : \"Customer/Name\", \"value\":\"John\"}]";
            var contentBytes = Encoding.UTF8.GetBytes(content);

            var modelState = new ModelStateDictionary();
            var httpContext = GetHttpContext(contentBytes, contentType: requestContentType);
            var formatterContext = new InputFormatterContext(
                httpContext,
                modelState,
                typeof(JsonPatchDocument<Customer>));

            // Act
            var result = formatter.CanRead(formatterContext);

            // Assert
            Assert.Equal(expectedCanRead, result);
        }

        [Theory]
        [InlineData(typeof(Customer))]
        [InlineData(typeof(IJsonPatchDocument))]
        public void CanRead_ReturnsFalse_NonJsonPatchContentType(Type modelType)
        {
            // Arrange
            var formatter = new JsonPatchInputFormatter();
            var content = "[{\"op\": \"add\", \"path\" : \"Customer/Name\", \"value\":\"John\"}]";
            var contentBytes = Encoding.UTF8.GetBytes(content);

            var modelState = new ModelStateDictionary();
            var httpContext = GetHttpContext(contentBytes, contentType: "application/json-patch+json");
            var formatterContext = new InputFormatterContext(httpContext, modelState, modelType);

            // Act
            var result = formatter.CanRead(formatterContext);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task JsonPatchInputFormatter_ReturnsModelStateErrors_InvalidModelType()
        {
            // Arrange
            var exceptionMessage = "Cannot deserialize the current JSON array (e.g. [1,2,3]) into type " +
                "'Microsoft.AspNet.Mvc.JsonPatchInputFormatterTest+Customer' because the type requires a JSON object ";

            var formatter = new JsonPatchInputFormatter();
            var content = "[{\"op\": \"add\", \"path\" : \"Customer/Name\", \"value\":\"John\"}]";
            var contentBytes = Encoding.UTF8.GetBytes(content);

            var modelState = new ModelStateDictionary();
            var httpContext = GetHttpContext(contentBytes, contentType: "application/json-patch+json");

            var context = new InputFormatterContext(httpContext, modelState, typeof(Customer));

            // Act
            var model = await formatter.ReadAsync(context);

            // Assert
            Assert.Contains(exceptionMessage, modelState[""].Errors[0].Exception.Message);
        }

        private static HttpContext GetHttpContext(
            byte[] contentBytes,
            string contentType = "application/json-patch+json")
        {
            var request = new Mock<HttpRequest>();
            var headers = new Mock<IHeaderDictionary>();
            request.SetupGet(r => r.Headers).Returns(headers.Object);
            request.SetupGet(f => f.Body).Returns(new MemoryStream(contentBytes));
            request.SetupGet(f => f.ContentType).Returns(contentType);

            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(c => c.Request).Returns(request.Object);
            httpContext.SetupGet(c => c.Request).Returns(request.Object);
            return httpContext.Object;
        }

        private class Customer
        {
            public string Name { get; set; }
        }
    }
}