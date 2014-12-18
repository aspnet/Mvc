using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace ActivatorWebSite.TagHelpers
{
    [HtmlElementName("span")]
    public class HiddenTagHelper : TagHelper
    {
        public string Name { get; set; }

        [Activate]
        public IHtmlHelper HtmlHelper { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content = await context.GetChildContentAsync();

            output.Content = HtmlHelper.Hidden(Name, content).ToString();
        }
    }
}