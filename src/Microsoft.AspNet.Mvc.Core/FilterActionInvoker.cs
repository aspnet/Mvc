// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using Microsoft.Framework.Internal;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Notification;

namespace Microsoft.AspNet.Mvc.Core
{
    public abstract class FilterActionInvoker : IActionInvoker
    {
        private readonly IReadOnlyList<IFilterProvider> _filterProviders;
        private readonly IReadOnlyList<IInputFormatter> _inputFormatters;
        private readonly IReadOnlyList<IModelBinder> _modelBinders;
        private readonly IReadOnlyList<IOutputFormatter> _outputFormatters;
        private readonly IReadOnlyList<IModelValidatorProvider> _modelValidatorProviders;
        private readonly IReadOnlyList<IValueProviderFactory> _valueProviderFactories;
        private readonly IActionBindingContextAccessor _actionBindingContextAccessor;
        private readonly ILogger _logger;
        private readonly INotifier _notifier;
        private readonly int _maxModelValidationErrors;

        private IFilterMetadata[] _filters;
        private FilterCursor _cursor;

        private AuthorizationContext _authorizationContext;

        private ResourceExecutingContext _resourceExecutingContext;
        private ResourceExecutedContext _resourceExecutedContext;

        private ExceptionContext _exceptionContext;

        private ActionExecutingContext _actionExecutingContext;
        private ActionExecutedContext _actionExecutedContext;

        private ResultExecutingContext _resultExecutingContext;
        private ResultExecutedContext _resultExecutedContext;

        private const string AuthorizationFailureLogMessage =
            "Authorization failed for the request at filter '{AuthorizationFilter}'.";
        private const string ResourceFilterShortCircuitLogMessage =
            "Request was short circuited at resource filter '{ResourceFilter}'.";
        private const string ActionFilterShortCircuitLogMessage =
            "Request was short circuited at action filter '{ActionFilter}'.";
        private const string ExceptionFilterShortCircuitLogMessage =
            "Request was short circuited at exception filter '{ExceptionFilter}'.";
        private const string ResultFilterShortCircuitLogMessage =
            "Request was short circuited at result filter '{ResultFilter}'.";

        public FilterActionInvoker(
            [NotNull] ActionContext actionContext,
            [NotNull] IReadOnlyList<IFilterProvider> filterProviders,
            [NotNull] IReadOnlyList<IInputFormatter> inputFormatters,
            [NotNull] IReadOnlyList<IOutputFormatter> outputFormatters,
            [NotNull] IReadOnlyList<IModelBinder> modelBinders,
            [NotNull] IReadOnlyList<IModelValidatorProvider> modelValidatorProviders,
            [NotNull] IReadOnlyList<IValueProviderFactory> valueProviderFactories,
            [NotNull] IActionBindingContextAccessor actionBindingContextAccessor,
            [NotNull] ILogger logger,
            [NotNull] INotifier notifier,
            int maxModelValidationErrors)
        {
            ActionContext = actionContext;

            _filterProviders = filterProviders;
            _inputFormatters = inputFormatters;
            _outputFormatters = outputFormatters;
            _modelBinders = modelBinders;
            _modelValidatorProviders = modelValidatorProviders;
            _valueProviderFactories = valueProviderFactories;
            _actionBindingContextAccessor = actionBindingContextAccessor;
            _logger = logger;
            _notifier = notifier;
            _maxModelValidationErrors = maxModelValidationErrors;
        }

        protected ActionContext ActionContext { get; private set; }

        protected ActionBindingContext ActionBindingContext
        {
            get
            {
                return _actionBindingContextAccessor.ActionBindingContext;
            }
            private set
            {
                _actionBindingContextAccessor.ActionBindingContext = value;
            }
        }

        protected object Instance { get; private set; }

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

        protected abstract Task<IActionResult> InvokeActionAsync(ActionExecutingContext actionExecutingContext);

        protected abstract Task<IDictionary<string, object>> BindActionArgumentsAsync(
            [NotNull] ActionContext context,
            [NotNull] ActionBindingContext bindingContext);

        public virtual async Task InvokeAsync()
        {
            _filters = GetFilters();
            _cursor = new FilterCursor(_filters);

            ActionContext.ModelState.MaxAllowedErrors = _maxModelValidationErrors;

            await InvokeAllAuthorizationFiltersAsync();

            // If Authorization Filters return a result, it's a short circuit because
            // authorization failed. We don't execute Result Filters around the result.
            Debug.Assert(_authorizationContext != null);
            if (_authorizationContext.Result != null)
            {
                await InvokeResultAsync(_authorizationContext.Result);
                return;
            }

            try
            {
                await InvokeAllResourceFiltersAsync();
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

            // We've reached the end of resource filters. If there's an unhandled exception on the context then
            // it should be thrown and middleware has a chance to handle it.
            Debug.Assert(_resourceExecutedContext != null);
            if (_resourceExecutedContext.Exception != null && !_resourceExecutedContext.ExceptionHandled)
            {
                if (_resourceExecutedContext.ExceptionDispatchInfo == null)
                {
                    throw _resourceExecutedContext.Exception;
                }
                else
                {
                    _resourceExecutedContext.ExceptionDispatchInfo.Throw();
                }
            }
        }

        private IFilterMetadata[] GetFilters()
        {
            var context = new FilterProviderContext(
                ActionContext,
                ActionContext.ActionDescriptor.FilterDescriptors.Select(fd => new FilterItem(fd)).ToList());

            foreach (var provider in _filterProviders)
            {
                provider.OnProvidersExecuting(context);
            }

            for (var i = _filterProviders.Count - 1; i >= 0; i--)
            {
                _filterProviders[i].OnProvidersExecuted(context);
            }

            return context.Results.Select(item => item.Filter).Where(filter => filter != null).ToArray();
        }

        private async Task InvokeAllAuthorizationFiltersAsync()
        {
            _cursor.SetStage(FilterStage.AuthorizationFilters);

            _authorizationContext = new AuthorizationContext(ActionContext, _filters);
            await InvokeAuthorizationFilterAsync();
        }

        private async Task InvokeAuthorizationFilterAsync()
        {
            // We should never get here if we already have a result.
            Debug.Assert(_authorizationContext != null);
            Debug.Assert(_authorizationContext.Result == null);

            var current = _cursor.GetNextFilter<IAuthorizationFilter, IAsyncAuthorizationFilter>();
            if (current.FilterAsync != null)
            {
                await current.FilterAsync.OnAuthorizationAsync(_authorizationContext);

                if (_authorizationContext.Result == null)
                {
                    // Only keep going if we don't have a result
                    await InvokeAuthorizationFilterAsync();
                }
                else
                {
                    _logger.LogWarning(AuthorizationFailureLogMessage, current.FilterAsync.GetType().FullName);
                }
            }
            else if (current.Filter != null)
            {
                current.Filter.OnAuthorization(_authorizationContext);

                if (_authorizationContext.Result == null)
                {
                    // Only keep going if we don't have a result
                    await InvokeAuthorizationFilterAsync();
                }
                else
                {
                    _logger.LogWarning(AuthorizationFailureLogMessage, current.Filter.GetType().FullName);
                }
            }
            else
            {
                // We've run out of Authorization Filters - if we haven't short circuited by now then this
                // request is authorized.
            }
        }

        private async Task InvokeAllResourceFiltersAsync()
        {
            _cursor.SetStage(FilterStage.ResourceFilters);

            var context = new ResourceExecutingContext(ActionContext, _filters);

            context.InputFormatters = new List<IInputFormatter>(_inputFormatters);
            context.OutputFormatters = new List<IOutputFormatter>(_outputFormatters);
            context.ModelBinders = new List<IModelBinder>(_modelBinders);
            context.ValidatorProviders = new List<IModelValidatorProvider>(_modelValidatorProviders);
            context.ValueProviderFactories = new List<IValueProviderFactory>(_valueProviderFactories);

            _resourceExecutingContext = context;
            await InvokeResourceFilterAsync();
        }

        private async Task<ResourceExecutedContext> InvokeResourceFilterAsync()
        {
            Debug.Assert(_resourceExecutingContext != null);

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

            var item = _cursor.GetNextFilter<IResourceFilter, IAsyncResourceFilter>();
            try
            {
                if (item.FilterAsync != null)
                {
                    await item.FilterAsync.OnResourceExecutionAsync(
                        _resourceExecutingContext,
                        InvokeResourceFilterAsync);

                    if (_resourceExecutedContext == null)
                    {
                        // If we get here then the filter didn't call 'next' indicating a short circuit
                        if (_resourceExecutingContext.Result != null)
                        {
                            _logger.LogVerbose(
                                ResourceFilterShortCircuitLogMessage,
                                item.FilterAsync.GetType().FullName);

                            await InvokeResultAsync(_resourceExecutingContext.Result);
                        }

                        _resourceExecutedContext = new ResourceExecutedContext(_resourceExecutingContext, _filters)
                        {
                            Canceled = true,
                            Result = _resourceExecutingContext.Result,
                        };
                    }
                }
                else if (item.Filter != null)
                {
                    item.Filter.OnResourceExecuting(_resourceExecutingContext);

                    if (_resourceExecutingContext.Result != null)
                    {
                        // Short-circuited by setting a result.

                        _logger.LogVerbose(ResourceFilterShortCircuitLogMessage, item.Filter.GetType().FullName);

                        await InvokeResultAsync(_resourceExecutingContext.Result);

                        _resourceExecutedContext = new ResourceExecutedContext(_resourceExecutingContext, _filters)
                        {
                            Canceled = true,
                            Result = _resourceExecutingContext.Result,
                        };
                    }
                    else
                    {
                        item.Filter.OnResourceExecuted(await InvokeResourceFilterAsync());
                    }
                }
                else
                {
                    // We've reached the end of resource filters, so move to setting up state to invoke model
                    // binding.
                    ActionBindingContext = new ActionBindingContext();
                    ActionBindingContext.InputFormatters = _resourceExecutingContext.InputFormatters;
                    ActionBindingContext.OutputFormatters = _resourceExecutingContext.OutputFormatters;
                    ActionBindingContext.ModelBinder = new CompositeModelBinder(_resourceExecutingContext.ModelBinders);
                    ActionBindingContext.ValidatorProvider = new CompositeModelValidatorProvider(
                        _resourceExecutingContext.ValidatorProviders);

                    var valueProviderFactoryContext = new ValueProviderFactoryContext(
                        ActionContext.HttpContext,
                        ActionContext.RouteData.Values);

                    ActionBindingContext.ValueProvider = await CompositeValueProvider.CreateAsync(
                        _resourceExecutingContext.ValueProviderFactories,
                        valueProviderFactoryContext);

                    // >> ExceptionFilters >> Model Binding >> ActionFilters >> Action
                    await InvokeAllExceptionFiltersAsync();

                    // If Exception Filters provide a result, it's a short-circuit due to an exception.
                    // We don't execute Result Filters around the result.
                    Debug.Assert(_exceptionContext != null);
                    if (_exceptionContext.Result != null)
                    {
                        // This means that exception filters returned a result to 'handle' an error.
                        // We're not interested in seeing the exception details since it was handled.
                        await InvokeResultAsync(_exceptionContext.Result);

                        _resourceExecutedContext = new ResourceExecutedContext(_resourceExecutingContext, _filters)
                        {
                            Result = _exceptionContext.Result,
                        };
                    }
                    else if (_exceptionContext.Exception != null)
                    {
                        // If we get here, this means that we have an unhandled exception.
                        // Exception filted didn't handle this, so send it on to resource filters.
                        _resourceExecutedContext = new ResourceExecutedContext(_resourceExecutingContext, _filters);

                        // Preserve the stack trace if possible.
                        _resourceExecutedContext.Exception = _exceptionContext.Exception;
                        if (_exceptionContext.ExceptionDispatchInfo != null)
                        {
                            _resourceExecutedContext.ExceptionDispatchInfo = _exceptionContext.ExceptionDispatchInfo;
                        }
                    }
                    else
                    {
                        // We have a successful 'result' from the action or an Action Filter, so run
                        // Result Filters.
                        Debug.Assert(_actionExecutedContext != null);
                        var result = _actionExecutedContext.Result;

                        // >> ResultFilters >> (Result)
                        await InvokeAllResultFiltersAsync(result);

                        _resourceExecutedContext = new ResourceExecutedContext(_resourceExecutingContext, _filters)
                        {
                            Result = _resultExecutedContext.Result,
                        };
                    }
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
            return _resourceExecutedContext;
        }

        private async Task InvokeAllExceptionFiltersAsync()
        {
            _cursor.SetStage(FilterStage.ExceptionFilters);

            await InvokeExceptionFilterAsync();
        }

        private async Task InvokeExceptionFilterAsync()
        {
            var current = _cursor.GetNextFilter<IExceptionFilter, IAsyncExceptionFilter>();
            if (current.FilterAsync != null)
            {
                // Exception filters run "on the way out" - so the filter is run after the rest of the
                // pipeline.
                await InvokeExceptionFilterAsync();

                Debug.Assert(_exceptionContext != null);
                if (_exceptionContext.Exception != null)
                {
                    // Exception filters only run when there's an exception - unsetting it will short-circuit
                    // other exception filters.
                    await current.FilterAsync.OnExceptionAsync(_exceptionContext);

                    if (_exceptionContext.Exception == null)
                    {
                        _logger.LogVerbose(
                            ExceptionFilterShortCircuitLogMessage,
                            current.FilterAsync.GetType().FullName);
                    }
                }
            }
            else if (current.Filter != null)
            {
                // Exception filters run "on the way out" - so the filter is run after the rest of the
                // pipeline.
                await InvokeExceptionFilterAsync();

                Debug.Assert(_exceptionContext != null);
                if (_exceptionContext.Exception != null)
                {
                    // Exception filters only run when there's an exception - unsetting it will short-circuit
                    // other exception filters.
                    current.Filter.OnException(_exceptionContext);

                    if (_exceptionContext.Exception == null)
                    {
                        _logger.LogVerbose(
                            ExceptionFilterShortCircuitLogMessage,
                            current.Filter.GetType().FullName);
                    }
                }
            }
            else
            {
                // We've reached the 'end' of the exception filter pipeline - this means that one stack frame has
                // been built for each exception. When we return from here, these frames will either:
                //
                // 1) Call the filter (if we have an exception)
                // 2) No-op (if we don't have an exception)
                Debug.Assert(_exceptionContext == null);
                _exceptionContext = new ExceptionContext(ActionContext, _filters);

                try
                {
                    await InvokeAllActionFiltersAsync();

                    // Action filters might 'return' an unhandled exception instead of throwing
                    Debug.Assert(_actionExecutedContext != null);
                    if (_actionExecutedContext.Exception != null && !_actionExecutedContext.ExceptionHandled)
                    {
                        _exceptionContext.Exception = _actionExecutedContext.Exception;
                        if (_actionExecutedContext.ExceptionDispatchInfo != null)
                        {
                            _exceptionContext.ExceptionDispatchInfo = _actionExecutedContext.ExceptionDispatchInfo;
                        }
                    }
                }
                catch (Exception exception)
                {
                    _exceptionContext.ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
                }
            }
        }

        private async Task InvokeAllActionFiltersAsync()
        {
            _cursor.SetStage(FilterStage.ActionFilters);

            Instance = CreateInstance();

            var arguments = await BindActionArgumentsAsync(ActionContext, ActionBindingContext);

            _actionExecutingContext = new ActionExecutingContext(
                ActionContext,
                _filters,
                arguments,
                Instance);

            await InvokeActionFilterAsync();
        }

        private async Task<ActionExecutedContext> InvokeActionFilterAsync()
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

            var item = _cursor.GetNextFilter<IActionFilter, IAsyncActionFilter>();
            try
            {
                if (item.FilterAsync != null)
                {
                    await item.FilterAsync.OnActionExecutionAsync(_actionExecutingContext, InvokeActionFilterAsync);

                    if (_actionExecutedContext == null)
                    {
                        // If we get here then the filter didn't call 'next' indicating a short circuit

                        _logger.LogVerbose(ActionFilterShortCircuitLogMessage, item.FilterAsync.GetType().FullName);

                        _actionExecutedContext = new ActionExecutedContext(
                            _actionExecutingContext,
                            _filters,
                            Instance)
                        {
                            Canceled = true,
                            Result = _actionExecutingContext.Result,
                        };
                    }
                }
                else if (item.Filter != null)
                {
                    item.Filter.OnActionExecuting(_actionExecutingContext);

                    if (_actionExecutingContext.Result != null)
                    {
                        // Short-circuited by setting a result.

                        _logger.LogVerbose(ActionFilterShortCircuitLogMessage, item.Filter.GetType().FullName);

                        _actionExecutedContext = new ActionExecutedContext(
                            _actionExecutingContext,
                            _filters,
                            Instance)
                        {
                            Canceled = true,
                            Result = _actionExecutingContext.Result,
                        };
                    }
                    else
                    {
                        item.Filter.OnActionExecuted(await InvokeActionFilterAsync());
                    }
                }
                else
                {
                    // All action filters have run, execute the action method.
                    IActionResult result = null;

                    try
                    {
                        if (_notifier.ShouldNotify("Microsoft.AspNet.Mvc.BeforeActionMethod"))
                        {
                            _notifier.Notify(
                                "Microsoft.AspNet.Mvc.BeforeActionMethod",
                                new { actionContext = ActionContext, arguments = _actionExecutingContext.ActionArguments });
                        }

                        result = await InvokeActionAsync(_actionExecutingContext);
                    }
                    finally
                    {
                        if (_notifier.ShouldNotify("Microsoft.AspNet.Mvc.AfterActionMethod"))
                        {
                            _notifier.Notify(
                                "Microsoft.AspNet.Mvc.AfterActionMethod",
                                new { actionContext = ActionContext, result });
                        }
                    }

                    _actionExecutedContext = new ActionExecutedContext(
                        _actionExecutingContext,
                        _filters,
                        Instance)
                    {
                        Result = result
                    };
                }
            }
            catch (Exception exception)
            {
                // Exceptions thrown by the action method OR filters bubble back up through ActionExcecutedContext.
                _actionExecutedContext = new ActionExecutedContext(
                    _actionExecutingContext,
                    _filters,
                    Instance)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception)
                };
            }
            return _actionExecutedContext;
        }

        private async Task InvokeAllResultFiltersAsync(IActionResult result)
        {
            _cursor.SetStage(FilterStage.ResultFilters);

            _resultExecutingContext = new ResultExecutingContext(ActionContext, _filters, result, Instance);
            await InvokeResultFilterAsync();

            Debug.Assert(_resultExecutingContext != null);
            if (_resultExecutedContext.Exception != null && !_resultExecutedContext.ExceptionHandled)
            {
                // There's an unhandled exception in filters
                if (_resultExecutedContext.ExceptionDispatchInfo != null)
                {
                    _resultExecutedContext.ExceptionDispatchInfo.Throw();
                }
                else
                {
                    throw _resultExecutedContext.Exception;
                }
            }
        }

        private async Task<ResultExecutedContext> InvokeResultFilterAsync()
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

            try
            {
                var item = _cursor.GetNextFilter<IResultFilter, IAsyncResultFilter>();
                if (item.FilterAsync != null)
                {
                    await item.FilterAsync.OnResultExecutionAsync(_resultExecutingContext, InvokeResultFilterAsync);

                    if (_resultExecutedContext == null)
                    {
                        // Short-circuited by not calling next

                        _logger.LogVerbose(ResourceFilterShortCircuitLogMessage, item.FilterAsync.GetType().FullName);

                        _resultExecutedContext = new ResultExecutedContext(
                            _resultExecutingContext,
                            _filters,
                            _resultExecutingContext.Result,
                            Instance)
                        {
                            Canceled = true,
                        };
                    }
                    else if (_resultExecutingContext.Cancel == true)
                    {
                        // Short-circuited by setting Cancel == true

                        _logger.LogVerbose(ResourceFilterShortCircuitLogMessage, item.FilterAsync.GetType().FullName);

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
                else if (item.Filter != null)
                {
                    item.Filter.OnResultExecuting(_resultExecutingContext);

                    if (_resultExecutingContext.Cancel == true)
                    {
                        // Short-circuited by setting Cancel == true

                        _logger.LogVerbose(ResourceFilterShortCircuitLogMessage, item.Filter.GetType().FullName);

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
                        item.Filter.OnResultExecuted(await InvokeResultFilterAsync());
                    }
                }
                else
                {
                    _cursor.SetStage(FilterStage.ActionResult);

                    // The empty result is always flowed back as the 'executed' result
                    if (_resultExecutingContext.Result == null)
                    {
                        _resultExecutingContext.Result = new EmptyResult();
                    }

                    await InvokeResultAsync(_resultExecutingContext.Result);

                    Debug.Assert(_resultExecutedContext == null);
                    _resultExecutedContext = new ResultExecutedContext(
                        _resultExecutingContext,
                        _filters,
                        _resultExecutingContext.Result,
                        Instance);
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

            return _resultExecutedContext;
        }

        private async Task InvokeResultAsync(IActionResult result)
        {
            if (_notifier.ShouldNotify("Microsoft.AspNet.Mvc.BeforeActionResult"))
            {
                _notifier.Notify(
                    "Microsoft.AspNet.Mvc.BeforeActionResult",
                    new { actionContext = ActionContext, result });
            }

            try
            {
                await result.ExecuteResultAsync(ActionContext);
            }
            finally
            {
                if (_notifier.ShouldNotify("Microsoft.AspNet.Mvc.AfterActionResult"))
                {
                    _notifier.Notify(
                        "Microsoft.AspNet.Mvc.AfterActionResult",
                        new { actionContext = ActionContext, result });
                }
            }
        }

        private enum FilterStage
        {
            Undefined,
            AuthorizationFilters,
            ResourceFilters,
            ExceptionFilters,
            ActionFilters,
            ActionMethod,
            ResultFilters,
            ActionResult
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
            private FilterStage _stage;
            private int _index;
            private readonly IFilterMetadata[] _filters;

            public FilterCursor(FilterStage stage, int index, IFilterMetadata[] filters)
            {
                _stage = stage;
                _index = index;
                _filters = filters;
            }

            public FilterCursor(IFilterMetadata[] filters)
            {
                _stage = FilterStage.Undefined;
                _index = 0;
                _filters = filters;
            }

            public void SetStage(FilterStage stage)
            {
                _stage = stage;
                _index = 0;
            }

            public FilterCursorItem<TFilter, TFilterAsync> GetNextFilter<TFilter, TFilterAsync>()
                where TFilter : class
                where TFilterAsync : class
            {
                while (_index < _filters.Length)
                {
                    var filter = _filters[_index] as TFilter;
                    var filterAsync = _filters[_index] as TFilterAsync;

                    _index += 1;

                    if (filter != null || filterAsync != null)
                    {
                        return new FilterCursorItem<TFilter, TFilterAsync>(_stage, _index, filter, filterAsync);
                    }
                }

                return default(FilterCursorItem<TFilter, TFilterAsync>);
            }

            public bool StillAt<TFilter, TFilterAsync>(FilterCursorItem<TFilter, TFilterAsync> current)
            {
                return current.Stage == _stage && current.Index == _index;
            }
        }

        private struct FilterCursorItem<TFilter, TFilterAsync>
        {
            public readonly FilterStage Stage;
            public readonly int Index;
            public readonly TFilter Filter;
            public readonly TFilterAsync FilterAsync;

            public FilterCursorItem(FilterStage stage, int index, TFilter filter, TFilterAsync filterAsync)
            {
                Stage = stage;
                Index = index;
                Filter = filter;
                FilterAsync = filterAsync;
            }
        }
    }
}
