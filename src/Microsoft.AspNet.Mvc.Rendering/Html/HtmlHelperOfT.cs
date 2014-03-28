using System;
using Microsoft.AspNet.Abstractions;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class HtmlHelper<TModel> : HtmlHelper, IHtmlHelper<TModel>
    {
        public HtmlHelper()
            : base()
        {
        }

        /// <inheritdoc />
        public new ViewDataDictionary<TModel> ViewData { get; private set;}

        public override void Contextualize([NotNull] ViewContext viewContext)
        {
            if (viewContext.ViewData == null)
            {
                throw new ArgumentException(Resources.HtmlHelper_ViewDataNull);
            }

            ViewData = viewContext.ViewData as ViewDataDictionary<TModel>;
            if (ViewData == null)
            {
                throw new ArgumentException(Resources.FormatHtmlHelper_ViewDataUnexpectedType(
                    viewContext.ViewData.GetType().Name,
                    typeof(ViewDataDictionary<TModel>).Name));
            }

            base.Contextualize(viewContext);
        }
    }
}
