﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class DataAnnotationsModelValidator : IModelValidator
    {
        public DataAnnotationsModelValidator([NotNull] ValidationAttribute attribute, [NotNull] ModelMetadata metadata)
        {
            Attribute = attribute;
            Metadata = metadata;
        }

        public ValidationAttribute Attribute { get; private set; }

        public ModelMetadata Metadata { get; set; }

        public bool IsRequired
        {
            get { return Attribute is RequiredAttribute; }
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            IEnumerable<ModelClientValidationRule> results = null;

            var clientValidatable = Attribute as IClientValidatable;
            if (clientValidatable != null)
            {
                results = clientValidatable.GetClientValidationRules(Metadata);
            }

            return results ?? Enumerable.Empty<ModelClientValidationRule>();
        }

        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext validationContext)
        {
            var metadata = validationContext.ModelMetadata;
            var memberName = metadata.PropertyName ?? metadata.ModelType.Name;
            var instance = metadata.Model ?? validationContext.ContainerMetadata.Model;
            var context = new ValidationContext(instance)
            {
                DisplayName = metadata.GetDisplayName(),
                MemberName = memberName
            };

            var result = Attribute.GetValidationResult(metadata.Model, context);
            if (result != ValidationResult.Success)
            {
                // ModelValidationResult.MemberName is used by invoking validators (such as ModelValidator) to 
                // construct the ModelKey for ModelStateDictionary. When validating at type level we want to append the 
                // returned MemberNames if specified (e.g. person.Address.FirstName). For property validation, the 
                // ModelKey can be constructed using the ModelMetadata and we should ignore MemberName (we don't want 
                // (person.Name.Name). However the invoking validator does not have a way to distinguish between these two 
                // cases. Consequently we'll only set MemberName if this validation returns a MemberName that is different
                // from the property being validated.

                var errorMemberName = result.MemberNames.FirstOrDefault();
                if (string.Equals(errorMemberName, memberName, StringComparison.Ordinal))
                {
                    errorMemberName = null;
                }

                var validationResult = new ModelValidationResult(errorMemberName, result.ErrorMessage);
                return new ModelValidationResult[] { validationResult };
            }

            return Enumerable.Empty<ModelValidationResult>();
        }
    }
}
