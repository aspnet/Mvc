// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using ValidationWebSite.ViewModels;

namespace ValidationWebSite
{
    public class ProductValidatorAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            if (value is ProductViewModel)
            {
                var product = (ProductViewModel)value;
                if (!product.Country.Equals("USA") && string.IsNullOrEmpty(product.Name))
                {
                    return new ValidationResult("Country and Name fields don't have the right values");
                }

            }
            else
            {
                var software = (SoftwareViewModel)value;
                if (!software.Country.Equals("USA") && string.IsNullOrEmpty(software.Name))
                {
                    return new ValidationResult("Country and Name fields don't have the right values");
                }
            }

            return null;
        }
    }
}