// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace MvcSample.Web
{
    public class Person
    {
        public string Name { get; set; }

        public Address Address { get; set; }

        public IEnumerable<Job> PastJobs { get; set; }
    }
}