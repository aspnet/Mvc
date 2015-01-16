// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace ModelBindingWebSite
{
    public class CompanyNameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            object instance = validationContext.ObjectInstance;
            var property = instance.GetType().GetProperty("CompanyName");
            var companyName = property.GetValue(instance);

            if (companyName == null)
            {
                return new ValidationResult("CompanyName cannot be null");
            }
            return null;
        }
    }
}