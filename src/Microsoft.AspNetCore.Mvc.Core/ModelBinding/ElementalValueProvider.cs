// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    internal class ElementalValueProvider : IValueProvider
    {
        public ElementalValueProvider(string key, string value, CultureInfo culture)
        {
            Key = key;
            Value = value;
            Culture = culture;
        }

        public CultureInfo Culture { get; }

        public string Key { get; }

        public string Value { get; }

        public bool ContainsPrefix(string prefix)
        {
            return ModelStateDictionary.StartsWithPrefix(prefix, Key);
        }

        public ValueProviderResult GetValue(string key)
        {
            if (string.Equals(key, Key, StringComparison.OrdinalIgnoreCase))
            {
                return new ValueProviderResult(Value, Culture);
            }
            else
            {
                return ValueProviderResult.None;
            }
        }
    }
}
