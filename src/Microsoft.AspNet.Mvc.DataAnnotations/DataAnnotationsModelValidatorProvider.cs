// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Localization;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc.ModelBinding.Validation
{
    /// <summary>
    /// An implementation of <see cref="IModelValidatorProvider"/> which provides validators
    /// for attributes which derive from <see cref="ValidationAttribute"/>. It also provides
    /// a validator for types which implement <see cref="IValidatableObject"/>.
    /// </summary>
    public class DataAnnotationsModelValidatorProvider : IModelValidatorProvider
    {
        private IOptions<MvcDataAnnotationsLocalizationOptions> _options;
        private IStringLocalizerFactory _stringLocalizerFactory;

        public DataAnnotationsModelValidatorProvider(
            IOptions<MvcDataAnnotationsLocalizationOptions> options,
            IStringLocalizerFactory stringLocalizerFactory)
        {
            _options = options;
            _stringLocalizerFactory = stringLocalizerFactory;
        }

        public void GetValidators(ModelValidatorProviderContext context)
        {
            IStringLocalizer stringLocalizer = null;
            if (_options != null &&
                _options.Value.DataAnnotationLocalizerProvider != null &&
                _stringLocalizerFactory != null)
            {
                stringLocalizer = _options.Value.DataAnnotationLocalizerProvider(
                    context.ModelMetadata.ContainerType ?? context.ModelMetadata.ModelType,
                    _stringLocalizerFactory);
            }

            foreach (var attribute in context.ValidatorMetadata.OfType<ValidationAttribute>())
            {
                if (stringLocalizer != null &&
                    !string.IsNullOrEmpty(attribute.ErrorMessage) &&
                    string.IsNullOrEmpty(attribute.ErrorMessageResourceName) &&
                    attribute.ErrorMessageResourceType == null)
                {
                    context.Validators.Add(new DataAnnotationsModelValidator(attribute, stringLocalizer));
                }
                else
                {
                    context.Validators.Add(new DataAnnotationsModelValidator(attribute));
                }
            }

            // Produce a validator if the type supports IValidatableObject
            if (typeof(IValidatableObject).IsAssignableFrom(context.ModelMetadata.ModelType))
            {
                context.Validators.Add(new ValidatableObjectAdapter());
            }
        }
    }
}
