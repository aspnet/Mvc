// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace XmlFormattersWebSite
{
    public class Customer
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public Address Address { get; set; }
    }
}