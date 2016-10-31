// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// An entry in a <see cref="ModelStateDictionary"/>.
    /// </summary>
    public abstract class ModelStateEntry
    {
        private ModelErrorCollection _errors;

        /// <summary>
        /// Gets the raw value from the request associated with this entry.
        /// </summary>
        public object RawValue { get; set; }

        /// <summary>
        /// Gets the set of values contained in <see cref="RawValue"/>, joined into a comma-separated string.
        /// </summary>
        public string AttemptedValue { get; set; }

        /// <summary>
        /// Gets the <see cref="ModelErrorCollection"/> for this entry.
        /// </summary>
        public ModelErrorCollection Errors
        {
            get
            {
                if (_errors == null)
                {
                    _errors = new ModelErrorCollection();
                }
                return _errors;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ModelValidationState"/> for this entry.
        /// </summary>
        public ModelValidationState ValidationState { get; set; }

        /// <summary>
        /// Gets a value that determines if the current instance of <see cref="ModelStateEntry"/> is a container node.
        /// Container nodes represent prefix nodes that aren't explicitly added to the
        /// <see cref="ModelStateDictionary"/>.
        /// </summary>
        public abstract bool IsContainerNode { get; }

        /// <summary>
        /// Gets the <see cref="ModelStateEntry"/> for a sub-property with the specified <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="propertyName">The property name to lookup.</param>
        /// <returns>The <see cref="ModelStateEntry"/> if a sub-property was found; otherwise <c>null</c>.</returns>
        /// <remarks>
        /// This method returns any existing entry, even those with <see cref="IsContainerNode"/> with value <c>true</c>.
        /// </remarks>
        public abstract ModelStateEntry GetModelStateForProperty(string propertyName);

        /// <summary>
        /// Gets the <see cref="ModelStateEntry"/> values for sub-properties.
        /// </summary>
        /// <remarks>
        /// This property returns all existing entries, even those with <see cref="IsContainerNode"/> with value <c>true</c>.
        /// </remarks>
        public abstract IReadOnlyList<ModelStateEntry> Children { get; }

        public virtual ModelStateEntry GetOrAddModelStateForProperty(
            ModelStateDictionary dictionary,
            string propertyName,
            string key)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return dictionary.GetOrAddModelState(key);
        }

        public virtual ModelValidationState GetFieldValidationState(ModelStateDictionary dictionary, string key)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return dictionary.GetFieldValidationState(key);
        }

        public virtual void SetModelValue(
            ModelStateDictionary dictionary,
            string key,
            ValueProviderResult valueProviderResult)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            dictionary.SetModelValue(key, valueProviderResult);
        }

        public virtual void SetModelValue(
            ModelStateDictionary dictionary,
            string key,
            object rawValue,
            string attemptedValue)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            dictionary.SetModelValue(key, rawValue, attemptedValue);
        }

        public virtual bool TryAddModelError(
            ModelStateDictionary dictionary,
            string key,
            Exception exception,
            ModelMetadata metadata)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            return dictionary.TryAddModelError(key, exception, metadata);
        }

        public virtual bool TryAddModelError(ModelStateDictionary dictionary, string key, string errorMessage)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (errorMessage == null)
            {
                throw new ArgumentNullException(nameof(errorMessage));
            }

            return dictionary.TryAddModelError(key, errorMessage);
        }
    }
}
