
using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultViewComponentInvoker : IViewComponentInvoker
    {
        private const string AsyncMethodName = "InvokeAsync";
        private const string SyncMethodName = "Invoke";

        private readonly IServiceProvider _serviceProvider;
        private readonly Type _componentType;
        private readonly object[] _args;

        public DefaultViewComponentInvoker([NotNull] IServiceProvider serviceProvider, [NotNull] Type componentType, [NotNull] object[] args)
        {
            _serviceProvider = serviceProvider;
            _componentType = componentType;
            _args = args;
        }

        public void Invoke([NotNull] ComponentContext context)
        {
            var method = FindSyncMethod();
            if (method == null)
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.CurrentCulture,
                    "Could not find an '{0}' method matching the parameters.",
                    SyncMethodName));
            }

            var result = InvokeSyncCore(method, context.ViewContext);
            result.Execute(context);
        }

        public async Task InvokeAsync([NotNull] ComponentContext context)
        {
            IViewComponentResult result;

            var asyncMethod = FindAsyncMethod();
            if (asyncMethod == null)
            {
                // We support falling back to synchronous if there is no InvokeAsync method, in this case we'll still get
                // execute the IViewResult asynchronously.
                var syncMethod = FindSyncMethod();
                if (syncMethod == null)
                {
                    throw new InvalidOperationException(string.Format(
                        CultureInfo.CurrentCulture,
                        "Could not find an '{0}' or '{1}' method matching the parameters.",
                        SyncMethodName,
                        AsyncMethodName));
                }
                else
                {
                    result = InvokeSyncCore(syncMethod, context.ViewContext);
                }
            }
            else
            {
                result = await InvokeAsyncCore(asyncMethod, context.ViewContext);
            }


            await result.ExecuteAsync(context);
        }

        private MethodInfo FindAsyncMethod()
        {
            var method = GetMethod(AsyncMethodName);
            if (method == null)
            {
                return null;
            }

            if (!method.IsGenericMethod || method.ReturnType.GetGenericTypeDefinition() != typeof(Task<>))
            {
                throw new InvalidOperationException(
                    Resources.FormatViewComponent_AsyncMethod_ShouldReturnTask(AsyncMethodName));
            }

            return method;
        }

        private MethodInfo FindSyncMethod()
        {
            var method = GetMethod(SyncMethodName);
            if (method == null)
            {
                return null;
            }

            if (method.ReturnType == typeof(void))
            {
                throw new InvalidOperationException(Resources.FormatViewComponent_SyncMethod_ShouldReturnValue(SyncMethodName));
            }

            return method;
        }

        private MethodInfo GetMethod([NotNull] string methodName)
        {
            var argumentExpressions = new Expression[_args.Length];
            for (var i = 0; i < _args.Length; i++)
            {
                argumentExpressions[i] = Expression.Constant(_args[i], _args[i].GetType());
            }

            try
            {
                // We're currently using this technique to make a call into a component method that looks like a regular method call.
                //
                // Ex: @Component.Invoke<Cart>("hello", 5) => cart.Invoke("hello", 5)
                //
                // This approach has some drawbacks, namely it doesn't account for default parameters, and more noticably, it throws
                // if the method is not found.
                //
                // Unfortunely the overload of Type.GetMethod that we would like to use is not present in CoreCLR. Item #160 in Jira
                // tracks these issues.
                var expression = Expression.Call(Expression.Constant(null, _componentType), methodName, null, argumentExpressions);
                return expression.Method;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private object CreateComponent([NotNull] ViewContext context)
        {
            var activator = _serviceProvider.GetService<ITypeActivator>();
            object component = activator.CreateInstance(_componentType);

            foreach (var prop in _componentType.GetRuntimeProperties())
            {
                if (prop.Name == "ViewContext" && typeof(ViewContext).GetTypeInfo().IsAssignableFrom(prop.PropertyType))
                {
                    prop.SetValue(component, context.HttpContext);
                }
                else if (prop.Name == "ViewData" && typeof(ViewData).GetTypeInfo().IsAssignableFrom(prop.PropertyType))
                {
                    // Creating a new copy of the view data, so changes aren't visible in the calling view.
                    var viewData = new ViewData(context.ViewData);
                    prop.SetValue(component, viewData);
                }
            }

            var method = _componentType.GetRuntimeMethods().FirstOrDefault(m => m.Name.Equals("Initialize", StringComparison.OrdinalIgnoreCase));
            if (method != null)
            {
                var args = method.GetParameters()
                                 .Select(p => _serviceProvider.GetService(p.ParameterType)).ToArray();

                method.Invoke(component, args);
            }

            return component;
        }

        private async Task<IViewComponentResult> InvokeAsyncCore([NotNull] MethodInfo method, [NotNull] ViewContext context)
        {
            var component = CreateComponent(context);

            var result = await ReflectedActionExecutor.ExecuteAsync(method, component, _args);

            return CoerceToViewComponentResult(result);
        }

        public IViewComponentResult InvokeSyncCore([NotNull] MethodInfo method, [NotNull] ViewContext context)
        {
            var component = CreateComponent(context);

            object result = null;

            try
            {
                result = method.Invoke(component, _args);
            }
            catch (TargetInvocationException ex)
            {
                // Preserve callstack of any user-thrown exceptions.
                var exceptionInfo = ExceptionDispatchInfo.Capture(ex.InnerException);
                exceptionInfo.Throw();
            }

            return CoerceToViewComponentResult(result);
        }

        private static IViewComponentResult CoerceToViewComponentResult(object value)
        {
            if (value == null)
            {
                throw new InvalidOperationException(Resources.ViewComponent_MustReturnValue);
            }

            var componentResult = value as IViewComponentResult;
            if (componentResult != null)
            {
                return componentResult;
            }

            var stringResult = value as string;
            if (stringResult != null)
            {
                return new ContentViewComponentResult(stringResult);
            }

            var htmlStringResult = value as HtmlString;
            if (htmlStringResult != null)
            {
                return new ContentViewComponentResult(htmlStringResult);
            }

            // Currently not handling POCO cases
            throw new NotImplementedException("No support for POCO types here.");
        }
    }
}
