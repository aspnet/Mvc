// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
        private const string FallbackAttributeName = "fallback";
        private const string OptionalAttributeName = "optional";
        private object _model;
        private bool _hasModel;
        private bool _hasFor;
        private ModelExpression _for;
        private string _fallback;
        private bool _hasFallback;

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
        /// Cannot be used together with <see cref="Fallback"/>.
        /// </summary>
        [HtmlAttributeName(OptionalAttributeName)]
        public bool Optional { get; set; }

        /// <summary>
        /// View to lookup if main view cannot be located.
        /// Will throw if main view and fallback cannot be located.
        /// Cannot be used together with <see cref="Optional"/>.
        /// </summary>
        [HtmlAttributeName(FallbackAttributeName)]
        public string Fallback
        {
            get => _fallback;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException(nameof(Fallback));
                }
                _fallback = value;
                _hasFallback = true;
            }
        }

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

            if (Optional && _hasFallback)
            {
                throw new InvalidOperationException(
                    Resources.FormatPartialTagHelper_InvalidModelAttributes(
                    typeof(PartialTagHelper).FullName,
                    FallbackAttributeName,
                    OptionalAttributeName));
            }

            ViewEngineResult result;
            if (Optional)
            {
                result = Find(Name);
            }
            else if (_hasFallback)
            {
                result = FindPartialWithFallback();
            }
            else
            {
                result = FindPartial();
            }
            
            if (result.Success)
            {
                var model = ResolveModel();
                var viewBuffer = new ViewBuffer(_viewBufferScope, result.ViewName, ViewBuffer.PartialViewPageSize);
                using (var writer = new ViewBufferTextWriter(viewBuffer, Encoding.UTF8))
                {
                    await RenderPartialViewAsync(writer, model, result.View);
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

        private ViewEngineResult FindPartial()
        {
            var viewEngineResult = Find(Name);

            if (!viewEngineResult.Success)
            {
                var notFoundMessage = NotFoundMessage(Name, viewEngineResult.SearchedLocations);
                throw new InvalidOperationException(notFoundMessage);
            }

            return viewEngineResult;
        }
        
        private ViewEngineResult FindPartialWithFallback()
        {
            var viewEngineResult = Find(Name);
            var partialSearchedLocations = viewEngineResult.SearchedLocations;

            if (!viewEngineResult.Success)
            {
                viewEngineResult = Find(Fallback);
            }

            if (!viewEngineResult.Success)
            {
                var partialNotFoundMessage = NotFoundMessage(Name, partialSearchedLocations);
                var fallbackNotFoundMessage = NotFoundMessage(Fallback, viewEngineResult.SearchedLocations);
                var notFoundMessage = partialNotFoundMessage + Environment.NewLine + fallbackNotFoundMessage;
                throw new InvalidOperationException(notFoundMessage);
            }

            return viewEngineResult;
        }

        private ViewEngineResult Find(string partialName)
        {
            var viewEngineResult = _viewEngine.GetView(ViewContext.ExecutingFilePath, partialName, isMainPage: false);
            var getViewLocations = viewEngineResult.SearchedLocations;
            if (!viewEngineResult.Success)
            {
                viewEngineResult = _viewEngine.FindView(ViewContext, partialName, isMainPage: false);
            }

            if (!viewEngineResult.Success)
            {
                var searchedLocations = Enumerable.Concat(getViewLocations, viewEngineResult.SearchedLocations);
                return ViewEngineResult.NotFound(partialName, searchedLocations);
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

        private string NotFoundMessage(string name, IEnumerable<string> searchedLocations)
        {
            var locations = string.Empty;
            if (searchedLocations.Any())
            {
                locations += Environment.NewLine + string.Join(Environment.NewLine, searchedLocations);
            }
            
            return Resources.FormatViewEngine_PartialViewNotFound(name, locations);
        }
    }
}
