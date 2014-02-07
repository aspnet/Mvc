using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    public class TypeMethodBasedActionInvoker : IActionInvoker
    {
        private readonly ActionContext _actionContext;
        private readonly TypeMethodBasedActionDescriptor _descriptor;
        private readonly IActionResultFactory _actionResultFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly IControllerFactory _controllerFactory;

        public TypeMethodBasedActionInvoker(ActionContext actionContext,
                                            TypeMethodBasedActionDescriptor descriptor,
                                            IActionResultFactory actionResultFactory,
                                            IControllerFactory controllerFactory,
                                            IServiceProvider serviceProvider,
                                            IEnumerable<IModelBinderProvider> )
        {
            _actionContext = actionContext;
            _descriptor = descriptor;
            _actionResultFactory = actionResultFactory;
            _controllerFactory = controllerFactory;
            _serviceProvider = serviceProvider;
        }

        public Task InvokeActionAsync()
        {
            IActionResult actionResult = null;

            object controller = _controllerFactory.CreateController(_actionContext.HttpContext, _descriptor);

            if (controller == null)
            {
                actionResult = new HttpStatusCodeResult(404);
            }
            else
            {
                Initialize(controller);

                var method = _descriptor.MethodInfo;

                if (method == null)
                {
                    actionResult = new HttpStatusCodeResult(404);
                }
                else
                {
                    object actionReturnValue = method.Invoke(controller, GetArgumentValues(method));
                    actionResult = _actionResultFactory.CreateActionResult(method.ReturnType, actionReturnValue, _actionContext);
                }
            }

            // TODO: This will probably move out once we got filters
            return actionResult.ExecuteResultAsync(_actionContext);
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
                        prop.SetValue(controller, _actionContext.HttpContext);
                    }
                }
            }

            var method = controllerType.GetRuntimeMethods().FirstOrDefault(m => m.Name.Equals("Initialize", StringComparison.OrdinalIgnoreCase));

            if (method == null)
            {
                return;
            }

            var args = method.GetParameters()
                             .Select(p => _serviceProvider.GetService(p.ParameterType))
                             .ToArray();

            method.Invoke(controller, args);
        }

        private object[] GetArgumentValues(MethodInfo method)
        {
            var parameters = method.GetParameters();
            var args = new object[parameters.Length];

            var contextProvider = _serviceProvider.GetService<IModelBindingConfigProvider>();
            var modelState = new ModelStateDictionary();
            ModelBinderConfig config = contextProvider.GetConfig(_actionContext);

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var service = _serviceProvider.GetService(parameter.ParameterType);
                if (service != null)
                {
                    args[i] = service;
                }
                else
                {
                    var context = CreateModelBindingContext(modelState, config, parameter);

                    object value = null;
                    if (config.ModelBinder.BindModel(context))
                    {
                        value = context.Model;
                    }

                    if (value == null)
                    {
                        value = GetDefaultValue(parameter, value);
                    }

                    args[i] = value;
                }
            }

            return args;
        }


        private ModelBindingContext CreateModelBindingContext(ModelStateDictionary modelState,
                                                              ModelBinderConfig config,
                                                              ParameterInfo parameter)
        {
            return new ModelBindingContext
            {
                ModelName = parameter.Name,
                ModelState = modelState,
                ModelMetadata = config.MetadataProvider.GetMetadataForParameter(parameter),
                ModelBinder = config.ModelBinder,
                ValueProvider = config.ValueProvider,
                MetadataProvider = config.MetadataProvider,
                HttpContext = _actionContext.HttpContext
            };
        }

        private static object GetDefaultValue(ParameterInfo parameter, object value)
        {
            if (parameter.HasDefaultValue)
            {
                return parameter.DefaultValue;
            }
            else if (parameter.ParameterType.IsValueType())
            {
                return Activator.CreateInstance(parameter.ParameterType);
            }
            return value;
        }
    }
}
