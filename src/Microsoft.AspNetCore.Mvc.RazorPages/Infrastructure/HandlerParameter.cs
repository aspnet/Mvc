// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure
{
    public class HandlerParameter : ParameterDescriptor
    {
        public HandlerParameter(string name, Type parameterType, object defaultValue)
        {
            Name = name;
            ParameterType = parameterType;
            DefaultValue = defaultValue;
        }

        public object DefaultValue { get; set; }
    }
}
