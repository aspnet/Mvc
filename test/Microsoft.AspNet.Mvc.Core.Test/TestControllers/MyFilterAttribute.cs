// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.Test.TestControllers
{
    public class MyFilterAttribute : Attribute, IFilter
    {
        public MyFilterAttribute(int value)
        {
            Value = value;
        }

        public int Value { get; private set; }
    }
}