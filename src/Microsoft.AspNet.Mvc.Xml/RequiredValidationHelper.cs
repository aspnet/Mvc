// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc.Xml
{
    public static class RequiredValidationHelper
    {
        public static void Validate(Type modelType, ModelStateDictionary modelStateDictionary)
        {
            Validate(modelType, modelStateDictionary, new HashSet<Type>());
        }

        private static void Validate(
            Type modelType,
            ModelStateDictionary modelStateDictionary,
            HashSet<Type> visited)
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
            if (visited.Contains(modelType))
            {
                return;
            }

            var referenceTypeProperties = new List<PropertyInfo>();
            foreach (var property in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.PropertyType.IsValueType() && !property.PropertyType.IsNullableValueType())
                {
                    var required = property.GetCustomAttribute(typeof(RequiredAttribute), inherit: true);
                    if (required == null)
                    {
                        continue;
                    }

                    var hasDataMemberRequired = false;

                    var dataMemberRequired = (DataMemberAttribute)property.GetCustomAttribute(
                        typeof(DataMemberAttribute),
                        inherit: true);

                    if (dataMemberRequired != null && dataMemberRequired.IsRequired)
                    {
                        hasDataMemberRequired = true;
                    }

                    if (!hasDataMemberRequired)
                    {
                        var propertyType = property.DeclaringType;
                        modelStateDictionary.TryAddModelError(
                            propertyType.FullName,
                            Resources.FormatRequiredProperty_MustHaveDataMemberRequired(
                                property.Name,
                                propertyType.FullName));
                    }
                }
                else
                {
                    referenceTypeProperties.Add(property);
                }
            }

            visited.Add(modelType);

            foreach (var referenceTypeProperty in referenceTypeProperties)
            {
                Validate(referenceTypeProperty.PropertyType, modelStateDictionary, visited);
            }
        }

        private static bool ExcludeTypeFromValidation(Type modelType)
        {
            return modelType.IsValueType()
                || modelType.IsNullableValueType();
        }
    }
}