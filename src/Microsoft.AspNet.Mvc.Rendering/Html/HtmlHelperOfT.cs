using System;
using System.Linq.Expressions;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering.Expressions;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class HtmlHelper<TModel> : HtmlHelper, IHtmlHelper<TModel>
    {
        public HtmlHelper([NotNull] IModelMetadataProvider metadataProvider)
            : base(metadataProvider)
        {
        }

        /// <inheritdoc />
        public ViewData<TModel> ViewData { get; private set; }

        public override void Contextualize([NotNull] ViewContext viewContext)
        {
            if (viewContext.ViewData == null)
            {
                throw new ArgumentException(Resources.HtmlHelper_ViewDataNull);
            }

            ViewData = viewContext.ViewData as ViewData<TModel>;
            if (ViewData == null)
            {
                throw new ArgumentException(Resources.FormatHtmlHelper_ViewDataUnexpectedType(
                    viewContext.ViewData.GetType().Name,
                    typeof(ViewData<TModel>).Name));
            }

            base.Contextualize(viewContext);
        }

        protected string GetExpressionName<TProperty>([NotNull] Expression<Func<TModel, TProperty>> expression)
        {
            return ExpressionEvaluator.GetExpressionText(expression);
        }
    }
}
