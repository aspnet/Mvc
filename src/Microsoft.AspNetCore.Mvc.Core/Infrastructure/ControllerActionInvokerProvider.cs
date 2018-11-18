// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.Infrastructure
{
    internal class ControllerActionInvokerProvider : IActionInvokerProvider
    {
        private readonly ControllerActionInvokerCache _controllerActionInvokerCache;
        private readonly IReadOnlyList<IValueProviderFactory> _valueProviderFactories;
        private readonly int _maxModelValidationErrors;
        private readonly ILogger _logger;
        private readonly DiagnosticListener _diagnosticListener;
        private readonly IActionResultTypeMapper _mapper;

        public ControllerActionInvokerProvider(
            ControllerActionInvokerCache controllerActionInvokerCache,
            IOptions<MvcOptions> optionsAccessor,
            ILoggerFactory loggerFactory,
            DiagnosticListener diagnosticListener,
            IActionResultTypeMapper mapper)
        {
            _controllerActionInvokerCache = controllerActionInvokerCache;
            _valueProviderFactories = optionsAccessor.Value.ValueProviderFactories.ToArray();
            _maxModelValidationErrors = optionsAccessor.Value.MaxModelValidationErrors;
            _logger = loggerFactory.CreateLogger<ControllerActionInvoker>();
            _diagnosticListener = diagnosticListener;
            _mapper = mapper;
        }

        public int Order => -1000;

        /// <inheritdoc />
        public void OnProvidersExecuting(ActionInvokerProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.ActionContext.ActionDescriptor is ControllerActionDescriptor)
            {
                var controllerContext = new ControllerContext(context.ActionContext);
                // PERF: These are rarely going to be changed, so let's go copy-on-write.
                controllerContext.ValueProviderFactories = new CopyOnWriteList<IValueProviderFactory>(_valueProviderFactories);
                controllerContext.ModelState.MaxAllowedErrors = _maxModelValidationErrors;

                var cacheResult = _controllerActionInvokerCache.GetCachedResult(controllerContext);

                var invoker = new ControllerActionInvoker(
                    _logger,
                    _diagnosticListener,
                    _mapper,
                    controllerContext,
                    cacheResult.cacheEntry,
                    cacheResult.filters);

                context.Result = invoker;
            }
        }

        /// <inheritdoc />
        public void OnProvidersExecuted(ActionInvokerProviderContext context)
        {
        }
    }
}
