﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.AspNet.Mvc.ViewFeatures.Internal;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// An <see cref="IActionResult"/> which renders a view component to the response.
    /// </summary>
    public class ViewComponentResult : ActionResult
    {
        /// <summary>
        /// Gets or sets the arguments provided to the view component.
        /// </summary>
        public object[] Arguments { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MediaTypeHeaderValue"/> representing the Content-Type header of the response.
        /// </summary>
        public MediaTypeHeaderValue ContentType { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ITempDataDictionary"/> for this result.
        /// </summary>
        public ITempDataDictionary TempData { get; set; }

        /// <summary>
        /// Gets or sets the name of the view component to invoke. Will be ignored if <see cref="ViewComponentType"/>
        /// is set to a non-<c>null</c> value.
        /// </summary>
        public string ViewComponentName { get; set; }

        /// <summary>
        /// Gets or sets the type of the view component to invoke.
        /// </summary>
        public Type ViewComponentType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ViewDataDictionary"/> for this result.
        /// </summary>
        public ViewDataDictionary ViewData { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IViewEngine"/> used to locate views.
        /// </summary>
        /// <remarks>When <c>null</c>, an instance of <see cref="ICompositeViewEngine"/> from
        /// <c>ActionContext.HttpContext.RequestServices</c> is used.</remarks>
        public IViewEngine ViewEngine { get; set; }

        /// <inheritdoc />
        public override async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            var services = context.HttpContext.RequestServices;

            var htmlHelperOptions = services.GetRequiredService<IOptions<MvcViewOptions>>().Options.HtmlHelperOptions;
            var viewComponentHelper = services.GetRequiredService<IViewComponentHelper>();

            var viewData = ViewData;
            if (viewData == null)
            {
                var modelMetadataProvider = services.GetRequiredService<IModelMetadataProvider>();
                viewData = new ViewDataDictionary(modelMetadataProvider, context.ModelState);
            }

            var contentType = ContentType ?? ViewExecutor.DefaultContentType;
            if (contentType.Encoding == null)
            {
                // Do not modify the user supplied content type, so copy it instead
                contentType = contentType.Copy();
                contentType.Encoding = Encoding.UTF8;
            }

            if (StatusCode != null)
            {
                response.StatusCode = (int)StatusCode;
            }

            response.ContentType = contentType.ToString();

            using (var writer = new HttpResponseStreamWriter(response.Body, contentType.Encoding))
            {
                var viewContext = new ViewContext(
                    context,
                    NullView.Instance,
                    viewData,
                    TempData,
                    writer,
                    htmlHelperOptions);

                (viewComponentHelper as ICanHasViewContext)?.Contextualize(viewContext);

                if (ViewComponentType == null && ViewComponentName == null)
                {
                    throw new InvalidOperationException(Resources.FormatViewComponentResult_NameOrTypeMustBeSet(
                        nameof(ViewComponentName),
                        nameof(ViewComponentType)));
                }
                else if (ViewComponentType == null)
                {
                    await viewComponentHelper.RenderInvokeAsync(ViewComponentName, Arguments);
                }
                else
                {
                    await viewComponentHelper.RenderInvokeAsync(ViewComponentType, Arguments);
                }
            }
        }
    }
}
