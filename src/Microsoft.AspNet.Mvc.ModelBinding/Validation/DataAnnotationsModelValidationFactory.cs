using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    // A factory for validators based on ValidationAttribute
    public delegate IModelValidator DataAnnotationsModelValidationFactory(ValidationAttribute attribute);
}