﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Test.TestControllers
{
    [MyFilter(2)]
    public class FiltersController
    {
        [MyFilter(3)]
        public void FilterAction()
        {
        }
    }
}