// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Identifies the simple types that the default model binding validation will exclude. 
    /// </summary>
    public class SimpleTypesExcludeFilter : IExcludeTypeValidationFilter
    {
        /// <summary>
        /// Returns true if the given type will be excluded from the default model validation. 
        /// </summary>
        public bool IsTypeExcluded(Type type)
        {
            Type[] actualTypes;

            var enumerable = type.ExtractGenericInterface(typeof(IEnumerable<>));
            if (null == enumerable)
            {
                actualTypes = new Type[] { type };
            }
            else
            {
                actualTypes = enumerable.GenericTypeArguments;

                if (actualTypes[0].IsGenericType()
                    && actualTypes.Length == 1
                    && actualTypes[0].GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                {
                    actualTypes = actualTypes[0].GenericTypeArguments;
                }
            }

            foreach (var actualType in actualTypes)
            {
                var underlingType = actualType.IsNullableValueType() ? Nullable.GetUnderlyingType(actualType)
                                                                       : actualType;

                if (!IsSimpleType(underlingType))
                {
                    return false;
                }
            }

            return true;
        }

        protected virtual bool IsSimpleType(Type type)
        {
            var result = type.GetTypeInfo().IsPrimitive ||
                            type.Equals(typeof(decimal)) ||
                            type.Equals(typeof(string)) ||
                            type.Equals(typeof(DateTime)) ||
                            type.Equals(typeof(Guid)) ||
                            type.Equals(typeof(DateTimeOffset)) ||
                            type.Equals(typeof(TimeSpan));

            return result;
        }
    }
}