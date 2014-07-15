// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Test.TestControllers
{
    public class MixedRpcAndRestController
    {
        public void Index()
        {
        }

        public void Get()
        {
        }

        public void Post()
        { }

        public void GetSomething()
        { }

        // This will be treated as an RPC method.
        public void Head()
        {
        }

        // This will be treated as an RPC method.
        public void Options()
        {
        }
    }
}