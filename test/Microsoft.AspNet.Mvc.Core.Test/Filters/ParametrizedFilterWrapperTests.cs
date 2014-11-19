// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Test
{
    public class ParametrizedFilterWrapperTests
    {
        [Fact]
        public async Task ParametrizedFilterWrapper_AllMethods_DelegatedProperly()
        {
            // Arrange
            var filter = new Mock<IParametrizedFilter<object>>();
            var parameters = new object();
            var wrapper = new ParametrizedFilterWrapper<object>(filter.Object, parameters);
            
            var actionContext = ActionFilterAttributeTests.CreateActionContext();
            var exceptionContext = new ExceptionContext(actionContext, new IFilter[0]);
            var authorizationContext = new AuthorizationContext(actionContext, new IFilter[0]);

            var actionExecutingContext = 
                ActionFilterAttributeTests.CreateActionExecutingContext(filter.Object);
            var actionExecutedContext = 
                ActionFilterAttributeTests.CreateActionExecutedContext(actionExecutingContext);
            ActionExecutionDelegate actionExecutionDelegate = () => null;

            var resultExecutingContext =
                ActionFilterAttributeTests.CreateResultExecutingContext(filter.Object);
            var resultExecutedContext =
                ActionFilterAttributeTests.CreateResultExecutedContext(resultExecutingContext);
            ResultExecutionDelegate resultExecutionDelegate = () => null;

            // Act
            wrapper.OnException(exceptionContext);
            await wrapper.OnExceptionAsync(exceptionContext);

            wrapper.OnAuthorization(authorizationContext);
            await wrapper.OnAuthorizationAsync(authorizationContext);

            wrapper.OnActionExecuting(actionExecutingContext);
            wrapper.OnActionExecuted(actionExecutedContext);
            await wrapper.OnActionExecutionAsync(actionExecutingContext, actionExecutionDelegate);

            wrapper.OnResultExecuting(resultExecutingContext);
            wrapper.OnResultExecuted(resultExecutedContext);
            await wrapper.OnResultExecutionAsync(resultExecutingContext, resultExecutionDelegate);

            // Assert
            filter.Verify(_ => _.OnException(exceptionContext, parameters));
            filter.Verify(_ => _.OnExceptionAsync(exceptionContext, parameters));

            filter.Verify(_ => _.OnAuthorization(authorizationContext, parameters));
            filter.Verify(_ => _.OnAuthorizationAsync(authorizationContext, parameters));

            filter.Verify(_ => _.OnActionExecuting(actionExecutingContext, parameters));
            filter.Verify(_ => _.OnActionExecuted(actionExecutedContext, parameters));
            filter.Verify(_ => _.OnActionExecutionAsync(actionExecutingContext, actionExecutionDelegate, parameters));

            filter.Verify(_ => _.OnResultExecuting(resultExecutingContext, parameters));
            filter.Verify(_ => _.OnResultExecuted(resultExecutedContext, parameters));
            filter.Verify(_ => _.OnResultExecutionAsync(resultExecutingContext, resultExecutionDelegate, parameters));
        }
    }
}

