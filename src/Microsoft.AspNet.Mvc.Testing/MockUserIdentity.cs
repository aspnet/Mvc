// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;

namespace Microsoft.AspNet.Mvc.Testing
{
    // Need to derived from ClaimsIdentity to make GetUserId work for the future
    internal class MockUserIdentity : ClaimsIdentity
    {
        public UserContext UserContext
        {
            get;
            set;
        }

        public override bool IsAuthenticated
        {
            get
            {
                return UserContext.IsAuthenticated;
            }
        }
    }
}