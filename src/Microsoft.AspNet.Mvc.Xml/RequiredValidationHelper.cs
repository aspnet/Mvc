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
        private static ConcurrentDictionary<Type, List<ValidationError>> cachedValidationErrors
            = new ConcurrentDictionary<Type, List<ValidationError>>();

        public static void Validate([NotNull] Type modelType, [NotNull] ModelStateDictionary modelStateDictionary)
        {
            var errors = cachedValidationErrors.GetOrAdd(modelType, (type) =>
            {
                var visitedTypes = new HashSet<Type>();
                var validationErrors = new List<ValidationError>();

                Validate(modelType, visitedTypes, validationErrors);

                return validationErrors;
            });

            foreach (var error in errors)
            {
                modelStateDictionary.TryAddModelError(
                    error.ModelType.FullName,
                    Resources.FormatRequiredProperty_MustHaveDataMemberRequired(
                                            error.PropertyName,
                                            error.ModelType.FullName));
            }
        }

        private static void Validate(
            Type modelType,
            HashSet<Type> visitedTypes,
            List<ValidationError> rootModelTypeErrors)
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

            var valueTypeProperties = new List<PropertyInfo>();
            var referenceTypeProperties = new List<PropertyInfo>();
            foreach (var propertyInfo in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (propertyInfo.PropertyType.IsValueType())
                {
                    if (!propertyInfo.PropertyType.IsNullableValueType())
                    {
                        valueTypeProperties.Add(propertyInfo);
                    }
                }
                else
                {
                    referenceTypeProperties.Add(propertyInfo);
                }
            }

            var currentModelTypeValidationErrors = GetValidationErrors(valueTypeProperties);

            // Avoid duplicate error messages.
            // Example: For the following setup, when probing the 'Store' type, we do not want to
            // get duplicate error messages for the 'Address' type. 
            // -'Address' type has required filed errors. 
            // -'Employee' type has a property of type 'Address'.
            // -'Store' type has a property of type 'Address' and also has a property of list of 'Employee'.
            if (currentModelTypeValidationErrors.Count > 0)
            {
                if (!rootModelTypeErrors.Any(error => error.ModelType == modelType))
                {
                    rootModelTypeErrors.AddRange(currentModelTypeValidationErrors);
                }
            }

            visitedTypes.Add(modelType);

            // reference properties
            foreach (var propertyInfo in referenceTypeProperties)
            {
                Validate(propertyInfo.PropertyType, visitedTypes, rootModelTypeErrors);
            }
        }

        private static List<ValidationError> GetValidationErrors(IEnumerable<PropertyInfo> valueTypeProperties)
        {
            var validationErrors = new List<ValidationError>();

            foreach (var propertyInfo in valueTypeProperties)
            {
                var required = propertyInfo.GetCustomAttribute(typeof(RequiredAttribute), inherit: true);
                if (required == null)
                {
                    continue;
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
                    validationErrors.Add(new ValidationError()
                    {
                        ModelType = propertyInfo.DeclaringType,
                        PropertyName = propertyInfo.Name
                    });
                }
            }

            return validationErrors;
        }

        private static bool ExcludeTypeFromValidation(Type modelType)
        {
            return modelType.IsValueType()
                || modelType.IsNullableValueType();
        }

        private struct ValidationError
        {
            public Type ModelType { get; set; }

            public string PropertyName { get; set; }
        }
    }
}