using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ModelBindingWebSite
{
    public class ProductValidatorAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var objectType = value.GetType();

            var neededProperties =
              objectType.GetProperties()
              .Where(propertyInfo => propertyInfo.Name == "Name" || propertyInfo.Name == "Country")
              .ToArray();

            if (neededProperties.Count() != 2)
            {
                return new ValidationResult("Could not find Name or Country properties");
            }

            var name = Convert.ToString(neededProperties.FirstOrDefault(p => p.Name == "Name").GetValue(value, null));
            var country = Convert.ToString(neededProperties.FirstOrDefault(p => p.Name == "Country").GetValue(value, null));

            if (!country.Equals("USA"))
            {
                return new ValidationResult("Country property does not have the right value");
            }

            if (string.IsNullOrEmpty(name))
            {
                return new ValidationResult("Name property cannot be empty");
            }

            return null;
        }
    }
}