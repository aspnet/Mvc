namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelClientValidationEqualToRule : ModelClientValidationRule
    {
        private const string EqualToValidationType = "equalto";
        private const string EqualToValidationParameter = "other";

        public ModelClientValidationEqualToRule([NotNull] string errorMessage, 
                                                [NotNull] object other)
            : base(EqualToValidationType, errorMessage)
        {
            ValidationParameters[EqualToValidationParameter] = other;
        }
    }
}
