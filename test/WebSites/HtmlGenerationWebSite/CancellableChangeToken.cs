// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Primitives;

namespace HtmlGenerationWebSite
{
    public class CancellableChangeToken : IChangeToken
    {
        public bool HasChanged { get; set; }

        public bool ActiveChangeCallbacks => false;

        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            throw new NotSupportedException();
        }
    }
}
