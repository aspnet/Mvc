// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
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
        /// Gets a list of metadata items for validators.
        /// </summary>
        /// <remarks>
        /// <see cref="IValidationMetadataProvider"/> implementations should store metadata items
        /// in this list, to be consumed later by an <see cref="Validation.IModelValidatorProvider"/>.
        /// </remarks>
        public IList<object> ValidatorMetadata { get; } = new List<object>();
    }
}