// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    public interface IParametrizedFilter<in TParameters> : IFilter
    {
        void OnException(ExceptionContext context, TParameters parameters);

        Task OnExceptionAsync(ExceptionContext context, TParameters parameters);

        void OnAuthorization(AuthorizationContext context, TParameters parameters);

        Task OnAuthorizationAsync(AuthorizationContext context, TParameters parameters);

        void OnActionExecuting(ActionExecutingContext context, TParameters parameters);

        void OnActionExecuted(ActionExecutedContext context, TParameters parameters);

        Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next, TParameters parameters);

        void OnResultExecuting(ResultExecutingContext context, TParameters parameters);

        void OnResultExecuted(ResultExecutedContext context, TParameters parameters);

        Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next, TParameters parameters);
    }
}