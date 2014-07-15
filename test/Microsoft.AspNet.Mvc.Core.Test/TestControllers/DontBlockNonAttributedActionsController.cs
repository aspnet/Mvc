// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Test.TestControllers
{
    [MyRouteConstraintAttribute(blockNonAttributedActions: false)]
    public class DontBlockNonAttributedActionsController
    {
        public void Create()
        {
        }
    }
}