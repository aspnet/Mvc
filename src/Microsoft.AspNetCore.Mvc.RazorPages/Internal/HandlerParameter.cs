// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public struct HandlerParameter
    {
        public HandlerParameter(string name, Type type, object defaultValue)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
        }

        public string Name { get; }

        public Type Type { get; }

        public object DefaultValue { get; }
    }
}
