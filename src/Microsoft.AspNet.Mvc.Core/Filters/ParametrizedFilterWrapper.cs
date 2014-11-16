// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    internal sealed class ParametrizedFilterWrapper<T> :
        IAuthorizationFilter,
        IAsyncAuthorizationFilter,
        IExceptionFilter,
        IAsyncExceptionFilter,
        IActionFilter,
        IAsyncActionFilter,
        IResultFilter,
        IAsyncResultFilter
    {
        private readonly T _data;
        private readonly IParametrizedFilter<T> _handler;

        public ParametrizedFilterWrapper([NotNull] IParametrizedFilter<T> handler, [NotNull] T data)
        {
            _handler = handler;
            _data = data;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _handler.OnActionExecuting(context, _data);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _handler.OnActionExecuted(context, _data);
        }

        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            return _handler.OnActionExecutionAsync(context, next, _data);
        }

        public Task OnAuthorizationAsync(AuthorizationContext context)
        {
            return _handler.OnAuthorizationAsync(context, _data);
        }

        public Task OnExceptionAsync(ExceptionContext context)
        {
            return _handler.OnExceptionAsync(context, _data);
        }

        public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            return _handler.OnResultExecutionAsync(context, next, _data);
        }

        public void OnAuthorization(AuthorizationContext context)
        {
            _handler.OnAuthorization(context, _data);
        }

        public void OnException(ExceptionContext context)
        {
            _handler.OnException(context, _data);
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            _handler.OnResultExecuting(context, _data);
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            _handler.OnResultExecuted(context, _data);
        }
    }
}