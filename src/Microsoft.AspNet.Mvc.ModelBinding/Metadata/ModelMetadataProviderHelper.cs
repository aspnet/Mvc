// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Provides static methods which can be used to get a combined list of attributes which are associated
    /// with a parameter or property.
    /// </summary>
    public static class ModelMetadataProviderHelper
    {
        /// <summary>
        /// Gets the attributes for the given <paramref name="parameter"/>.
        /// </summary>
        /// <param name="parameter">Represents a <see cref="ParameterInfo"/> for which attributes need to be resolved.
        /// </param>
        /// <returns>An <see cref="IEnumerable{object}"/> which represents the attributes on the
        /// <paramref name="parameter"/> before the attributes on the <paramref name="parameter"/> type.</returns>
        public static IEnumerable<object> GetAttributesForParameter(ParameterInfo parameter)
        {
            // Return the list in sorted order parameter attributes first.
            var parameterAttributes = parameter.GetCustomAttributes();
            var typeAttributes = parameter.ParameterType.GetTypeInfo().GetCustomAttributes();

            return parameterAttributes.Concat(typeAttributes);
        }

        /// <summary>
        /// Gets the attributes for the given <paramref name="property"/>.
        /// </summary>
        /// <param name="property">Represents a <see cref="ParameterInfo"/> for which attributes need to be resolved.
        /// </param>
        /// <returns>An <see cref="IEnumerable{object}"/> which represents the attributes on the
        /// <paramref name="property"/> before the attributes on the <paramref name="property"/> type.</returns>
        public static IEnumerable<object> GetAttributesForProperty(PropertyInfo property)
        {
            // Return the list in sorted order property attributes first.
            var propertyAttributes = property.GetCustomAttributes();
            var typeAttributes = property.PropertyType.GetTypeInfo().GetCustomAttributes();

            return propertyAttributes.Concat(typeAttributes);
        }
    }
}
