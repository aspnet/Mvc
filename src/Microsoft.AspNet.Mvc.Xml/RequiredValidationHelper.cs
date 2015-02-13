// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc.Xml
{
    /// <summary>
    /// Validates types having value type properties decorated with <see cref="RequiredAttribute"/>
    /// but no <see cref="DataMemberAttribute"/>.
    /// </summary>
    public static class RequiredValidationHelper
    {
        private static ConcurrentDictionary<Type, Dictionary<Type, List<string>>> cachedValidationErrors
            = new ConcurrentDictionary<Type, Dictionary<Type, List<string>>>();

        public static void Validate([NotNull] Type modelType, [NotNull] ModelStateDictionary modelStateDictionary)
        {
            var visitedTypes = new HashSet<Type>();
            var rootNodeErrorDictionary = new Dictionary<Type, List<string>>();

            Validate(modelType, visitedTypes, rootNodeErrorDictionary);

            foreach (var errorKeyPair in rootNodeErrorDictionary)
            {
                foreach (var validationErrorMessage in errorKeyPair.Value)
                {
                    modelStateDictionary.TryAddModelError(
                        errorKeyPair.Key.FullName,
                        validationErrorMessage);
                }
            }
        }

        private static void Validate(
            Type modelType,
            HashSet<Type> visitedTypes,
            Dictionary<Type, List<string>> currentNodeErrorDictionary)
        {
            if (modelType.IsGenericType())
            {
                var enumerableOfT = modelType.ExtractGenericInterface(typeof(IEnumerable<>));
                if (enumerableOfT != null)
                {
                    modelType = enumerableOfT.GetGenericArguments()[0];
                }
            }

            if (ExcludeTypeFromValidation(modelType))
            {
                return;
            }

            // Avoid infinite loop in case of self-referencing properties
            if (visitedTypes.Contains(modelType))
            {
                return;
            }

            visitedTypes.Add(modelType);

            Dictionary<Type, List<string>> cachedCurrentNodeErrorDictionary;
            if (cachedValidationErrors.TryGetValue(modelType, out cachedCurrentNodeErrorDictionary))
            {
                foreach (var error in cachedCurrentNodeErrorDictionary)
                {
                    currentNodeErrorDictionary.Add(error.Key, error.Value);
                }

                return;
            }

            foreach (var propertyInfo in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (propertyInfo.PropertyType.IsValueType() && !propertyInfo.PropertyType.IsNullableValueType())
                {
                    var validationError = GetValidationError(propertyInfo);
                    if (validationError == null)
                    {
                        continue;
                    }

                    List<string> errors;
                    if (!currentNodeErrorDictionary.TryGetValue(validationError.Value.ModelType, out errors))
                    {
                        errors = new List<string>();
                        currentNodeErrorDictionary.Add(validationError.Value.ModelType, errors);
                    }
                    errors.Add(Resources.FormatRequiredProperty_MustHaveDataMemberRequired(
                                    validationError.Value.PropertyName,
                                    validationError.Value.ModelType.FullName));
                }
                else
                {
                    var childNodeErrorDictionary = new Dictionary<Type, List<string>>();

                    Validate(propertyInfo.PropertyType, visitedTypes, childNodeErrorDictionary);

                    // Avoid adding duplicate errors at current node.
                    // Example: 'Store' type has a 'Address' property and also has list of 
                    // 'Employee', which in turn has 'Address' property. From the 'Store' type
                    // level, we want to avoid duplicate validation errors for 'Address'. 
                    foreach (var modelTypeKey in childNodeErrorDictionary.Keys)
                    {
                        if (!currentNodeErrorDictionary.ContainsKey(modelTypeKey))
                        {
                            currentNodeErrorDictionary.Add(modelTypeKey, childNodeErrorDictionary[modelTypeKey]);
                        }
                    }
                }
            }

            cachedValidationErrors.TryAdd(modelType, currentNodeErrorDictionary);

            visitedTypes.Remove(modelType);
        }

        private static ValidationError? GetValidationError(PropertyInfo propertyInfo)
        {
            var required = propertyInfo.GetCustomAttribute(typeof(RequiredAttribute), inherit: true);
            if (required == null)
            {
                return null;
            }

            var hasDataMemberRequired = false;

            var dataMemberRequired = (DataMemberAttribute)propertyInfo.GetCustomAttribute(
                typeof(DataMemberAttribute),
                inherit: true);

            if (dataMemberRequired != null && dataMemberRequired.IsRequired)
            {
                hasDataMemberRequired = true;
            }

            if (!hasDataMemberRequired)
            {
                return new ValidationError()
                {
                    ModelType = propertyInfo.DeclaringType,
                    PropertyName = propertyInfo.Name
                };
            }

            return null;
        }

        private static bool ExcludeTypeFromValidation(Type modelType)
        {
            return modelType.IsValueType()
                || modelType.IsNullableValueType();
        }

        public struct ValidationError
        {
            public Type ModelType { get; set; }

            public string PropertyName { get; set; }
        }
    }
}
