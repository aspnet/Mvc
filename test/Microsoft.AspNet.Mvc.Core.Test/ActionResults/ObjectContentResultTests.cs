// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Internal;
using Microsoft.AspNet.Routing;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test.ActionResults
{
    public class ObjectContentResultTests
    {
        [Fact]
        public void ObjectContentResult_Execute_CallsContentResult_AccessValue()
        {
            // Arrange
            var input = "testInput";
            var actionContext = GetMockActionContext();

            // Act
            var result = new ObjectContentResult(input);

            // Assert
            Assert.Equal(input, result.Value);
        }

        [Fact]
        public async Task ObjectContentResult_Execute_CallsContentResult_SetsContent()
        {
            // Arrange
            var expectedContentType = "text/plain";
            var input = "testInput";
            var httpResponse = new Mock<HttpResponse>();
            httpResponse.SetupSet(r => r.ContentType = expectedContentType).Verifiable();
            httpResponse.Object.Body = new MemoryStream();
            var actionContext = GetMockActionContext(httpResponse.Object);

            // Act
            var result = new ObjectContentResult(input);
            await result.ExecuteResultAsync(actionContext);

            // Assert
            httpResponse.VerifySet(r => r.ContentType = expectedContentType);
            // The following verifies the content correct Content was written to Body
            httpResponse.Verify(o => o.WriteAsync(input), Times.Exactly(1));
        }

        [Fact]
        public void ObjectContentResult_Execute_CallsJsonResult_AccessValue()
        {
            // Arrange
            var nonStringValue = new { x1 = 10, y1 = "Hello" };
            var actionContext = GetMockActionContext();

            // Act
            var result = new ObjectContentResult(nonStringValue);

            // Assert
            Assert.Equal(nonStringValue, result.Value);
        }

        [Fact]
        public async Task ObjectContentResult_Execute_CallsJsonResult_SetsContent()
        {
            // Arrange
            var expectedContentType = "application/json";
            var nonStringValue = new { x1 = 10, y1 = "Hello" };
            var httpResponse = Mock.Of<HttpResponse>();
            httpResponse.Body = new MemoryStream();
            var actionContext = GetMockActionContext(httpResponse);

            var tempStream = new MemoryStream();
            using (var writer = new StreamWriter(tempStream, UTF8EncodingWithoutBOM.Encoding, 1024, leaveOpen: true))
            {
                var formatter = new JsonOutputFormatter(JsonOutputFormatter.CreateDefaultSettings(), false);
                formatter.WriteObject(writer, nonStringValue);
            }

            // Act
            var result = new ObjectContentResult(nonStringValue);
            await result.ExecuteResultAsync(actionContext);

            // Assert
            Assert.Equal(expectedContentType, httpResponse.ContentType);
            Assert.Equal(GetStringFromStream(tempStream),
                GetStringFromStream(actionContext.HttpContext.Response.Body as MemoryStream));
        }

        private static string GetStringFromStream(MemoryStream inputStream)
        {
            return Encoding.UTF8.GetString(inputStream.ToArray());
        }

        private static ActionContext GetMockActionContext(HttpResponse response = null)
        {
            var httpContext = new Mock<HttpContext>();
            if (response != null)
            {
                httpContext.Setup(o => o.Response).Returns(response);
            }
            
            return new ActionContext(httpContext.Object, Mock.Of<IRouter>(), new Dictionary<string, object>(),
                new ActionDescriptor());
        }
    }
}