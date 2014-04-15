using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// A factory for validators based on ValidationAttribute.
    /// </summary>
    public delegate IModelValidator DataAnnotationsModelValidationFactory(ValidationAttribute attribute);
}