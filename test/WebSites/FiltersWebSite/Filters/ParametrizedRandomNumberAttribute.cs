// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc;

namespace FiltersWebSite
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ParametrizedRandomNumberAttribute : ParametrizedFilterAttribute
    {
        public int MinValue { get; set; }

        public int MaxValue { get; set; } = int.MaxValue;
    }
}