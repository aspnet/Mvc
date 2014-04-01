using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public static class ValueExtensions
    {
        public static HtmlString Value<TModel>([NotNull] this IHtmlHelper<TModel> htmlHelper, string name)
        {
            return htmlHelper.Value(name, format: null);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static HtmlString ValueFor<TModel, TProperty>([NotNull] this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            return htmlHelper.ValueFor(expression, format: null);
        }

        public static HtmlString ValueForModel<TModel>([NotNull] this IHtmlHelper<TModel> htmlHelper)
        {
            return ValueForModel<TModel>(htmlHelper, format: null);
        }

        public static HtmlString ValueForModel<TModel>([NotNull] this IHtmlHelper<TModel> htmlHelper, string format)
        {
            return htmlHelper.Value(string.Empty, format);
        }
    }
}
