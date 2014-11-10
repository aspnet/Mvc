// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    public class SimpleTypesExcludeFilter : IExcludeTypeValidationFilter
    {
        public bool IsTypeExcluded(Type type)
        {
            Type actualType = null;

            if (type.IsArray)
            {
                actualType = type.GetElementType();
            }
            else
            {
                actualType = type;
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