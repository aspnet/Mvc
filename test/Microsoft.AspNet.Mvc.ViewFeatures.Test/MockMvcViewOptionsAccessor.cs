// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc
{
    public class MockMvcViewOptionsAccessor : IOptions<MvcViewOptions>
    {
        public MockMvcViewOptionsAccessor()
        {
            Value = new MvcViewOptions();
        }

        public MvcViewOptions Value { get; private set; }
    }
}