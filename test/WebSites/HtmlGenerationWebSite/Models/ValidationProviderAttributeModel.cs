// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations;

namespace HtmlGenerationWebSite.Models
{
    public class ValidationProviderAttributeModel
    {
        [Text]
        public string Name { get; set; }
    }

    public class TextAttribute : ValidationProviderAttribute
    {
        public override IEnumerable<ValidationAttribute> GetValidationAttributes()
        {
            return new ValidationAttribute[]
            {
                new StringLengthAttribute(maximumLength: 10),
                new RegularExpressionAttribute(pattern: @"^[a-zA-Z]+$")
            };
        }
    }
}
