// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNet.Mvc.Razor.Precompilation
{
    public class DerivedTagHelper : BaseTagHelper
    {
        public override string VirtualProperty { get; set; }

        [Base(BaseProperty = "BaseValue")]
        public string DerivedProperty { get; set; }

        [HtmlAttributeName("new-property")]
        public new Type NewProperty { get; set; }

        public override string Order { get; set; }
    }
}
