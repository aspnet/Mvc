// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering.Expressions;
using Microsoft.AspNet.Mvc.Rendering.Html;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// Default implementation of IHtmlHelper.
    /// </summary>
    public class HtmlHelper : IHtmlHelper, INeedContext
    {
        // Previously an internal const string in HttpRequestExtensions
        public static readonly string XHttpMethodOverrideKey = "X-HTTP-Method-Override";

        public static readonly string ValidationInputCssClassName = "input-validation-error";
        public static readonly string ValidationInputValidCssClassName = "input-validation-valid";
        public static readonly string ValidationMessageCssClassName = "field-validation-error";
        public static readonly string ValidationMessageValidCssClassName = "field-validation-valid";
        public static readonly string ValidationSummaryCssClassName = "validation-summary-errors";
        public static readonly string ValidationSummaryValidCssClassName = "validation-summary-valid";

        private IHtmlSettings _htmlSettings;

        public HtmlHelper([NotNull] IModelMetadataProvider metadataProvider,
            [NotNull] IHtmlSettings htmlSettings)
        {
            _htmlSettings = htmlSettings;
            ClientValidationRuleFactory = DefaultClientValidationRuleFactory;
            MetadataProvider = metadataProvider;
        }

        /// <summary>
        /// Temporary constructor to allow creation of <see cref="HtmlHelper{TModel}"/> instances using
        /// dependency injection for most of the heavy lifting.
        /// </summary>
        /// <param name="original">Original <see cref="HtmlHelper"/> instance to duplicate.</param>
        protected HtmlHelper([NotNull] IHtmlHelper original)
        {
            ViewContext = original.ViewContext;

            var originalHelper = original as HtmlHelper;
            if (originalHelper != null)
            {
                _htmlSettings = originalHelper._htmlSettings;
                ClientValidationRuleFactory = originalHelper.ClientValidationRuleFactory;
                MetadataProvider = originalHelper.MetadataProvider;
            }
        }

        // Provide previously-static properties on the instance.
        #region IHtmlSettings
        /// <inheritdoc />
        public bool ClientValidationEnabled
        {
            get
            {
                return _htmlSettings.ClientValidationEnabled;
            }
            set
            {
                _htmlSettings.ClientValidationEnabled = value;
            }
        }

        /// <inheritdoc />
        public string IdAttributeDotReplacement
        {
            get
            {
                return _htmlSettings.IdAttributeDotReplacement;
            }
            set
            {
                _htmlSettings.IdAttributeDotReplacement = value;
            }
        }

        /// <inheritdoc />
        public bool UnobtrusiveJavaScriptEnabled
        {
            get
            {
                return _htmlSettings.UnobtrusiveJavaScriptEnabled;
            }
            set
            {
                _htmlSettings.UnobtrusiveJavaScriptEnabled = value;
            }
        }
        #endregion

        public HttpContext HttpContext
        {
            get
            {
                return ViewContext.HttpContext;
            }
        }

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

        private Func<string, ModelMetadata, IEnumerable<ModelClientValidationRule>> ClientValidationRuleFactory
        {
            get;
            set;
        }

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

        /// <summary>
        /// Creates an HTML element ID using the specified element name and a string that replaces dots in the name.
        /// </summary>
        /// <param name="name">The name of the HTML element.</param>
        /// <returns>The ID of the HTML element.</returns>
        public static string GenerateIdFromName(string name, [NotNull] string idAttributeDotReplacement)
        {
            return TagBuilder.CreateSanitizedId(name, idAttributeDotReplacement);
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

        public virtual void Contextualize(ViewContext viewContext)
        {
            ViewContext = viewContext;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "For consistency, all helpers are instance methods.")]
        public string Encode(string value)
        {
            return (!string.IsNullOrEmpty(value)) ? WebUtility.HtmlEncode(value) : string.Empty;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "For consistency, all helpers are instance methods.")]
        public string Encode(object value)
        {
            return value != null ? WebUtility.HtmlEncode(value.ToString()) : string.Empty;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "For consistency, all helpers are instance methods.")]
        public string FormatValue(object value, string format)
        {
            return ViewDataEvaluator.FormatValue(value, format);
        }

        /// <inheritdoc />
        public string GenerateIdFromName(string name)
        {
            return TagBuilder.CreateSanitizedId(name, IdAttributeDotReplacement);
        }

        /// <inheritdoc />
        public HtmlString HttpMethodOverride(HttpVerbs httpVerb)
        {
            string httpMethod;
            switch (httpVerb)
            {
                case HttpVerbs.Delete:
                    httpMethod = "DELETE";
                    break;
                case HttpVerbs.Head:
                    httpMethod = "HEAD";
                    break;
                case HttpVerbs.Put:
                    httpMethod = "PUT";
                    break;
                case HttpVerbs.Patch:
                    httpMethod = "PATCH";
                    break;
                case HttpVerbs.Options:
                    httpMethod = "OPTIONS";
                    break;
                default:
                    throw new ArgumentException(Resources.HtmlHelper_InvalidHttpVerb, "httpVerb");
            }

            return HttpMethodOverride(httpMethod);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "For consistency, all helpers are instance methods.")]
        public HtmlString HttpMethodOverride([NotNull] string httpMethod)
        {
            if (string.IsNullOrEmpty(httpMethod))
            {
                throw new ArgumentException(Resources.Common_NullOrEmpty, "httpMethod");
            }
            if (string.Equals(httpMethod, "GET", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(httpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(Resources.HtmlHelper_InvalidHttpMethod, "httpMethod");
            }

            var tagBuilder = new TagBuilder("input");
            tagBuilder.Attributes["type"] = "hidden";
            tagBuilder.Attributes["name"] = XHttpMethodOverrideKey;
            tagBuilder.Attributes["value"] = httpMethod;

            return tagBuilder.ToHtmlString(TagRenderMode.SelfClosing);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "For consistency, all helpers are instance methods.")]
        public HtmlString Raw(string value)
        {
            return new HtmlString(value);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "For consistency, all helpers are instance methods.")]
        public HtmlString Raw(object value)
        {
            return new HtmlString(value == null ? null : value.ToString());
        }

        #region Input helpers
        /// <inheritdoc />
        public HtmlString CheckBox(string name, bool? isChecked, IDictionary<string, object> htmlAttributes)
        {
            return RenderCheckBox(metadata: null, name: name, isChecked: isChecked, htmlAttributes: htmlAttributes);
        }

        /// <inheritdoc />
        public HtmlString Hidden(string name, object value, IDictionary<string, object> htmlAttributes)
        {
            return RenderHidden(metadata: null, name: name, value: value, useViewData: (value == null),
                htmlAttributes: htmlAttributes);
        }

        /// <inheritdoc />
        public HtmlString Password(string name, object value, IDictionary<string, object> htmlAttributes)
        {
            return RenderPassword(metadata: null, name: name, value: value, htmlAttributes: htmlAttributes);
        }

        /// <inheritdoc />
        public HtmlString RadioButton(string name, object value, bool? isChecked,
            IDictionary<string, object> htmlAttributes)
        {
            return RenderRadioButton(metadata: null, name: name, value: value, isChecked: isChecked,
                htmlAttributes: htmlAttributes);
        }

        /// <inheritdoc />
        public HtmlString TextBox(string name, object value, string format, IDictionary<string, object> htmlAttributes)
        {
            return RenderTextBox(metadata: null, name: name, value: value, format: format,
                htmlAttributes: htmlAttributes);
        }
        #endregion

        #region Helper methods
        protected bool EvalBoolean(string key)
        {
            return Convert.ToBoolean(ViewDataEvaluator.Eval(ViewContext.ViewData, key), CultureInfo.InvariantCulture);
        }

        protected string EvalString(string key)
        {
            return Convert.ToString(ViewDataEvaluator.Eval(ViewContext.ViewData, key), CultureInfo.CurrentCulture);
        }

        protected string EvalString(string key, string format)
        {
            return Convert.ToString(ViewDataEvaluator.Eval(ViewContext.ViewData, key, format),
                CultureInfo.CurrentCulture);
        }

        protected static IView FindPartialView(ViewContext viewContext, string partialViewName, IViewEngine viewEngine)
        {
            var result = viewEngine.FindView(viewContext, partialViewName).Result;
            if (result.View != null)
            {
                return result.View;
            }

            var locationsText = new StringBuilder();
            foreach (string location in result.SearchedLocations)
            {
                locationsText.AppendLine();
                locationsText.Append(location);
            }

            throw new InvalidOperationException(
                Resources.FormatCommon_PartialViewNotFound(partialViewName, locationsText));
        }

        protected FormContext GetFormContextForClientValidation()
        {
            return ClientValidationEnabled ? ViewContext.FormContext : null;
        }

        protected string GetFullHtmlFieldId(string partialFieldName)
        {
            return GenerateIdFromName(ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(partialFieldName));
        }

        protected object GetModelStateValue(string key, Type destinationType)
        {
            // No ModelState yet ...
            return null;
        }

        protected IDictionary<string, object> GetValidationAttributes(string name)
        {
            return GetValidationAttributes(name, metadata: null);
        }

        // Only render attributes if unobtrusive client-side validation is enabled, and then only if we've
        // never rendered validation for a field with this name in this form. Also, if there's no form context,
        // then we can't render the attributes (we'd have no <form> to attach them to).
        protected IDictionary<string, object> GetValidationAttributes(string name, ModelMetadata metadata)
        {
            // The ordering of these 3 checks (and the early exits) is for performance reasons.
            if (!UnobtrusiveJavaScriptEnabled)
            {
                return null;
            }

            FormContext formContext = GetFormContextForClientValidation();
            if (formContext == null)
            {
                return null;
            }

            string fullName = ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            if (formContext.RenderedField(fullName))
            {
                return null;
            }

            formContext.RenderedField(fullName, true);

            if (metadata == null)
            {
                metadata =
                    ExpressionMetadataProvider.FromStringExpression(name, ViewContext.ViewData, MetadataProvider);
            }

            IEnumerable<ModelClientValidationRule> clientRules = ClientValidationRuleFactory(name, metadata);
            return AttributeProvider.GetValidationAttributes(clientRules);
        }

        private IEnumerable<ModelClientValidationRule> DefaultClientValidationRuleFactory(string name,
            [NotNull] ModelMetadata metadata)
        {
            // TODO: Need a few validator providers.
            return Enumerable.Empty<ModelClientValidationRule>();
        }
        #endregion

        #region Helper methods for input helpers
        protected virtual HtmlString RenderCheckBox(ModelMetadata metadata, string name, bool? isChecked,
            IDictionary<string, object> htmlAttributes)
        {
            if (metadata != null)
            {
                // CheckBoxFor() case. That API does not support passing isChecked directly.
                Contract.Assert(!isChecked.HasValue);

                if (metadata.Model != null)
                {
                    bool modelChecked;
                    if (Boolean.TryParse(metadata.Model.ToString(), out modelChecked))
                    {
                        isChecked = modelChecked;
                    }
                }
            }

            var explicitValue = isChecked.HasValue;
            if (explicitValue && htmlAttributes != null)
            {
                // Explicit value must override dictionary
                htmlAttributes.Remove("checked");
            }

            return RenderInput(InputType.CheckBox,
                metadata,
                name,
                value: "true",
                useViewData: !explicitValue,
                isChecked: isChecked ?? false,
                setId: true,
                isExplicitValue: false,
                format: null,
                htmlAttributes: htmlAttributes);
        }

        protected virtual HtmlString RenderHidden(ModelMetadata metadata, string name, object value, bool useViewData,
            IDictionary<string, object> htmlAttributes)
        {
            var byteArrayValue = value as byte[];
            if (byteArrayValue != null)
            {
                value = Convert.ToBase64String(byteArrayValue);
            }

            return RenderInput(InputType.Hidden,
                metadata,
                name,
                value,
                useViewData,
                isChecked: false,
                setId: true,
                isExplicitValue: true,
                format: null,
                htmlAttributes: htmlAttributes);
        }

        protected virtual HtmlString RenderPassword(ModelMetadata metadata, string name, object value,
            IDictionary<string, object> htmlAttributes)
        {
            return RenderInput(InputType.Password,
                metadata,
                name,
                value,
                useViewData: false,
                isChecked: false,
                setId: true,
                isExplicitValue: true,
                format: null,
                htmlAttributes: htmlAttributes);
        }

        protected virtual HtmlString RenderRadioButton(ModelMetadata metadata, string name, object value,
            bool? isChecked, IDictionary<string, object> htmlAttributes)
        {
            if (metadata == null)
            {
                // RadioButton() case. Do not override checked attribute if isChecked is implicit.
                if (!isChecked.HasValue && htmlAttributes != null && !htmlAttributes.ContainsKey("checked"))
                {
                    // Note value may be null if isChecked is non-null.
                    if (value == null)
                    {
                        throw new ArgumentNullException("value");
                    }

                    // isChecked not provided nor found in the given attributes; fall back to view data.
                    var valueString = Convert.ToString(value, CultureInfo.CurrentCulture);
                    isChecked = !string.IsNullOrEmpty(name) &&
                        string.Equals(EvalString(name), valueString, StringComparison.OrdinalIgnoreCase);
                }
            }
            else
            {
                // RadioButtonFor() case. That API does not support passing isChecked directly.
                Contract.Assert(!isChecked.HasValue);
                if (value == null)
                {
                    // Need a value to determine isChecked.
                    throw new ArgumentNullException("value");
                }

                var model = metadata.Model;
                var valueString = Convert.ToString(value, CultureInfo.CurrentCulture);
                isChecked = model != null &&
                    string.Equals(model.ToString(), valueString, StringComparison.OrdinalIgnoreCase);
            }

            var explicitValue = isChecked.HasValue;
            if (explicitValue && htmlAttributes != null)
            {
                // Explicit value must override dictionary
                htmlAttributes.Remove("checked");
            }

            return RenderInput(InputType.Radio,
                metadata,
                name,
                value,
                useViewData: false,
                isChecked: isChecked ?? false,
                setId: true,
                isExplicitValue: true,
                format: null,
                htmlAttributes: htmlAttributes);
        }

        protected virtual HtmlString RenderTextBox(ModelMetadata metadata, string name, object value, string format,
            IDictionary<string, object> htmlAttributes)
        {
            return RenderInput(InputType.Text,
                metadata,
                name,
                value,
                useViewData: (metadata == null && value == null),
                isChecked: false,
                setId: true,
                isExplicitValue: true,
                format: format,
                htmlAttributes: htmlAttributes);
        }

        protected virtual HtmlString RenderInput(InputType inputType, ModelMetadata metadata, string name,
            object value, bool useViewData, bool isChecked, bool setId, bool isExplicitValue, string format,
            IDictionary<string, object> htmlAttributes)
        {
            var fullName = ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            if (string.IsNullOrEmpty(fullName))
            {
                throw new ArgumentException(Resources.Common_NullOrEmpty, "name");
            }

            var tagBuilder = new TagBuilder("input");
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("type", GetInputTypeString(inputType));
            tagBuilder.MergeAttribute("name", fullName, true);

            var valueParameter = FormatValue(value, format);
            var usedModelState = false;

            switch (inputType)
            {
                case InputType.CheckBox:
                    var modelStateWasChecked = GetModelStateValue(fullName, typeof(bool)) as bool?;
                    if (modelStateWasChecked.HasValue)
                    {
                        isChecked = modelStateWasChecked.Value;
                        usedModelState = true;
                    }

                    goto case InputType.Radio;

                case InputType.Radio:
                    if (!usedModelState)
                    {
                        var modelStateValue = GetModelStateValue(fullName, typeof(string)) as string;
                        if (modelStateValue != null)
                        {
                            isChecked = string.Equals(modelStateValue, valueParameter, StringComparison.Ordinal);
                            usedModelState = true;
                        }
                    }

                    if (!usedModelState && useViewData)
                    {
                        isChecked = EvalBoolean(fullName);
                    }

                    if (isChecked)
                    {
                        tagBuilder.MergeAttribute("checked", "checked");
                    }

                    tagBuilder.MergeAttribute("value", valueParameter, isExplicitValue);
                    break;

                case InputType.Password:
                    if (value != null)
                    {
                        tagBuilder.MergeAttribute("value", valueParameter, isExplicitValue);
                    }

                    break;

                default:
                    var attemptedValue = (string)GetModelStateValue(fullName, typeof(string));
                    tagBuilder.MergeAttribute("value",
                        attemptedValue ?? ((useViewData) ? EvalString(fullName, format) : valueParameter),
                        isExplicitValue);
                    break;
            }

            if (setId)
            {
                tagBuilder.GenerateId(fullName, IdAttributeDotReplacement);
            }

            // If there are any errors for a named field, we add the CSS attribute.
            ModelState modelState;
            if (ViewContext.ViewData.ModelState.TryGetValue(fullName, out modelState))
            {
                if (modelState.Errors.Count > 0)
                {
                    tagBuilder.AddCssClass(ValidationInputCssClassName);
                }
            }

            tagBuilder.MergeAttributes(GetValidationAttributes(name, metadata));

            if (inputType == InputType.CheckBox)
            {
                // Render an additional <input type="hidden".../> for checkboxes. This
                // addresses scenarios where unchecked checkboxes are not sent in the request.
                // Sending a hidden input makes it possible to know that the checkbox was present
                // on the page when the request was submitted.
                var inputItemBuilder = new StringBuilder();
                inputItemBuilder.Append(tagBuilder.ToString(TagRenderMode.SelfClosing));

                var hiddenInput = new TagBuilder("input");
                hiddenInput.MergeAttribute("type", GetInputTypeString(InputType.Hidden));
                hiddenInput.MergeAttribute("name", fullName);
                hiddenInput.MergeAttribute("value", "false");
                inputItemBuilder.Append(hiddenInput.ToString(TagRenderMode.SelfClosing));
                return new HtmlString(inputItemBuilder.ToString());
            }

            return tagBuilder.ToHtmlString(TagRenderMode.SelfClosing);
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
        #endregion
    }
}
