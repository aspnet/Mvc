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

            // Every node maintains a dictionary of Type => Errors. 
            // It's a dictionary as we want to avoid adding duplicate error messages.
            // Example:
            // Type 'A' has a child node of type 'B' somewhere in the left sub-tree
            // and it has type 'B' again  somewhere down the righ sub-tree. From the perspective of
            // type 'A', it should not have duplicate error messgaes for type 'B'.
            var rootNodeValidationErrors = new Dictionary<Type, List<string>>();

            Validate(modelType, visitedTypes, rootNodeValidationErrors);

            foreach (var validatoinError in rootNodeValidationErrors)
            {
                foreach (var validationErrorMessage in validatoinError.Value)
                {
                    modelStateDictionary.TryAddModelError(
                        validatoinError.Key.FullName,
                        validationErrorMessage);
                }
            }
        }

        private static void Validate(
            Type modelType,
            HashSet<Type> visitedTypes,
            Dictionary<Type, List<string>> currentNodeValidationErrors)
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

            Dictionary<Type, List<string>> cachedCurrentNodeValidationErrors;
            if (cachedValidationErrors.TryGetValue(modelType, out cachedCurrentNodeValidationErrors))
            {
                foreach (var validationError in cachedCurrentNodeValidationErrors)
                {
                    currentNodeValidationErrors.Add(validationError.Key, validationError.Value);
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

                    List<string> errorMessages;
                    if (!currentNodeValidationErrors.TryGetValue(validationError.Value.ModelType, out errorMessages))
                    {
                        errorMessages = new List<string>();
                        currentNodeValidationErrors.Add(validationError.Value.ModelType, errorMessages);
                    }
                    errorMessages.Add(Resources.FormatRequiredProperty_MustHaveDataMemberRequired(
                                    validationError.Value.PropertyName,
                                    validationError.Value.ModelType.FullName));
                }
                else
                {
                    var childNodeValidationErrors = new Dictionary<Type, List<string>>();

                    Validate(propertyInfo.PropertyType, visitedTypes, childNodeValidationErrors);

                    // Avoid adding duplicate errors at current node.
                    foreach (var modelTypeKey in childNodeValidationErrors.Keys)
                    {
                        if (!currentNodeValidationErrors.ContainsKey(modelTypeKey))
                        {
                            currentNodeValidationErrors.Add(modelTypeKey, childNodeValidationErrors[modelTypeKey]);
                        }
                    }
                }
            }

            cachedValidationErrors.TryAdd(modelType, currentNodeValidationErrors);

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
