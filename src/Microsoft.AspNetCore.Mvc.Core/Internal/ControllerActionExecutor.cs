// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public static class ControllerActionExecutor
    {
        public static Task<object> ExecuteAsync(
            ObjectMethodExecutor actionMethodExecutor,
            object instance,
            IDictionary<string, object> actionArguments)
        {
            var orderedArguments = PrepareArguments(actionArguments, actionMethodExecutor.MethodInfo.GetParameters());
            return ExecuteAsync(actionMethodExecutor, instance, orderedArguments);
        }

        public static Task<object> ExecuteAsync(
            ObjectMethodExecutor actionMethodExecutor,
            object instance,
            object[] orderedActionArguments)
        {
            return actionMethodExecutor.ExecuteAsync(instance, orderedActionArguments);
        }

        public static async Task<TResult> ExecuteAsync<TResult>(
            ObjectMethodExecutor actionMethodExecutor,
            object instance,
            object[] orderedActionArguments,
            Func<ObjectMethodExecutor, object, TResult> createActionResult
            )
        {
            var returnType = actionMethodExecutor.MethodInfo.ReturnType;

            if (actionMethodExecutor.CanExecuteInSync)
            {
                var result = actionMethodExecutor.Execute(instance, orderedActionArguments);
                if (returnType == typeof(Task))
                {
                    await (Task)result;
                }

                return createActionResult(actionMethodExecutor, result);
            }
            else
            {
                var result = await actionMethodExecutor.ExecuteAsync(instance, orderedActionArguments);
                return createActionResult(actionMethodExecutor, result);
            }
        }

        public static object[] PrepareArguments(
            IDictionary<string, object> actionParameters,
            ParameterInfo[] declaredParameterInfos)
        {
            var count = declaredParameterInfos.Length;
            if (count == 0)
            {
                return null;
            }

            var arguments = new object[count];
            for (var index = 0; index < count; index++)
            {
                var parameterInfo = declaredParameterInfos[index];
                object value;

                if (!actionParameters.TryGetValue(parameterInfo.Name, out value))
                {
                    if (parameterInfo.HasDefaultValue)
                    {
                        value = parameterInfo.DefaultValue;
                    }
                    else
                    {
                        var defaultValueAttribute = 
                            parameterInfo.GetCustomAttribute<DefaultValueAttribute>(inherit: false);

                        if (defaultValueAttribute?.Value == null)
                        {
                            value = parameterInfo.ParameterType.GetTypeInfo().IsValueType
                                ? Activator.CreateInstance(parameterInfo.ParameterType)
                                : null;
                        }
                        else
                        {
                            value = defaultValueAttribute.Value;
                        }
                    }
                }

                arguments[index] = value;
            }

            return arguments;
        }
    }
}