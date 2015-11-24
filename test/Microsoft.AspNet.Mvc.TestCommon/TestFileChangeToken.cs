// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Extensions.Primitives
{
    public class TestFileChangeToken : IChangeToken
    {
        public bool ActiveChangeCallbacks => false;

        public bool HasChanged { get; set; }

        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            throw new NotImplementedException();
        }
    }
}