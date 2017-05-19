// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorWebSite.Controllers
{
    public class AddTagHelperComponentController : Controller
    {
        private readonly ITagHelperComponentManager _tagHelperComponentManager;

        public AddTagHelperComponentController(ITagHelperComponentManager tagHelperComponentManager)
        {
            _tagHelperComponentManager = tagHelperComponentManager;
        }

        public IActionResult AddComponent()
        {
            _tagHelperComponentManager.Add(new TestTagHelperComponent());
            return View("AddComponent");
        }

        private class TestTagHelperComponent : ITagHelperComponent
        {
            public int Order => 2;

            public void Init(TagHelperContext context)
            {
                context.Items["Key"] = "Value";
            }

            public Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
            {
                if (string.Equals(context.TagName, "body", StringComparison.Ordinal))
                {
                    output.PostContent.AppendHtml("\r\nProcessed TagHelperComponent added from controller.");
                }
                return Task.CompletedTask;
            }
        }
    }
}
