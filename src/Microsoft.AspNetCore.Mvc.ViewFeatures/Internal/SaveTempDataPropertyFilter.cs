// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public class SaveTempDataPropertyFilter : ISaveTempDataCallback, IActionFilter
    {
        private readonly ITempDataDictionaryFactory _factory;

        public SaveTempDataPropertyFilter(ITempDataDictionaryFactory factory)
        {
            _factory = factory;
        }

        public string Prefix = "TempDataProperty-";

        public object Subject { get; set; }

        public IDictionary<PropertyInfo, object> OriginalValues { get; set; }

        internal IList<PropertyHelper> PropertyHelpers { get; set; }

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
            var properties = PropertyHelpers.Select(p => p.Property).ToArray();
            OriginalValues = new Dictionary<PropertyInfo, object>();

            foreach (var property in properties)
            {
                var value = tempData[Prefix + property.Name];

                OriginalValues[property] = value;

                var propertyInfo = property.PropertyType.GetTypeInfo();

                if (value != null)
                {
                    property.SetValue(Subject, value);
                }

                else if (propertyInfo.IsGenericTypeDefinition &&
                    propertyInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    property.SetValue(Subject, null);
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}

