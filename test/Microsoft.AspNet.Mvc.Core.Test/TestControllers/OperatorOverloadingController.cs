// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Test.TestControllers
{
    public class OperatorOverloadingController : Controller
    {
        public static OperatorOverloadingController operator +(
            OperatorOverloadingController c1,
            OperatorOverloadingController c2)
        {
            return new OperatorOverloadingController();
        }
    }
}