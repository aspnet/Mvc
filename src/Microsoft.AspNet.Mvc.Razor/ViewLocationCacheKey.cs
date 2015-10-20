// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNet.Mvc.Razor
{
    public struct ViewLocationCacheKey : IEquatable<ViewLocationCacheKey>
    {
        public ViewLocationCacheKey(
            string viewName,
            string controllerName,
            string areaName,
            bool isPartial,
            IReadOnlyDictionary<string, string> values)
        {
            ViewName = viewName;
            ControllerName = controllerName;
            AreaName = areaName;
            IsPartial = isPartial;
            ViewLocationExpanderValues = values;
        }

        public string ViewName { get; }

        public string ControllerName { get; }

        public string AreaName { get; }

        public bool IsPartial { get; }

        public IReadOnlyDictionary<string, string> ViewLocationExpanderValues { get; }

        public bool Equals(ViewLocationCacheKey y)
        {
            if (IsPartial != y.IsPartial ||
                !string.Equals(ViewName, y.ViewName, StringComparison.Ordinal) ||
                !string.Equals(ControllerName, y.ControllerName, StringComparison.Ordinal) ||
                !string.Equals(AreaName, y.AreaName, StringComparison.Ordinal))
            {
                return false;
            }

            if (ReferenceEquals(ViewLocationExpanderValues, y.ViewLocationExpanderValues))
            {
                return true;
            }

            if (ViewLocationExpanderValues == null ||
                y.ViewLocationExpanderValues == null ||
                (ViewLocationExpanderValues.Count != y.ViewLocationExpanderValues.Count))
            {
                return false;
            }

            foreach (var item in ViewLocationExpanderValues)
            {
                string yValue;
                if (!y.ViewLocationExpanderValues.TryGetValue(item.Key, out yValue) ||
                    !string.Equals(item.Value, yValue, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(ViewLocationCacheKey key)
        {
            var hashCodeCombiner = HashCodeCombiner.Start();
            hashCodeCombiner.Add(key.IsPartial ? 1 : 0);
            hashCodeCombiner.Add(key.ViewName, StringComparer.Ordinal);
            hashCodeCombiner.Add(key.ControllerName, StringComparer.Ordinal);
            hashCodeCombiner.Add(key.AreaName, StringComparer.Ordinal);

            if (key.ViewLocationExpanderValues != null)
            {
                foreach (var item in key.ViewLocationExpanderValues)
                {
                    hashCodeCombiner.Add(item.Key, StringComparer.Ordinal);
                    hashCodeCombiner.Add(item.Value, StringComparer.Ordinal);
                }
            }

            return hashCodeCombiner;
        }
    }
}
