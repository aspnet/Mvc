// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// Provides access to the combined list of attributes associated with a <see cref="Type"/>, property, or parameter.
    /// </summary>
    public class ModelAttributes
    {
        private static readonly IEnumerable<object> _emptyAttributesCollection = Enumerable.Empty<object>();

        /// <summary>
        /// Creates a new <see cref="ModelAttributes"/> for a <see cref="Type"/>.
        /// </summary>
        /// <param name="typeAttributes">The set of attributes for the <see cref="Type"/>.</param>
        [Obsolete("This constructor is obsolete and will be removed in a future version. The recommended alternative is " + nameof(ModelAttributes) + "." + nameof(GetAttributesForType) + ".")]
        public ModelAttributes(IEnumerable<object> typeAttributes)
            : this(typeAttributes, null, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ModelAttributes"/> for a property.
        /// </summary>
        /// <param name="propertyAttributes">The set of attributes for the property.</param>
        /// <param name="typeAttributes">
        /// The set of attributes for the property's <see cref="Type"/>. See <see cref="PropertyInfo.PropertyType"/>.
        /// </param>
        [Obsolete("This constructor is obsolete and will be removed in a future version. The recommended alternative is " + nameof(ModelAttributes) + "." + nameof(GetAttributesForProperty) + ".")]
        public ModelAttributes(IEnumerable<object> propertyAttributes, IEnumerable<object> typeAttributes)
            : this(typeAttributes, propertyAttributes, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ModelAttributes"/>.
        /// </summary>
        /// <param name="typeAttributes">
        /// If this instance represents a type, the set of attributes for that type.
        /// If this instance represents a property, the set of attributes for the property's <see cref="Type"/>.
        /// Otherwise, <c>null</c>.
        /// </param>
        /// <param name="propertyAttributes">
        /// If this instance represents a property, the set of attributes for that property.
        /// Otherwise, <c>null</c>.
        /// </param>
        /// <param name="parameterAttributes">
        /// If this instance represents a parameter, the set of attributes for that parameter.
        /// Otherwise, <c>null</c>.
        /// </param>
        internal ModelAttributes(
            IEnumerable<object> typeAttributes,
            IEnumerable<object> propertyAttributes,
            IEnumerable<object> parameterAttributes)
        {
            if (propertyAttributes != null)
            {
                // Represents a property
                if (typeAttributes == null)
                {
                    throw new ArgumentNullException(nameof(typeAttributes));
                }

                PropertyAttributes = propertyAttributes.ToArray();
                TypeAttributes = typeAttributes.ToArray();
                Attributes = PropertyAttributes.Concat(TypeAttributes).ToArray();
            }
            else if (parameterAttributes != null)
            {
                // Represents a parameter
                if (typeAttributes == null)
                {
                    throw new ArgumentNullException(nameof(typeAttributes));
                }

                ParameterAttributes = parameterAttributes.ToArray();
                TypeAttributes = typeAttributes.ToArray();
                Attributes = ParameterAttributes.Concat(TypeAttributes).ToArray();
            }
            else if (typeAttributes != null)
            {
                // Represents a type
                if (typeAttributes == null)
                {
                    throw new ArgumentNullException(nameof(typeAttributes));
                }

                Attributes = TypeAttributes = typeAttributes.ToArray();
            }
        }

        /// <summary>
        /// Gets the set of all attributes. If this instance represents the attributes for a property, the attributes
        /// on the property definition are before those on the property's <see cref="Type"/>. If this instance
        /// represents the attributes for a parameter, the attributes on the parameter definition are before those on
        /// the parameter's <see cref="Type"/>.
        /// </summary>
        public IReadOnlyList<object> Attributes { get; }

        /// <summary>
        /// Gets the set of attributes on the property, or <c>null</c> if this instance does not represent the attributes
        /// for a property.
        /// </summary>
        public IReadOnlyList<object> PropertyAttributes { get; }

        /// <summary>
        /// Gets the set of attributes on the parameter, or <c>null</c> if this instance does not represent the attributes
        /// for a parameter.
        /// </summary>
        public IReadOnlyList<object> ParameterAttributes { get; }

        /// <summary>
        /// Gets the set of attributes on the <see cref="Type"/>. If this instance represents a property, then
        /// <see cref="TypeAttributes"/> contains attributes retrieved from <see cref="PropertyInfo.PropertyType"/>.
        /// If this instance represents a parameter, then contains attributes retrieved from
        /// <see cref="ParameterInfo.ParameterType"/>.
        /// </summary>
        public IReadOnlyList<object> TypeAttributes { get; }

        /// <summary>
        /// Gets the attributes for the given <paramref name="property"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> in which caller found <paramref name="property"/>.
        /// </param>
        /// <param name="property">A <see cref="PropertyInfo"/> for which attributes need to be resolved.
        /// </param>
        /// <returns>
        /// A <see cref="ModelAttributes"/> instance with the attributes of the property and its <see cref="Type"/>.
        /// </returns>
        public static ModelAttributes GetAttributesForProperty(Type type, PropertyInfo property)
        {
            return GetAttributesForProperty(type, property, property.PropertyType);
        }

        /// <summary>
        /// Gets the attributes for the given <paramref name="property"/> with the specified <paramref name="modelType"/>.
        /// </summary>
        /// <param name="containerType">The <see cref="Type"/> in which caller found <paramref name="property"/>.
        /// </param>
        /// <param name="property">A <see cref="PropertyInfo"/> for which attributes need to be resolved.
        /// </param>
        /// <param name="modelType">The model type</param>
        /// <returns>
        /// A <see cref="ModelAttributes"/> instance with the attributes of the property and its <see cref="Type"/>.
        /// </returns>
        public static ModelAttributes GetAttributesForProperty(Type containerType, PropertyInfo property, Type modelType)
        {
            if (containerType == null)
            {
                throw new ArgumentNullException(nameof(containerType));
            }

            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            var propertyAttributes = property.GetCustomAttributes();
            var typeAttributes = modelType.GetCustomAttributes();

            var metadataType = GetMetadataType(containerType);
            if (metadataType != null)
            {
                var metadataProperty = metadataType.GetRuntimeProperty(property.Name);
                if (metadataProperty != null)
                {
                    propertyAttributes = propertyAttributes.Concat(metadataProperty.GetCustomAttributes());
                }
            }

            return new ModelAttributes(typeAttributes, propertyAttributes, parameterAttributes: null);
        }

        /// <summary>
        /// Gets the attributes for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> for which attributes need to be resolved.
        /// </param>
        /// <returns>A <see cref="ModelAttributes"/> instance with the attributes of the <see cref="Type"/>.</returns>
        public static ModelAttributes GetAttributesForType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var attributes = type.GetCustomAttributes();

            var metadataType = GetMetadataType(type);
            if (metadataType != null)
            {
                attributes = attributes.Concat(metadataType.GetCustomAttributes());
            }

            return new ModelAttributes(attributes, propertyAttributes: null, parameterAttributes: null);
        }

        /// <summary>
        /// Gets the attributes for the given <paramref name="parameterInfo"/>.
        /// </summary>
        /// <param name="parameterInfo">
        /// The <see cref="ParameterInfo"/> for which attributes need to be resolved.
        /// </param>
        /// <returns>
        /// A <see cref="ModelAttributes"/> instance with the attributes of the parameter and its <see cref="Type"/>.
        /// </returns>
        public static ModelAttributes GetAttributesForParameter(ParameterInfo parameterInfo)
        {
            // Prior versions called IModelMetadataProvider.GetMetadataForType(...) and therefore
            // GetAttributesForType(...) for parameters. Maintain that set of attributes (including those from an
            // ModelMetadataTypeAttribute reference) for back-compatibility.
            var typeAttributes = GetAttributesForType(parameterInfo.ParameterType).TypeAttributes;
            var parameterAttributes = parameterInfo.GetCustomAttributes();

            return new ModelAttributes(typeAttributes, propertyAttributes: null, parameterAttributes);
        }

        /// <summary>
        /// Gets the attributes for the given <paramref name="parameterInfo"/> with the specified <paramref name="modelType"/>.
        /// </summary>
        /// <param name="parameterInfo">
        /// The <see cref="ParameterInfo"/> for which attributes need to be resolved.
        /// </param>
        /// <param name="modelType">The model type.</param>
        /// <returns>
        /// A <see cref="ModelAttributes"/> instance with the attributes of the parameter and its <see cref="Type"/>.
        /// </returns>
        public static ModelAttributes GetAttributesForParameter(ParameterInfo parameterInfo, Type modelType)
        {
            if (parameterInfo == null)
            {
                throw new ArgumentNullException(nameof(parameterInfo));
            }

            if (modelType == null)
            {
                throw new ArgumentNullException(nameof(modelType));
            }

            // Prior versions called IModelMetadataProvider.GetMetadataForType(...) and therefore
            // GetAttributesForType(...) for parameters. Maintain that set of attributes (including those from an
            // ModelMetadataTypeAttribute reference) for back-compatibility.
            var typeAttributes = GetAttributesForType(modelType).TypeAttributes;
            var parameterAttributes = parameterInfo.GetCustomAttributes();

            return new ModelAttributes(typeAttributes, propertyAttributes: null, parameterAttributes);
        }

        private static Type GetMetadataType(Type type)
        {
            return type.GetCustomAttribute<ModelMetadataTypeAttribute>()?.MetadataType;
        }
    }
}
