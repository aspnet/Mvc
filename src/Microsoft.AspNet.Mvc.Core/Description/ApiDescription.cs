// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.Description
{
    public class ApiDescription
    {
        public ApiDescription()
        {
            ParameterDescriptions = new List<ApiParameterDescriptor>();
            SupportedResponseFormats = new List<ApiResponseFormat>();
        }

        public ActionDescriptor ActionDescriptor { get; set; }

        public string HttpMethod { get; set; }

        public List<ApiParameterDescriptor> ParameterDescriptions { get; private set; }

        public string RelativePath { get; set; }

        public Type ResponseType { get; set; }

        public List<ApiResponseFormat> SupportedResponseFormats { get; private set; }
    }
}