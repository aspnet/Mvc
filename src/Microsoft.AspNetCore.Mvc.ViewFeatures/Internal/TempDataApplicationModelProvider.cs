// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public class TempDataApplicationModelProvider : IApplicationModelProvider
    {
        /// <inheritdoc />
        public int Order { get { return -1000 + 10; } }

        /// <inheritdoc />
        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
        }

        /// <inheritdoc />
        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var controllerModel in context.Result.Controllers)
            {
                SaveTempDataPropertyFilterFactory provider = null;
                var propertyHelpers = PropertyHelper.GetVisibleProperties(controllerModel.ControllerType.AsType());
                for (int i = 0; i < propertyHelpers.Length; i++)
                {
                    if (propertyHelpers[i].Property.GetCustomAttribute<TempDataAttribute>() != null
                        && ValidateProperty(propertyHelpers[i]))
                    {
                        if (provider == null)
                        {
                            provider = new SaveTempDataPropertyFilterFactory()
                            {
                                TempDataProperties = new List<PropertyHelper>()
                            };
                        }

                        provider.TempDataProperties.Add(propertyHelpers[i]);
                    }
                }

                if (provider != null)
                {
                    controllerModel.Filters.Add(provider);
                }
            }
        }

        private bool ValidateProperty(PropertyHelper propertyHelper)
        {
            var property = propertyHelper.Property;
            if (!(property.SetMethod != null &&
                property.SetMethod.IsPublic &&
                property.GetMethod != null &&
                property.GetMethod.IsPublic))
            {
                throw new InvalidOperationException(
                    Resources.FormatTempDataProperties_PublicGetterSetter(property.Name));
            }

            if (!(property.PropertyType.GetTypeInfo().IsPrimitive || property.PropertyType == typeof(string)))
            {
                throw new InvalidOperationException(
                    Resources.FormatTempDataProperties_PrimitiveTypeOrString(property.Name));
            }

            else
            {
                return true;
            }
        }
    }
}
