// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNet.Mvc.Razor
{
    public struct RazorPageFactoryResult
    {
        public RazorPageFactoryResult(IList<IChangeToken> expirationTokens)
        {
            ExpirationTokens = expirationTokens;
            RazorPageFactory = null;
        }

        public RazorPageFactoryResult(
            Func<IRazorPage> razorPageFactory,
            IList<IChangeToken> expirationTokens)
        {
            RazorPageFactory = razorPageFactory;
            ExpirationTokens = expirationTokens;
        }

        public Func<IRazorPage> RazorPageFactory { get; }

        public IList<IChangeToken> ExpirationTokens { get; }

        public bool IsFoundResult => RazorPageFactory != null;
    }
}
