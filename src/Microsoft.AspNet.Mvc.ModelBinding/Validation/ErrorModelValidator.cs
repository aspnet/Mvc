using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ModelBinding.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// A <see cref="ModelValidator"/> to represent an error. This validator will always throw an exception regardless of the actual model value.
    /// </summary>
    public class ErrorModelValidator : IModelValidator
    {
        private readonly string _errorMessage;

        public ErrorModelValidator(string errorMessage)
        {
            if (errorMessage == null)
            {
                throw Error.ArgumentNull("errorMessage");
            }

            _errorMessage = errorMessage;
        }

        public bool IsRequired { get { return false; } }

        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            throw new InvalidOperationException(_errorMessage);
        }
    }
}
