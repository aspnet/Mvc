// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public abstract class FilterActionInvoker : IActionInvoker
    {
        private readonly ControllerActionInvokerCache _controllerActionInvokerCache;
        private readonly IReadOnlyList<IValueProviderFactory> _valueProviderFactories;
        private readonly DiagnosticSource _diagnosticSource;
        private readonly int _maxModelValidationErrors;

        private IFilterMetadata[] _filters;
        private ObjectMethodExecutor _controllerActionMethodExecutor;
        private FilterCursor _cursor;

        private AuthorizationFilterContext _authorizationContext;

        private ResourceExecutingContext _resourceExecutingContext;
        private ResourceExecutedContext _resourceExecutedContext;

        private ExceptionContext _exceptionContext;

        private ActionExecutingContext _actionExecutingContext;
        private ActionExecutedContext _actionExecutedContext;

        private ResultExecutingContext _resultExecutingContext;
        private ResultExecutedContext _resultExecutedContext;

        private IDictionary<string, object> _arguments;
        private IActionResult _result;

        public FilterActionInvoker(
            ActionContext actionContext,
            ControllerActionInvokerCache controllerActionInvokerCache,
            IReadOnlyList<IValueProviderFactory> valueProviderFactories,
            ILogger logger,
            DiagnosticSource diagnosticSource,
            int maxModelValidationErrors)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            if (controllerActionInvokerCache == null)
            {
                throw new ArgumentNullException(nameof(controllerActionInvokerCache));
            }

            if (valueProviderFactories == null)
            {
                throw new ArgumentNullException(nameof(valueProviderFactories));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (diagnosticSource == null)
            {
                throw new ArgumentNullException(nameof(diagnosticSource));
            }


            _controllerActionInvokerCache = controllerActionInvokerCache;
            _valueProviderFactories = valueProviderFactories;
            Logger = logger;
            _diagnosticSource = diagnosticSource;
            _maxModelValidationErrors = maxModelValidationErrors;

            Context = new ControllerContext(actionContext);
            Context.ModelState.MaxAllowedErrors = _maxModelValidationErrors;

                
            var factories = new List<IValueProviderFactory>(_valueProviderFactories.Count);
            for (var i = 0; i < _valueProviderFactories.Count; i++)
            {
                factories.Add(_valueProviderFactories[i]);
            }

            Context.ValueProviderFactories = factories;
        }

        protected ControllerContext Context { get; }

        protected object Instance { get; private set; }

        protected ILogger Logger { get; }

        /// <summary>
        /// Called to create an instance of an object which will act as the reciever of the action invocation.
        /// </summary>
        /// <returns>The constructed instance or <c>null</c>.</returns>
        protected abstract object CreateInstance();

        /// <summary>
        /// Called to create an instance of an object which will act as the reciever of the action invocation.
        /// </summary>
        /// <param name="instance">The instance to release.</param>
        /// <remarks>This method will not be called if <see cref="CreateInstance"/> returns <c>null</c>.</remarks>
        protected abstract void ReleaseInstance(object instance);

        protected abstract Task<IActionResult> InvokeActionAsync(IDictionary<string, object> arguments);

        protected abstract Task BindActionArgumentsAsync(IDictionary<string, object> arguments);

        public virtual async Task InvokeAsync()
        {
            var controllerActionInvokerState = _controllerActionInvokerCache.GetState(Context);
            _filters = controllerActionInvokerState.Filters;
            _controllerActionMethodExecutor = controllerActionInvokerState.ActionMethodExecutor;
            _cursor = new FilterCursor(_filters);

            await InvokeNextAuthorizationFilterAsync();

            // If Authorization Filters return a result, it's a short circuit because
            // authorization failed. We don't execute Result Filters around the result.
            if (_authorizationContext?.Result != null)
            {
                var result = _authorizationContext.Result;
                _diagnosticSource.BeforeActionResult(Context, result);

                try
                {
                    await result.ExecuteResultAsync(Context);
                }
                finally
                {
                    _diagnosticSource.AfterActionResult(Context, result);
                }


                return;
            }

            try
            {
                _cursor.Reset();
                await InvokeNextResourceFilterAsync();

                if (_resourceExecutedContext != null)
                {
                    // We've reached the end of resource filters. If there's an unhandled exception on the context then
                    // it should be thrown and middleware has a chance to handle it.
                    if (_resourceExecutedContext.Exception != null && !_resourceExecutedContext.ExceptionHandled)
                    {
                        _resourceExecutedContext.ExceptionDispatchInfo?.Throw();
                        throw _resourceExecutedContext.Exception;
                    }

                    // If we ran resource filters then we've executed the action and action result so we're all done.
                    return;
                }
                else
                {
                    // If resource filters collapsed, then we need to run exception filters here.
                    //
                    // >> ExceptionFilters >> Model Binding >> ActionFilters >> Action
                    _cursor.Reset();
                    await InvokeNextExceptionFilterAsync();
                }

                if (_exceptionContext != null)
                {
                    // If we get here then exception filters didn't collapse, they will have called the action
                    // filters and the action we need to handle any unhandled exceptions or short-circuits.

                    // If Exception Filters provide a result, it's a short-circuit due to an exception.
                    // We don't execute Result Filters around the result.
                    if (_exceptionContext.Result != null)
                    {
                        // This means that exception filters returned a result to 'handle' an error.
                        // We're not interested in seeing the exception details since it was handled.
                        // We also don't run result filters when this happens.
                        var result = _exceptionContext.Result;

                        _diagnosticSource.BeforeActionResult(Context, result);

                        try
                        {
                            await result.ExecuteResultAsync(Context);
                        }
                        finally
                        {
                            _diagnosticSource.AfterActionResult(Context, result);
                        }

                        return;
                    }
                    else if (_exceptionContext.Exception != null)
                    {
                        // If we get here, this means that we have an unhandled exception.
                        //
                        // Preserve the stack trace if possible.
                        _exceptionContext.ExceptionDispatchInfo?.Throw();
                        throw _exceptionContext.Exception;
                    }

                    // Running exception filters runs the action filters and action action, but not the action result, 
                    // so continue for now.
                    if (_actionExecutedContext != null)
                    {
                        _result = _actionExecutedContext.Result;
                    }
                }
                else
                {
                    // If exception filters collapse, we need to run the action filters from here.
                    Instance = CreateInstance();

                    _arguments = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    await BindActionArgumentsAsync(_arguments);

                    _cursor.Reset();
                    await InvokeNextActionFilterAsync();

                    if (_actionExecutedContext != null)
                    {
                        if (_actionExecutedContext.Exception != null &&
                            !_actionExecutedContext.ExceptionHandled)
                        {
                            // If we get here, this means that we have an unhandled exception.
                            //
                            // Preserve the stack trace if possible.
                            _actionExecutedContext.ExceptionDispatchInfo?.Throw();
                            throw _actionExecutedContext.Exception;
                        }

                        // Action filters ran, but not the result or result filters. Propegate the result
                        // so that it will run.
                        _result = _actionExecutedContext.Result;
                    }
                    else
                    {
                        // If action filters collapse, then run the action directly.
                        try
                        {
                            _diagnosticSource.BeforeActionMethod(Context, _arguments, Instance);

                            _result = await InvokeActionAsync(_arguments);
                        }
                        finally
                        {
                            _diagnosticSource.AfterActionMethod(Context, _arguments, Instance, _result);
                        }
                    }
                }

                // >> ResultFilters >> (Result)
                _cursor.Reset();
                await InvokeNextResultFilterAsync();

                if (_resultExecutedContext != null)
                {
                    // Result filters ran, so we have nothing to do unless there was an unhandled exception.
                    if (_resultExecutedContext?.Exception != null &&
                        !_resultExecutedContext.ExceptionHandled)
                    {
                        // If we get here, this means that we have an unhandled exception.
                        //
                        // Preserve the stack trace if possible.
                        _resultExecutedContext.ExceptionDispatchInfo?.Throw();
                        throw _resultExecutedContext.Exception;
                    }
                }
                else
                {
                    // If we get here, then result filters collapsed, so run the result directly.
                    //
                    // Treat the null result as 'empty result' for diagnostics if there wasn't one.
                    var result = _result ?? new EmptyResult();

                    _diagnosticSource.BeforeActionResult(Context, result);

                    try
                    {
                        await result.ExecuteResultAsync(Context);
                    }
                    finally
                    {
                        _diagnosticSource.AfterActionResult(Context, result);
                    }
                }
            }
            finally
            {
                // Release the instance after all filters have run. We don't need to surround
                // Authorizations filters because the instance will be created much later than
                // that.
                if (Instance != null)
                {
                    ReleaseInstance(Instance);
                }
            }
        }

        protected ObjectMethodExecutor GetControllerActionMethodExecutor()
        {
            return _controllerActionMethodExecutor;
        }

        private Task InvokeNextAuthorizationFilterAsync()
        {
            // We should never get here if we already have a result.
            Debug.Assert(_authorizationContext?.Result == null);

            var current = _cursor.GetNextFilter<IAuthorizationFilter, IAsyncAuthorizationFilter>();
            if (current.FilterAsync != null)
            {
                _authorizationContext = _authorizationContext ?? new AuthorizationFilterContext(Context, _filters);
                return InvokeAsyncAuthorizationFilterAsync(current.FilterAsync);
            }
            else if (current.Filter != null)
            {
                _authorizationContext = _authorizationContext ?? new AuthorizationFilterContext(Context, _filters);
                return InvokeSyncAuthorizationFilterAsync(current.Filter);
            }
            else
            {
                // We have no authorization filters so we can 'collapse'.
                return TaskCache.CompletedTask;
            }
        }

        private async Task InvokeSyncAuthorizationFilterAsync(IAuthorizationFilter filter)
        {
            _diagnosticSource.BeforeOnAuthorization(_authorizationContext, filter);
            filter.OnAuthorization(_authorizationContext);
            _diagnosticSource.AfterOnAuthorization(_authorizationContext, filter);

            if (_authorizationContext.Result == null)
            {
                // Only keep going if we don't have a result
                await InvokeNextAuthorizationFilterAsync();
            }
            else
            {
                Logger.AuthorizationFailure(filter);
            }

        }

        private async Task InvokeAsyncAuthorizationFilterAsync(IAsyncAuthorizationFilter filter)
        {
            _diagnosticSource.BeforeOnAuthorizationAsync(_authorizationContext, filter);
            await filter.OnAuthorizationAsync(_authorizationContext);
            _diagnosticSource.AfterOnAuthorizationAsync(_authorizationContext, filter);

            if (_authorizationContext.Result == null)
            {
                // Only keep going if we don't have a result
                await InvokeNextAuthorizationFilterAsync();
            }
            else
            {
                Logger.AuthorizationFailure(filter);
            }
        }

        private Task InvokeNextResourceFilterAsync()
        {
            var current = _cursor.GetNextFilter<IResourceFilter, IAsyncResourceFilter>();
            if (current.FilterAsync != null)
            {
                _resourceExecutingContext = _resourceExecutingContext ?? new ResourceExecutingContext(Context, _filters);
                return InvokeAsyncResourceFilterAsync(current.FilterAsync);
            }
            else if (current.Filter != null)
            {
                _resourceExecutingContext = _resourceExecutingContext ?? new ResourceExecutingContext(Context, _filters);
                return InvokeSyncResourceFilterAsync(current.Filter);
            }
            else if (_resourceExecutingContext != null)
            {
                // We've found some resource filters, we can't 'collapse', we need to execute the exception
                // filters but interpret the results for a resource filter to see.
                return InvokeExceptionFiltersInResourceFilter();
            }
            else
            {
                // We have no resource filters so we can 'collapse'. We count on the caller to call
                // into the rest of the filter pipeline.
                return TaskCache.CompletedTask;
            }
        }

        private async Task<ResourceExecutedContext> InvokeNextResourceFilterAwaitedAsync()
        {
            if (_resourceExecutingContext.Result != null)
            {
                // If we get here, it means that an async filter set a result AND called next(). This is forbidden.
                var message = Resources.FormatAsyncResourceFilter_InvalidShortCircuit(
                    typeof(IAsyncResourceFilter).Name,
                    nameof(ResourceExecutingContext.Result),
                    typeof(ResourceExecutingContext).Name,
                    typeof(ResourceExecutionDelegate).Name);

                throw new InvalidOperationException(message);
            }

            await InvokeNextResourceFilterAsync();
            return _resourceExecutedContext;
        }

        private async Task InvokeSyncResourceFilterAsync(IResourceFilter filter)
        {
            Debug.Assert(_resourceExecutingContext != null);

            try
            {
                _diagnosticSource.BeforeOnResourceExecuting(_resourceExecutingContext, filter);

                filter.OnResourceExecuting(_resourceExecutingContext);

                _diagnosticSource.AfterOnResourceExecuting(_resourceExecutingContext, filter);

                if (_resourceExecutingContext.Result != null)
                {
                    // Short-circuited by setting a result.
                    Logger.ResourceFilterShortCircuited(filter);

                    var result = _resourceExecutingContext.Result;

                    _diagnosticSource.BeforeActionResult(Context, result);

                    try
                    {
                        await result.ExecuteResultAsync(Context);
                    }
                    finally
                    {
                        _diagnosticSource.AfterActionResult(Context, result);
                    }

                    _resourceExecutedContext = new ResourceExecutedContext(_resourceExecutingContext, _filters)
                    {
                        Canceled = true,
                        Result = result,
                    };
                }
                else
                {
                    await InvokeNextResourceFilterAsync();

                    _diagnosticSource.BeforeOnResourceExecuted(_resourceExecutedContext, filter);

                    filter.OnResourceExecuted(_resourceExecutedContext);

                    _diagnosticSource.AfterOnResourceExecuted(_resourceExecutedContext, filter);
                }
            }
            catch (Exception exception)
            {
                _resourceExecutedContext = new ResourceExecutedContext(_resourceExecutingContext, _filters)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception)
                };
            }

            Debug.Assert(_resourceExecutedContext != null);
        }

        private async Task InvokeAsyncResourceFilterAsync(IAsyncResourceFilter filter)
        {
            Debug.Assert(_resourceExecutingContext != null);

            try
            {
                _diagnosticSource.BeforeOnResourceExecution(_resourceExecutingContext, filter);

                await filter.OnResourceExecutionAsync(_resourceExecutingContext, InvokeNextResourceFilterAwaitedAsync);

                _diagnosticSource.AfterOnResourceExecution(_resourceExecutedContext, filter);

                if (_resourceExecutedContext == null)
                {
                    // If we get here then the filter didn't call 'next' indicating a short circuit
                    if (_resourceExecutingContext.Result != null)
                    {
                        Logger.ResourceFilterShortCircuited(filter);

                        var result = _resourceExecutingContext.Result;

                        _diagnosticSource.BeforeActionResult(Context, result);

                        try
                        {
                            await result.ExecuteResultAsync(Context);
                        }
                        finally
                        {
                            _diagnosticSource.AfterActionResult(Context, result);
                        }
                    }

                    _resourceExecutedContext = new ResourceExecutedContext(_resourceExecutingContext, _filters)
                    {
                        Canceled = true,
                        Result = _resourceExecutingContext.Result,
                    };
                }
            }
            catch (Exception exception)
            {
                _resourceExecutedContext = new ResourceExecutedContext(_resourceExecutingContext, _filters)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception)
                };
            }

            Debug.Assert(_resourceExecutedContext != null);
        }

        private async Task InvokeExceptionFiltersInResourceFilter()
        {
            try
            {
                // >> ExceptionFilters >> Model Binding >> ActionFilters >> Action
                _cursor.Reset();
                await InvokeNextExceptionFilterAsync();

                if (_exceptionContext != null)
                {
                    // If we get here then exception filters didn't collapse, they will have called the action
                    // filters and the action we need to handle any unhandled exceptions or short-circuits.

                    // If Exception Filters provide a result, it's a short-circuit due to an exception.
                    // We don't execute Result Filters around the result.
                    if (_exceptionContext.Result != null)
                    {
                        // This means that exception filters returned a result to 'handle' an error.
                        // We're not interested in seeing the exception details since it was handled.
                        // We also don't run result filters when this happens.
                        var result = _exceptionContext.Result;

                        _diagnosticSource.BeforeActionResult(Context, result);

                        try
                        {
                            await result.ExecuteResultAsync(Context);
                        }
                        finally
                        {
                            _diagnosticSource.AfterActionResult(Context, result);
                        }

                        _resourceExecutedContext = new ResourceExecutedContext(_resourceExecutingContext, _filters)
                        {
                            Result = result,
                        };

                        return;
                    }
                    else if (_exceptionContext.Exception != null)
                    {
                        // If we get here, this means that we have an unhandled exception.
                        // Exception filters didn't handle this, so send it on to resource filters.
                        _resourceExecutedContext = new ResourceExecutedContext(_resourceExecutingContext, _filters);

                        // Preserve the stack trace if possible.
                        _resourceExecutedContext.Exception = _exceptionContext.Exception;
                        if (_exceptionContext.ExceptionDispatchInfo != null)
                        {
                            _resourceExecutedContext.ExceptionDispatchInfo = _exceptionContext.ExceptionDispatchInfo;
                        }

                        return;
                    }

                    // Running exception filters runs the action filters and action action, but not the action result, 
                    // so continue for now.
                    if (_actionExecutedContext != null)
                    {
                        _result = _actionExecutedContext.Result;
                    }
                }
                else
                {
                    // If exception filters collapse, we need to run the action filters from here.
                    Instance = CreateInstance();

                    _arguments = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    await BindActionArgumentsAsync(_arguments);

                    _cursor.Reset();
                    await InvokeNextActionFilterAsync();

                    if (_actionExecutedContext != null)
                    {
                        if (_actionExecutedContext.Exception != null &&
                            !_actionExecutedContext.ExceptionHandled)
                        {
                            // If running action filters directly resulted in an unhandled exception, propagate it.
                            _resourceExecutedContext = new ResourceExecutedContext(_resourceExecutingContext, _filters);

                            // Preserve the stack trace if possible.
                            _resourceExecutedContext.Exception = _actionExecutedContext.Exception;
                            if (_actionExecutedContext.ExceptionDispatchInfo != null)
                            {
                                _resourceExecutedContext.ExceptionDispatchInfo = _actionExecutedContext.ExceptionDispatchInfo;
                            }

                            return;
                        }

                        // Action filters ran, but not the result or result filters. Propegate the result
                        // so that it will run.
                        _result = _actionExecutedContext.Result;
                    }
                    else
                    {
                        // If action filters collapse, then run the action directly.
                        try
                        {
                            _diagnosticSource.BeforeActionMethod(Context, _arguments, Instance);

                            _result = await InvokeActionAsync(_arguments);
                        }
                        finally
                        {
                            _diagnosticSource.AfterActionMethod(Context, _arguments, Instance, _result);
                        }
                    }
                }

                // We have a successful 'result' from the action or an Action Filter, so run
                // Result Filters.
                //
                // >> ResultFilters >> (Result)
                _cursor.Reset();
                await InvokeNextResultFilterAsync();

                if (_resultExecutedContext != null)
                {
                    // Result filters ran, so we have nothing to do unless there was an unhandled exception.
                    if (_resultExecutedContext?.Exception != null &&
                        !_resultExecutedContext.ExceptionHandled)
                    {
                        // If we get here, this means that we have an unhandled exception.
                        //
                        // Preserve the stack trace if possible.
                        _resultExecutedContext.ExceptionDispatchInfo?.Throw();
                        throw _resultExecutedContext.Exception;
                    }

                    _resourceExecutedContext = new ResourceExecutedContext(_resourceExecutingContext, _filters)
                    {
                        Result = _resultExecutedContext?.Result ?? _result ?? new EmptyResult(),
                    };
                }
                else
                {
                    // If we get here, then result filters collapsed, so run the result directly.
                    //
                    // Treat the null result as 'empty result' for diagnostics if there wasn't one.
                    var result = _result ?? new EmptyResult();

                    _diagnosticSource.BeforeActionResult(Context, result);

                    try
                    {
                        await result.ExecuteResultAsync(Context);
                    }
                    finally
                    {
                        _diagnosticSource.AfterActionResult(Context, result);
                    }

                    _resourceExecutedContext = new ResourceExecutedContext(_resourceExecutingContext, _filters)
                    {
                        Result = result,
                    };
                }
            }
            catch (Exception exception)
            {
                _resourceExecutedContext = new ResourceExecutedContext(_resourceExecutingContext, _filters)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception)
                };
            }
        }

        private Task InvokeNextExceptionFilterAsync()
        {
            var current = _cursor.GetNextFilter<IExceptionFilter, IAsyncExceptionFilter>();
            if (current.FilterAsync != null)
            {
                _exceptionContext = _exceptionContext ?? new ExceptionContext(Context, _filters);
                return InvokeAsyncExceptionFilterAsync(current.FilterAsync);
            }
            else if (current.Filter != null)
            {
                _exceptionContext = _exceptionContext ?? new ExceptionContext(Context, _filters);
                return InvokeSyncExceptionFilterAsync(current.Filter);
            }
            else if (_exceptionContext != null)
            {
                return InvokeActionFiltersInExceptionFilter();
            }
            else
            {
                // We have no exception filters so we can 'collapse'. We count on the caller to call
                // into the rest of the filter pipeline.
                return TaskCache.CompletedTask;
            }
        }

        private async Task InvokeSyncExceptionFilterAsync(IExceptionFilter filter)
        {
            Debug.Assert(_exceptionContext != null);

            // Exception filters run "on the way out" - so the filter is run after the rest of the
            // pipeline.
            await InvokeNextExceptionFilterAsync();

            if (_exceptionContext.Exception != null)
            {
                _diagnosticSource.BeforeOnException(_exceptionContext, filter);

                // Exception filters only run when there's an exception - unsetting it will short-circuit
                // other exception filters.
                filter.OnException(_exceptionContext);

                _diagnosticSource.AfterOnException(_exceptionContext, filter);

                if (_exceptionContext.Exception == null)
                {
                    Logger.ExceptionFilterShortCircuited(filter);
                }
            }
        }

        private async Task InvokeAsyncExceptionFilterAsync(IAsyncExceptionFilter filter)
        {
            Debug.Assert(_exceptionContext != null);

            // Exception filters run "on the way out" - so the filter is run after the rest of the
            // pipeline.
            await InvokeNextExceptionFilterAsync();

            if (_exceptionContext.Exception != null)
            {
                _diagnosticSource.BeforeOnExceptionAsync(_exceptionContext, filter);

                // Exception filters only run when there's an exception - unsetting it will short-circuit
                // other exception filters.
                await filter.OnExceptionAsync(_exceptionContext);

                _diagnosticSource.AfterOnExceptionAsync(_exceptionContext, filter);

                if (_exceptionContext.Exception == null)
                {
                    Logger.ExceptionFilterShortCircuited(filter);
                }
            }
        }

        private async Task InvokeActionFiltersInExceptionFilter()
        {
            Debug.Assert(_exceptionContext != null);

            // We've reached the 'end' of the exception filter pipeline - this means that one stack frame has
            // been built for each exception. When we return from here, these frames will either:
            //
            // 1) Call the filter (if we have an exception)
            // 2) No-op (if we don't have an exception)

            try
            {
                Instance = CreateInstance();

                _arguments = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                await BindActionArgumentsAsync(_arguments);

                _cursor.Reset();
                await InvokeNextActionFilterAsync();

                if (_actionExecutedContext != null)
                {
                    // If we get here then executing action filters also executed the action, so we don't need
                    // to do anything else other deal with unhandled exceptions.

                    if (_actionExecutedContext?.Exception != null && !_actionExecutedContext.ExceptionHandled)
                    {
                        // Action filters might 'return' an unhandled exception instead of throwing
                        _exceptionContext.Exception = _actionExecutedContext.Exception;
                        if (_actionExecutedContext.ExceptionDispatchInfo != null)
                        {
                            _exceptionContext.ExceptionDispatchInfo = _actionExecutedContext.ExceptionDispatchInfo;
                        }
                    }

                    // Action filters ran, but not the result or result filters. Propegate the result
                    // so that it will run.
                    _result = _actionExecutedContext.Result;

                    return;
                }

                // If we get here this means that action filters were 'skipped' so we need to call the action directly.
                try
                {
                    _diagnosticSource.BeforeActionMethod(Context, _arguments, Instance);

                    _result = await InvokeActionAsync(_arguments);
                }
                finally
                {
                    _diagnosticSource.AfterActionMethod(Context, _arguments, Instance, _result);
                }
            }
            catch (Exception exception)
            {
                _exceptionContext.ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
            }
        }

        private Task InvokeNextActionFilterAsync()
        {
            Debug.Assert(Instance != null);
            Debug.Assert(_arguments != null);

            var current = _cursor.GetNextFilter<IActionFilter, IAsyncActionFilter>();
            if (current.FilterAsync != null)
            {
                _actionExecutingContext = _actionExecutingContext ?? new ActionExecutingContext(Context, _filters, _arguments, Instance);
                return InvokeAsyncActionFilterAsync(current.FilterAsync);
            }
            else if (current.Filter != null)
            {
                _actionExecutingContext = _actionExecutingContext ?? new ActionExecutingContext(Context, _filters, _arguments, Instance);
                return InvokeSyncActionFilterAsync(current.Filter);
            }
            else if (_actionExecutingContext != null)
            {
                return InvokeActionInActionFilterAsync();
            }
            else
            {
                // We have no exception filters so we can 'collapse'. We count on the caller to call
                // into the action method.
                return TaskCache.CompletedTask;
            }
        }

        private async Task<ActionExecutedContext> InvokeNextActionFilterAwaitedAsync()
        {
            Debug.Assert(_actionExecutingContext != null);
            if (_actionExecutingContext.Result != null)
            {
                // If we get here, it means that an async filter set a result AND called next(). This is forbidden.
                var message = Resources.FormatAsyncActionFilter_InvalidShortCircuit(
                    typeof(IAsyncActionFilter).Name,
                    nameof(ActionExecutingContext.Result),
                    typeof(ActionExecutingContext).Name,
                    typeof(ActionExecutionDelegate).Name);

                throw new InvalidOperationException(message);
            }

            await InvokeNextActionFilterAsync();
            return _actionExecutedContext;
        }

        private async Task InvokeSyncActionFilterAsync(IActionFilter filter)
        {
            Debug.Assert(_actionExecutingContext != null);

            try
            {
                _diagnosticSource.BeforeOnActionExecuting(_actionExecutingContext, filter);

                filter.OnActionExecuting(_actionExecutingContext);

                _diagnosticSource.AfterOnActionExecuting(_actionExecutingContext, filter);

                if (_actionExecutingContext.Result != null)
                {
                    // Short-circuited by setting a result.
                    Logger.ActionFilterShortCircuited(filter);

                    _actionExecutedContext = new ActionExecutedContext(_actionExecutingContext, _filters, Instance)
                    {
                        Canceled = true,
                        Result = _actionExecutingContext.Result,
                    };
                }
                else
                {
                    await InvokeNextActionFilterAsync();

                    _diagnosticSource.BeforeOnActionExecuted(_actionExecutedContext, filter);

                    filter.OnActionExecuted(_actionExecutedContext);

                    _diagnosticSource.BeforeOnActionExecuted(_actionExecutedContext, filter);
                }
            }
            catch (Exception exception)
            {
                // Exceptions thrown by the action method OR filters bubble back up through ActionExcecutedContext.
                _actionExecutedContext = new ActionExecutedContext(_actionExecutingContext, _filters, Instance)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception)
                };
            }
        }

        private async Task InvokeAsyncActionFilterAsync(IAsyncActionFilter filter)
        {
            Debug.Assert(_actionExecutingContext != null);

            try
            {
                _diagnosticSource.BeforeOnActionExecution(_actionExecutingContext, filter);

                await filter.OnActionExecutionAsync(_actionExecutingContext, InvokeNextActionFilterAwaitedAsync);

                _diagnosticSource.AfterOnActionExecution(_actionExecutedContext, filter);

                if (_actionExecutedContext == null)
                {
                    // If we get here then the filter didn't call 'next' indicating a short circuit
                    Logger.ActionFilterShortCircuited(filter);

                    _actionExecutedContext = new ActionExecutedContext(_actionExecutingContext, _filters, Instance)
                    {
                        Canceled = true,
                        Result = _actionExecutingContext.Result,
                    };
                }
            }
            catch (Exception exception)
            {
                // Exceptions thrown by the action method OR filters bubble back up through ActionExcecutedContext.
                _actionExecutedContext = new ActionExecutedContext(_actionExecutingContext, _filters, Instance)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception)
                };
            }
        }

        private async Task InvokeActionInActionFilterAsync()
        {
            Debug.Assert(_actionExecutingContext != null);

            try
            {
                // All action filters have run, execute the action method.
                IActionResult result = null;

                try
                {
                    _diagnosticSource.BeforeActionMethod(Context, _arguments, Instance);

                    result = await InvokeActionAsync(_actionExecutingContext.ActionArguments);
                }
                finally
                {
                    _diagnosticSource.AfterActionMethod(Context, _arguments, Instance, result);
                }

                _actionExecutedContext = new ActionExecutedContext(_actionExecutingContext, _filters, Instance)
                {
                    Result = result
                };
            }
            catch (Exception exception)
            {
                // Exceptions thrown by the action method OR filters bubble back up through ActionExcecutedContext.
                _actionExecutedContext = new ActionExecutedContext(_actionExecutingContext, _filters, Instance)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception)
                };
            }
        }

        private Task InvokeNextResultFilterAsync()
        {
            var current = _cursor.GetNextFilter<IResultFilter, IAsyncResultFilter>();
            if (current.FilterAsync != null)
            {
                _resultExecutingContext = _resultExecutingContext ?? new ResultExecutingContext(Context, _filters, _result, Instance);
                return InvokeAsyncResultFilterAsync(current.FilterAsync);
            }
            else if (current.Filter != null)
            {
                _resultExecutingContext = _resultExecutingContext ?? new ResultExecutingContext(Context, _filters, _result, Instance);
                return InvokeSyncResultFilterAsync(current.Filter);
            }
            else if (_resultExecutingContext != null)
            {
                // The empty result is always flowed back as the 'executed' result
                if (_resultExecutingContext.Result == null)
                {
                    _resultExecutingContext.Result = new EmptyResult();
                }

                return InvokeResultInFilterAsync(_resultExecutingContext.Result);
            }
            else
            {
                // We have no exception filters so we can 'collapse'. We count on the caller to call
                // into the action result.
                return TaskCache.CompletedTask;
            }
        }

        private async Task<ResultExecutedContext> InvokeNextResultFilterAwaitedAsync()
        {
            Debug.Assert(_resultExecutingContext != null);
            if (_resultExecutingContext.Cancel == true)
            {
                // If we get here, it means that an async filter set cancel == true AND called next().
                // This is forbidden.
                var message = Resources.FormatAsyncResultFilter_InvalidShortCircuit(
                    typeof(IAsyncResultFilter).Name,
                    nameof(ResultExecutingContext.Cancel),
                    typeof(ResultExecutingContext).Name,
                    typeof(ResultExecutionDelegate).Name);

                throw new InvalidOperationException(message);
            }

            await InvokeNextResultFilterAsync();

            Debug.Assert(_resultExecutedContext != null);
            return _resultExecutedContext;
        }

        private async Task InvokeSyncResultFilterAsync(IResultFilter filter)
        {
            Debug.Assert(_resultExecutingContext != null);

            try
            {
                _diagnosticSource.BeforeOnResultExecuting(_resultExecutingContext, filter);

                filter.OnResultExecuting(_resultExecutingContext);

                _diagnosticSource.AfterOnResultExecuting(_resultExecutingContext, filter);

                if (_resultExecutingContext.Cancel == true)
                {
                    // Short-circuited by setting Cancel == true
                    Logger.ResourceFilterShortCircuited(filter);

                    _resultExecutedContext = new ResultExecutedContext(
                        _resultExecutingContext,
                        _filters,
                        _resultExecutingContext.Result,
                        Instance)
                    {
                        Canceled = true,
                    };
                }
                else
                {
                    await InvokeNextResultFilterAsync();

                    _diagnosticSource.BeforeOnResultExecuted(_resultExecutedContext, filter);

                    filter.OnResultExecuted(_resultExecutedContext);

                    _diagnosticSource.AfterOnResultExecuted(_resultExecutedContext, filter);
                }
            }
            catch (Exception exception)
            {
                _resultExecutedContext = new ResultExecutedContext(
                    _resultExecutingContext,
                    _filters,
                    _resultExecutingContext.Result,
                    Instance)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception)
                };
            }
        }

        private async Task InvokeAsyncResultFilterAsync(IAsyncResultFilter filter)
        {
            Debug.Assert(_resultExecutingContext != null);

            try
            {
                _diagnosticSource.BeforeOnResultExecution(_resultExecutingContext, filter);

                await filter.OnResultExecutionAsync(_resultExecutingContext, InvokeNextResultFilterAwaitedAsync);

                _diagnosticSource.AfterOnResultExecution(_resultExecutedContext, filter);

                if (_resultExecutedContext == null || _resultExecutingContext.Cancel == true)
                {
                    // Short-circuited by not calling next || Short-circuited by setting Cancel == true
                    Logger.ResourceFilterShortCircuited(filter);

                    _resultExecutedContext = new ResultExecutedContext(
                        _resultExecutingContext,
                        _filters,
                        _resultExecutingContext.Result,
                        Instance)
                    {
                        Canceled = true,
                    };
                }
            }
            catch (Exception exception)
            {
                _resultExecutedContext = new ResultExecutedContext(
                    _resultExecutingContext,
                    _filters,
                    _resultExecutingContext.Result,
                    Instance)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception)
                };
            }
        }

        private async Task InvokeResultInFilterAsync(IActionResult result)
        {
            // Should only be invoked if we had a result filter
            Debug.Assert(_resultExecutingContext != null);

            try
            {
                _diagnosticSource.BeforeActionResult(Context, result);

                try
                {
                    await result.ExecuteResultAsync(Context);
                }
                finally
                {
                    _diagnosticSource.AfterActionResult(Context, result);
                }

                Debug.Assert(_resultExecutedContext == null);
                _resultExecutedContext = new ResultExecutedContext(
                    _resultExecutingContext,
                    _filters,
                    _resultExecutingContext.Result,
                    Instance);
            }
            catch (Exception exception)
            {
                _resultExecutedContext = new ResultExecutedContext(
                   _resultExecutingContext,
                   _filters,
                   _resultExecutingContext.Result,
                   Instance)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception)
                };
            }
        }

        /// <summary>
        /// A one-way cursor for filters.
        /// </summary>
        /// <remarks>
        /// This will iterate the filter collection once per-stage, and skip any filters that don't have
        /// the one of interfaces that applies to the current stage.
        ///
        /// Filters are always executed in the following order, but short circuiting plays a role.
        ///
        /// Indentation reflects nesting.
        ///
        /// 1. Exception Filters
        ///     2. Authorization Filters
        ///     3. Action Filters
        ///        Action
        ///
        /// 4. Result Filters
        ///    Result
        ///
        /// </remarks>
        private struct FilterCursor
        {
            private int _index;
            private readonly IFilterMetadata[] _filters;

            public FilterCursor(int index, IFilterMetadata[] filters)
            {
                _index = index;
                _filters = filters;
            }

            public FilterCursor(IFilterMetadata[] filters)
            {
                _index = 0;
                _filters = filters;
            }

            public void Reset()
            {
                _index = 0;
            }

            public FilterCursorItem<TFilter, TFilterAsync> GetNextFilter<TFilter, TFilterAsync>()
                where TFilter : class
                where TFilterAsync : class
            {
                // Perf: Be really careful with changes here - this method is SUPER hot. We're very careful
                // here to avoid repeated access of _index, and do things in the order that's most likely
                // to no-op.

                var index = _index;
                var length = _filters.Length;

                while (index < length)
                {
                    var filter = _filters[index++];

                    var filterAsync = filter as TFilterAsync;
                    TFilter filterSync = null;

                    if (filterAsync != null || (filterSync = filter as TFilter) != null)
                    {
                        _index = index;
                        return new FilterCursorItem<TFilter, TFilterAsync>(filterSync, filterAsync);
                    }
                }

                _index = index;
                return default(FilterCursorItem<TFilter, TFilterAsync>);
            }
        }

        private struct FilterCursorItem<TFilter, TFilterAsync>
        {
            public readonly TFilter Filter;
            public readonly TFilterAsync FilterAsync;

            public FilterCursorItem(TFilter filter, TFilterAsync filterAsync)
            {
                Filter = filter;
                FilterAsync = filterAsync;
            }
        }
    }
}
