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
        public ViewData<TModel> ViewData
        {
            get
            {
                var viewData = ViewContext.ViewData as ViewData<TModel>;
                if (viewData == null)
                {
                    // Rewrap ViewData on first use or if user has overwritten ViewContext.ViewData with an
                    // incompatible ViewData or ViewData<T>.
                    viewData = new ViewData<TModel>(ViewContext.ViewData);
                    ViewContext.ViewData = viewData;
                }

                return null;
            }
        }

        protected string GetExpressionName<TProperty>([NotNull] Expression<Func<TModel, TProperty>> expression)
        {
            return ExpressionEvaluator.GetExpressionText(expression);
        }
    }
}
