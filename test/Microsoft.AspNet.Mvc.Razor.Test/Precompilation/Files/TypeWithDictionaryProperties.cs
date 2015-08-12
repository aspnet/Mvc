// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.Razor.Precompilation
{
    public class TypeWithDictionaryProperties
    {
        public IDictionary<string, string> RouteValues1 { get; set; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<int, string> RouteValues2 { get; set; } =
            new Dictionary<int, string>();

        public IReadOnlyDictionary<List<string>, float> RouteValues3 { get; set; } =
            new Dictionary<List<string>, float>();

        public IDictionary<string, CustomType> CustomDictionary { get; set; } =
            new Dictionary<string, CustomType>();

        public IDictionary NonGenericDictionary { get; set; } =
            new Dictionary<string, string>();

        public object ObjectType { get; set; } =
            new Dictionary<string, string>();
    }

    public class CustomType
    {
    }
}
