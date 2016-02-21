// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;

namespace FiltersWebSite
{
    [SerializationActionFilter]
    public class XmlSerializerController : Controller
    {
        public DummyClass GetDummyClass(int sampleInput)
        {
            return new DummyClass { SampleInt = sampleInput };
        }
    }
}