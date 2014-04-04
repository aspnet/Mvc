﻿#if NET45
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ValidatableObjectAdapter : IModelValidator
    {
        public bool IsRequired
        {
            get { return false; }
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method may perform non-trivial work.")]
        public virtual IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return Enumerable.Empty<ModelClientValidationRule>();
        }

        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            var model = context.ModelMetadata.Model;
            if (model == null)
            {
                return Enumerable.Empty<ModelValidationResult>();
            }

            var validatable = model as IValidatableObject;
            if (validatable == null)
            {
                var message = Resources.FormatValidatableObjectAdapter_IncompatibleType(
                                    typeof(IValidatableObject).Name,
                                    model.GetType());

                throw new InvalidOperationException(message);
            }

            var validationContext = new ValidationContext(validatable, serviceProvider: null, items: null);
            return ConvertResults(validatable.Validate(validationContext));
        }

        private IEnumerable<ModelValidationResult> ConvertResults(IEnumerable<ValidationResult> results)
        {
            foreach (var result in results)
            {
                if (result != ValidationResult.Success)
                {
                    if (result.MemberNames == null || !result.MemberNames.Any())
                    {
                        yield return new ModelValidationResult(memberName: null, message: result.ErrorMessage);
                    }
                    else
                    {
                        foreach (var memberName in result.MemberNames)
                        {
                            yield return new ModelValidationResult(memberName, result.ErrorMessage);
                        }
                    }
                }
            }
        }
    }
}
#endif