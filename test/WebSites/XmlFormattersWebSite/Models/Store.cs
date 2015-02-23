﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace XmlFormattersWebSite
{
    public class Store
    {
        [Required]
        public int Id { get; set; }

        public List<Customer> Customers { get; set; }

        public Address Address { get; set; }
    }
}