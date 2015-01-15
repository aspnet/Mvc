// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace FormatterWebSite
{
    public class AdminValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var objectType = value.GetType();

            var neededProperties =
              objectType.GetProperties()
              .Where(propertyInfo => propertyInfo.Name == "Designation" || propertyInfo.Name == "AdminAccessCode")
              .ToArray();

            if (neededProperties.Count() != 2)
            {
                return new ValidationResult("Could not find Designation and AdminAccessCode properties");
            }

            var adminAccessCode = Convert.ToInt32(neededProperties[0].GetValue(value, null));
            var designation = Convert.ToString(neededProperties[1].GetValue(value, null));

            if (string.IsNullOrEmpty(designation) || !designation.Equals("Administrator"))
            {
                return new ValidationResult("Designation property does not have the right value");
            }

            if (adminAccessCode != 1)
            {
                return new ValidationResult ("AdminAccessCode property does not have the right value");
            }

            return null;
        }
    }
}