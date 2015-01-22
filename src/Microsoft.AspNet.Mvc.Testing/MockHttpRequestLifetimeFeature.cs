// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.AspNet.Http.Interfaces;

namespace Microsoft.AspNet.Mvc.Testing
{
    internal class MockHttpRequestLifetimeFeature : IHttpRequestLifetimeFeature
    {
        private CancellationToken _cancelationToken = CancellationToken.None;

        CancellationToken IHttpRequestLifetimeFeature.RequestAborted
        {
            get
            {
                return _cancelationToken;
            }
        }
        void IHttpRequestLifetimeFeature.Abort()

        {
            _cancelationToken = new CancellationToken(true);
        }
    }
}