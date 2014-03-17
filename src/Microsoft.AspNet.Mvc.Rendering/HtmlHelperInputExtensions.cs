// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public static class HtmlHelperInputExtensions
    {
        // CheckBox

        public static HtmlString CheckBox([NotNull] this IHtmlHelper htmlHelper, string name)
        {
            return CheckBox(htmlHelper, name, htmlAttributes: null);
        }

        public static HtmlString CheckBox([NotNull] this IHtmlHelper htmlHelper, string name, bool isChecked)
        {
            return CheckBox(htmlHelper, name, isChecked, htmlAttributes: null);
        }

        public static HtmlString CheckBox([NotNull] this IHtmlHelper htmlHelper, string name, bool isChecked,
            object htmlAttributes)
        {
            return htmlHelper.CheckBox(name, isChecked,
                HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static HtmlString CheckBox([NotNull] this IHtmlHelper htmlHelper, string name, object htmlAttributes)
        {
            return CheckBox(htmlHelper, name, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static HtmlString CheckBox([NotNull] this IHtmlHelper htmlHelper, string name,
            IDictionary<string, object> htmlAttributes)
        {
            return htmlHelper.CheckBox(name: name, isChecked: null, htmlAttributes: htmlAttributes);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This is an appropriate nesting of generic types")]
        public static HtmlString CheckBoxFor<TModel>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            [NotNull] Expression<Func<TModel, bool>> expression)
        {
            return CheckBoxFor(htmlHelper, expression, htmlAttributes: null);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This is an appropriate nesting of generic types")]
        public static HtmlString CheckBoxFor<TModel>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            [NotNull] Expression<Func<TModel, bool>> expression, object htmlAttributes)
        {
            return htmlHelper.CheckBoxFor(expression,
                HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        // Hidden

        public static HtmlString Hidden([NotNull] this IHtmlHelper htmlHelper, string name)
        {
            return Hidden(htmlHelper, name, value: null, htmlAttributes: null);
        }

        public static HtmlString Hidden([NotNull] this IHtmlHelper htmlHelper, string name, object value)
        {
            return Hidden(htmlHelper, name, value, htmlAttributes: null);
        }

        public static HtmlString Hidden([NotNull] this IHtmlHelper htmlHelper, string name, object value,
            object htmlAttributes)
        {
            return htmlHelper.Hidden(name, value,
                HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This is an appropriate nesting of generic types")]
        public static HtmlString HiddenFor<TModel, TProperty>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            [NotNull] Expression<Func<TModel, TProperty>> expression)
        {
            return HiddenFor(htmlHelper, expression, null);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This is an appropriate nesting of generic types")]
        public static HtmlString HiddenFor<TModel, TProperty>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            [NotNull] Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        {
            return htmlHelper.HiddenFor(expression,
                HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        // Password

        public static HtmlString Password([NotNull] this IHtmlHelper htmlHelper, string name)
        {
            return Password(htmlHelper, name, value: null);
        }

        public static HtmlString Password([NotNull] this IHtmlHelper htmlHelper, string name, object value)
        {
            return Password(htmlHelper, name, value, htmlAttributes: null);
        }

        public static HtmlString Password([NotNull] this IHtmlHelper htmlHelper, string name, object value,
            object htmlAttributes)
        {
            return htmlHelper.Password(name, value,
                HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This is an appropriate nesting of generic types")]
        public static HtmlString PasswordFor<TModel, TProperty>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            [NotNull] Expression<Func<TModel, TProperty>> expression)
        {
            return PasswordFor(htmlHelper, expression, htmlAttributes: null);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This is an appropriate nesting of generic types")]
        public static HtmlString PasswordFor<TModel, TProperty>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            [NotNull] Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        {
            return htmlHelper.PasswordFor(expression,
                HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        // RadioButton

        public static HtmlString RadioButton([NotNull] this IHtmlHelper htmlHelper, string name, object value)
        {
            return RadioButton(htmlHelper, name, value, htmlAttributes: null);
        }

        public static HtmlString RadioButton([NotNull] this IHtmlHelper htmlHelper, string name, object value,
            object htmlAttributes)
        {
            return RadioButton(htmlHelper, name, value,
                HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static HtmlString RadioButton([NotNull] this IHtmlHelper htmlHelper, string name, object value,
            IDictionary<string, object> htmlAttributes)
        {
            return htmlHelper.RadioButton(name, value, isChecked: null, htmlAttributes: htmlAttributes);
        }

        public static HtmlString RadioButton([NotNull] this IHtmlHelper htmlHelper, string name, object value,
            bool isChecked)
        {
            return RadioButton(htmlHelper, name, value, isChecked, htmlAttributes: null);
        }

        public static HtmlString RadioButton([NotNull] this IHtmlHelper htmlHelper, string name, object value,
            bool isChecked, object htmlAttributes)
        {
            return htmlHelper.RadioButton(name, value, isChecked,
                HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This is an appropriate nesting of generic types")]
        public static HtmlString RadioButtonFor<TModel, TProperty>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            [NotNull] Expression<Func<TModel, TProperty>> expression, object value)
        {
            return RadioButtonFor(htmlHelper, expression, value, htmlAttributes: null);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This is an appropriate nesting of generic types")]
        public static HtmlString RadioButtonFor<TModel, TProperty>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            [NotNull] Expression<Func<TModel, TProperty>> expression, object value, object htmlAttributes)
        {
            return htmlHelper.RadioButtonFor(expression, value,
                HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        // TextBox

        public static HtmlString TextBox([NotNull] this IHtmlHelper htmlHelper, string name)
        {
            return TextBox(htmlHelper, name, value: null);
        }

        public static HtmlString TextBox([NotNull] this IHtmlHelper htmlHelper, string name, object value)
        {
            return TextBox(htmlHelper, name, value, format: null);
        }

        public static HtmlString TextBox([NotNull] this IHtmlHelper htmlHelper, string name, object value,
            string format)
        {
            return TextBox(htmlHelper, name, value, format, htmlAttributes: null);
        }

        public static HtmlString TextBox([NotNull] this IHtmlHelper htmlHelper, string name, object value,
            object htmlAttributes)
        {
            return TextBox(htmlHelper, name, value, format: null, htmlAttributes: htmlAttributes);
        }

        public static HtmlString TextBox([NotNull] this IHtmlHelper htmlHelper, string name, object value,
            string format, object htmlAttributes)
        {
            return htmlHelper.TextBox(name, value, format, 
                HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static HtmlString TextBox([NotNull] this IHtmlHelper htmlHelper, string name, object value,
            IDictionary<string, object> htmlAttributes)
        {
            return htmlHelper.TextBox(name, value, format: null, htmlAttributes: htmlAttributes);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This is an appropriate nesting of generic types")]
        public static HtmlString TextBoxFor<TModel, TProperty>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            [NotNull] Expression<Func<TModel, TProperty>> expression)
        {
            return TextBoxFor(htmlHelper, expression, format: null);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This is an appropriate nesting of generic types")]
        public static HtmlString TextBoxFor<TModel, TProperty>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            [NotNull] Expression<Func<TModel, TProperty>> expression, string format)
        {
            return TextBoxFor(htmlHelper, expression, format, htmlAttributes: null);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This is an appropriate nesting of generic types")]
        public static HtmlString TextBoxFor<TModel, TProperty>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            [NotNull] Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        {
            return TextBoxFor(htmlHelper, expression, format: null, htmlAttributes: htmlAttributes);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This is an appropriate nesting of generic types")]
        public static HtmlString TextBoxFor<TModel, TProperty>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            [NotNull] Expression<Func<TModel, TProperty>> expression, string format, object htmlAttributes)
        {
            return htmlHelper.TextBoxFor(expression, format: format,
                htmlAttributes: HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This is an appropriate nesting of generic types")]
        public static HtmlString TextBoxFor<TModel, TProperty>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            [NotNull] Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes)
        {
            return htmlHelper.TextBoxFor(expression, format: null, htmlAttributes: htmlAttributes);
        }
    }
}
