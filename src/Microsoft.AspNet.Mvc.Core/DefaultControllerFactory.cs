using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultControllerFactory : IControllerFactory
    {
        private readonly ITypeActivator _activator;
        private readonly IServiceProvider _serviceProvider;

        public DefaultControllerFactory(IServiceProvider serviceProvider, ITypeActivator activator)
        {
            _serviceProvider = serviceProvider;
            _activator = activator;
        }

        public object CreateController(ActionContext actionContext, ModelStateDictionary modelState)
        {
            var actionDescriptor = actionContext.ActionDescriptor as ReflectedActionDescriptor;
            if (actionDescriptor == null)
            {
                return null;
            }

            try
            {
                var controller = _activator.CreateInstance(actionDescriptor.ControllerDescriptor.ControllerTypeInfo.AsType());

                // TODO: How do we feed the controller with context (need DI improvements)
                InitializeController(controller, actionContext, modelState);

                return controller;
            }
            catch (ReflectionTypeLoadException)
            {
            }

            return null;
        }

        public void ReleaseController(object controller)
        {
        }

        private void InitializeController(object controller, ActionContext actionContext, ModelStateDictionary modelState)
        {
            var controllerType = controller.GetType();

            foreach (var prop in controllerType.GetRuntimeProperties())
            {
                if (prop.Name == "Context" && prop.PropertyType == typeof(HttpContext))
                {
                    prop.SetValue(controller, actionContext.HttpContext);
                }
                else if (prop.Name == "ModelState" && prop.PropertyType == typeof(ModelStateDictionary))
                {
                    prop.SetValue(controller, modelState);
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
