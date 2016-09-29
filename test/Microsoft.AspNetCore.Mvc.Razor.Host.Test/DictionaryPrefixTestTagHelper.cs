// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.Razor.Host.Test
{
    [HtmlTargetElement(Attributes = "prefix-*")]
    public class DictionaryPrefixTestTagHelper : TagHelper
    {
        [HtmlAttributeName(DictionaryAttributePrefix = "prefix-")]
        public IDictionary<string, ModelExpression> PrefixValues { get; set; } = new Dictionary<string, ModelExpression>();

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
        }
    }
}
