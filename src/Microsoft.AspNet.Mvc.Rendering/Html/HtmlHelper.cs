﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// Default implementation of <see cref="IHtmlHelper">.
    /// </summary>
    public class HtmlHelper : IHtmlHelper, INeedViewContext
    {
        public static readonly string ValidationInputCssClassName = "input-validation-error";
        public static readonly string ValidationInputValidCssClassName = "input-validation-valid";
        public static readonly string ValidationMessageCssClassName = "field-validation-error";
        public static readonly string ValidationMessageValidCssClassName = "field-validation-valid";
        public static readonly string ValidationSummaryCssClassName = "validation-summary-errors";
        public static readonly string ValidationSummaryValidCssClassName = "validation-summary-valid";

        /// <summary>
        /// Create new <see cref="HtmlHelper"/> instance for dependency injection.
        /// </summary>
        /// <param name="metadataProvider"><see cref="IModelMetadataProvider"/> provider.</param>
        public HtmlHelper([NotNull] IModelMetadataProvider metadataProvider)
        {
            MetadataProvider = metadataProvider;

            // Underscores are fine characters in id's.
            IdAttributeDotReplacement = "_";
        }

        /// <inheritdoc />
        public string IdAttributeDotReplacement { get; set; }

        /// <inheritdoc />
        public dynamic ViewBag
        {
            get
            {
                return ViewContext.ViewData;
            }
        }

        /// <inheritdoc />
        public ViewContext ViewContext { get; private set; }

        protected IModelMetadataProvider MetadataProvider { get; private set; }

        /// <summary>
        /// Creates a dictionary of HTML attributes from the input object, 
        /// translating underscores to dashes.
        /// <example>
        /// new { data_name="value" } will translate to the entry { "data-name" , "value" }
        /// in the resulting dictionary.
        /// </example>
        /// </summary>
        /// <param name="htmlAttributes">Anonymous object describing HTML attributes.</param>
        /// <returns>A dictionary that represents HTML attributes.</returns>
        public static Dictionary<string, object> AnonymousObjectToHtmlAttributes(object htmlAttributes)
        {
            Dictionary<string, object> result;
            var valuesAsDictionary = htmlAttributes as IDictionary<string, object>;
            if (valuesAsDictionary != null)
            {
                result = new Dictionary<string, object>(valuesAsDictionary, StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                if (htmlAttributes != null)
                {
                    foreach (var prop in htmlAttributes.GetType().GetRuntimeProperties())
                    {
                        var value = prop.GetValue(htmlAttributes);
                        result.Add(prop.Name, value);
                    }
                }
            }

            return result;
        }

        public virtual void Contextualize(ViewContext viewContext)
        {
            ViewContext = viewContext;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "For consistency, all helpers are instance methods.")]
        public string Encode(string value)
        {
            return (!string.IsNullOrEmpty(value)) ? WebUtility.HtmlEncode(value) : string.Empty;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "For consistency, all helpers are instance methods.")]
        public string Encode(object value)
        {
            return value != null ? WebUtility.HtmlEncode(value.ToString()) : string.Empty;
        }

        /// <inheritdoc />
        public string GenerateIdFromName([NotNull] string name)
        {
            return TagBuilder.CreateSanitizedId(name, IdAttributeDotReplacement);
        }

        /// <summary>
        /// Returns the HTTP method that handles form input (GET or POST) as a string.
        /// </summary>
        /// <param name="method">The HTTP method that handles the form.</param>
        /// <returns>The form method string, either "get" or "post".</returns>
        public static string GetFormMethodString(FormMethod method)
        {
            switch (method)
            {
                case FormMethod.Get:
                    return "get";
                case FormMethod.Post:
                    return "post";
                default:
                    return "post";
            }
        }

        private static string GetInputTypeString(InputType inputType)
        {
            switch (inputType)
            {
                case InputType.CheckBox:
                    return "checkbox";
                case InputType.Hidden:
                    return "hidden";
                case InputType.Password:
                    return "password";
                case InputType.Radio:
                    return "radio";
                case InputType.Text:
                    return "text";
                default:
                    return "text";
            }
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "For consistency, all helpers are instance methods.")]
        public HtmlString Raw(string value)
        {
            return new HtmlString(value);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "For consistency, all helpers are instance methods.")]
        public HtmlString Raw(object value)
        {
            return new HtmlString(value == null ? null : value.ToString());
        }
    }
}
