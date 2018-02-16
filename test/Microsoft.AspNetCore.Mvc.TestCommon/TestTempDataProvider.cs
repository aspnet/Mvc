// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Microsoft.AspNetCore.Mvc
{
    public class TestTempDataProvider : ITempDataProvider
    {
        public TestTempDataProvider(IDictionary<string, object> data = null)
        {
            Initial = data ?? new Dictionary<string, object>();
        }

        public IDictionary<string, object> Initial { get; }

        public IDictionary<string, object> Saved { get; private set; }

        public IDictionary<string, object> LoadTempData(HttpContext context) => Initial;

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
            Saved = values;
        }
    }
}
