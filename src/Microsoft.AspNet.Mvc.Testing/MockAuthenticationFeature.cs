// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using Microsoft.AspNet.Http.Interfaces.Security;

namespace Microsoft.AspNet.Mvc.Testing
{
    internal class MockAuthenticationFeature : IHttpAuthenticationFeature
    {
        public IAuthenticationHandler Handler
        {
            get;
            set;
        }

        public ClaimsPrincipal User
        {
            get;
            set;
        }
    }
}