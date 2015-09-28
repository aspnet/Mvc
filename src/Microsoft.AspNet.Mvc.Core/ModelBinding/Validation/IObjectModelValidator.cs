// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding.Validation
{
    /// <summary>
    /// Provides methods to validate an object graph.
    /// </summary>
    public interface IObjectModelValidator
    {
        /// <summary>
        /// Validates the provided object.
        /// </summary>
        /// <param name="validatorProvider">The <see cref="IModelValidatorProvider"/>.</param>
        /// <param name="modelState">The <see cref="ModelStateDictionary"/>.</param>
        /// <param name="validationState">The <see cref="ValidationStateDictionary"/>. May be null.</param>
        /// <param name="prefix">
        /// The model prefix. Used to map the model object to entries in <paramref name="modelState"/>.
        /// </param>
        /// <param name="model">The model object.</param>
        void Validate(
            IModelValidatorProvider validatorProvider,
            ModelStateDictionary modelState,
            ValidationStateDictionary validationState,
            string prefix,
            object model);
    }
}
