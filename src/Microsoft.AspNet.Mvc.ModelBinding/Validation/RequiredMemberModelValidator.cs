using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class RequiredMemberModelValidator : IModelValidator
    {
        public bool IsRequired
        {
            get { return true; }
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method may perform non-trivial work.")]
        public virtual IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return Enumerable.Empty<ModelClientValidationRule>();
        }

        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            return Enumerable.Empty<ModelValidationResult>();
        }
    }
}
