// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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

            var actionContext = GetActionContext(contentBytes);
            var metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(JsonPatchDocument<Customer>));
            var context = new InputFormatterContext(actionContext, metadata.ModelType);

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

            var actionContext = GetActionContext(contentBytes);
            var metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(JsonPatchDocument<Customer>));
            var context = new InputFormatterContext(actionContext, metadata.ModelType);

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
        public void CanRead_ReturnsTrueOnlyForJsonPatchContentType(string requestContentType, bool expectedCanRead)
        {
            // Arrange
            var formatter = new JsonPatchInputFormatter();
            var content = "[{\"op\": \"add\", \"path\" : \"Customer/Name\", \"value\":\"John\"}]";
            var contentBytes = Encoding.UTF8.GetBytes(content);

            var actionContext = GetActionContext(contentBytes, contentType: requestContentType);
            var metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(JsonPatchDocument<Customer>));
            var formatterContext = new InputFormatterContext(actionContext, metadata.ModelType);

            // Act
            var result = formatter.CanRead(formatterContext);

            // Assert
            Assert.Equal(expectedCanRead, result);
        }

        [Fact]
        public void CanRead_ReturnsFalse_NonJsonPatchContentType()
        {
            // Arrange
            var formatter = new JsonPatchInputFormatter();
            var content = "[{\"op\": \"add\", \"path\" : \"Customer/Name\", \"value\":\"John\"}]";
            var contentBytes = Encoding.UTF8.GetBytes(content);

            var actionContext = GetActionContext(contentBytes, contentType: "application/json-patch+json");
            var metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(Customer));
            var formatterContext = new InputFormatterContext(actionContext, metadata.ModelType);

            // Act
            var result = formatter.CanRead(formatterContext);

            // Assert
            Assert.False(result);
        }

        private static ActionContext GetActionContext(byte[] contentBytes,
            string contentType = "application/json-patch+json")
        {
            return new ActionContext(GetHttpContext(contentBytes, contentType),
                                     new AspNet.Routing.RouteData(),
                                     new ActionDescriptor());
        }

        private static HttpContext GetHttpContext(byte[] contentBytes,
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

        public class Customer
        {
            public string Name { get; set; }
        }
    }
}