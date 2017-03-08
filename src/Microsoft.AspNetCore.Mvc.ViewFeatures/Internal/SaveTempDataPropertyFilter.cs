// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    /// <summary>
    /// A filter that saves properties with the <see cref="TempDataAttribute"/>. 
    /// </summary>
    public class SaveTempDataPropertyFilter : ISaveTempDataCallback, IActionFilter
    {
        private readonly ITempDataDictionaryFactory _factory;

        internal IList<PropertyHelper> PropertyHelpers { get; set; }

        public SaveTempDataPropertyFilter(ITempDataDictionaryFactory factory)
        {
            _factory = factory;
        }

        public static readonly string Prefix = "TempDataProperty-";

        public object Subject { get; set; }

        public IDictionary<PropertyInfo, object> OriginalValues { get; set; }

        public void OnTempDataSaving(ITempDataDictionary tempData)
        {
            if (Subject != null && OriginalValues != null)
            {
                foreach (var kvp in OriginalValues)
                {
                    var property = kvp.Key;
                    var originalValue = kvp.Value;

                    var newValue = property.GetValue(Subject);
                    if (newValue != null && !newValue.Equals(originalValue))
                    {
                        tempData[Prefix + property.Name] = newValue;
                    }
                }
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            Subject = context.Controller;
            var tempData = _factory.GetTempData(context.HttpContext);
            var properties = GetSubjectProperties();
            OriginalValues = new Dictionary<PropertyInfo, object>();

            foreach (var property in properties)
            {
                var value = tempData[Prefix + property.Name];

                OriginalValues[property] = value;

                // TODO: Clarify what behavior should be for null values here
                if (value != null && property.PropertyType.IsAssignableFrom(value.GetType()))
                {
                    property.SetValue(Subject, value);
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        private IEnumerable<PropertyInfo> GetSubjectProperties()
        {
            var properties = new List<PropertyInfo>();
            foreach (var propertyHelper in PropertyHelpers)
            {
                if (!(propertyHelper.Property.SetMethod != null &&
                    propertyHelper.Property.SetMethod.IsPublic &&
                    propertyHelper.Property.GetMethod != null &&
                    propertyHelper.Property.GetMethod.IsPublic))
                {
                    throw new InvalidOperationException("TempData properties must have a public getter and setter.");
                }

                if (!(propertyHelper.Property.PropertyType.GetTypeInfo().IsPrimitive || propertyHelper.Property.PropertyType == typeof(string)))
                {
                    throw new InvalidOperationException("TempData properties must be declared as primitive types or string only.");
                }

                else
                {
                    properties.Add(propertyHelper.Property);
                }
            }

            return properties;
        }
    }
}

