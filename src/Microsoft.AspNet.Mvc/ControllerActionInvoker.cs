﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class ControllerActionInvoker : IActionInvoker
    {
        private readonly RequestContext _requestContext;
        private readonly ControllerBasedActionDescriptor _descriptor;
        private readonly IActionResultFactory _actionResultFactory;
        private readonly IServiceProvider _serviceProvider;

        public ControllerActionInvoker(RequestContext requestContext,
                                       ControllerBasedActionDescriptor descriptor,
                                       IActionResultFactory actionResultFactory,
                                       IServiceProvider serviceProvider)
        {
            _requestContext = requestContext;
            _descriptor = descriptor;
            _actionResultFactory = actionResultFactory;
            _serviceProvider = serviceProvider;
        }

        public Task InvokeActionAsync()
        {
            var factory = _serviceProvider.GetService<IControllerFactory>();
            object controller = factory.CreateController(_requestContext.HttpContext, _descriptor.ControllerName);

            if (controller == null)
            {
                throw new InvalidOperationException(String.Format("Couldn't find controller '{0}'.", _descriptor.ControllerName));
            }

            Initialize(controller);

            var method = controller.GetType().GetRuntimeMethods().FirstOrDefault(m => m.Name.Equals(_descriptor.ActionName, StringComparison.OrdinalIgnoreCase));

            if (method == null)
            {
                throw new InvalidOperationException(String.Format("Could not find action method '{0}'", _descriptor.ActionName));
            }

            object actionReturnValue = method.Invoke(controller, null);

            var actionResult = _actionResultFactory.CreateActionResult(method.ReturnType, actionReturnValue, _requestContext);

            return actionResult.ExecuteResultAsync(_requestContext);
        }

        private void Initialize(object controller)
        {
            var controllerType = controller.GetType();

            foreach (var prop in controllerType.GetRuntimeProperties())
            {
                if (prop.Name == "Context")
                {
                    if (prop.PropertyType == typeof(HttpContext))
                    {
                        prop.SetValue(controller, _requestContext.HttpContext);
                    }
                }
            }

            var method = controllerType.GetRuntimeMethods().FirstOrDefault(m => m.Name.Equals("Initialize", StringComparison.OrdinalIgnoreCase));

            if (method == null)
            {
                return;
            }

            var args = method.GetParameters()
                             .Select(p => _serviceProvider.GetService(p.ParameterType)).ToArray();

            method.Invoke(controller, args);
        }
    }
}
