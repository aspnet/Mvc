using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.Rendering.Html
{
    public static class ValueExtensions
    {
        public static MvcHtmlString Value(this HtmlHelper html, string name)
        {
            return Value(html, name, format: null);
        }

        public static MvcHtmlString Value(this HtmlHelper html, string name, string format)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            return ValueForHelper(html, name, value: null, format: format, useViewData: true);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString ValueFor<TModel, TProperty>(this HtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression)
        {
            return ValueFor(html, expression, format: null);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString ValueFor<TModel, TProperty>(this HtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string format)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            return ValueForHelper(html, ExpressionHelper.GetExpressionText(expression), metadata.Model, format, useViewData: false);
        }

        public static MvcHtmlString ValueForModel(this HtmlHelper html)
        {
            return ValueForModel(html, format: null);
        }

        public static MvcHtmlString ValueForModel(this HtmlHelper html, string format)
        {
            return Value(html, String.Empty, format);
        }

        internal static MvcHtmlString ValueForHelper(HtmlHelper html, string name, object value, string format, bool useViewData)
        {
            string fullName = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            string attemptedValue = (string)html.GetModelStateValue(fullName, typeof(string));
            string resolvedValue;

            if (attemptedValue != null)
            {
                // case 1: if ModelState has a value then it's already formatted so ignore format string
                resolvedValue = attemptedValue;
            }
            else if (useViewData)
            {
                if (name.Length == 0)
                {
                    // case 2(a): format the value from ModelMetadata for the current model
                    ModelMetadata metadata = ModelMetadata.FromStringExpression(String.Empty, html.ViewContext.ViewData);
                    resolvedValue = html.FormatValue(metadata.Model, format);
                }
                else
                {
                    // case 2(b): format the value from ViewData
                    resolvedValue = html.EvalString(name, format);
                }
            }
            else
            {
                // case 3: format the explicit value from ModelMetadata
                resolvedValue = html.FormatValue(value, format);
            }

            return MvcHtmlString.Create(html.AttributeEncode(resolvedValue));
        }
    }
}
