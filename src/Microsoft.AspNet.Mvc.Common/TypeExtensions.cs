// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc
{
    internal static class TypeExtensions
    {
        public static TypeInfo ExtractGenericInterface([NotNull] this Type queryType, Type interfaceType)
        {
            Func<TypeInfo, bool> matchesInterface = 
                typeInfo => typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == interfaceType;
            var queryTypeInfo = queryType.GetTypeInfo();

            if (matchesInterface(queryTypeInfo))
            {
                return queryTypeInfo;
            }
            else
            {
                return queryTypeInfo
                    .ImplementedInterfaces
                    .Select(type => type.GetTypeInfo())
                    .FirstOrDefault(matchesInterface);
            }
        }

        public static bool IsCompatibleWith([NotNull] this Type type, object value)
        {
            return (value == null && AllowsNullValue(type)) ||
                (value != null && type.GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo()));
        }

        public static bool IsNullableValueType([NotNull] this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public static bool AllowsNullValue([NotNull] this Type type)
        {
            return (!type.GetTypeInfo().IsValueType || IsNullableValueType(type));
        }
    }
}
