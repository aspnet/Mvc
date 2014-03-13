
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

namespace MvcSample.Web.Components
{
    public class TagCloud : ViewComponent
    {
        // Hipster lorum ipsum, generated with http://hipsum.co/
        private readonly string[] Tags =
            ("Post-ironic iPhone +1 small-batch meggings occupy ennui Truffaut ethical try-hard gastropub" +
            "brunch High-Life Schlitz Photo booth scenester forage Cosby sweater food truck Truffaut" + 
            "narwhal Brooklyn fashion axe beard chambray craft beer Drinking vinegar PBR&B Cosby sweater" +
            "asymmetrical lo-fi beard cray mixtape locavore Master cleanse squid mumblecore ethnic " +
            "Intelligentsia Godard Odd Future XOXO asymmetrical gastropub distillery PBR&B swag" +
            "Helvetica yr art party occupy ug Leggings Austin plaid pork belly")
            .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var tags = await GetTagsAsync(20);
            return View(tags);
        }

        public IViewComponentResult Invoke()
        {
            var tags = GetTags(20);
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