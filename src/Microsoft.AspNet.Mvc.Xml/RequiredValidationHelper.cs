// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
    public class DataAnnotationRequiredAttributeValidation
    {
        private ConcurrentDictionary<Type, Dictionary<Type, List<string>>> _cachedValidationErrors
            = new ConcurrentDictionary<Type, Dictionary<Type, List<string>>>();

        public void Validate([NotNull] Type modelType, [NotNull] ModelStateDictionary modelStateDictionary)
        {
            var visitedTypes = new HashSet<Type>();

            // Every node maintains a dictionary of Type => Errors. 
            // It's a dictionary as we want to avoid adding duplicate error messages.
            // Example:
            // In the following case, from the perspective of type 'Store', we should not see duplicate
            // errors related to type 'Address'
            // public class Store
            // {
            //    [Required]
            //    public int Id { get; set; }
            //    public Address Address { get; set; }
            // }
            // public class Employee
            // {
            //    [Required]
            //    public int Id { get; set; }
            //    public Address Address { get; set; }
            // }
            // public class Address
            // {
            //    [Required]
            //    public string Line1 { get; set; }
            //    [Required]
            //    public int Zipcode { get; set; }
            //    [Required]
            //    public string State { get; set; }
            // }
            var rootNodeValidationErrors = new Dictionary<Type, List<string>>();

            Validate(modelType, visitedTypes, rootNodeValidationErrors);

            foreach (var validationError in rootNodeValidationErrors)
            {
                foreach (var validationErrorMessage in validationError.Value)
                {
                    modelStateDictionary.TryAddModelError(
                        validationError.Key.FullName,
                        validationErrorMessage);
                }
            }
        }

        private void Validate(
            Type modelType,
            HashSet<Type> visitedTypes,
            Dictionary<Type, List<string>> errors)
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
            if (!visitedTypes.Add(modelType))
            {
                return;
            }

            Dictionary<Type, List<string>> cachedErrors;
            if (_cachedValidationErrors.TryGetValue(modelType, out cachedErrors))
            {
                foreach (var validationError in cachedErrors)
                {
                    errors.Add(validationError.Key, validationError.Value);
                }

                return;
            }

            foreach (var propertyHelper in PropertyHelper.GetProperties(modelType))
            {
                var propertyInfo = propertyHelper.Property;
                var propertyType = propertyInfo.PropertyType;

                if (propertyType.IsValueType() && !propertyType.IsNullableValueType())
                {
                    // Scenarios:
                    // a. [Required]
                    //    public int Id { get; set;}
                    // b. [Required]
                    //    public Point Coordinate { get; set;}
                    // c. public int Id { get; set;}
                    // d. public Point Coordinate { get; set;}
                    var validationError = GetValidationError(propertyInfo);
                    if (validationError != null)
                    {
                        List<string> errorMessages;
                        if (!errors.TryGetValue(validationError.Value.ModelType, out errorMessages))
                        {
                            errorMessages = new List<string>();
                            errors.Add(validationError.Value.ModelType, errorMessages);
                        }

                        errorMessages.Add(Resources.FormatRequiredProperty_MustHaveDataMemberRequired(
                                        validationError.Value.PropertyName,
                                        validationError.Value.ModelType.FullName));
                    }
                    
                    // if the type is not primitve, then it could be a struct in which case
                    // we need to probe its properties for validation
                    if (propertyType.GetTypeInfo().IsPrimitive)
                    {
                        continue;
                    }
                }

                var childNodeErrors = new Dictionary<Type, List<string>>();
                Validate(propertyType, visitedTypes, childNodeErrors);

                // Avoid adding duplicate errors at current node.
                foreach (var modelTypeKey in childNodeErrors.Keys)
                {
                    if (!errors.ContainsKey(modelTypeKey))
                    {
                        errors.Add(modelTypeKey, childNodeErrors[modelTypeKey]);
                    }
                }
            }

            _cachedValidationErrors.TryAdd(modelType, errors);

            visitedTypes.Remove(modelType);
        }

        private ValidationError? GetValidationError(PropertyInfo propertyInfo)
        {
            var required = propertyInfo.GetCustomAttribute(typeof(RequiredAttribute), inherit: true);
            if (required == null)
            {
                return null;
            }

            var dataMemberRequired = (DataMemberAttribute)propertyInfo.GetCustomAttribute(
                typeof(DataMemberAttribute),
                inherit: true);

            if (dataMemberRequired != null && dataMemberRequired.IsRequired)
            {
                return null;
            }

            return new ValidationError()
            {
                ModelType = propertyInfo.DeclaringType,
                PropertyName = propertyInfo.Name
            };
        }

        private bool ExcludeTypeFromValidation(Type modelType)
        {
            return modelType.GetTypeInfo().IsPrimitive ||
                            modelType.Equals(typeof(decimal)) ||
                            modelType.Equals(typeof(string)) ||
                            modelType.Equals(typeof(DateTime)) ||
                            modelType.Equals(typeof(Guid)) ||
                            modelType.Equals(typeof(DateTimeOffset)) ||
                            modelType.Equals(typeof(TimeSpan)) ||
                            modelType.Equals(typeof(Uri));
        }

        private struct ValidationError
        {
            public Type ModelType { get; set; }

            public string PropertyName { get; set; }
        }
    }
}
