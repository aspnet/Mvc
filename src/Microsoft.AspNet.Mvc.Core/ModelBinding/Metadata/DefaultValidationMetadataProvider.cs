// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
{
    /// <summary>
    /// A default implementation of <see cref="IValidationMetadataProvider"/>.
    /// </summary>
    public class DefaultValidationMetadataProvider : IValidationMetadataProvider
    {
        private readonly ValidationErrorMetadata _validationErrorMetadata;

        public DefaultValidationMetadataProvider(ValidationErrorMetadata validationErrorMetadata)
        {
            _validationErrorMetadata = validationErrorMetadata;
        }

        /// <inheritdoc />
        public void GetValidationMetadata([NotNull] ValidationMetadataProviderContext context)
        {
            context.ValidationMetadata.ValidationErrorMetadata = _validationErrorMetadata;

            foreach (var attribute in context.Attributes)
            {
                if (attribute is IModelValidator || attribute is IClientModelValidator)
                {
                    // If another provider has already added this attribute, do not repeat it.
                    // This will prevent attributes like RemoteAttribute (which implement ValidationAttribute and
                    // IClientModelValidator) to be added to the ValidationMetadata twice.
                    // This is to ensure we do not end up with duplication validation rules on the client side.
                    if (!context.ValidationMetadata.ValidatorMetadata.Contains(attribute))
                    {
                        context.ValidationMetadata.ValidatorMetadata.Add(attribute);
                    }
                }
            }
        }
    }
}