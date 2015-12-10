// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;

namespace HtmlGenerationWebSite
{
    public class TokenProviderService
    {
        private readonly ConcurrentDictionary<object, CancellableChangeToken> _changeTokens
            = new ConcurrentDictionary<object, CancellableChangeToken>();

        public CancellableChangeToken GetToken(object key)
        {
            return _changeTokens.GetOrAdd(key, new CancellableChangeToken());
        }

        public void ExpireToken(object key)
        {
            CancellableChangeToken changeToken;
            if (_changeTokens.TryRemove(key, out changeToken))
            {
                changeToken.HasChanged = true;
            }
        }
    }
}
