﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace PrecompilationWebSite.Models
{
    public class Person
    {
        [Range(10, 100)]
        public int Age { get; set; }
    }
}