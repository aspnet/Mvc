// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.Extensions.Localization;

namespace Microsoft.AspNet.Mvc.ModelBinding.Validation
{
    public class CompareAttributeAdapter : DataAnnotationsClientModelValidator<CompareAttribute>
    {
        public CompareAttributeAdapter(CompareAttribute attribute, IStringLocalizer stringLocalizer)
            : base(new CompareAttributeWrapper(attribute, stringLocalizer), stringLocalizer)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException(nameof(attribute));
            }
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules(
            ClientModelValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var errorMessage = ((CompareAttributeWrapper)Attribute).FormatErrorMessage(context);
            var clientRule = new ModelClientValidationEqualToRule(errorMessage,
                                                            FormatPropertyForClientValidation(Attribute.OtherProperty));
            return new[] { clientRule };
        }

        private static string FormatPropertyForClientValidation(string property)
        {
            return "*." + property;
        }

        private sealed class CompareAttributeWrapper : CompareAttribute
        {
            private readonly IStringLocalizer _stringLocalizer;
            public CompareAttributeWrapper(CompareAttribute attribute, IStringLocalizer stringLocalizer)
                : base(attribute.OtherProperty)
            {
                _stringLocalizer = stringLocalizer;

                // Copy settable properties from wrapped attribute. Don't reset default message accessor (set as
                // CompareAttribute constructor calls ValidationAttribute constructor) when all properties are null to
                // preserve default error message. Reset the message accessor when just ErrorMessageResourceType is
                // non-null to ensure correct InvalidOperationException.
                if (!string.IsNullOrEmpty(attribute.ErrorMessage) ||
                    !string.IsNullOrEmpty(attribute.ErrorMessageResourceName) ||
                    attribute.ErrorMessageResourceType != null)
                {
                    ErrorMessage = attribute.ErrorMessage;
                    ErrorMessageResourceName = attribute.ErrorMessageResourceName;
                    ErrorMessageResourceType = attribute.ErrorMessageResourceType;
                }
            }

            public string FormatErrorMessage(ClientModelValidationContext context)
            {
                var otherPropertyDisplayName = GetOtherPropertyDisplayName(context);
                var displayName = context.ModelMetadata.GetDisplayName();
                if (_stringLocalizer != null &&
                    !string.IsNullOrEmpty(ErrorMessage) &&
                    string.IsNullOrEmpty(ErrorMessageResourceName) &&
                    ErrorMessageResourceType == null)
                {
                    return _stringLocalizer[ErrorMessageString, displayName, otherPropertyDisplayName];
                }

                return string.Format(CultureInfo.CurrentCulture,
                                 ErrorMessageString,
                                 displayName,
                                 otherPropertyDisplayName);
            }

            private string GetOtherPropertyDisplayName(ClientModelValidationContext context)
            {
                // The System.ComponentModel.DataAnnotations.CompareAttribute doesn't populate the
                // OtherPropertyDisplayName until after IsValid() is called. Therefore, by the time we get
                // the error message for client validation, the display name is not populated and won't be used.
                var metadata = context.ModelMetadata;
                var otherPropertyDisplayName = OtherPropertyDisplayName;
                if (otherPropertyDisplayName == null && metadata.ContainerType != null)
                {
                    var otherProperty = context.MetadataProvider.GetMetadataForProperty(
                        metadata.ContainerType,
                        OtherProperty);
                    if (otherProperty != null)
                    {
                        return otherProperty.GetDisplayName();
                    }
                }

                return OtherProperty;
            }
        }
    }
}