// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class MvcAttributeRouteHandler : IRouter
    {
        private readonly ActionDescriptor[] _actionDescriptors;

        private bool _servicesRetrieved;

        private IActionContextAccessor _actionContextAccessor;
        private IActionInvokerFactory _actionInvokerFactory;
        private IActionSelector _actionSelector;
        private ILogger _logger;
        private DiagnosticSource _diagnosticSource;

        public MvcAttributeRouteHandler(ActionDescriptor[] actionDescriptors)
        {
            if (actionDescriptors == null)
            {
                throw new ArgumentNullException(nameof(actionDescriptors));
            }

            _actionDescriptors = actionDescriptors;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // We return null here because we're not responsible for generating the url, the route is.
            return null;
        }

        public Task RouteAsync(RouteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            EnsureServices(context.HttpContext);

            var actionDescriptor = _actionSelector.SelectBestCandidate(context, _actionDescriptors);
            if (actionDescriptor == null)
            {
                _logger.NoActionsMatched();
                return TaskCache.CompletedTask;
            }

            foreach (var kvp in actionDescriptor.RouteValues)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    context.RouteData.Values[kvp.Key] = kvp.Value;
                }
            }

            context.Handler = (c) => InvokeActionAsync(c, actionDescriptor);
            return TaskCache.CompletedTask;
        }

        private async Task InvokeActionAsync(HttpContext httpContext, ActionDescriptor actionDescriptor)
        {
            var routeData = httpContext.GetRouteData();
            try
            {
                _diagnosticSource.BeforeAction(actionDescriptor, httpContext, routeData);

                using (_logger.ActionScope(actionDescriptor))
                {
                    _logger.ExecutingAction(actionDescriptor);

                    var startTimestamp = _logger.IsEnabled(LogLevel.Information) ? Stopwatch.GetTimestamp() : 0;

                    var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
                    if (_actionContextAccessor != null)
                    {
                        _actionContextAccessor.ActionContext = actionContext;
                    }

                    var invoker = _actionInvokerFactory.CreateInvoker(actionContext);
                    if (invoker == null)
                    {
                        throw new InvalidOperationException(
                            Resources.FormatActionInvokerFactory_CouldNotCreateInvoker(
                                actionDescriptor.DisplayName));
                    }

                    await invoker.InvokeAsync();

                    _logger.ExecutedAction(actionDescriptor, startTimestamp);
                }
            }
            finally
            {
                _diagnosticSource.AfterAction(actionDescriptor, httpContext, routeData);
            }
        }

        private void EnsureServices(HttpContext context)
        {
            if (_servicesRetrieved)
            {
                return;
            }

            var services = context.RequestServices;

            // The IActionContextAccessor is optional. We want to avoid the overhead of using CallContext
            // if possible.
            _actionContextAccessor = services.GetService<IActionContextAccessor>();

            _actionInvokerFactory = services.GetRequiredService<IActionInvokerFactory>();
            _actionSelector = services.GetRequiredService<IActionSelector>();
            _diagnosticSource = services.GetRequiredService<DiagnosticSource>();

            var factory = services.GetRequiredService<ILoggerFactory>();
            _logger = factory.CreateLogger<MvcRouteHandler>();

            _servicesRetrieved = true;
        }
    }
}
