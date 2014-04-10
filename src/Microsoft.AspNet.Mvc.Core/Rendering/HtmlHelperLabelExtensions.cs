using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public static class HtmlHelperLabelExtensions
    {
        public static HtmlString Label<TModel>(this IHtmlHelper<TModel> html, string expression)
        {
            return Label(html,
                         expression,
                         labelText: null);
        }

        public static HtmlString Label<TModel>(this IHtmlHelper<TModel> html, string expression, string labelText)
        {
            return html.Label(expression, labelText, htmlAttributes: null);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static HtmlString LabelFor<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            return LabelFor<TModel, TValue>(html, expression, labelText: null);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static HtmlString LabelFor<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string labelText)
        {
            return html.LabelFor(expression, labelText, htmlAttributes: null);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static HtmlString LabelFor<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes)
        {
            return html.LabelFor(expression, labelText: null, htmlAttributes: htmlAttributes);
        }

        public static HtmlString LabelForModel<TModel>(this IHtmlHelper<TModel> html)
        {
            return LabelForModel(html, labelText: null);
        }

        public static HtmlString LabelForModel<TModel>(this IHtmlHelper<TModel> html, string labelText)
        {
            return html.Label(String.Empty, labelText, htmlAttributes: null);
        }

        public static HtmlString LabelForModel<TModel>(this IHtmlHelper<TModel> html, object htmlAttributes)
        {
            return html.Label(String.Empty, labelText: null, htmlAttributes: htmlAttributes);
        }

        public static HtmlString LabelForModel<TModel>(this IHtmlHelper<TModel> html, string labelText, object htmlAttributes)
        {
            return html.Label(String.Empty, labelText, htmlAttributes);
        }
    }
}