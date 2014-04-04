using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// A <see cref="IModelValidator"/> to represent an error. This validator will always throw an exception regardless 
    /// of the actual model value.
    /// This is used to perform meta-validation - that is to verify the validation attributes make sense.
    /// </summary>
    public class ErrorModelValidator : IModelValidator
    {
        private readonly string _errorMessage;

        public ErrorModelValidator([NotNull] string errorMessage)
        {
            _errorMessage = errorMessage;
        }

        public bool IsRequired { get { return false; } }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method may perform non-trivial work.")]
        public virtual IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return Enumerable.Empty<ModelClientValidationRule>();
        }

        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            throw new InvalidOperationException(_errorMessage);
        }
    }
}
