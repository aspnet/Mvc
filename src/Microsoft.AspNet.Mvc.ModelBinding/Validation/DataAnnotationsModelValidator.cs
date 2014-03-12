﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class DataAnnotationsModelValidator : IModelValidator
    {
        public DataAnnotationsModelValidator(ValidationAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }

            Attribute = attribute;
        }

        public ValidationAttribute Attribute { get; private set; }

        public bool IsRequired
        {
            get { return Attribute is RequiredAttribute; }
        }

        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext validationContext)
        {
            var metadata = validationContext.ModelMetadata;
            var memberName = metadata.PropertyName ?? metadata.ModelType.Name;
            var context = new ValidationContext(metadata.Model)
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

                var validationResult = new ModelValidationResult
                {
                    Message = result.ErrorMessage,
                    MemberName = errorMemberName
                };

                return new ModelValidationResult[] { validationResult };
            }

            return Enumerable.Empty<ModelValidationResult>();
        }
    }
}
