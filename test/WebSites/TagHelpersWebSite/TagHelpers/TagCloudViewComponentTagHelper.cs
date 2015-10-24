// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.ViewComponents;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.AspNet.Razor.TagHelpers;

namespace MvcSample.Web.Components
{
    [HtmlTargetElement("tag-cloud")]
    [ViewComponent(Name = "Tags")]
    public class TagCloudViewComponentTagHelper : ITagHelper
    {
        private static readonly string[] Tags =
            ("Lorem ipsum dolor sit amet consectetur adipisicing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua" +
             "Ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat Duis aute irure " +
             "dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur Excepteur sint occaecat cupidatat" +
             "non proident, sunt in culpa qui officia deserunt mollit anim id est laborum")
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToArray();

        public int Count { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public int Order { get; } = 0;

        public void Init(TagHelperContext context)
        {
        }

        public async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var result = await InvokeAsync(Count);
            var writer = new StringWriter();

            var viewComponentDescriptor = new ViewComponentDescriptor()
            {
                Type = typeof(TagCloudViewComponentTagHelper),
                ShortName = "TagCloudViewComponentTagHelper",
                FullName = "TagCloudViewComponentTagHelper",
            };

            await result.ExecuteAsync(new ViewComponentContext(
                viewComponentDescriptor,
                new object[0],
                ViewContext,
                writer));

            output.TagName = null;
            output.Content.AppendHtml(writer.ToString());
        }

        public async Task<IViewComponentResult> InvokeAsync(int count)
        {
            var tags = await GetTagsAsync(count);
            return new JsonViewComponentResult(tags);
        }

        private Task<string[]> GetTagsAsync(int count)
        {
            return Task.FromResult(GetTags(count));
        }

        private string[] GetTags(int count)
        {
            return Tags.Take(count).ToArray();
        }
    }
}