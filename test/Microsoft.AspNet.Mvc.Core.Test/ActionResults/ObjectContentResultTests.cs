// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test.ActionResults
{
    public class ObjectContentResultTests
    {
        [Fact]
        public async Task ObjectContentResult_Execute_CallsContentResult()
        {
            // Arrange
            var input = "testInput";
            var expectedContentType = "text/plain";
            var actionContext = GetMockActionContext();

            var ocr = new ObjectContentResult(input);

            // Act
            await ocr.ExecuteResultAsync(actionContext);

            // Assert
            Assert.Equal(expectedContentType, actionContext.HttpContext.Response.ContentType);
            Assert.Equal(input, ocr.Value);
        }

        [Fact]
        public async Task ObjectContentResult_Execute_CallsJsonResult()
        {
            // Arrange
            var expectedContentType = "application/json";
            var actionResultHelperMock = new Mock<IActionResultHelper>();
            var nonStringValue = new Collection<object>();
            var jsonResult = new JsonResult(nonStringValue);
            actionResultHelperMock.Setup(a => a.Json(It.IsAny<object>())).Returns(jsonResult);
            var actionContext = GetMockActionContext();
            var tempActionContext = GetMockActionContext();

            var ocr = new ObjectContentResult(nonStringValue);

            // Act
            await ocr.ExecuteResultAsync(actionContext);
            await jsonResult.ExecuteResultAsync(tempActionContext);

            // Assert
            Assert.Equal(expectedContentType, actionContext.HttpContext.Response.ContentType);
            Assert.Equal(nonStringValue, ocr.Value);
        }

        private static ActionContext GetMockActionContext()
        {
            var httpContext = new Mock<HttpContext>();
            var httpResponse = Mock.Of<HttpResponse>();
            httpResponse.Body = new MemoryStream();
            httpContext.Setup(o => o.Response).Returns(httpResponse);

            return new ActionContext(httpContext.Object, Mock.Of<IRouter>(), new Dictionary<string, object>(),
                new ActionDescriptor());
        }
    }
}