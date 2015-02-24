// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Internal;
using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Utility class for clearing entries in <see cref="ModelStateDictionary"/>.
    /// </summary>
    public static class ModelStateDictionaryUtility
    {
        /// <summary>
        /// Clears <see cref="ModelStateDictionary"/>.
        /// </summary>
        /// <param name="modelState">The <see cref="ModelStateDictionary"/>.</param>
        /// <param name="type">The <see cref="Type"/> of object whose properties entries have to be cleared from
        /// <see cref="ModelStateDictionary"/>.</param>
        public static void ClearModelStateDictionary([NotNull] ModelStateDictionary modelState, [NotNull] Type type)
        {
            var properties = type.GetProperties();
            if (properties.Count() > 0)
            {
                foreach (var p in properties)
                {
                    if (IsSimpleType(p.PropertyType))
                    {
                        ClearModelStateDictionaryEntry(modelState, p.Name);
                    }
                    else
                    {
                        ClearModelStateDictionary(modelState, p.PropertyType);
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Clears <see cref="ModelStateDictionary"/>.
        /// </summary>
        /// <param name="modelState">The <see cref="ModelStateDictionary"/>.</param>
        /// <param name="name">The key of <see cref="ModelStateDictionary"/> entry to be cleared.</param>
        public static void ClearModelStateDictionaryEntry(
            [NotNull] ModelStateDictionary modelState,
            [NotNull] string name)
        {
            if (modelState.ContainsKey(name))
            {
                modelState[name].Errors.Clear();
                modelState[name].ValidationState = ModelValidationState.Unvalidated;
            }
        }

        private static bool IsSimpleType(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return
                typeInfo.IsValueType || typeInfo.IsPrimitive ||
                new Type[] {
                typeof(String),
                typeof(Decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(Guid)
                }.Contains(type);
        }
    }
}