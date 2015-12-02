// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.Razor.Buffer
{
    public class TestRazorBufferSource : IRazorBufferScope
    {
        public RazorBufferSegment GetSegment()
        {
            return new RazorBufferSegment(new ArraySegment<RazorValue>(new RazorValue[128]));
        }
    }
}
