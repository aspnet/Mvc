﻿
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

namespace MvcSample.Web.Components
{
    [ViewComponent(Name="Tags")]
    public class TagCloud : ViewComponent
    {
        private readonly string[] Tags =
            ("Post-ironic iPhone +1 small-batch meggings occupy ennui Truffaut ethical try-hard gastropub" +
             "brunch High-Life Schlitz Photo booth scenester forage Cosby sweater food truck Truffaut" +
             "narwhal Brooklyn fashion axe beard chambray craft beer Drinking vinegar PBR&B Cosby sweater" +
             "asymmetrical lo-fi beard cray mixtape locavore Master cleanse squid mumblecore ethnic " +
             "Intelligentsia Godard Odd Future XOXO asymmetrical gastropub distillery PBR&B swag" +
             "Helvetica yr art party occupy ug Leggings Austin plaid pork belly")
                .Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries)
                .OrderBy(s => Guid.NewGuid().ToString())
                .ToArray();

        public async Task<IViewComponentResult> InvokeAsync(int count)
        {
            var tags = await GetTagsAsync(count);
            return View(tags);
        }

        public IViewComponentResult Invoke(int count)
        {
            var tags = GetTags(count);
            return View(tags);
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