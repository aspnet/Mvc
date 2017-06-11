// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    /// <summary>
    /// Context for view execution.
    /// </summary>
    public class ViewContext : ActionContext
    {
        private FormContext _formContext;
        private DynamicViewData _viewBag;

        /// <summary>
        /// Creates an empty <see cref="ViewContext"/>.
        /// </summary>
        /// <remarks>
        /// The default constructor is provided for unit test purposes only.
        /// </remarks>
        public ViewContext()
        {
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), ModelState);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ViewContext"/>.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/>.</param>
        /// <param name="view">The <see cref="IView"/> being rendered.</param>
        /// <param name="viewData">The <see cref="ViewDataDictionary"/>.</param>
        /// <param name="tempData">The <see cref="ITempDataDictionary"/>.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to render output to.</param>
        /// <param name="htmlHelperOptions">The <see cref="HtmlHelperOptions"/> to apply to this instance.</param>
        public ViewContext(
            ActionContext actionContext,
            IView view,
            ViewDataDictionary viewData,
            ITempDataDictionary tempData,
            TextWriter writer,
            HtmlHelperOptions htmlHelperOptions)
            : base(actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            if (viewData == null)
            {
                throw new ArgumentNullException(nameof(viewData));
            }

            if (tempData == null)
            {
                throw new ArgumentNullException(nameof(tempData));
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (htmlHelperOptions == null)
            {
                throw new ArgumentNullException(nameof(htmlHelperOptions));
            }

            View = view;
            ViewData = viewData;
            TempData = tempData;
            Writer = writer;

            FormContext = new FormContext();

            ClientValidationEnabled = htmlHelperOptions.ClientValidationEnabled;
            Html5DateRenderingMode = htmlHelperOptions.Html5DateRenderingMode;
            ValidationSummaryMessageElement = htmlHelperOptions.ValidationSummaryMessageElement;
            ValidationMessageElement = htmlHelperOptions.ValidationMessageElement;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ViewContext"/>.
        /// </summary>
        /// <param name="viewContext">The <see cref="ViewContext"/> to copy values from.</param>
        /// <param name="view">The <see cref="IView"/> being rendered.</param>
        /// <param name="viewData">The <see cref="ViewDataDictionary"/>.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to render output to.</param>
        public ViewContext(
            ViewContext viewContext,
            IView view,
            ViewDataDictionary viewData,
            TextWriter writer)
            : base(viewContext)
        {
            if (viewContext == null)
            {
                throw new ArgumentNullException(nameof(viewContext));
            }

            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            if (viewData == null)
            {
                throw new ArgumentNullException(nameof(viewData));
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            FormContext = viewContext.FormContext;

            ClientValidationEnabled = viewContext.ClientValidationEnabled;
            Html5DateRenderingMode = viewContext.Html5DateRenderingMode;
            ValidationSummaryMessageElement = viewContext.ValidationSummaryMessageElement;
            ValidationMessageElement = viewContext.ValidationMessageElement;

            ExecutingFilePath = viewContext.ExecutingFilePath;
            View = view;
            ViewData = viewData;
            TempData = viewContext.TempData;
            Writer = writer;
        }

        /// <summary>
        /// Gets or sets the <see cref="FormContext"/> for the form element being rendered.
        /// A default context is returned if no form is currently being rendered.
        /// </summary>
        public virtual FormContext FormContext
        {
            get => _formContext;

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _formContext = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether client-side validation is enabled.
        /// </summary>
        public bool ClientValidationEnabled { get; set; }

        /// <summary>
        /// Set this property to <see cref="Html5DateRenderingMode.CurrentCulture" /> to have templated helpers such as
        /// <see cref="IHtmlHelper.Editor" /> and <see cref="IHtmlHelper{TModel}.EditorFor" /> render date and time
        /// values using the current culture. By default, these helpers render dates and times as RFC 3339 compliant strings.
        /// </summary>
        public Html5DateRenderingMode Html5DateRenderingMode { get; set; }

        /// <summary>
        /// Element name used to wrap a top-level message generated by <see cref="IHtmlHelper.ValidationSummary"/> and
        /// other overloads.
        /// </summary>
        public string ValidationSummaryMessageElement { get; set; }

        /// <summary>
        /// Element name used to wrap a top-level message generated by <see cref="IHtmlHelper.ValidationMessage"/> and
        /// other overloads.
        /// </summary>
        public string ValidationMessageElement { get; set; }

        /// <summary>
        /// Gets the dynamic view bag.
        /// </summary>
        public dynamic ViewBag
        {
            get
            {
                if (_viewBag == null)
                {
                    _viewBag = new DynamicViewData(() => ViewData);
                }

                return _viewBag;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IView"/> currently being rendered, if any.
        /// </summary>
        public IView View { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ViewDataDictionary"/>.
        /// </summary>
        public ViewDataDictionary ViewData { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ITempDataDictionary"/> instance.
        /// </summary>
        public ITempDataDictionary TempData { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TextWriter"/> used to write the output.
        /// </summary>
        public TextWriter Writer { get; set; }

        /// <summary>
        /// Gets or sets the path of the view file currently being rendered.
        /// </summary>
        /// <remarks>
        /// The rendering of a view may involve one or more files (e.g. _ViewStart, Layouts etc).
        /// This property contains the path of the file currently being rendered.
        /// </remarks>
        public string ExecutingFilePath { get; set; }

        public FormContext GetFormContextForClientValidation()
        {
            return ClientValidationEnabled ? FormContext : null;
        }
    }
}
