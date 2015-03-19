// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Core;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.ModelBinding.Metadata;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Testing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class ControllerActionInvokerTest
    {
        // Intentionally choosing an uncommon exception type.
        private readonly Exception _actionException = new TimeZoneNotFoundException();

        private readonly JsonResult _result = new JsonResult(new { message = "Hello, world!" });

        private struct SampleStruct
        {
            public int x;
        }

        [Fact]
        public async Task InvokeAction_DoesNotInvokeExceptionFilter_WhenActionDoesNotThrow()
        {
            // Arrange
            var filter = new Mock<IExceptionFilter>(MockBehavior.Strict);
            filter
                .Setup(f => f.OnException(It.IsAny<ExceptionContext>()))
                .Verifiable();

            var invoker = CreateInvoker(filter.Object, actionThrows: false);

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter.Verify(f => f.OnException(It.IsAny<ExceptionContext>()), Times.Never());
        }

        [Fact]
        public async Task InvokeAction_SavesTempData_WhenActionDoesNotThrow()
        {
            // Arrange
            var tempData = new Mock<ITempDataDictionary>();
            tempData.Setup(t => t.Save()).Verifiable();

            var invoker = CreateInvoker(Mock.Of<IFilter>(), actionThrows: false, tempData: tempData.Object);

            // Act
            await invoker.InvokeAsync();

            // Assert
            tempData.Verify(t => t.Save(), Times.Once());
        }

        [Fact]
        public async Task InvokeAction_SavesTempData_WhenActionThrows()
        {
            // Arrange
            var tempData = new Mock<ITempDataDictionary>();
            tempData.Setup(t => t.Save()).Verifiable();

            var invoker = CreateInvoker(Mock.Of<IFilter>(), actionThrows: true, tempData: tempData.Object);

            // Act & Assert
            await Assert.ThrowsAsync(_actionException.GetType(), async () => await invoker.InvokeAsync());
            tempData.Verify(t => t.Save(), Times.Once());
        }

        [Fact]
        public async Task InvokeAction_DoesNotAsyncInvokeExceptionFilter_WhenActionDoesNotThrow()
        {
            // Arrange
            var filter = new Mock<IAsyncExceptionFilter>(MockBehavior.Strict);
            filter
                .Setup(f => f.OnExceptionAsync(It.IsAny<ExceptionContext>()))
                .Returns<ExceptionContext>((context) => Task.FromResult(true))
                .Verifiable();

            var invoker = CreateInvoker(filter.Object, actionThrows: false);

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter.Verify(
                f => f.OnExceptionAsync(It.IsAny<ExceptionContext>()),
                Times.Never());
        }

        [Fact]
        public async Task InvokeAction_InvokesExceptionFilter_WhenActionThrows()
        {
            // Arrange
            Exception exception = null;
            IActionResult result = null;

            var filter = new Mock<IExceptionFilter>(MockBehavior.Strict);
            filter
                .Setup(f => f.OnException(It.IsAny<ExceptionContext>()))
                .Callback<ExceptionContext>(context =>
                {
                    exception = context.Exception;
                    result = context.Result;

                    // Handle the exception
                    context.Result = new EmptyResult();
                })
                .Verifiable();

            var invoker = CreateInvoker(filter.Object, actionThrows: true);

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter.Verify(f => f.OnException(It.IsAny<ExceptionContext>()), Times.Once());

            Assert.Same(_actionException, exception);
            Assert.Null(result);
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncExceptionFilter_WhenActionThrows()
        {
            // Arrange
            Exception exception = null;
            IActionResult result = null;

            var filter = new Mock<IAsyncExceptionFilter>(MockBehavior.Strict);
            filter
                .Setup(f => f.OnExceptionAsync(It.IsAny<ExceptionContext>()))
                .Callback<ExceptionContext>(context =>
                {
                    exception = context.Exception;
                    result = context.Result;

                    // Handle the exception
                    context.Result = new EmptyResult();
                })
                .Returns<ExceptionContext>((context) => Task.FromResult(true))
                .Verifiable();

            var invoker = CreateInvoker(filter.Object, actionThrows: true);

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter.Verify(
                f => f.OnExceptionAsync(It.IsAny<ExceptionContext>()),
                Times.Once());

            Assert.Same(_actionException, exception);
            Assert.Null(result);
        }

        [Fact]
        public async Task InvokeAction_InvokesExceptionFilter_ShortCircuit()
        {
            // Arrange
            var filter1 = new Mock<IExceptionFilter>(MockBehavior.Strict);

            var filter2 = new Mock<IExceptionFilter>(MockBehavior.Strict);
            filter2
                .Setup(f => f.OnException(It.IsAny<ExceptionContext>()))
                .Callback<ExceptionContext>(context =>
                {
                    filter2.ToString();
                    context.Exception = null;
                })
                .Verifiable();

            var invoker = CreateInvoker(new[] { filter1.Object, filter2.Object }, actionThrows: true);

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter2.Verify(
                f => f.OnException(It.IsAny<ExceptionContext>()),
                Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncExceptionFilter_ShortCircuit()
        {
            // Arrange
            var filter1 = new Mock<IExceptionFilter>(MockBehavior.Strict);

            var filter2 = new Mock<IAsyncExceptionFilter>(MockBehavior.Strict);
            filter2
                .Setup(f => f.OnExceptionAsync(It.IsAny<ExceptionContext>()))
                .Callback<ExceptionContext>(context =>
                {
                    filter2.ToString();
                    context.Exception = null;
                })
                .Returns<ExceptionContext>((context) => Task.FromResult(true))
                .Verifiable();

            var invoker = CreateInvoker(new IFilter[] { filter1.Object, filter2.Object }, actionThrows: true);

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter2.Verify(
                f => f.OnExceptionAsync(It.IsAny<ExceptionContext>()),
                Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesExceptionFilter_UnhandledExceptionIsThrown()
        {
            // Arrange
            var filter = new Mock<IExceptionFilter>(MockBehavior.Strict);
            filter
                .Setup(f => f.OnException(It.IsAny<ExceptionContext>()))
                .Verifiable();

            var invoker = CreateInvoker(filter.Object, actionThrows: true);

            // Act
            await Assert.ThrowsAsync(_actionException.GetType(), async () => await invoker.InvokeAsync());

            // Assert
            filter.Verify(f => f.OnException(It.IsAny<ExceptionContext>()), Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesExceptionFilter_ResultIsExecuted_WithoutResultFilters()
        {
            // Arrange
            var result = new Mock<IActionResult>(MockBehavior.Strict);
            result
                .Setup(r => r.ExecuteResultAsync(It.IsAny<ActionContext>()))
                .Returns<ActionContext>((context) => Task.FromResult(true))
                .Verifiable();

            var filter = new Mock<IExceptionFilter>(MockBehavior.Strict);
            filter
                .Setup(f => f.OnException(It.IsAny<ExceptionContext>()))
                .Callback<ExceptionContext>(c => c.Result = result.Object)
                .Verifiable();

            var resultFilter = new Mock<IResultFilter>(MockBehavior.Strict);

            var invoker = CreateInvoker(new IFilter[] { filter.Object, resultFilter.Object }, actionThrows: true);

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter.Verify(f => f.OnException(It.IsAny<ExceptionContext>()), Times.Once());
            result.Verify(r => r.ExecuteResultAsync(It.IsAny<ActionContext>()), Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesAuthorizationFilter()
        {
            // Arrange
            var filter = new Mock<IAuthorizationFilter>(MockBehavior.Strict);
            filter.Setup(f => f.OnAuthorization(It.IsAny<AuthorizationContext>())).Verifiable();

            var invoker = CreateInvoker(filter.Object);

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter.Verify(f => f.OnAuthorization(It.IsAny<AuthorizationContext>()), Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncAuthorizationFilter()
        {
            // Arrange
            var filter = new Mock<IAsyncAuthorizationFilter>(MockBehavior.Strict);
            filter
                .Setup(f => f.OnAuthorizationAsync(It.IsAny<AuthorizationContext>()))
                .Returns<AuthorizationContext>(context => Task.FromResult(true))
                .Verifiable();

            var invoker = CreateInvoker(filter.Object);

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter.Verify(
                f => f.OnAuthorizationAsync(It.IsAny<AuthorizationContext>()),
                Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesAuthorizationFilter_ShortCircuit()
        {
            // Arrange
            var challenge = new Mock<IActionResult>(MockBehavior.Loose).Object;

            var filter1 = new Mock<IAuthorizationFilter>(MockBehavior.Strict);
            filter1
                .Setup(f => f.OnAuthorization(It.IsAny<AuthorizationContext>()))
                .Callback<AuthorizationContext>(c => c.Result = challenge)
                .Verifiable();

            var filter2 = new Mock<IAuthorizationFilter>(MockBehavior.Strict);

            var invoker = CreateInvoker(new[] { filter1.Object, filter2.Object });

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter1.Verify(f => f.OnAuthorization(It.IsAny<AuthorizationContext>()), Times.Once());
            Assert.False(invoker.ControllerFactory.CreateCalled);
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncAuthorizationFilter_ShortCircuit()
        {
            // Arrange
            var challenge = new Mock<IActionResult>(MockBehavior.Loose).Object;

            var filter1 = new Mock<IAsyncAuthorizationFilter>(MockBehavior.Strict);
            filter1
                .Setup(f => f.OnAuthorizationAsync(It.IsAny<AuthorizationContext>()))
                .Returns<AuthorizationContext>((context) =>
                {
                    context.Result = challenge;
                    return Task.FromResult(true);
                })
                .Verifiable();

            var filter2 = new Mock<IAuthorizationFilter>(MockBehavior.Strict);

            var invoker = CreateInvoker(new IFilter[] { filter1.Object, filter2.Object });

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter1.Verify(
                f => f.OnAuthorizationAsync(It.IsAny<AuthorizationContext>()),
                Times.Once());

            Assert.False(invoker.ControllerFactory.CreateCalled);
        }

        [Fact]
        public async Task InvokeAction_ExceptionInAuthorizationFilterCannotBeHandledByOtherFilters()
        {
            // Arrange
            var expected = new InvalidCastException();

            var exceptionFilter = new Mock<IExceptionFilter>(MockBehavior.Strict);
            exceptionFilter
                .Setup(f => f.OnException(It.IsAny<ExceptionContext>()))
                .Callback<ExceptionContext>(context =>
                {
                    // Mark as handled
                    context.Result = new EmptyResult();
                })
                .Verifiable();

            var authorizationFilter1 = new Mock<IAuthorizationFilter>(MockBehavior.Strict);
            authorizationFilter1
                .Setup(f => f.OnAuthorization(It.IsAny<AuthorizationContext>()))
                .Callback<AuthorizationContext>(c => { throw expected; })
                .Verifiable();

            // None of these filters should run
            var authorizationFilter2 = new Mock<IAuthorizationFilter>(MockBehavior.Strict);
            var resourceFilter = new Mock<IResourceFilter>(MockBehavior.Strict);
            var actionFilter = new Mock<IActionFilter>(MockBehavior.Strict);
            var resultFilter = new Mock<IResultFilter>(MockBehavior.Strict);

            var invoker = CreateInvoker(new IFilter[]
            {
                exceptionFilter.Object,
                authorizationFilter1.Object,
                authorizationFilter2.Object,
                resourceFilter.Object,
                actionFilter.Object,
                resultFilter.Object,
            });

            // Act
            var thrown = await Assert.ThrowsAsync<InvalidCastException>(invoker.InvokeAsync);

            // Assert
            Assert.Same(expected, thrown);
            exceptionFilter.Verify(f => f.OnException(It.IsAny<ExceptionContext>()), Times.Never());
            authorizationFilter1.Verify(f => f.OnAuthorization(It.IsAny<AuthorizationContext>()), Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesAuthorizationFilter_ChallengeNotSeenByResultFilters()
        {
            // Arrange
            var challenge = new Mock<IActionResult>(MockBehavior.Strict);
            challenge
                .Setup(r => r.ExecuteResultAsync(It.IsAny<ActionContext>()))
                .Returns<ActionContext>((context) => Task.FromResult(true))
                .Verifiable();

            var authorizationFilter = new Mock<IAuthorizationFilter>(MockBehavior.Strict);
            authorizationFilter
                .Setup(f => f.OnAuthorization(It.IsAny<AuthorizationContext>()))
                .Callback<AuthorizationContext>(c => c.Result = challenge.Object)
                .Verifiable();

            var resultFilter = new Mock<IResultFilter>(MockBehavior.Strict);

            var invoker = CreateInvoker(new IFilter[] { authorizationFilter.Object, resultFilter.Object });

            // Act
            await invoker.InvokeAsync();

            // Assert
            authorizationFilter.Verify(f => f.OnAuthorization(It.IsAny<AuthorizationContext>()), Times.Once());
            challenge.Verify(c => c.ExecuteResultAsync(It.IsAny<ActionContext>()), Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesActionFilter()
        {
            // Arrange
            IActionResult result = null;

            var filter = new Mock<IActionFilter>(MockBehavior.Strict);
            filter.Setup(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>())).Verifiable();
            filter
                .Setup(f => f.OnActionExecuted(It.IsAny<ActionExecutedContext>()))
                .Callback<ActionExecutedContext>(c => result = c.Result)
                .Verifiable();

            var invoker = CreateInvoker(filter.Object);

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter.Verify(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>()), Times.Once());
            filter.Verify(f => f.OnActionExecuted(It.IsAny<ActionExecutedContext>()), Times.Once());

            Assert.Same(_result, result);
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncActionFilter()
        {
            // Arrange
            IActionResult result = null;

            var filter = new Mock<IAsyncActionFilter>(MockBehavior.Strict);
            filter
                .Setup(f => f.OnActionExecutionAsync(It.IsAny<ActionExecutingContext>(), It.IsAny<ActionExecutionDelegate>()))
                .Returns<ActionExecutingContext, ActionExecutionDelegate>(async (context, next) =>
                {
                    var resultContext = await next();
                    result = resultContext.Result;
                })
                .Verifiable();

            var invoker = CreateInvoker(filter.Object);

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter.Verify(
                f => f.OnActionExecutionAsync(It.IsAny<ActionExecutingContext>(), It.IsAny<ActionExecutionDelegate>()),
                Times.Once());

            Assert.Same(_result, result);
        }

        [Fact]
        public async Task InvokeAction_InvokesActionFilter_ShortCircuit()
        {
            // Arrange
            var result = new EmptyResult();

            ActionExecutedContext context = null;

            var actionFilter1 = new Mock<IActionFilter>(MockBehavior.Strict);
            actionFilter1.Setup(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>())).Verifiable();
            actionFilter1
                .Setup(f => f.OnActionExecuted(It.IsAny<ActionExecutedContext>()))
                .Callback<ActionExecutedContext>(c => context = c)
                .Verifiable();

            var actionFilter2 = new Mock<IActionFilter>(MockBehavior.Strict);
            actionFilter2
                .Setup(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>()))
                .Callback<ActionExecutingContext>(c => c.Result = result)
                .Verifiable();

            var actionFilter3 = new Mock<IActionFilter>(MockBehavior.Strict);

            var resultFilter = new Mock<IResultFilter>(MockBehavior.Strict);
            resultFilter.Setup(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>())).Verifiable();
            resultFilter.Setup(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>())).Verifiable();

            var invoker = CreateInvoker(new IFilter[]
            {
                actionFilter1.Object,
                actionFilter2.Object,
                actionFilter3.Object,
                resultFilter.Object,
            });

            // Act
            await invoker.InvokeAsync();

            // Assert
            actionFilter1.Verify(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>()), Times.Once());
            actionFilter1.Verify(f => f.OnActionExecuted(It.IsAny<ActionExecutedContext>()), Times.Once());

            actionFilter2.Verify(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>()), Times.Once());

            resultFilter.Verify(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()), Times.Once());
            resultFilter.Verify(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>()), Times.Once());

            Assert.True(context.Canceled);
            Assert.Same(context.Result, result);
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncActionFilter_ShortCircuit_WithResult()
        {
            // Arrange
            var result = new EmptyResult();

            ActionExecutedContext context = null;

            var actionFilter1 = new Mock<IActionFilter>(MockBehavior.Strict);
            actionFilter1.Setup(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>())).Verifiable();
            actionFilter1
                .Setup(f => f.OnActionExecuted(It.IsAny<ActionExecutedContext>()))
                .Callback<ActionExecutedContext>(c => context = c)
                .Verifiable();

            var actionFilter2 = new Mock<IAsyncActionFilter>(MockBehavior.Strict);
            actionFilter2
                .Setup(f => f.OnActionExecutionAsync(It.IsAny<ActionExecutingContext>(), It.IsAny<ActionExecutionDelegate>()))
                .Returns<ActionExecutingContext, ActionExecutionDelegate>((c, next) =>
                {
                    // Notice we're not calling next
                    c.Result = result;
                    return Task.FromResult(true);
                })
                .Verifiable();

            var actionFilter3 = new Mock<IActionFilter>(MockBehavior.Strict);

            var resultFilter = new Mock<IResultFilter>(MockBehavior.Strict);
            resultFilter.Setup(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>())).Verifiable();
            resultFilter.Setup(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>())).Verifiable();

            var invoker = CreateInvoker(new IFilter[]
            {
                actionFilter1.Object,
                actionFilter2.Object,
                actionFilter3.Object,
                resultFilter.Object,
            });

            // Act
            await invoker.InvokeAsync();

            // Assert
            actionFilter1.Verify(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>()), Times.Once());
            actionFilter1.Verify(f => f.OnActionExecuted(It.IsAny<ActionExecutedContext>()), Times.Once());

            actionFilter2.Verify(
                f => f.OnActionExecutionAsync(It.IsAny<ActionExecutingContext>(), It.IsAny<ActionExecutionDelegate>()),
                Times.Once());

            resultFilter.Verify(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()), Times.Once());
            resultFilter.Verify(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>()), Times.Once());

            Assert.True(context.Canceled);
            Assert.Same(context.Result, result);
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncActionFilter_ShortCircuit_WithoutResult()
        {
            // Arrange
            ActionExecutedContext context = null;

            var actionFilter1 = new Mock<IActionFilter>(MockBehavior.Strict);
            actionFilter1.Setup(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>())).Verifiable();
            actionFilter1
                .Setup(f => f.OnActionExecuted(It.IsAny<ActionExecutedContext>()))
                .Callback<ActionExecutedContext>(c => context = c)
                .Verifiable();

            var actionFilter2 = new Mock<IAsyncActionFilter>(MockBehavior.Strict);
            actionFilter2
                .Setup(f => f.OnActionExecutionAsync(It.IsAny<ActionExecutingContext>(), It.IsAny<ActionExecutionDelegate>()))
                .Returns<ActionExecutingContext, ActionExecutionDelegate>((c, next) =>
                {
                    // Notice we're not calling next
                    return Task.FromResult(true);
                })
                .Verifiable();

            var actionFilter3 = new Mock<IActionFilter>(MockBehavior.Strict);

            var resultFilter = new Mock<IResultFilter>(MockBehavior.Strict);
            resultFilter.Setup(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>())).Verifiable();
            resultFilter.Setup(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>())).Verifiable();

            var invoker = CreateInvoker(new IFilter[]
            {
                actionFilter1.Object,
                actionFilter2.Object,
                actionFilter3.Object,
                resultFilter.Object,
            });

            // Act
            await invoker.InvokeAsync();

            // Assert
            actionFilter1.Verify(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>()), Times.Once());
            actionFilter1.Verify(f => f.OnActionExecuted(It.IsAny<ActionExecutedContext>()), Times.Once());

            actionFilter2.Verify(
                f => f.OnActionExecutionAsync(It.IsAny<ActionExecutingContext>(), It.IsAny<ActionExecutionDelegate>()),
                Times.Once());

            resultFilter.Verify(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()), Times.Once());
            resultFilter.Verify(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>()), Times.Once());

            Assert.True(context.Canceled);
            Assert.Null(context.Result);
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncActionFilter_ShortCircuit_WithResult_CallNext()
        {
            // Arrange
            var actionFilter = new Mock<IAsyncActionFilter>(MockBehavior.Strict);
            actionFilter
                .Setup(f => f.OnActionExecutionAsync(It.IsAny<ActionExecutingContext>(), It.IsAny<ActionExecutionDelegate>()))
                .Returns<ActionExecutingContext, ActionExecutionDelegate>(async (c, next) =>
                {
                    c.Result = new EmptyResult();
                    await next();
                })
                .Verifiable();

            var message =
                "If an IAsyncActionFilter provides a result value by setting the Result property of " +
                "ActionExecutingContext to a non-null value, then it cannot call the next filter by invoking " +
                "ActionExecutionDelegate.";

            var invoker = CreateInvoker(actionFilter.Object);

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<InvalidOperationException>(
                async () => await invoker.InvokeAsync(),
                message);
        }

        [Fact]
        public async Task InvokeAction_InvokesActionFilter_WithExceptionThrownByAction()
        {
            // Arrange
            ActionExecutedContext context = null;

            var filter = new Mock<IActionFilter>(MockBehavior.Strict);
            filter.Setup(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>())).Verifiable();
            filter
                .Setup(f => f.OnActionExecuted(It.IsAny<ActionExecutedContext>()))
                .Callback<ActionExecutedContext>(c =>
                {
                    context = c;

                    // Handle the exception so the test doesn't throw.
                    Assert.False(c.ExceptionHandled);
                    c.ExceptionHandled = true;
                })
                .Verifiable();

            var invoker = CreateInvoker(filter.Object, actionThrows: true);

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter.Verify(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>()), Times.Once());
            filter.Verify(f => f.OnActionExecuted(It.IsAny<ActionExecutedContext>()), Times.Once());

            Assert.Same(_actionException, context.Exception);
            Assert.Null(context.Result);
        }

        [Fact]
        public async Task InvokeAction_InvokesActionFilter_WithExceptionThrownByActionFilter()
        {
            // Arrange
            var exception = new DataMisalignedException();
            ActionExecutedContext context = null;

            var filter1 = new Mock<IActionFilter>(MockBehavior.Strict);
            filter1.Setup(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>())).Verifiable();
            filter1
                .Setup(f => f.OnActionExecuted(It.IsAny<ActionExecutedContext>()))
                .Callback<ActionExecutedContext>(c =>
                {
                    context = c;

                    // Handle the exception so the test doesn't throw.
                    Assert.False(c.ExceptionHandled);
                    c.ExceptionHandled = true;
                })
                .Verifiable();

            var filter2 = new Mock<IActionFilter>(MockBehavior.Strict);
            filter2.Setup(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>())).Verifiable();
            filter2
                .Setup(f => f.OnActionExecuted(It.IsAny<ActionExecutedContext>()))
                .Callback<ActionExecutedContext>(c => { throw exception; })
                .Verifiable();

            var invoker = CreateInvoker(new[] { filter1.Object, filter2.Object });

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter1.Verify(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>()), Times.Once());
            filter1.Verify(f => f.OnActionExecuted(It.IsAny<ActionExecutedContext>()), Times.Once());

            filter2.Verify(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>()), Times.Once());

            Assert.Same(exception, context.Exception);
            Assert.Null(context.Result);
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncActionFilter_WithExceptionThrownByActionFilter()
        {
            // Arrange
            var exception = new DataMisalignedException();
            ActionExecutedContext context = null;

            var filter1 = new Mock<IAsyncActionFilter>(MockBehavior.Strict);
            filter1
                .Setup(f => f.OnActionExecutionAsync(It.IsAny<ActionExecutingContext>(), It.IsAny<ActionExecutionDelegate>()))
                .Returns<ActionExecutingContext, ActionExecutionDelegate>(async (c, next) =>
                {
                    context = await next();

                    // Handle the exception so the test doesn't throw.
                    Assert.False(context.ExceptionHandled);
                    context.ExceptionHandled = true;
                })
                .Verifiable();

            var filter2 = new Mock<IActionFilter>(MockBehavior.Strict);
            filter2.Setup(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>())).Verifiable();
            filter2
                .Setup(f => f.OnActionExecuted(It.IsAny<ActionExecutedContext>()))
                .Callback<ActionExecutedContext>(c => { throw exception; })
                .Verifiable();

            var invoker = CreateInvoker(new IFilter[] { filter1.Object, filter2.Object });

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter1.Verify(
                f => f.OnActionExecutionAsync(It.IsAny<ActionExecutingContext>(), It.IsAny<ActionExecutionDelegate>()),
                Times.Once());

            filter2.Verify(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>()), Times.Once());

            Assert.Same(exception, context.Exception);
            Assert.Null(context.Result);
        }

        [Fact]
        public async Task InvokeAction_InvokesActionFilter_HandleException()
        {
            // Arrange
            var result = new Mock<IActionResult>(MockBehavior.Strict);
            result
                .Setup(r => r.ExecuteResultAsync(It.IsAny<ActionContext>()))
                .Returns<ActionContext>((context) => Task.FromResult(true))
                .Verifiable();

            var actionFilter = new Mock<IActionFilter>(MockBehavior.Strict);
            actionFilter.Setup(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>())).Verifiable();
            actionFilter
                .Setup(f => f.OnActionExecuted(It.IsAny<ActionExecutedContext>()))
                .Callback<ActionExecutedContext>(c =>
                {
                    // Handle the exception so the test doesn't throw.
                    Assert.False(c.ExceptionHandled);
                    c.ExceptionHandled = true;

                    c.Result = result.Object;
                })
                .Verifiable();

            var resultFilter = new Mock<IResultFilter>(MockBehavior.Strict);
            resultFilter.Setup(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>())).Verifiable();
            resultFilter.Setup(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>())).Verifiable();

            var invoker = CreateInvoker(new IFilter[] { actionFilter.Object, resultFilter.Object }, actionThrows: true);

            // Act
            await invoker.InvokeAsync();

            // Assert
            actionFilter.Verify(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>()), Times.Once());
            actionFilter.Verify(f => f.OnActionExecuted(It.IsAny<ActionExecutedContext>()), Times.Once());

            resultFilter.Verify(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()), Times.Once());
            resultFilter.Verify(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>()), Times.Once());

            result.Verify(r => r.ExecuteResultAsync(It.IsAny<ActionContext>()), Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesResultFilter()
        {
            // Arrange
            var filter = new Mock<IResultFilter>(MockBehavior.Strict);
            filter.Setup(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>())).Verifiable();
            filter.Setup(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>())).Verifiable();

            var invoker = CreateInvoker(filter.Object);

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter.Verify(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()), Times.Once());
            filter.Verify(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>()), Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncResultFilter()
        {
            // Arrange
            var filter = new Mock<IAsyncResultFilter>(MockBehavior.Strict);
            filter
                .Setup(f => f.OnResultExecutionAsync(It.IsAny<ResultExecutingContext>(), It.IsAny<ResultExecutionDelegate>()))
                .Returns<ResultExecutingContext, ResultExecutionDelegate>(async (context, next) => await next())
                .Verifiable();

            var invoker = CreateInvoker(filter.Object);

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter.Verify(
                f => f.OnResultExecutionAsync(It.IsAny<ResultExecutingContext>(), It.IsAny<ResultExecutionDelegate>()),
                Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesResultFilter_ShortCircuit()
        {
            // Arrange
            ResultExecutedContext context = null;

            var filter1 = new Mock<IResultFilter>(MockBehavior.Strict);
            filter1.Setup(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>())).Verifiable();
            filter1
                .Setup(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>()))
                .Callback<ResultExecutedContext>(c => context = c)
                .Verifiable();

            var filter2 = new Mock<IResultFilter>(MockBehavior.Strict);
            filter2
                .Setup(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()))
                .Callback<ResultExecutingContext>(c =>
                {
                    filter2.ToString();
                    c.Cancel = true;
                })
                .Verifiable();

            var filter3 = new Mock<IResultFilter>(MockBehavior.Strict);

            var invoker = CreateInvoker(new IFilter[] { filter1.Object, filter2.Object, filter3.Object });

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter1.Verify(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()), Times.Once());
            filter1.Verify(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>()), Times.Once());

            filter2.Verify(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()), Times.Once());

            Assert.True(context.Canceled);
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncResultFilter_ShortCircuit_WithCancel()
        {
            // Arrange
            ResultExecutedContext context = null;

            var filter1 = new Mock<IResultFilter>(MockBehavior.Strict);
            filter1.Setup(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>())).Verifiable();
            filter1
                .Setup(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>()))
                .Callback<ResultExecutedContext>(c => context = c)
                .Verifiable();

            var filter2 = new Mock<IAsyncResultFilter>(MockBehavior.Strict);
            filter2
                .Setup(f => f.OnResultExecutionAsync(It.IsAny<ResultExecutingContext>(), It.IsAny<ResultExecutionDelegate>()))
                .Returns<ResultExecutingContext, ResultExecutionDelegate>((c, next) =>
                {
                    // Not calling next here
                    c.Cancel = true;
                    return Task.FromResult(true);
                })
                .Verifiable();

            var filter3 = new Mock<IResultFilter>(MockBehavior.Strict);

            var invoker = CreateInvoker(new IFilter[] { filter1.Object, filter2.Object, filter3.Object });

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter1.Verify(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()), Times.Once());
            filter1.Verify(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>()), Times.Once());

            filter2.Verify(
                f => f.OnResultExecutionAsync(It.IsAny<ResultExecutingContext>(), It.IsAny<ResultExecutionDelegate>()),
                Times.Once());

            Assert.True(context.Canceled);
            Assert.IsType<JsonResult>(context.Result);
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncResultFilter_ShortCircuit_WithoutCancel()
        {
            // Arrange
            ResultExecutedContext context = null;

            var filter1 = new Mock<IResultFilter>(MockBehavior.Strict);
            filter1.Setup(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>())).Verifiable();
            filter1
                .Setup(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>()))
                .Callback<ResultExecutedContext>(c => context = c)
                .Verifiable();

            var filter2 = new Mock<IAsyncResultFilter>(MockBehavior.Strict);
            filter2
                .Setup(f => f.OnResultExecutionAsync(It.IsAny<ResultExecutingContext>(), It.IsAny<ResultExecutionDelegate>()))
                .Returns<ResultExecutingContext, ResultExecutionDelegate>((c, next) =>
                {
                    // Not calling next here
                    return Task.FromResult(true);
                })
                .Verifiable();

            var filter3 = new Mock<IResultFilter>(MockBehavior.Strict);

            var invoker = CreateInvoker(new IFilter[] { filter1.Object, filter2.Object, filter3.Object });

            // Act
            await invoker.InvokeAsync();

            // Assert
            filter1.Verify(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()), Times.Once());
            filter1.Verify(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>()), Times.Once());

            filter2.Verify(
                f => f.OnResultExecutionAsync(It.IsAny<ResultExecutingContext>(), It.IsAny<ResultExecutionDelegate>()),
                Times.Once());

            Assert.True(context.Canceled);
            Assert.IsType<JsonResult>(context.Result);
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncResultFilter_ShortCircuit_WithoutCancel_CallNext()
        {
            // Arrange
            var filter = new Mock<IAsyncResultFilter>(MockBehavior.Strict);
            filter
                .Setup(f => f.OnResultExecutionAsync(It.IsAny<ResultExecutingContext>(), It.IsAny<ResultExecutionDelegate>()))
                .Returns<ResultExecutingContext, ResultExecutionDelegate>(async (c, next) =>
                {
                    // Not calling next here
                    c.Cancel = true;
                    await next();
                })
                .Verifiable();

            var message =
                "If an IAsyncResultFilter cancels execution by setting the Cancel property of " +
                "ResultExecutingContext to 'true', then it cannot call the next filter by invoking " +
                "ResultExecutionDelegate.";

            var invoker = CreateInvoker(filter.Object);

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<InvalidOperationException>(
                async () => await invoker.InvokeAsync(),
                message);
        }

        [Fact]
        public async Task InvokeAction_InvokesResultFilter_ExceptionGoesUnhandled()
        {
            // Arrange
            var exception = new DataMisalignedException();

            var result = new Mock<IActionResult>(MockBehavior.Strict);
            result
                .Setup(r => r.ExecuteResultAsync(It.IsAny<ActionContext>()))
                .Throws(exception)
                .Verifiable();

            var filter = new Mock<IResultFilter>(MockBehavior.Strict);
            filter
                .Setup(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()))
                .Callback<ResultExecutingContext>(c => c.Result = result.Object)
                .Verifiable();

            filter.Setup(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>())).Verifiable();

            var invoker = CreateInvoker(filter.Object);

            // Act
            await Assert.ThrowsAsync(exception.GetType(), async () => await invoker.InvokeAsync());

            // Assert
            result.Verify(r => r.ExecuteResultAsync(It.IsAny<ActionContext>()), Times.Once());

            filter.Verify(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()), Times.Once());
            filter.Verify(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>()), Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesResultFilter_WithExceptionThrownByResult()
        {
            // Arrange
            ResultExecutedContext context = null;
            var exception = new DataMisalignedException();

            var result = new Mock<IActionResult>(MockBehavior.Strict);
            result
                .Setup(r => r.ExecuteResultAsync(It.IsAny<ActionContext>()))
                .Throws(exception)
                .Verifiable();

            var filter = new Mock<IResultFilter>(MockBehavior.Strict);
            filter
                .Setup(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()))
                .Callback<ResultExecutingContext>(c => c.Result = result.Object)
                .Verifiable();

            filter
                .Setup(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>()))
                .Callback<ResultExecutedContext>(c =>
                {
                    context = c;

                    // Handle the exception
                    Assert.False(c.ExceptionHandled);
                    c.ExceptionHandled = true;
                })
                .Verifiable();

            var invoker = CreateInvoker(filter.Object);

            // Act
            await invoker.InvokeAsync();

            // Assert
            Assert.Same(exception, context.Exception);

            result.Verify(r => r.ExecuteResultAsync(It.IsAny<ActionContext>()), Times.Once());

            filter.Verify(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()), Times.Once());
            filter.Verify(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>()), Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncResultFilter_WithExceptionThrownByResult()
        {
            // Arrange
            ResultExecutedContext context = null;
            var exception = new DataMisalignedException();

            var result = new Mock<IActionResult>(MockBehavior.Strict);
            result
                .Setup(r => r.ExecuteResultAsync(It.IsAny<ActionContext>()))
                .Throws(exception)
                .Verifiable();

            var filter = new Mock<IAsyncResultFilter>(MockBehavior.Strict);
            filter
                .Setup(f => f.OnResultExecutionAsync(It.IsAny<ResultExecutingContext>(), It.IsAny<ResultExecutionDelegate>()))
                .Returns<ResultExecutingContext, ResultExecutionDelegate>(async (c, next) =>
                {
                    c.Result = result.Object;

                    context = await next();

                    // Handle the exception
                    Assert.False(context.ExceptionHandled);
                    context.ExceptionHandled = true;
                })
                .Verifiable();

            var invoker = CreateInvoker(filter.Object);

            // Act
            await invoker.InvokeAsync();

            // Assert
            Assert.Same(exception, context.Exception);

            result.Verify(r => r.ExecuteResultAsync(It.IsAny<ActionContext>()), Times.Once());

            filter.Verify(
                f => f.OnResultExecutionAsync(It.IsAny<ResultExecutingContext>(), It.IsAny<ResultExecutionDelegate>()),
                Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesResultFilter_WithExceptionThrownByResultFilter()
        {
            // Arrange
            ResultExecutedContext context = null;
            var exception = new DataMisalignedException();

            var resultFilter1 = new Mock<IResultFilter>(MockBehavior.Strict);
            resultFilter1.Setup(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>())).Verifiable();
            resultFilter1
                .Setup(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>()))
                .Callback<ResultExecutedContext>(c =>
                {
                    context = c;

                    // Handle the exception
                    Assert.False(c.ExceptionHandled);
                    c.ExceptionHandled = true;
                })
                .Verifiable();

            var resultFilter2 = new Mock<IResultFilter>(MockBehavior.Strict);
            resultFilter2
                .Setup(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()))
                .Throws(exception)
                .Verifiable();

            var resultFilter3 = new Mock<IResultFilter>(MockBehavior.Strict);

            var invoker = CreateInvoker(new IFilter[] { resultFilter1.Object, resultFilter2.Object, resultFilter3.Object });

            // Act
            await invoker.InvokeAsync();

            // Assert
            Assert.Same(exception, context.Exception);

            resultFilter1.Verify(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()), Times.Once());
            resultFilter1.Verify(f => f.OnResultExecuted(It.IsAny<ResultExecutedContext>()), Times.Once());

            resultFilter2.Verify(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()), Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncResultFilter_WithExceptionThrownByResultFilter()
        {
            // Arrange
            ResultExecutedContext context = null;
            var exception = new DataMisalignedException();

            var resultFilter1 = new Mock<IAsyncResultFilter>(MockBehavior.Strict);
            resultFilter1
                .Setup(f => f.OnResultExecutionAsync(It.IsAny<ResultExecutingContext>(), It.IsAny<ResultExecutionDelegate>()))
                .Returns<ResultExecutingContext, ResultExecutionDelegate>(async (c, next) =>
                {
                    context = await next();

                    // Handle the exception
                    Assert.False(context.ExceptionHandled);
                    context.ExceptionHandled = true;
                })
                .Verifiable();

            var resultFilter2 = new Mock<IResultFilter>(MockBehavior.Strict);
            resultFilter2
                .Setup(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()))
                .Throws(exception)
                .Verifiable();

            var resultFilter3 = new Mock<IResultFilter>(MockBehavior.Strict);

            var invoker = CreateInvoker(new IFilter[] { resultFilter1.Object, resultFilter2.Object, resultFilter3.Object });

            // Act
            await invoker.InvokeAsync();

            // Assert
            Assert.Same(exception, context.Exception);

            resultFilter1.Verify(
                f => f.OnResultExecutionAsync(It.IsAny<ResultExecutingContext>(), It.IsAny<ResultExecutionDelegate>()),
                Times.Once());

            resultFilter2.Verify(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()), Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncResourceFilter()
        {
            // Arrange
            var resourceFilter = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            resourceFilter
                .Setup(f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()))
                .Returns<ResourceExecutingContext, ResourceExecutionDelegate>(async (c, next) =>
                {
                    await next();
                })
                .Verifiable();

            var invoker = CreateInvoker(new IFilter[] { resourceFilter.Object });

            // Act
            await invoker.InvokeAsync();

            // Assert
            resourceFilter.Verify(
                f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()),
                Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesResourceFilter()
        {
            // Arrange
            var resourceFilter = new Mock<IResourceFilter>(MockBehavior.Strict);
            resourceFilter
                .Setup(f => f.OnResourceExecuting(It.IsAny<ResourceExecutingContext>()))
                .Verifiable();
            resourceFilter
                .Setup(f => f.OnResourceExecuted(It.IsAny<ResourceExecutedContext>()))
                .Verifiable();

            var invoker = CreateInvoker(resourceFilter.Object);

            // Act
            await invoker.InvokeAsync();

            // Assert
            resourceFilter.Verify(
                f => f.OnResourceExecuted(It.IsAny<ResourceExecutedContext>()),
                Times.Once());
            resourceFilter.Verify(
                f => f.OnResourceExecuted(It.IsAny<ResourceExecutedContext>()),
                Times.Once());

        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncResourceFilter_WithActionResult_FromAction()
        {
            // Arrange
            ResourceExecutedContext context = null;
            var resourceFilter = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            resourceFilter
                .Setup(f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()))
                .Returns<ResourceExecutingContext, ResourceExecutionDelegate>(async (c, next) =>
                {
                    context = await next();
                })
                .Verifiable();

            var invoker = CreateInvoker(resourceFilter.Object);

            // Act
            await invoker.InvokeAsync();

            // Assert
            Assert.Same(_result, context.Result);

            resourceFilter.Verify(
                f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()),
                Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncResourceFilter_WithActionResult_FromActionFilter()
        {
            // Arrange
            var expected = Mock.Of<IActionResult>();

            ResourceExecutedContext context = null;
            var resourceFilter = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            resourceFilter
                .Setup(f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()))
                .Returns<ResourceExecutingContext, ResourceExecutionDelegate>(async (c, next) =>
                {
                    context = await next();
                })
                .Verifiable();

            var actionFilter = new Mock<IActionFilter>(MockBehavior.Strict);
            actionFilter
                .Setup(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>()))
                .Callback<ActionExecutingContext>((c) =>
                {
                    c.Result = expected;
                });

            var invoker = CreateInvoker(new IFilter[] { resourceFilter.Object, actionFilter.Object });

            // Act
            await invoker.InvokeAsync();

            // Assert
            Assert.Same(expected, context.Result);

            resourceFilter.Verify(
                f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()),
                Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncResourceFilter_WithActionResult_FromExceptionFilter()
        {
            // Arrange
            var expected = Mock.Of<IActionResult>();

            ResourceExecutedContext context = null;
            var resourceFilter = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            resourceFilter
                .Setup(f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()))
                .Returns<ResourceExecutingContext, ResourceExecutionDelegate>(async (c, next) =>
                {
                    context = await next();
                })
                .Verifiable();

            var exceptionFilter = new Mock<IExceptionFilter>(MockBehavior.Strict);
            exceptionFilter
                .Setup(f => f.OnException(It.IsAny<ExceptionContext>()))
                .Callback<ExceptionContext>((c) =>
                {
                    c.Result = expected;
                });

            var invoker = CreateInvoker(new IFilter[] { resourceFilter.Object, exceptionFilter.Object }, actionThrows: true);

            // Act
            await invoker.InvokeAsync();

            // Assert
            Assert.Same(expected, context.Result);
            Assert.Null(context.Exception);
            Assert.Null(context.ExceptionDispatchInfo);

            resourceFilter.Verify(
                f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()),
                Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncResourceFilter_WithActionResult_FromResultFilter()
        {
            // Arrange
            var expected = Mock.Of<IActionResult>();

            ResourceExecutedContext context = null;
            var resourceFilter = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            resourceFilter
                .Setup(f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()))
                .Returns<ResourceExecutingContext, ResourceExecutionDelegate>(async (c, next) =>
                {
                    context = await next();
                })
                .Verifiable();

            var resultFilter = new Mock<IResultFilter>(MockBehavior.Loose);
            resultFilter
                .Setup(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()))
                .Callback<ResultExecutingContext>((c) =>
                {
                    c.Result = expected;
                });

            var invoker = CreateInvoker(new IFilter[] { resourceFilter.Object, resultFilter.Object });

            // Act
            await invoker.InvokeAsync();

            // Assert
            Assert.Same(expected, context.Result);

            resourceFilter.Verify(
                f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()),
                Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncResourceFilter_HandleException_FromAction()
        {
            // Arrange
            ResourceExecutedContext context = null;
            var resourceFilter = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            resourceFilter
                .Setup(f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()))
                .Returns<ResourceExecutingContext, ResourceExecutionDelegate>(async (c, next) =>
                {
                    context = await next();
                    context.ExceptionHandled = true;
                })
                .Verifiable();

            var invoker = CreateInvoker(new IFilter[] { resourceFilter.Object }, actionThrows: true);

            // Act
            await invoker.InvokeAsync();

            // Assert
            Assert.Same(_actionException, context.Exception);
            Assert.Same(_actionException, context.ExceptionDispatchInfo.SourceException);

            resourceFilter.Verify(
                f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()),
                Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncResourceFilter_HandleException_FromActionFilter()
        {
            // Arrange
            var expected = new DataMisalignedException();

            ResourceExecutedContext context = null;
            var resourceFilter = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            resourceFilter
                .Setup(f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()))
                .Returns<ResourceExecutingContext, ResourceExecutionDelegate>(async (c, next) =>
                {
                    context = await next();
                    context.ExceptionHandled = true;
                })
                .Verifiable();

            var actionFilter = new Mock<IActionFilter>(MockBehavior.Strict);
            actionFilter
                .Setup(f => f.OnActionExecuting(It.IsAny<ActionExecutingContext>()))
                .Callback<ActionExecutingContext>((c) =>
                {
                    throw expected;
                });

            var invoker = CreateInvoker(new IFilter[] { resourceFilter.Object, actionFilter.Object });

            // Act
            await invoker.InvokeAsync();

            // Assert
            Assert.Same(expected, context.Exception);
            Assert.Same(expected, context.ExceptionDispatchInfo.SourceException);

            resourceFilter.Verify(
                f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()),
                Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncResourceFilter_HandlesException_FromExceptionFilter()
        {
            // Arrange
            var expected = new DataMisalignedException();

            ResourceExecutedContext context = null;
            var resourceFilter = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            resourceFilter
                .Setup(f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()))
                .Returns<ResourceExecutingContext, ResourceExecutionDelegate>(async (c, next) =>
                {
                    context = await next();
                    context.ExceptionHandled = true;
                })
                .Verifiable();

            var exceptionFilter = new Mock<IExceptionFilter>(MockBehavior.Strict);
            exceptionFilter
                .Setup(f => f.OnException(It.IsAny<ExceptionContext>()))
                .Callback<ExceptionContext>((c) =>
                {
                    throw expected;
                });

            var invoker = CreateInvoker(new IFilter[] { resourceFilter.Object, exceptionFilter.Object }, actionThrows: true);

            // Act
            await invoker.InvokeAsync();

            // Assert
            Assert.Same(expected, context.Exception);
            Assert.Same(expected, context.ExceptionDispatchInfo.SourceException);

            resourceFilter.Verify(
                f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()),
                Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncResourceFilter_HandlesException_FromResultFilter()
        {
            // Arrange
            var expected = new DataMisalignedException();

            ResourceExecutedContext context = null;
            var resourceFilter = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            resourceFilter
                .Setup(f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()))
                .Returns<ResourceExecutingContext, ResourceExecutionDelegate>(async (c, next) =>
                {
                    context = await next();
                    context.ExceptionHandled = true;
                })
                .Verifiable();

            var resultFilter = new Mock<IResultFilter>(MockBehavior.Loose);
            resultFilter
                .Setup(f => f.OnResultExecuting(It.IsAny<ResultExecutingContext>()))
                .Callback<ResultExecutingContext>((c) =>
                {
                    throw expected;
                });

            var invoker = CreateInvoker(new IFilter[] { resourceFilter.Object, resultFilter.Object });

            // Act
            await invoker.InvokeAsync();

            // Assert
            Assert.Same(expected, context.Exception);
            Assert.Same(expected, context.ExceptionDispatchInfo.SourceException);

            resourceFilter.Verify(
                f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()),
                Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncResourceFilter_HandleException_BySettingNull()
        {
            // Arrange
            ResourceExecutedContext context = null;
            var resourceFilter = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            resourceFilter
                .Setup(f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()))
                .Returns<ResourceExecutingContext, ResourceExecutionDelegate>(async (c, next) =>
                {
                    context = await next();

                    Assert.Same(_actionException, context.Exception);
                    Assert.Same(_actionException, context.ExceptionDispatchInfo.SourceException);

                    context.Exception = null;
                })
                .Verifiable();

            var invoker = CreateInvoker(new IFilter[] { resourceFilter.Object }, actionThrows: true);

            // Act
            await invoker.InvokeAsync();

            // Assert
            resourceFilter.Verify(
                f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()),
                Times.Once());
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncResourceFilter_ThrowsUnhandledException()
        {
            // Arrange
            var expected = new DataMisalignedException();

            ResourceExecutedContext context = null;
            var resourceFilter1 = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            resourceFilter1
                .Setup(f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()))
                .Returns<ResourceExecutingContext, ResourceExecutionDelegate>(async (c, next) =>
                {
                    context = await next();
                })
                .Verifiable();

            var resourceFilter2 = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            resourceFilter2
                .Setup(f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()))
                .Returns<ResourceExecutingContext, ResourceExecutionDelegate>((c, next) =>
                {
                    throw expected;
                })
                .Verifiable();

            var invoker = CreateInvoker(new IFilter[] { resourceFilter1.Object, resourceFilter2.Object }, actionThrows: true);

            // Act
            var exception = await Assert.ThrowsAsync<DataMisalignedException>(invoker.InvokeAsync);

            // Assert
            Assert.Same(expected, exception);
            Assert.Same(expected, context.Exception);
            Assert.Same(expected, context.ExceptionDispatchInfo.SourceException);
        }

        [Fact]
        public async Task InvokeAction_InvokesResourceFilter_OnResourceExecuting_ThrowsUnhandledException()
        {
            // Arrange
            var expected = new DataMisalignedException();

            ResourceExecutedContext context = null;
            var resourceFilter1 = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            resourceFilter1
                .Setup(f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()))
                .Returns<ResourceExecutingContext, ResourceExecutionDelegate>(async (c, next) =>
                {
                    context = await next();
                })
                .Verifiable();

            var resourceFilter2 = new Mock<IResourceFilter>(MockBehavior.Strict);
            resourceFilter2
                .Setup(f => f.OnResourceExecuting(It.IsAny<ResourceExecutingContext>()))
                .Callback<ResourceExecutingContext>((c) =>
                {
                    throw expected;
                })
                .Verifiable();

            var invoker = CreateInvoker(new IFilter[] { resourceFilter1.Object, resourceFilter2.Object }, actionThrows: true);

            // Act
            var exception = await Assert.ThrowsAsync<DataMisalignedException>(invoker.InvokeAsync);

            // Assert
            Assert.Same(expected, exception);
            Assert.Same(expected, context.Exception);
            Assert.Same(expected, context.ExceptionDispatchInfo.SourceException);
        }

        [Fact]
        public async Task InvokeAction_InvokesResourceFilter_OnResourceExecuted_ThrowsUnhandledException()
        {
            // Arrange
            var expected = new DataMisalignedException();

            ResourceExecutedContext context = null;
            var resourceFilter1 = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            resourceFilter1
                .Setup(f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()))
                .Returns<ResourceExecutingContext, ResourceExecutionDelegate>(async (c, next) =>
                {
                    context = await next();
                })
                .Verifiable();

            var resourceFilter2 = new Mock<IResourceFilter>(MockBehavior.Loose);
            resourceFilter2
                .Setup(f => f.OnResourceExecuted(It.IsAny<ResourceExecutedContext>()))
                .Callback<ResourceExecutedContext>((c) =>
                {
                    throw expected;
                })
                .Verifiable();

            var invoker = CreateInvoker(new IFilter[] { resourceFilter1.Object, resourceFilter2.Object }, actionThrows: true);

            // Act
            var exception = await Assert.ThrowsAsync<DataMisalignedException>(invoker.InvokeAsync);

            // Assert
            Assert.Same(expected, exception);
            Assert.Same(expected, context.Exception);
            Assert.Same(expected, context.ExceptionDispatchInfo.SourceException);
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncResourceFilter_ShortCircuit()
        {
            // Arrange
            var expected = new Mock<IActionResult>(MockBehavior.Strict);
            expected
                .Setup(r => r.ExecuteResultAsync(It.IsAny<ActionContext>()))
                .Returns(Task.FromResult(true))
                .Verifiable();

            ResourceExecutedContext context = null;
            var resourceFilter1 = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            resourceFilter1
                .Setup(f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()))
                .Returns<ResourceExecutingContext, ResourceExecutionDelegate>(async (c, next) =>
                {
                    context = await next();
                })
                .Verifiable();

            var resourceFilter2 = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            resourceFilter2
                .Setup(f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()))
                .Returns<ResourceExecutingContext, ResourceExecutionDelegate>((c, next) =>
                {
                    c.Result = expected.Object;
                    return Task.FromResult(true);
                })
                .Verifiable();

            var resourceFilter3 = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            var exceptionFilter = new Mock<IExceptionFilter>(MockBehavior.Strict);
            var actionFilter = new Mock<IAsyncActionFilter>(MockBehavior.Strict);
            var resultFilter = new Mock<IAsyncResultFilter>(MockBehavior.Strict);

            var invoker = CreateInvoker(
                new IFilter[]
                {
                    resourceFilter1.Object, // This filter should see the result retured from resourceFilter2
                    resourceFilter2.Object, // This filter will short circuit
                    resourceFilter3.Object, // This shouldn't run - it will throw if it does
                    exceptionFilter.Object, // This shouldn't run - it will throw if it does
                    actionFilter.Object, // This shouldn't run - it will throw if it does
                    resultFilter.Object // This shouldn't run - it will throw if it does
                },
                // The action won't run
                actionThrows: true);

            // Act
            await invoker.InvokeAsync();

            // Assert
            Assert.Same(expected.Object, context.Result);
            Assert.True(context.Canceled);
            Assert.False(invoker.ControllerFactory.CreateCalled);
        }

        [Fact]
        public async Task InvokeAction_InvokesResourceFilter_ShortCircuit()
        {
            // Arrange
            var expected = new Mock<IActionResult>(MockBehavior.Strict);
            expected
                .Setup(r => r.ExecuteResultAsync(It.IsAny<ActionContext>()))
                .Returns(Task.FromResult(true))
                .Verifiable();

            ResourceExecutedContext context = null;
            var resourceFilter1 = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            resourceFilter1
                .Setup(f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()))
                .Returns<ResourceExecutingContext, ResourceExecutionDelegate>(async (c, next) =>
                {
                    context = await next();
                });

            var resourceFilter2 = new Mock<IResourceFilter>(MockBehavior.Strict);
            resourceFilter2
                .Setup(f => f.OnResourceExecuting(It.IsAny<ResourceExecutingContext>()))
                .Callback<ResourceExecutingContext>((c) =>
                {
                    c.Result = expected.Object;
                });

            var resourceFilter3 = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            var actionFilter = new Mock<IAsyncActionFilter>(MockBehavior.Strict);
            var resultFilter = new Mock<IAsyncResultFilter>(MockBehavior.Strict);

            var invoker = CreateInvoker(
                new IFilter[]
                {
                    resourceFilter1.Object, // This filter should see the result retured from resourceFilter2
                    resourceFilter2.Object,
                    resourceFilter3.Object, // This shouldn't run - it will throw if it does
                    actionFilter.Object, // This shouldn't run - it will throw if it does
                    resultFilter.Object // This shouldn't run - it will throw if it does
                },
                // The action won't run
                actionThrows: true);

            // Act
            await invoker.InvokeAsync();

            // Assert
            Assert.Same(expected.Object, context.Result);
            Assert.True(context.Canceled);
            Assert.False(invoker.ControllerFactory.CreateCalled);
        }

        [Fact]
        public async Task InvokeAction_InvokesAsyncResourceFilter_InvalidShortCircuit()
        {
            // Arrange
            var message =
                "If an IAsyncResourceFilter provides a result value by setting the Result property of " +
                "ResourceExecutingContext to a non-null value, then it cannot call the next filter by invoking " +
                "ResourceExecutionDelegate.";

            ResourceExecutedContext context = null;
            var resourceFilter = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            resourceFilter
                .Setup(f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()))
                .Returns<ResourceExecutingContext, ResourceExecutionDelegate>(async (c, next) =>
                {
                    // This is not valid.
                    c.Result = Mock.Of<IActionResult>();
                    context = await next();
                });

            var invoker = CreateInvoker(new IFilter[] { resourceFilter.Object, });

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(invoker.InvokeAsync);

            // Assert
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public async Task InvokeAction_AuthorizationFilter_ChallengePreventsResourceFiltersFromRunning()
        {
            // Arrange
            var resourceFilter = new Mock<IAsyncResourceFilter>(MockBehavior.Strict);
            resourceFilter
                .Setup(f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()))
                .Returns<ResourceExecutingContext, ResourceExecutionDelegate>(async (c, next) =>
                {
                    await next();
                })
                .Verifiable();

            var authorizationFilter = new Mock<IAuthorizationFilter>(MockBehavior.Strict);
            authorizationFilter
                .Setup(f => f.OnAuthorization(It.IsAny<AuthorizationContext>()))
                .Callback<AuthorizationContext>((c) =>
                {
                    c.Result = _result;
                });

            var invoker = CreateInvoker(new IFilter[] { authorizationFilter.Object, resourceFilter.Object, });

            // Act
            await invoker.InvokeAsync();

            // Assert
            resourceFilter.Verify(
                f => f.OnResourceExecutionAsync(It.IsAny<ResourceExecutingContext>(), It.IsAny<ResourceExecutionDelegate>()),
                Times.Never());

            Assert.False(invoker.ControllerFactory.CreateCalled);
        }

        [Fact]
        public void CreateActionResult_ReturnsSameActionResult()
        {
            // Arrange
            var mockActionResult = new Mock<IActionResult>();

            // Assert
            var result = ControllerActionInvoker.CreateActionResult(
                mockActionResult.Object.GetType(), mockActionResult.Object);

            // Act
            Assert.Same(mockActionResult.Object, result);
        }

        [Fact]
        [ReplaceCulture]
        public void CreateActionResult_NullActionResultReturnValueThrows()
        {
            // Arrange, Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => ControllerActionInvoker.CreateActionResult(typeof(IActionResult), null),
                "Cannot return null from an action method with a return type of '"
                    + typeof(IActionResult)
                    + "'.");
        }

        [Theory]
        [InlineData(typeof(void))]
        [InlineData(typeof(Task))]
        public void CreateActionResult_Types_ReturnsObjectResultForTaskAndVoidReturnTypes(Type type)
        {
            // Arrange & Act
            var result = ControllerActionInvoker.CreateActionResult(type, null);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);

            // Since we unwrap the Task type to void, the expected type will always be void.
            Assert.Equal(typeof(void), objectResult.DeclaredType);
            Assert.Null(objectResult.Value);
        }

        public static IEnumerable<object[]> CreateActionResult_ReturnsObjectContentResultData
        {
            get
            {
                var anonymousObject = new { x1 = 10, y1 = "Hello" };
                yield return new object[] { anonymousObject.GetType(), anonymousObject, };
                yield return new object[] { typeof(int), 5 };
                yield return new object[] { typeof(string), "sample input" };

                SampleStruct test;
                test.x = 10;
                yield return new object[] { test.GetType(), test };
                yield return new object[] { typeof(Task<int>), 5 };
                yield return new object[] { typeof(Task<string>), "Hello world" };
            }
        }

        [Theory]
        [MemberData(nameof(CreateActionResult_ReturnsObjectContentResultData))]
        public void CreateActionResult_ReturnsObjectContentResult(Type type, object input)
        {
            // Arrange & Act
            var actualResult = ControllerActionInvoker.CreateActionResult(type, input);

            // Assert
            var contentResult = Assert.IsType<ObjectResult>(actualResult);
            Assert.Same(input, contentResult.Value);
        }

        private TestControllerActionInvoker CreateInvoker(
            IFilter filter,
            bool actionThrows = false,
            ITempDataDictionary tempData = null)
        {
            return CreateInvoker(new[] { filter }, actionThrows, tempData);
        }

        private TestControllerActionInvoker CreateInvoker(
            IFilter[] filters,
            bool actionThrows = false,
            ITempDataDictionary tempData = null)
        {
            var actionDescriptor = new ControllerActionDescriptor()
            {
                FilterDescriptors = new List<FilterDescriptor>(),
                Parameters = new List<ParameterDescriptor>(),
            };

            if (actionThrows)
            {
                actionDescriptor.MethodInfo = typeof(ControllerActionInvokerTest).GetMethod("ThrowingActionMethod");
            }
            else
            {
                actionDescriptor.MethodInfo = typeof(ControllerActionInvokerTest).GetMethod("ActionMethod");
            }

            tempData = tempData ?? new Mock<ITempDataDictionary>().Object;
            var httpContext = new Mock<HttpContext>(MockBehavior.Loose);
            var httpRequest = new DefaultHttpContext().Request;
            var httpResponse = new DefaultHttpContext().Response;
            var mockFormattersProvider = new Mock<IOutputFormattersProvider>();
            mockFormattersProvider.SetupGet(o => o.OutputFormatters)
                                  .Returns(
                                        new List<IOutputFormatter>()
                                        {
                                            new JsonOutputFormatter()
                                        });
            httpContext.SetupGet(c => c.Request).Returns(httpRequest);
            httpContext.SetupGet(c => c.Response).Returns(httpResponse);
            httpContext.Setup(o => o.RequestServices.GetService(typeof(IOutputFormattersProvider)))
                       .Returns(mockFormattersProvider.Object);
            httpContext.Setup(o => o.RequestServices.GetService(typeof(ITempDataDictionary)))
                       .Returns(tempData);
            httpResponse.Body = new MemoryStream();

            var options = new Mock<IOptions<MvcOptions>>();
            options.SetupGet(o => o.Options)
                       .Returns(new MvcOptions());
            httpContext.Setup(o => o.RequestServices.GetService(typeof(IOptions<MvcOptions>)))
                       .Returns(options.Object);

            var actionContext = new ActionContext(
                httpContext: httpContext.Object,
                routeData: new RouteData(),
                actionDescriptor: actionDescriptor);

            var filterProvider = new Mock<IFilterProvider>(MockBehavior.Strict);
            filterProvider
                .Setup(fp => fp.OnProvidersExecuting(It.IsAny<FilterProviderContext>()))
                .Callback<FilterProviderContext>(context =>
                    {
                        foreach (var filter in filters.Select(f => new FilterItem(null, f)))
                        {
                            context.Results.Add(filter);
                        }
                    });

            filterProvider.Setup(fp => fp.OnProvidersExecuted(It.IsAny<FilterProviderContext>()))
                          .Verifiable();

            filterProvider.SetupGet(fp => fp.Order)
                          .Returns(DefaultOrder.DefaultFrameworkSortOrder);


            var inputFormattersProvider = new Mock<IInputFormattersProvider>();
            inputFormattersProvider.SetupGet(o => o.InputFormatters)
                                            .Returns(new List<IInputFormatter>());
            var excludeFilterProvider = new Mock<IValidationExcludeFiltersProvider>();
            excludeFilterProvider.SetupGet(o => o.ExcludeFilters)
                                 .Returns(new List<IExcludeTypeValidationFilter>());
            var invoker = new TestControllerActionInvoker(
                actionContext,
                new[] { filterProvider.Object },
                new MockControllerFactory(this),
                actionDescriptor,
                inputFormattersProvider.Object,
                Mock.Of<IControllerActionArgumentBinder>(),
                new MockModelBinderProvider(),
                new MockModelValidatorProviderProvider(),
                new MockValueProviderFactoryProvider(),
                new MockScopedInstance<ActionBindingContext>(),
                tempData);

            return invoker;
        }

        [Fact]
        public async Task Invoke_UsesDefaultValuesIfNotBound()
        {
            // Arrange
            var actionDescriptor = new ControllerActionDescriptor
            {
                MethodInfo = typeof(TestController).GetTypeInfo()
                    .DeclaredMethods
                    .First(m => m.Name.Equals("ActionMethodWithDefaultValues", StringComparison.Ordinal)),

                Parameters = new List<ParameterDescriptor>
                {
                    new ParameterDescriptor
                    {
                        Name = "value",
                        ParameterType = typeof(int),
                        BindingInfo = new BindingInfo(),
                    }
                },
                FilterDescriptors = new List<FilterDescriptor>()
            };

            var binder = new Mock<IModelBinder>();
            binder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
                  .Returns(Task.FromResult<ModelBindingResult>(result: null));
            var context = new Mock<HttpContext>();
            context.SetupGet(c => c.Items)
                   .Returns(new Dictionary<object, object>());
            context.Setup(c => c.RequestServices.GetService(typeof(ITempDataDictionary)))
                       .Returns(new Mock<ITempDataDictionary>().Object);

            var actionContext = new ActionContext(context.Object, new RouteData(), actionDescriptor);

            var controllerFactory = new Mock<IControllerFactory>();
            controllerFactory.Setup(c => c.CreateController(It.IsAny<ActionContext>()))
                             .Returns(new TestController());
            var inputFormattersProvider = new Mock<IInputFormattersProvider>();
            inputFormattersProvider.SetupGet(o => o.InputFormatters)
                                            .Returns(new List<IInputFormatter>());
            var metadataProvider = new EmptyModelMetadataProvider();
            var invoker = new ControllerActionInvoker(
                actionContext,
                new List<IFilterProvider>(),
                controllerFactory.Object,
                actionDescriptor,
                inputFormattersProvider.Object,
                new DefaultControllerActionArgumentBinder(
                    metadataProvider,
                    new DefaultObjectValidator(Mock.Of<IValidationExcludeFiltersProvider>(), metadataProvider),
                    new MockMvcOptionsAccessor()),
                new MockModelBinderProvider() { ModelBinders = new List<IModelBinder>() { binder.Object } },
                new MockModelValidatorProviderProvider(),
                new MockValueProviderFactoryProvider(),
                new MockScopedInstance<ActionBindingContext>(),
                Mock.Of<ITempDataDictionary>());

            // Act
            await invoker.InvokeAsync();

            // Assert
            Assert.Equal(5, context.Object.Items["Result"]);
        }

        public JsonResult ActionMethod()
        {
            return _result;
        }

        public JsonResult ThrowingActionMethod()
        {
            throw _actionException;
        }

        public JsonResult ActionMethodWithBodyParameter([FromBody] Person bodyParam)
        {
            return new JsonResult(bodyParam);
        }

        public class Person
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }

        private sealed class TestController
        {
            public IActionResult ActionMethodWithDefaultValues(int value = 5)
            {
                return new TestActionResult { Value = value };
            }
        }

        private sealed class TestActionResult : IActionResult
        {
            public int Value { get; set; }

            public Task ExecuteResultAsync(ActionContext context)
            {
                context.HttpContext.Items["Result"] = Value;
                return Task.FromResult(0);
            }
        }

        private class MockControllerFactory : IControllerFactory
        {
            private object _controller;

            public MockControllerFactory(object controller)
            {
                _controller = controller;
            }

            public bool CreateCalled { get; private set; }

            public bool ReleaseCalled { get; private set; }

            public object CreateController(ActionContext actionContext)
            {
                CreateCalled = true;
                return _controller;
            }

            public void ReleaseController(object controller)
            {
                Assert.NotNull(controller);
                Assert.Same(_controller, controller);
                ReleaseCalled = true;
            }

            public void Verify()
            {
                if (CreateCalled && !ReleaseCalled)
                {
                    Assert.False(true, "ReleaseController should have been called.");
                }
            }
        }

        private class TestControllerActionInvoker : ControllerActionInvoker
        {
            public TestControllerActionInvoker(
                ActionContext actionContext,
                IFilterProvider[] filterProvider,
                MockControllerFactory controllerFactory,
                ControllerActionDescriptor descriptor,
                IInputFormattersProvider inputFormattersProvider,
                IControllerActionArgumentBinder controllerActionArgumentBinder,
                IModelBinderProvider modelBinderProvider,
                IModelValidatorProviderProvider modelValidatorProviderProvider,
                IValueProviderFactoryProvider valueProviderFactoryProvider,
                IScopedInstance<ActionBindingContext> actionBindingContext,
                ITempDataDictionary tempData)
                : base(
                      actionContext,
                      filterProvider,
                      controllerFactory,
                      descriptor,
                      inputFormattersProvider,
                      controllerActionArgumentBinder,
                      modelBinderProvider,
                      modelValidatorProviderProvider,
                      valueProviderFactoryProvider,
                      actionBindingContext,
                      tempData)
            {
                ControllerFactory = controllerFactory;
            }

            public MockControllerFactory ControllerFactory { get; }

            public async override Task InvokeAsync()
            {
                await base.InvokeAsync();

                // Make sure that the controller was disposed in every test that creates ones.
                ControllerFactory.Verify();
            }
        }
    }
}
