// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public readonly struct BoundPropertyManager
    {
        public static readonly BoundPropertySource[] DefaultAllowedSources = new[]
        {
            BoundPropertySource.TempData,
            BoundPropertySource.ViewData,
        };

        private BoundPropertyManager(IReadOnlyList<PropertyItem> propertyItems)
        {
            PropertyItems = propertyItems;
        }

        // Internal for unit testing
        internal IReadOnlyList<PropertyItem> PropertyItems { get; }

        public static BoundPropertyManager Create(
            MvcViewOptions options,
            Type type,
            IReadOnlyList<BoundPropertySource> allowedSources = null,
            bool requireViewDataPropertySetters = false)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            allowedSources = allowedSources ?? DefaultAllowedSources;

            var tempDataPrefix = options.SuppressTempDataPropertyPrefix ?
                string.Empty :
                "TempDataProperty-";

            var properties = GetProperties(type, allowedSources, tempDataPrefix, requireViewDataPropertySetters);
            return new BoundPropertyManager(properties ?? Array.Empty<PropertyItem>());
        }

        public void Populate(object instance, BoundPropertyContext context)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            for (var i = 0; i < PropertyItems.Count; i++)
            {
                var propertyItem = PropertyItems[i];
                var propertyName = propertyItem.PropertyHelper.Name;

                object value;
                switch (propertyItem.Source)
                {
                    case BoundPropertySource.TempData:
                        value = context.TempData[propertyItem.SourceKey];
                        break;
                    case BoundPropertySource.ViewData:
                        value = context.ViewData[propertyItem.SourceKey];
                        break;
                    default:
                        throw new InvalidOperationException(Resources.FormatUnsupportedEnumType(propertyItem.Source));
                }

                if (value == null)
                {
                    continue;
                }

                propertyItem.PropertyHelper.SetValue(instance, value);
            }
        }

        public void Save(object instance, BoundPropertyContext context)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            for (var i = 0; i < PropertyItems.Count; i++)
            {
                var propertyItem = PropertyItems[i];
                var key = propertyItem.SourceKey;

                var currentValue = propertyItem.PropertyHelper.GetValue(instance);
                if (propertyItem.Source == BoundPropertySource.TempData)
                {
                    var originalValue = context.TempData[key];
                    if (currentValue != null && !currentValue.Equals(originalValue))
                    {
                        context.TempData[key] = currentValue;
                        // Mark the key to be kept. This ensures that even if something later in the execution pipeline reads it,
                        // such as another view with a `TempData` property, the key is preserved through the current request.
                        context.TempData.Keep(key);
                    }
                }
                else if (propertyItem.Source == BoundPropertySource.ViewData)
                {
                    var originalValue = context.ViewData[key];
                    if (currentValue != null && !currentValue.Equals(originalValue))
                    {
                        context.ViewData[key] = currentValue;
                    }
                }
            }
        }

        private static IReadOnlyList<PropertyItem> GetProperties(
            Type type,
            IReadOnlyList<BoundPropertySource> allowedSources,
            string tempDataPrefix,
            bool requireViewDataPropertySetters)
        {
            List<PropertyItem> propertyItems = null;
            var propertyHelpers = PropertyHelper.GetVisibleProperties(type);
            for (var i = 0; i < propertyHelpers.Length; i++)
            {
                var propertyHelper = propertyHelpers[i];
                var property = propertyHelper.Property;

                var customAttributes = property.GetCustomAttributes(inherit: false);
                for (var j = 0; j < customAttributes.Length; j++)
                {
                    var attribute = customAttributes[j];
                    string key;
                    BoundPropertySource lifetimeKind;
                    if (attribute is ViewDataAttribute viewData && IsAllowed(BoundPropertySource.ViewData))
                    {
                        ValidateViewDataProperty(property, requireViewDataPropertySetters);

                        key = viewData.Key ?? property.Name;
                        lifetimeKind = BoundPropertySource.ViewData;
                    }
                    else if (attribute is TempDataAttribute tempData && IsAllowed(BoundPropertySource.TempData))
                    {
                        ValidateTempDataProperty(property);

                        key = tempData.Key ?? tempDataPrefix + property.Name;
                        lifetimeKind = BoundPropertySource.TempData;
                    }
                    else
                    {
                        continue;
                    }

                    propertyItems = propertyItems ?? new List<PropertyItem>();
                    propertyItems.Add(new PropertyItem(propertyHelper, lifetimeKind, key));
                }
            }

            return propertyItems;

            bool IsAllowed(BoundPropertySource source)
            {
                for (var i = 0; i < allowedSources.Count; i++)
                {
                    if (allowedSources[i] == source)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private static void ValidateViewDataProperty(PropertyInfo property, bool requireViewDataPropertySetters)
        {
            if (requireViewDataPropertySetters && (property.SetMethod == null || !property.SetMethod.IsPublic))
            {
                var attributeName = nameof(ViewDataAttribute);
                var propertyTypeName = property.DeclaringType.FullName;
                throw new InvalidOperationException(
                    Resources.FormatProperty_MustHaveAPublicSetter(property.DeclaringType.FullName, property.Name, attributeName));
            }
        }

        private static void ValidateTempDataProperty(PropertyInfo property)
        {
            var attributeName = nameof(TempDataAttribute);
            var propertyTypeName = property.DeclaringType.FullName;

            if (property.SetMethod == null || !property.SetMethod.IsPublic)
            {
                throw new InvalidOperationException(
                    Resources.FormatProperty_MustHaveAPublicSetter(property.DeclaringType.FullName, property.Name, attributeName));
            }

            var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            if (!TempDataSerializer.CanSerializeType(propertyType, out var errorMessage))
            {
                var messageWithPropertyInfo = Resources.FormatTempDataProperties_InvalidType(
                    propertyTypeName,
                    property.Name,
                    attributeName);

                throw new InvalidOperationException($"{messageWithPropertyInfo} {errorMessage}");
            }
        }

        // Internal for unit testing
        internal readonly struct PropertyItem
        {
            public PropertyItem(
                PropertyHelper propertyHelper,
                BoundPropertySource source,
                string sourceKey)
            {
                PropertyHelper = propertyHelper;
                Source = source;
                SourceKey = sourceKey;
            }

            public PropertyHelper PropertyHelper { get; }

            public BoundPropertySource Source { get; }

            public string SourceKey { get; }
        }
    }
}
