// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    public abstract class ParametrizedFilterBase<TParameters> : IParametrizedFilter<TParameters>
    {
        public virtual void OnException(ExceptionContext context, TParameters parameters)
        {
        }

        public virtual Task OnExceptionAsync(ExceptionContext context, TParameters parameters)
        {
            return Task.CompletedTask;
        }

        public virtual void OnAuthorization(AuthorizationContext context, TParameters parameters)
        {
        }

        public virtual Task OnAuthorizationAsync(AuthorizationContext context, TParameters parameters)
        {
            return Task.CompletedTask;
        }

        public virtual void OnActionExecuting(ActionExecutingContext context, TParameters parameters)
        {
        }

        public virtual void OnActionExecuted(ActionExecutedContext context, TParameters parameters)
        {
        }

        public virtual Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next,
            TParameters parameters)
        {
            return next();
        }

        public virtual void OnResultExecuting(ResultExecutingContext context, TParameters parameters)
        {
        }

        public virtual void OnResultExecuted(ResultExecutedContext context, TParameters parameters)
        {
        }

        public virtual Task OnResultExecutionAsync(
            ResultExecutingContext context,
            ResultExecutionDelegate next,
            TParameters parameters)
        {
            return next();
        }
    }
}