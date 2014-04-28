﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public static class HtmlHelperValidationExtensions
    {
        public static HtmlString ValidationSummary([NotNull] this IHtmlHelper htmlHelper)
        {
            return ValidationSummary(htmlHelper, excludePropertyErrors: false);
        }

        public static HtmlString ValidationSummary([NotNull] this IHtmlHelper htmlHelper, bool excludePropertyErrors)
        {
            return ValidationSummary(htmlHelper, excludePropertyErrors, message: null);
        }

        public static HtmlString ValidationSummary([NotNull] this IHtmlHelper htmlHelper, string message)
        {
            return ValidationSummary(htmlHelper, excludePropertyErrors: false, message: message,
                htmlAttributes: (object)null);
        }

        public static HtmlString ValidationSummary(
            [NotNull] this IHtmlHelper htmlHelper,
            bool excludePropertyErrors,
            string message)
        {
            return ValidationSummary(htmlHelper, excludePropertyErrors, message, htmlAttributes: (object)null);
        }

        public static HtmlString ValidationSummary(
            [NotNull] this IHtmlHelper htmlHelper,
            string message,
            object htmlAttributes)
        {
            return ValidationSummary(htmlHelper, excludePropertyErrors: false, message: message,
                htmlAttributes: HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static HtmlString ValidationSummary(
            [NotNull] this IHtmlHelper htmlHelper,
            bool excludePropertyErrors,
            string message,
            object htmlAttributes)
        {
            return htmlHelper.ValidationSummary(excludePropertyErrors, message,
                HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static HtmlString ValidationSummary(
            [NotNull] this IHtmlHelper htmlHelper,
            string message,
            IDictionary<string, object> htmlAttributes)
        {
            return htmlHelper.ValidationSummary(excludePropertyErrors: false, message: message,
                htmlAttributes: htmlAttributes);
        }

        public static HtmlString ValidationMessage<TModel>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            string modelName)
        {
            return ValidationMessage(htmlHelper, modelName, message: null, htmlAttributes: (object) null);
        }

        public static HtmlString ValidationMessage<TModel>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            string modelName, string message)
        {
            return ValidationMessage(htmlHelper, modelName, message, htmlAttributes: (object) null);
        }

        public static HtmlString ValidationMessage<TModel>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            string modelName, object htmlAttributes)
        {
            return ValidationMessage(htmlHelper, modelName, message: null, htmlAttributes: htmlAttributes);
        }

        public static HtmlString ValidationMessage<TModel>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            string modelName, IDictionary<string, object> htmlAttributes)
        {
            return ValidationMessage(htmlHelper, modelName, message: null, htmlAttributes: htmlAttributes);
        }

        public static HtmlString ValidationMessage<TModel>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            string modelName, string message, IDictionary<string, object> htmlAttributes)
        {
            return htmlHelper.ValidationMessage(modelName, message, htmlAttributes);
        }

        public static HtmlString ValidationMessage<TModel>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            string modelName, string message, object htmlAttributes)
        {
            return htmlHelper.ValidationMessage(modelName, message, htmlAttributes);
        }

        public static HtmlString ValidationMessageFor<TModel, TProperty>([NotNull] this IHtmlHelper<TModel> htmlHelper, 
            [NotNull] Expression<Func<TModel, TProperty>> expression)
        {
            return ValidationMessageFor(htmlHelper, expression, message: null, htmlAttributes: (object) null);
        }

        public static HtmlString ValidationMessageFor<TModel, TProperty>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            [NotNull] Expression<Func<TModel, TProperty>> expression, string message)
        {
            return ValidationMessageFor(htmlHelper, expression, message, htmlAttributes: (object) null);
        }

        public static HtmlString ValidationMessageFor<TModel, TProperty>([NotNull] this IHtmlHelper<TModel> htmlHelper, 
            [NotNull] Expression<Func<TModel, TProperty>> expression, string message,
            IDictionary<string, object> htmlAttributes)
        {
            return htmlHelper.ValidationMessageFor(expression, message, htmlAttributes);
        }

        public static HtmlString ValidationMessageFor<TModel, TProperty>([NotNull] this IHtmlHelper<TModel> htmlHelper,
            [NotNull] Expression<Func<TModel, TProperty>> expression, string message, object htmlAttributes)
        {
            return htmlHelper.ValidationMessageFor(expression, message, htmlAttributes);
        }
    }
}
