﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.Core.ViewComponents
{
    public static class ViewComponentMethodSelector
    {
        public const string AsyncMethodName = "InvokeAsync";
        public const string SyncMethodName = "Invoke";

        public static MethodInfo FindAsyncMethod([NotNull] Type componentType, [NotNull] object[] args)
        {
            var method = GetMethod(componentType, args, AsyncMethodName);
            if (method == null)
            {
                return null;
            }

            if (!method.ReturnType.IsGenericType || method.ReturnType.GetGenericTypeDefinition() != typeof(Task<>))
            {
                throw new InvalidOperationException(
                    Resources.FormatViewComponent_AsyncMethod_ShouldReturnTask(AsyncMethodName));
            }

            return method;
        }

        public static MethodInfo FindSyncMethod([NotNull] Type componentType, [NotNull] object[] args)
        {
            var method = GetMethod(componentType, args, SyncMethodName);
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

        private static MethodInfo GetMethod([NotNull] Type componentType, [NotNull] object[] args, [NotNull] string methodName)
        {
            var argumentExpressions = new Expression[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                argumentExpressions[i] = Expression.Constant(args[i], args[i].GetType());
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
                var expression = Expression.Call(Expression.Constant(null, componentType), methodName, null, argumentExpressions);
                return expression.Method;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}
