using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.Rendering.Html;

namespace Microsoft.AspNet.Mvc.Razor
{
    public abstract class RazorView<TModel> : RazorView
    {
        public TModel Model
        {
            get
            {
                return ViewData == null ? default(TModel) : ViewData.Model;
            }
        }

        public dynamic ViewBag
        {
            get { return ViewData; }
        }

        public ViewData<TModel> ViewData { get; set; }

        public IHtmlHelper<TModel> Html { get; set; }

        public IHtmlSettings HtmlSettings { get; private set; }

        public override Task RenderAsync(ViewContext context, TextWriter writer)
        {
            ViewData = context.ViewData as ViewData<TModel>;
            if (ViewData == null)
            {
                if (context.ViewData != null)
                {
                    ViewData = new ViewData<TModel>(context.ViewData);
                }
                else
                {
                    var metadataProvider = context.ServiceProvider.GetService<IModelMetadataProvider>();
                    ViewData = new ViewData<TModel>(metadataProvider);
                }

                // Have new ViewData; make sure it's visible everywhere.
                context = new ViewContext(context.HttpContext, ViewData, context.ServiceProvider);
            }

            InitHelpers(context);

            return base.RenderAsync(context, writer);
        }

        private void InitHelpers(ViewContext context)
        {
            var baseHelper = context.ServiceProvider.GetService<IHtmlHelper>();

            // Temporary workaround; just until DI supports open generics.
            Html = new HtmlHelper<TModel>(baseHelper);

            var contextable = Html as INeedContext;
            if (contextable != null)
            {
                contextable.Contextualize(context);
            }

            HtmlSettings = context.ServiceProvider.GetService<IHtmlSettings>();
        }
    }
}
