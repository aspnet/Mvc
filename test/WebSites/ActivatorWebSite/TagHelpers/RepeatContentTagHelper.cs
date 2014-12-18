using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace ActivatorWebSite.TagHelpers
{
    [HtmlElementName("div")]
    public class RepeatContentTagHelper : TagHelper
    {
        public int RepeatContent { get; set; }

        public ModelExpression Expression { get; set; }

        [Activate]
        public IHtmlHelper HtmlHelper { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content = await context.GetChildContentAsync();
            var repeatContent = HtmlHelper.Encode(Expression.Metadata.Model.ToString());

            if (string.IsNullOrEmpty(repeatContent))
            {
                repeatContent = content;
            }

            for (int i = 0; i < RepeatContent; i++)
            {
                output.Content += repeatContent;
            }
        }

    }
}