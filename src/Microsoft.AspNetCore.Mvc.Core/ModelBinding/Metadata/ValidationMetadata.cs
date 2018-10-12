// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Metadata
{
    /// <summary>
    /// Validation metadata details for a <see cref="ModelMetadata"/>.
    /// </summary>
    public class ValidationMetadata
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not the model is a required value. Will be ignored
        /// if the model metadata being created is not a property. If <c>null</c> then
        /// <see cref="ModelMetadata.IsRequired"/> will be computed based on the model <see cref="System.Type"/>.
        /// See <see cref="ModelMetadata.IsRequired"/>.
        /// </summary>
        public bool? IsRequired { get; set; }

        /// <summary>
        /// Gets or sets an <see cref="IPropertyValidationFilter"/> implementation that indicates whether this model
        /// should be validated. See <see cref="ModelMetadata.PropertyValidationFilter"/>.
        /// </summary>
        public IPropertyValidationFilter PropertyValidationFilter { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether children of the model should be validated. If <c>null</c>
        /// then <see cref="ModelMetadata.ValidateChildren"/> will be <c>true</c> if either of
        /// <see cref="ModelMetadata.IsComplexType"/> or <see cref="ModelMetadata.IsEnumerableType"/> is <c>true</c>;
        /// <c>false</c> otherwise.
        /// </summary>
        public bool? ValidateChildren { get; set; }

        /// <summary>
        /// Gets a list of metadata items for validators.
        /// </summary>
        /// <remarks>
        /// <see cref="IValidationMetadataProvider"/> implementations should store metadata items
        /// in this list, to be consumed later by an <see cref="Validation.IModelValidatorProvider"/>.
        /// </remarks>
        public IList<object> ValidatorMetadata { get; } = new List<object>();

        /// <summary>
        /// Gets a value that indicates if the model has validators .
        /// </summary>
        public bool? HasValidators { get; set; }
    }
}