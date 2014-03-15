// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering.Expressions;

namespace Microsoft.AspNet.Mvc.Rendering.Html
{
    public class HtmlHelper<TModel> : HtmlHelper, IHtmlHelper<TModel>
    {
        private ViewData<TModel> _viewData;

        public HtmlHelper([NotNull] IHtmlHelper original)
            : base(original)
        {
        }

        /// <inheritdoc />
        public ViewData<TModel> ViewData
        {
            get
            {
                return _viewData;
            }
        }

        public override void Contextualize(ViewContext viewContext)
        {
            _viewData = viewContext.ViewData as ViewData<TModel>;
            if (_viewData == null)
            {
                _viewData = new ViewData<TModel>(viewContext.ViewData);

                // Ensure references to ViewContext.ViewData refer to the same instance.
                viewContext = new ViewContext(viewContext.HttpContext, _viewData, viewContext.ServiceProvider);
            }

            base.Contextualize(viewContext);
        }

        #region Input helpers
        /// <inheritdoc />
        public HtmlString CheckBoxFor([NotNull] Expression<Func<TModel, bool>> expression,
            IDictionary<string, object> htmlAttributes)
        {
            var metadata = GetModelMetadata(expression);
            return RenderCheckBox(metadata, GetExpressionName(expression), isChecked: null,
                htmlAttributes: htmlAttributes);
        }

        /// <inheritdoc />
        public HtmlString HiddenFor<TProperty>([NotNull] Expression<Func<TModel, TProperty>> expression,
            IDictionary<string, object> htmlAttributes)
        {
            var metadata = GetModelMetadata(expression);
            return RenderHidden(metadata, GetExpressionName(expression), metadata.Model, useViewData: false,
                htmlAttributes: htmlAttributes);
        }

        /// <inheritdoc />
        public HtmlString PasswordFor<TProperty>([NotNull] Expression<Func<TModel, TProperty>> expression,
            IDictionary<string, object> htmlAttributes)
        {
            var metadata = GetModelMetadata(expression);
            return RenderPassword(metadata, GetExpressionName(expression), value: null, htmlAttributes: htmlAttributes);
        }

        /// <inheritdoc />
        public HtmlString RadioButtonFor<TProperty>([NotNull] Expression<Func<TModel, TProperty>> expression,
            object value, IDictionary<string, object> htmlAttributes)
        {
            var metadata = GetModelMetadata(expression);
            return RenderRadioButton(metadata, GetExpressionName(expression), value, isChecked: null,
                htmlAttributes: htmlAttributes);
        }

        /// <inheritdoc />
        public HtmlString TextBoxFor<TProperty>([NotNull] Expression<Func<TModel, TProperty>> expression,
            string format, IDictionary<string, object> htmlAttributes)
        {
            var metadata = GetModelMetadata(expression);
            return RenderTextBox(metadata, GetExpressionName(expression), metadata.Model, format, htmlAttributes);
        }
        #endregion

        protected string GetExpressionName<TProperty>([NotNull] Expression<Func<TModel, TProperty>> expression)
        {
            return ExpressionEvaluator.GetExpressionText(expression);
        }

        protected ModelMetadata GetModelMetadata<TProperty>([NotNull] Expression<Func<TModel, TProperty>> expression)
        {
            var metadata = ExpressionMetadataProvider.FromLambdaExpression(expression, ViewData, MetadataProvider);
            Contract.Assert(metadata != null);

            return metadata;
        }
    }
}
