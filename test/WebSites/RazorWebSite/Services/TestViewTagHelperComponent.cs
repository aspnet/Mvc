// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorWebSite
{
    public class TestViewTagHelperComponent : ITagHelperComponent
    {
        public int Order => 3;

        public void Init(TagHelperContext context)
        {
            context.Items["Key"] = "Value";
        }

        public Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.Equals(context.TagName, "body", StringComparison.Ordinal))
            {
                output.PostContent.AppendHtml("\r\nProcessed TagHelperComponent added from view.\r\n");
            }
            return Task.CompletedTask;
        }

    }
}
