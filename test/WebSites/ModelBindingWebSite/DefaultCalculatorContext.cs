﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace ModelBindingWebSite
{
    public class DefaultCalculatorContext : CalculatorContext
    {
        public int Precision { get; set; }
    }
}