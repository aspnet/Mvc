using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// Default implementation of non-generic portions of <see cref="IHtmlHelper{T}">.
    /// </summary>
    public class HtmlHelper : ICanHasViewContext
    {
        public static readonly string ValidationInputCssClassName = "input-validation-error";
        public static readonly string ValidationInputValidCssClassName = "input-validation-valid";
        public static readonly string ValidationMessageCssClassName = "field-validation-error";
        public static readonly string ValidationMessageValidCssClassName = "field-validation-valid";
        public static readonly string ValidationSummaryCssClassName = "validation-summary-errors";
        public static readonly string ValidationSummaryValidCssClassName = "validation-summary-valid";

        private ViewContext _viewContext;
        private IViewEngine _viewEngine;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlHelper"/> class.
        /// </summary>
        public HtmlHelper([NotNull] IViewEngine viewEngine, [NotNull] IModelMetadataProvider metadataProvider)
        {
            _viewEngine = viewEngine;
            MetadataProvider = metadataProvider;

            // Underscores are fine characters in id's.
            IdAttributeDotReplacement = "_";
        }

        public string IdAttributeDotReplacement { get; set; }

        public HttpContext HttpContext { get; private set; }

        public ViewContext ViewContext
        {
            get
            {
                if (_viewContext == null)
                {
                    throw new InvalidOperationException(Resources.HtmlHelper_NotContextualized);
                }

                return _viewContext;
            }
            private set
            {
                _viewContext = value;
            }
        }

        public dynamic ViewBag
        {
            get
            {
                return ViewContext.ViewBag;
            }
        }

        public ViewDataDictionary ViewData
        {
            get
            {
                return ViewContext.ViewData;
            }
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
            IDictionary<string, object> valuesAsDictionary = htmlAttributes as IDictionary<string, object>;
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
                        object val = prop.GetValue(htmlAttributes);
                        result.Add(prop.Name, val);
                    }
                }
            }

            return result;
        }

        public virtual void Contextualize([NotNull] ViewContext viewContext)
        {
            ViewContext = viewContext;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "For consistency, all helpers are instance methods.")]
        public string Encode(string value)
        {
            return (!string.IsNullOrEmpty(value)) ? WebUtility.HtmlEncode(value) : string.Empty;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "For consistency, all helpers are instance methods.")]
        public string Encode(object value)
        {
            return value != null ? WebUtility.HtmlEncode(value.ToString()) : string.Empty;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "For consistency, all helpers are instance methods.")]
        public string FormatValue(object value, string format)
        {
            return ViewDataDictionary.FormatValue(value, format);
        }

        public string GenerateIdFromName([NotNull] string name)
        {
            return TagBuilder.CreateSanitizedId(name, IdAttributeDotReplacement);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames",
            MessageId = "1#", Justification = "This is a shipped API.")]
        public virtual HtmlString Name(string name)
        {
            var fullName = ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            return new HtmlString(Encode(fullName));
        }

        public async Task<HtmlString> PartialAsync([NotNull] string partialViewName, object model, ViewDataDictionary viewData)
        {
            using (var writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                await RenderPartialCoreAsync(partialViewName, model, viewData, writer);

                return new HtmlString(writer.ToString());
            }
        }

        public Task RenderPartialAsync([NotNull] string partialViewName, object model, ViewDataDictionary viewData)
        {
            return RenderPartialCoreAsync(partialViewName, model, viewData, ViewContext.Writer);
        }

        protected virtual async Task RenderPartialCoreAsync([NotNull] string partialViewName, 
                                                               object model, 
                                                               ViewDataDictionary viewData, 
                                                               TextWriter writer)
        {
            // Determine which ViewData we should use to construct a new ViewData
            var baseViewData = viewData ?? ViewData;

            var newViewData = new ViewDataDictionary(baseViewData, model);

            var newViewContext = new ViewContext(ViewContext)
            {
                ViewData = newViewData,
                Writer = writer
            };

            var viewEngineResult = await _viewEngine.FindPartialView(newViewContext.ViewEngineContext, partialViewName);

            await viewEngineResult.View.RenderAsync(newViewContext);
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

        /// <inheritdoc />
        public HtmlString TextBox(string name, object value, string format, IDictionary<string, object> htmlAttributes)
        {
            return RenderTextBox(metadata: null, name: name, value: value, format: format,
                htmlAttributes: htmlAttributes);
        }

        protected string EvalString(string key, string format)
        {
            return Convert.ToString(ViewData.Eval(key, format), CultureInfo.CurrentCulture);
        }

        protected object GetModelStateValue(string key, Type destinationType)
        {
            ModelState modelState;
            if (ViewData.ModelState.TryGetValue(key, out modelState))
            {
                if (modelState.Value != null)
                {
                    return modelState.Value.ConvertTo(destinationType, culture: null);
                }
            }

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
            // TODO: Add validation attributes to input helpers.
            return new Dictionary<string, object>();
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
                throw new ArgumentException(Resources.ArgumentNullOrEmpty, "name");
            }

            var tagBuilder = new TagBuilder("input");
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("type", GetInputTypeString(inputType));
            tagBuilder.MergeAttribute("name", fullName, true);

            var valueParameter = FormatValue(value, format);
            switch (inputType)
            {
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

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "For consistency, all helpers are instance methods.")]
        public HtmlString Raw(string value)
        {
            return new HtmlString(value);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "For consistency, all helpers are instance methods.")]
        public HtmlString Raw(object value)
        {
            return new HtmlString(value == null ? null : value.ToString());
        }
    }
}
