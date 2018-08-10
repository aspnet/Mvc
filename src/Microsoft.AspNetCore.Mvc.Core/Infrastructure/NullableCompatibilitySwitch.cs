// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.Infrastructure
{
    internal class NullableCompatibilitySwitch<TValue> : ICompatibilitySwitch where TValue : struct
    {
        public NullableCompatibilitySwitch(string name)
        {
            Name = name;
        }

        public bool IsValueSet => Value.HasValue;

        public string Name { get; }

        public TValue? Value { get; set; }

        object ICompatibilitySwitch.Value
        {
            get => Value;
            set => Value = (TValue?)value;
        }
    }
}
