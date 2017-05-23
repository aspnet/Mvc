﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorWebSite
{
    public class TestBodyTagHelperComponent : TagHelperComponent
    {
        public override int Order => 1;

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.Equals(context.TagName, "body", StringComparison.Ordinal) && output.Attributes.ContainsName("inject"))
            {
                output.PostContent.AppendHtml("<script>'This was injected!!'</script>\r\n");
            }

            return Task.FromResult(0);
        }
    }
}
