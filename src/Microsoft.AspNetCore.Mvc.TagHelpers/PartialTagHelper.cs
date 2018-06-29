﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// Renders a partial view.
    /// </summary>
    [HtmlTargetElement("partial", Attributes = "name", TagStructure = TagStructure.WithoutEndTag)]
    public class PartialTagHelper : TagHelper
    {
        private const string ForAttributeName = "for";
        private const string ModelAttributeName = "model";
        private const string OptionalAttributeName = "optional";
        private object _model;
        private bool _hasModel;
        private bool _hasFor;
        private ModelExpression _for;

        private readonly ICompositeViewEngine _viewEngine;
        private readonly IViewBufferScope _viewBufferScope;

        public PartialTagHelper(
            ICompositeViewEngine viewEngine,
#pragma warning disable PUB0001 // Pubternal type in public API
            IViewBufferScope viewBufferScope
#pragma warning restore PUB0001
            )
        {
            _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
            _viewBufferScope = viewBufferScope ?? throw new ArgumentNullException(nameof(viewBufferScope));
        }

        /// <summary>
        /// The name or path of the partial view that is rendered to the response.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// An expression to be evaluated against the current model. Cannot be used together with <see cref="Model"/>.
        /// </summary>
        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For
        {
            get => _for;
            set
            {
                _for = value ?? throw new ArgumentNullException(nameof(value));
                _hasFor = true;
            }
        }

        /// <summary>
        /// The model to pass into the partial view. Cannot be used together with <see cref="For"/>.
        /// </summary>
        [HtmlAttributeName(ModelAttributeName)]
        public object Model
        {
            get => _model;
            set
            {
                _model = value;
                _hasModel = true;
            }
        }

        /// <summary>
        /// When optional, executing the tag helper will no-op if the view cannot be located. 
        /// Otherwise will throw stating the view could not be found.
        /// </summary>
        [HtmlAttributeName(OptionalAttributeName)]
        public bool Optional { get; set; }

        /// <summary>
        /// A <see cref="ViewDataDictionary"/> to pass into the partial view.
        /// </summary>
        public ViewDataDictionary ViewData { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <inheritdoc />
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var viewEngineResult = FindView();

            if (viewEngineResult.Success)
            {
                var model = ResolveModel();
                var viewBuffer = new ViewBuffer(_viewBufferScope, Name, ViewBuffer.PartialViewPageSize);
                using (var writer = new ViewBufferTextWriter(viewBuffer, Encoding.UTF8))
                {
                    await RenderPartialViewAsync(writer, model, viewEngineResult.View);
                    output.Content.SetHtmlContent(viewBuffer);
                }
            }

            // Reset the TagName. We don't want `partial` to render.
            output.TagName = null;
        }

        // Internal for testing
        internal object ResolveModel()
        {
            // 1. Disallow specifying values for both Model and For
            // 2. If a Model was assigned, use it even if it's null.
            // 3. For cannot have a null value. Use it if it was assigned to.
            // 4. Fall back to using the Model property on ViewContext.ViewData if none of the above conditions are met.

            if (_hasFor && _hasModel)
            {
                throw new InvalidOperationException(
                    Resources.FormatPartialTagHelper_InvalidModelAttributes(
                        typeof(PartialTagHelper).FullName,
                        ForAttributeName,
                        ModelAttributeName));
            }

            if (_hasModel)
            {
                return Model;
            }

            if (_hasFor)
            {
                return For.Model;
            }

            // A value for Model or For was not specified, fallback to the ViewContext's ViewData model.
            return ViewContext.ViewData.Model;
        }

        private ViewEngineResult FindView()
        {
            var viewEngineResult = _viewEngine.GetView(ViewContext.ExecutingFilePath, Name, isMainPage: false);
            var getViewLocations = viewEngineResult.SearchedLocations;
            if (!viewEngineResult.Success)
            {
                viewEngineResult = _viewEngine.FindView(ViewContext, Name, isMainPage: false);
            }

            if (!viewEngineResult.Success && !Optional)
            {
                var searchedLocations = Enumerable.Concat(getViewLocations, viewEngineResult.SearchedLocations);
                var locations = string.Empty;
                if (searchedLocations.Any())
                {
                    locations += Environment.NewLine + string.Join(Environment.NewLine, searchedLocations);
                }

                throw new InvalidOperationException(
                    Resources.FormatViewEngine_PartialViewNotFound(Name, locations));
            }

            return viewEngineResult;
        }

        private async Task RenderPartialViewAsync(TextWriter writer, object model, IView view)
        {
            // Determine which ViewData we should use to construct a new ViewData
            var baseViewData = ViewData ?? ViewContext.ViewData;
            var newViewData = new ViewDataDictionary<object>(baseViewData, model);
            var partialViewContext = new ViewContext(ViewContext, view, newViewData, writer);

            if (For?.Name != null)
            {
                newViewData.TemplateInfo.HtmlFieldPrefix = newViewData.TemplateInfo.GetFullHtmlFieldName(For.Name);
            }

            using (view as IDisposable)
            {
                await view.RenderAsync(partialViewContext);
            }
        }
    }
}
