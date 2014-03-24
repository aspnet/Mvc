﻿

using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc
{
    public class ContentViewComponentResult : IViewComponentResult
    {
        private readonly HtmlString _encoded;

        public ContentViewComponentResult([NotNull] string content)
        {
            _encoded = new HtmlString(WebUtility.HtmlEncode(content));
        }

        public ContentViewComponentResult([NotNull] HtmlString encoded)
        {
            _encoded = encoded;
        }

        public void Execute([NotNull] ComponentContext componentContext)
        {
            componentContext.Writer.Write(_encoded.ToString());
        }

        public async Task ExecuteAsync([NotNull] ComponentContext componentContext)
        {
            await componentContext.Writer.WriteAsync(_encoded.ToString());
        }
    }
}
