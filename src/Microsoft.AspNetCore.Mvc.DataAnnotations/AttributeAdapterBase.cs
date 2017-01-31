// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

namespace Microsoft.AspNetCore.Mvc.DataAnnotations
{
    /// <summary>
    /// An abstract subclass of <see cref="ValidationAttributeAdapter{TAttribute}"/> which wraps up all the required
    /// interfaces for the adapters.
    /// </summary>
    /// <typeparam name="TAttribute">The type of <see cref="ValidationAttribute"/> which is being wrapped.</typeparam>
    public abstract class AttributeAdapterBase<TAttribute> :
        ValidationAttributeAdapter<TAttribute>,
        IAttributeAdapter
        where TAttribute : ValidationAttribute
    {
        /// <summary>
        /// Instantiates a new <see cref="AttributeAdapterBase{TAttribute}"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="ValidationAttribute"/> being wrapped.</param>
        /// <param name="stringLocalizer">The <see cref="IStringLocalizer"/> to be used in error generation.</param>
        public AttributeAdapterBase(TAttribute attribute, IStringLocalizer stringLocalizer)
            : base(attribute, stringLocalizer)
        {
        }

        /// <inheritdoc/>
        public abstract string GetErrorMessage(ModelValidationContextBase validationContext);
    }
}
