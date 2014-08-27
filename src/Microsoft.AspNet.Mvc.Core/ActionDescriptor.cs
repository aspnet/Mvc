// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Routing;

namespace Microsoft.AspNet.Mvc
{
    public class ActionDescriptor
    {
        public ActionDescriptor()
        {
            ExtensionData = new Dictionary<Type, object>();
            RouteValueDefaults = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public virtual string Name { get; set; }

        public List<RouteDataActionConstraint> RouteConstraints { get; set; }

        public AttributeRouteInfo AttributeRouteInfo { get; set; }

        public Dictionary<string, object> RouteValueDefaults { get; private set; }

        public List<HttpMethodConstraint> MethodConstraints { get; set; }

        public List<IActionConstraint> DynamicConstraints { get; set; }

        public List<ParameterDescriptor> Parameters { get; set; }

        public List<FilterDescriptor> FilterDescriptors { get; set; }

        /// <summary>
        /// A friendly name for this action.
        /// </summary>
        public virtual string DisplayName { get; set; }

        /// <summary>
        /// Stores arbitrary extension metadata associated with the <see cref="ActionDescriptor"/>.
        /// </summary>
        public IDictionary<Type, object> ExtensionData { get; private set; }

        /// <summary>
        /// Gets the value of an extension data from the <see cref="ExtensionData"/> collection.
        /// </summary>
        /// <typeparam name="T">The type of the extension data.</typeparam>
        /// <returns>The extension data or the default value of <typeparamref name="T"/>.</returns>
        public T GetExtension<T>()
        {
            object value;
            if (ExtensionData.TryGetValue(typeof(T), out value))
            {
                return (T)value;
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Sets the value of an extension data in the <see cref="ExtensionData"/> collection.
        /// </summary>
        /// <typeparam name="T">The type of the extension data.</typeparam>
        /// <param name="value">The value of an extension data.</param>
        public void SetExtension<T>([NotNull] T value)
        {
            ExtensionData[typeof(T)] = value;
        }
    }
}
