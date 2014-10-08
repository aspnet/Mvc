// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class InputTestTagHelper : TagHelper
    {
        public ModelExpression For { get; set; }
    }
}

namespace Microsoft.AspNet.Mvc.Rendering
{
    // This is here to mimic the ModelExpression type defined in Microsoft.AspNet.Mvc.Core.
    // Normally it's understood by the MvcRazorHost via a stringified version of it (will change once we have a DTH)
    // so we need to provide some polyfill so we can test its functionality in Microsoft.AspNet.Mvc.Razor.Host.Test.
    public class ModelExpression
    {
    }
}