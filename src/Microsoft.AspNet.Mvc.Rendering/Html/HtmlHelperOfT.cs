﻿using System;
using System.Linq.Expressions;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering.Expressions;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class HtmlHelper<TModel> : HtmlHelper, IHtmlHelper<TModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlHelper{TModel}"/> class.
        /// </summary>
        public HtmlHelper([NotNull] IViewEngine viewEngine, [NotNull] IModelMetadataProvider metadataProvider)
             : base(viewEngine, metadataProvider)
        {
        }

        /// <inheritdoc />
        public new ViewDataDictionary<TModel> ViewData { get; private set;}

        public override void Contextualize([NotNull] ViewContext viewContext)
        {
            if (viewContext.ViewData == null)
            {
                throw new ArgumentException(Resources.FormatArgumentPropertyNull("ViewData"), "viewContext");
            }

            ViewData = viewContext.ViewData as ViewDataDictionary<TModel>;
            if (ViewData == null)
            {
                // viewContext may contain a base ViewDataDictionary instance. So complain about that type, not TModel.
                throw new ArgumentException(Resources.FormatArgumentPropertyUnexpectedType(
                        "ViewData",
                        viewContext.ViewData.GetType().FullName,
                        typeof(ViewDataDictionary<TModel>).FullName),
                    "viewContext");
            }

            base.Contextualize(viewContext);
        }

        /// <inheritdoc />
        public HtmlString NameFor<TProperty>([NotNull] Expression<Func<TModel, TProperty>> expression)
        {
            var expressionName = GetExpressionName(expression);
            return Name(expressionName);
        }

        protected string GetExpressionName<TProperty>([NotNull] Expression<Func<TModel, TProperty>> expression)
        {
            return ExpressionHelper.GetExpressionText(expression);
        }
    }
}
