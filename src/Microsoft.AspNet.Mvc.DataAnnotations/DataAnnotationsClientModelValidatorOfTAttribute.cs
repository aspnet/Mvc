// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Framework.Localization;

namespace Microsoft.AspNet.Mvc.ModelBinding.Validation
{
    /// <summary>
    /// An implementation of <see cref="IClientModelValidator"/> which understands data annotation attributes.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    public abstract class DataAnnotationsClientModelValidator<TAttribute> : IClientModelValidator
        where TAttribute : ValidationAttribute
    {
        /// <summary>
        /// Create a new instance of <see cref="DataAnnotationsClientModelValidator{TAttribute}"/>.
        /// </summary>
        /// <param name="attribute">The <typeparamref name="TAttribute"/> instance to validate.</param>
        /// <param name="stringLocalizer">The <see cref="IStringLocalizer"/>.</param>
        public DataAnnotationsClientModelValidator(TAttribute attribute, IStringLocalizer stringLocalizer)
        {
            Attribute = attribute;
            StringLocalizer = stringLocalizer;
        }

        /// <summary>
        /// Gets the <typeparamref name="TAttribute"/> instance.
        /// </summary>
        public TAttribute Attribute
        {
            get;
        }

        /// <summary>
        /// Gets the <see cref="IStringLocalizer"/>.
        /// </summary>
        public IStringLocalizer StringLocalizer
        {
            get;
        }

        /// <inheritdoc />
        public abstract IEnumerable<ModelClientValidationRule> GetClientValidationRules(
            ClientModelValidationContext context);

        /// <summary>
        /// Gets the error message formatted using the <see cref="Attribute"/>.
        /// </summary>
        /// <param name="modelMetadata">The <see cref="ModelMetadata"/> associated with the model annotated with
        /// <see cref="Attribute"/>.</param>
        /// <returns>Formatted error string.</returns>
        protected virtual string GetErrorMessage(ModelMetadata modelMetadata)
        {
            if (modelMetadata == null)
            {
                throw new ArgumentNullException(nameof(modelMetadata));
            }

            if (StringLocalizer != null)
            {
                return StringLocalizer[modelMetadata.GetDisplayName()];
            }
            
            return Attribute.FormatErrorMessage(modelMetadata.GetDisplayName());
        }
    }
}
