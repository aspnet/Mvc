// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;

namespace Microsoft.AspNet.Mvc.Description
{
    public class ApiResponseFormat
    {
        public Type DataType { get; set; }

        public IOutputFormatter Formatter { get; set; }

        public MediaTypeHeaderValue MediaType { get; set; }
    }
}