// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public static class ParameterDefaultValues
    {
        public static object[] GetParameterDefaultValues(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            var parameters = methodInfo.GetParameters();
            var values = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                values[i] = GetParameterDefaultValue(parameters[i], considerDefaultValueAttribute: true);
            }

            return values;
        }

        public static object GetParameterDefaultValue(ParameterInfo parameterInfo, bool considerDefaultValueAttribute)
        {
            // In a scenario where the method signature is like "void Foo(Guid id = default(Guid))", 
            // HasDefaultValue returns true, but the DefaultValue is null. This happens only in case of Reflection,
            // whereas during execution time, calling the method "Foo()" does indeed supply the default value of
            // 00000000-0000-0000-0000-000000000000 to the parameter "id".
            if (parameterInfo.HasDefaultValue && parameterInfo.DefaultValue != null)
            {
                return parameterInfo.DefaultValue;
            }
            else
            {
                object defaultValue = null;
                if (considerDefaultValueAttribute)
                {
                    var defaultValueAttribute = parameterInfo.GetCustomAttribute<DefaultValueAttribute>(inherit: false);
                    defaultValue = defaultValueAttribute?.Value;
                }

                if (defaultValue == null && parameterInfo.ParameterType.GetTypeInfo().IsValueType)
                {
                    defaultValue = Activator.CreateInstance(parameterInfo.ParameterType);
                }

                return defaultValue;
            }
        }
    }
}
