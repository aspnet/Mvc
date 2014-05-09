// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Testing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class ActionResultFactoryTests
    {
        [Fact]
        public void CreateActionResult_ActionResultReturnsItself()
        {
            // Arrange
            var factory = new ActionResultFactory(CreateActionResultHelper());
            var actionContext = CreateActionContext();
            var actionResultMock = new Mock<IActionResult>();

            // Act & Assert
            Assert.Equal(actionResultMock.Object,
                factory.CreateActionResult(actionResultMock.Object.GetType(),
                    actionResultMock.Object,
                    actionContext));
        }

        [Fact]
        [ReplaceCulture]
        public void CreateActionResult_NullActionResultReturnValueThrows()
        {
            // Arrange
            var factory = new ActionResultFactory(CreateActionResultHelper());
            var actionContext = CreateActionContext();
            var actionReturnValueMock = new Mock<IActionResult>();

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => factory.CreateActionResult(actionReturnValueMock.Object.GetType(), null, actionContext),
                "Cannot return null from an action method with a return type of '"
                    + actionReturnValueMock.Object.GetType()
                    + "'.");
        }

        [Fact]
        public void CreateActionResult_NullValueReturnsNoContentResult()
        {
            // Arrange
            var factory = new ActionResultFactory(CreateActionResultHelper());
            var actionContext = CreateActionContext();

            // Act & Assert
            Assert.Equal(typeof(NoContentResult),
                factory.CreateActionResult(typeof(void), null, actionContext).GetType());
        }

        [Fact]
        public void CreateActionResult_StringValueReturnsContentResult()
        {
            // Arrange
            var factory = new ActionResultFactory(CreateActionResultHelper());
            var actionContext = CreateActionContext();
            const string stringValue = "sample result";

            // Act
            var actualResult = factory.CreateActionResult(stringValue.GetType(), stringValue, actionContext)
                as ContentResult;

            // Assert
            Assert.NotNull(actualResult);
            Assert.Equal("text/plain", actualResult.ContentType);
            Assert.Equal(stringValue, actualResult.Content);
        }

        [Fact]
        public void CreateActionResult_NonStringValueReturnsJsonResult()
        {
            // Arrange
            var actionResultHelperMock = new Mock<IActionResultHelper>();
            var nonStringValue = new Collection<object>();
            var jsonResult = new JsonResult(nonStringValue);
            actionResultHelperMock.Setup(a => a.Json(It.IsAny<object>())).Returns(jsonResult);
            var factory = new ActionResultFactory(actionResultHelperMock.Object);
            var actionContext = CreateActionContext();
            
            // Act
            var actualResult = factory.CreateActionResult(nonStringValue.GetType(), nonStringValue, actionContext);

            // Assert
            Assert.Equal(jsonResult, actualResult);
        }

        private IActionResultHelper CreateActionResultHelper()
        {
            var actionResultHelperMock = new Mock<IActionResultHelper>();
            return actionResultHelperMock.Object;
        }

        private static ActionContext CreateActionContext()
        {
            return new ActionContext((new Mock<HttpContext>()).Object,
                (new Mock<IRouter>()).Object,
                new Dictionary<string, object>(),
                new ActionDescriptor());
        }
    }
}
