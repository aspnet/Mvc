// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor.TagHelperComponent;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorWebSite
{
    public class TestHeadTagHelperComponent : ITagHelperComponent
    {
        public TestHeadTagHelperComponent()
        {
        }

        public bool AppliesTo(TagHelperContext context) => string.Equals("head", context.TagName, StringComparison.OrdinalIgnoreCase);

        public int Order => 1;

        public void Init(TagHelperContext context)
        {
        }

        public Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var customAttribute = new TagHelperAttribute("inject");
            context.AllAttributes.TryGetAttribute("inject", out customAttribute);
            if (customAttribute?.Value.ToString() == "true")
            {
                output.PostContent.AppendHtml("<script>'This was injected!!'</script>");
            }

            return Task.FromResult(0);
        }
    }
}
