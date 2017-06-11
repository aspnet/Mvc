// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Validation
{
    /// <summary>
    /// A context for <see cref="IModelValidatorProvider"/>.
    /// </summary>
    public class ModelValidatorProviderContext
    {
        /// <summary>
        /// Creates a new <see cref="ModelValidatorProviderContext"/>.
        /// </summary>
        /// <param name="modelMetadata">The <see cref="ModelBinding.ModelMetadata"/>.</param>
        /// <param name="items">The list of <see cref="ValidatorItem"/>s.</param>
        public ModelValidatorProviderContext(ModelMetadata modelMetadata, IList<ValidatorItem> items)
        {
            ModelMetadata = modelMetadata;
            Results = items;
        }

        /// <summary>
        /// Gets the <see cref="ModelBinding.ModelMetadata"/>.
        /// </summary>
        public ModelMetadata ModelMetadata { get; set; }

        /// <summary>
        /// Gets the validator metadata.
        /// </summary>
        /// <remarks>
        /// This property provides convenience access to <see cref="ModelMetadata.ValidatorMetadata"/>.
        /// </remarks>
        public IReadOnlyList<object> ValidatorMetadata => ModelMetadata.ValidatorMetadata;

        /// <summary>
        /// Gets the list of <see cref="ValidatorItem"/> instances. <see cref="IModelValidatorProvider"/> instances
        /// should add the appropriate <see cref="ValidatorItem.Validator"/> properties when
        /// <see cref="IModelValidatorProvider.CreateValidators(ModelValidatorProviderContext)"/>
        /// is called.
        /// </summary>
        public IList<ValidatorItem> Results { get; }
    }
}