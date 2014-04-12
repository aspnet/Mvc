using System.Runtime.CompilerServices;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelClientValidationRegexRule : ModelClientValidationRule
    {
        private const string ValidationType = "regex";

        public ModelClientValidationRegexRule(string errorMessage, string pattern)
            : base(ValidationType, errorMessage)
        {
            ValidationParameters.Add("pattern", pattern);
        }
    }
}
