// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace ModelBindingWebSite
{
    [ProductValidator]
    public class Product
    {
        public string Name { get; set; }

        [StringLength(20)]
        [Display(Name = "ContactUs")]
        public string Contact { get; set; }

        [Range(0, 10)]
        public virtual int Price { get; set; }

        [CompanyName]
        public string CompanyName { get; set; }

        public string Country { get; set; }

        [Required]
        public ModelWithValidation ProductDetails { get; set; }
    }
}