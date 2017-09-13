// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class ProblemDetailsFilterTest
    {
        [Fact]
        public void OnActionExecuting_NoOpsIfActionResultIsNull()
        {
            // Arrange
            var actionExecutingContext = CreateActionExecutingContext();
            var filter = new ProblemDetailsFilter(CreateErrorDescriptionFactory());

            // Act
            filter.OnActionExecuting(actionExecutingContext);

            // Assert
            Assert.Null(actionExecutingContext.Result);
        }

        public static TheoryData OnActionExecuting_NoOpsIfActionResultIsNotAnInstanceOfStatusCodeResultData => 
            new TheoryData<IActionResult>()
            {
                new ObjectResult(new object()),
                new ViewResult(),
                new BadRequestObjectResult(new ModelStateDictionary()),
            };

        [Theory]
        [MemberData(nameof(OnActionExecuting_NoOpsIfActionResultIsNotAnInstanceOfStatusCodeResultData))]
        public void OnActionExecuting_NoOpsIfActionResultIsNotAnInstanceOfStatusCodeResult(IActionResult input)
        {
            // Arrange
            var actionExecutingContext = CreateActionExecutingContext();
            actionExecutingContext.Result = input;
            var filter = new ProblemDetailsFilter(CreateErrorDescriptionFactory());

            // Act
            filter.OnActionExecuting(actionExecutingContext);

            // Assert
            Assert.Equal(input, actionExecutingContext.Result);
        }

        [Fact]
        public void OnActionExecuting_NoOpsIfActionResultIsNotAWellKnownStatusCodeResult()
        {
            // Arrange
            var input = new OkResult();
            var actionExecutingContext = CreateActionExecutingContext();
            actionExecutingContext.Result = input;
            var filter = new ProblemDetailsFilter(CreateErrorDescriptionFactory());

            // Act
            filter.OnActionExecuting(actionExecutingContext);

            // Assert
            Assert.Equal(input, actionExecutingContext.Result);
        }

        [Fact]
        public void OnActionExecuting_ConvertsNotFoundResultToBadRequestObjectResultWithProblemDetails()
        {
            // Arrange
            var actionExecutingContext = CreateActionExecutingContext();
            actionExecutingContext.Result = new NotFoundResult();
            var filter = new ProblemDetailsFilter(CreateErrorDescriptionFactory());

            // Act
            filter.OnActionExecuting(actionExecutingContext);

            // Assert
            var actual = Assert.IsType<BadRequestObjectResult>(actionExecutingContext.Result);
            Assert.Equal(404, actual.StatusCode);
            var problemDetails = Assert.IsType<ProblemDetails>(actual.Value);
            Assert.Equal(404, problemDetails.Status);
            Assert.Equal("Resource with the specified id could not be found.", problemDetails.Title);
        }

        [Fact]
        public void OnActionExecuting_ConvertsUnsupportedMediaTypeResultToBadRequestObjectResultWithProblemDetails()
        {
            // Arrange
            var actionExecutingContext = CreateActionExecutingContext();
            actionExecutingContext.Result = new UnsupportedMediaTypeResult();
            var filter = new ProblemDetailsFilter(CreateErrorDescriptionFactory());

            // Act
            filter.OnActionExecuting(actionExecutingContext);

            // Assert
            var actual = Assert.IsType<BadRequestObjectResult>(actionExecutingContext.Result);
            Assert.Equal(415, actual.StatusCode);
            var problemDetails = Assert.IsType<ProblemDetails>(actual.Value);
            Assert.Equal(415, problemDetails.Status);
            Assert.Equal("The request entity is in a format that is not supported by the requested resource.", problemDetails.Title);
        }

        [Fact]
        public void OnActionExecuting_Converts400StatusCodeResultToBadRequestObjectResultWithProblemDetails()
        {
            // Arrange
            var actionExecutingContext = CreateActionExecutingContext();
            actionExecutingContext.Result = new StatusCodeResult(400);
            var filter = new ProblemDetailsFilter(CreateErrorDescriptionFactory());

            // Act
            filter.OnActionExecuting(actionExecutingContext);

            // Assert
            var actual = Assert.IsType<BadRequestObjectResult>(actionExecutingContext.Result);
            Assert.Equal(400, actual.StatusCode);
            var problemDetails = Assert.IsType<ProblemDetails>(actual.Value);
            Assert.Equal(400, problemDetails.Status);
            Assert.Equal("400 Bad Request", problemDetails.Title);
        }

        [Fact]
        public void OnActionExecuting_Converts404StatusCodeResultToBadRequestObjectResultWithProblemDetails()
        {
            // Arrange
            var actionExecutingContext = CreateActionExecutingContext();
            actionExecutingContext.Result = new StatusCodeResult(404);
            var filter = new ProblemDetailsFilter(CreateErrorDescriptionFactory());

            // Act
            filter.OnActionExecuting(actionExecutingContext);

            // Assert
            var actual = Assert.IsType<BadRequestObjectResult>(actionExecutingContext.Result);
            Assert.Equal(404, actual.StatusCode);
            var problemDetails = Assert.IsType<ProblemDetails>(actual.Value);
            Assert.Equal(404, problemDetails.Status);
            Assert.Equal("Resource with the specified id could not be found.", problemDetails.Title);
        }

        [Fact]
        public void OnActionExecuting_Converts415StatusCodeResultToBadRequestObjectResultWithProblemDetails()
        {
            // Arrange
            var actionExecutingContext = CreateActionExecutingContext();
            actionExecutingContext.Result = new StatusCodeResult(415);
            var filter = new ProblemDetailsFilter(CreateErrorDescriptionFactory());

            // Act
            filter.OnActionExecuting(actionExecutingContext);

            // Assert
            var actual = Assert.IsType<BadRequestObjectResult>(actionExecutingContext.Result);
            Assert.Equal(415, actual.StatusCode);
            var problemDetails = Assert.IsType<ProblemDetails>(actual.Value);
            Assert.Equal(415, problemDetails.Status);
            Assert.Equal("The request entity is in a format that is not supported by the requested resource.", problemDetails.Title);
        }

        [Fact]
        public void OnActionExecuting_InvokesErrorActionInvokerFactory()
        {
            // Arrange
            var expected = new object();
            var actionExecutingContext = CreateActionExecutingContext();
            actionExecutingContext.Result = new StatusCodeResult(404);
            var provider = new Mock<IErrorDescriptorProvider>();
            provider.Setup(p => p.OnProvidersExecuting(It.IsAny<ErrorDescriptionContext>()))
                .Callback((ErrorDescriptionContext context) =>
                {
                    Assert.Same(actionExecutingContext.ActionDescriptor, context.ActionDescriptor);
                    var problemDetails = Assert.IsType<ProblemDetails>(context.Result);
                    Assert.Equal(404, problemDetails.Status);
                    Assert.Equal("Resource with the specified id could not be found.", problemDetails.Title);

                    context.Result = expected;
                });
            var filter = new ProblemDetailsFilter(CreateErrorDescriptionFactory(provider.Object));

            // Act
            filter.OnActionExecuting(actionExecutingContext);

            // Assert
            var actual = Assert.IsType<BadRequestObjectResult>(actionExecutingContext.Result);
            Assert.Equal(404, actual.StatusCode);
            Assert.Same(expected, actual.Value);
        }

        private static ActionExecutingContext CreateActionExecutingContext()
        {
            return new ActionExecutingContext(
                new ActionContext
                {
                    HttpContext = new DefaultHttpContext(),
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor(),
                },
                Array.Empty<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new object());
        }

        private static IErrorDescriptionFactory CreateErrorDescriptionFactory(IErrorDescriptorProvider provider = null)
        {
            var providers = new List<IErrorDescriptorProvider>();
            if (provider != null)
            {
                providers.Add(provider);
            }

            return new DefaultErrorDescriptorFactory(providers);
        }
    }
}
