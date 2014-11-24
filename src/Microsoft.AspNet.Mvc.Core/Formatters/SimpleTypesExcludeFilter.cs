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
        private static HashSet<Type> _collections = null;
        static SimpleTypesExcludeFilter()
        {
            _collections = new HashSet<Type>()
            {
                typeof(List<>),
                typeof(Dictionary<,>),
                typeof(LinkedList<>),
                typeof(SortedSet<>),
                typeof(SortedDictionary<,>),
                typeof(Queue<>),
                typeof(Stack<>),
                typeof(HashSet<>)
            };
        }

        /// <summary>
        /// Returns true if the given type will be excluded from the default model validation. 
        /// </summary>
        public bool IsTypeExcluded(Type type)
        {
            Type actualType = null;
            if (type.IsArray)
            {
                actualType = type.GetElementType();
            }
            else if (type.IsGenericType && _collections.Contains(type.GetGenericTypeDefinition()))
            {
                var types = type.GenericTypeArguments;
                actualType = types[types.Length - 1];
            }
            else
            {
                actualType = type;
            }

            if(actualType.IsNullableValueType())
            {
                actualType = Nullable.GetUnderlyingType(actualType);
            }

            return IsSimpleType(actualType);
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