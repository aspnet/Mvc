using System.ComponentModel.DataAnnotations;
using ValidationWebSite.Models;

namespace ValidationWebSite
{
    public class ProductValidatorAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var product = (Product)value;

            if (!product.Country.Equals("USA") && string.IsNullOrEmpty(product.Name))
            {
                return new ValidationResult("Country and Name fields don't have the right values");
            }

            return null;
        }
    }
}