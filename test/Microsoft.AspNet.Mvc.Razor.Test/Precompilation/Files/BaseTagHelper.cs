// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNet.Mvc.Razor.Precompilation
{
    public class BaseTagHelper : TagHelper
    {
        [Derived(DerivedProperty = "DerivedPropertyValue")]
        public string BaseProperty { get; set; }

        [HtmlAttributeNotBound]
        public virtual string VirtualProperty { get; set; }

        public int NewProperty { get; set; }

        public virtual new string Order { get; set; }
    }
}
