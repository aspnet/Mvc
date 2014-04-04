using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public interface IModelValidator
    {
        bool IsRequired { get; }

        IEnumerable<ModelClientValidationRule> GetClientValidationRules();

        IEnumerable<ModelValidationResult> Validate(ModelValidationContext context);
    }
}
