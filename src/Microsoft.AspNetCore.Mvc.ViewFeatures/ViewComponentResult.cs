// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// An <see cref="IActionResult"/> which renders a view component to the response.
    /// </summary>
    public class ViewComponentResult : ActionResult
    {
        /// <summary>
        /// Gets or sets the arguments provided to the view component.
        /// </summary>
        public object Arguments { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public int? StatusCode { get; set; }

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
        /// Get the view data model.
        /// </summary>
        public object Model => ViewData?.Model;

        /// <summary>
        /// Gets or sets the <see cref="ViewDataDictionary"/> for this result.
        /// </summary>
        public ViewDataDictionary ViewData { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ITempDataDictionary"/> for this result.
        /// </summary>
        public ITempDataDictionary TempData { get; set; }

        /// <summary>
        /// <para>
        /// This property is unused and will be removed in the next major version.
        /// </para>
        /// <para>
        /// Gets or sets the <see cref="IViewEngine"/> used to locate views.
        /// </para>
        /// </summary>
        /// <remarks>When <c>null</c>, an instance of <see cref="ICompositeViewEngine"/> from
        /// <c>ActionContext.HttpContext.RequestServices</c> is used.</remarks>
        [Obsolete("This property is unused and will be removed in the next major version.")]
        public IViewEngine ViewEngine { get; set; }

        /// <summary>
        /// Gets or sets the Content-Type header for the response.
        /// </summary>
        public string ContentType { get; set; }

        /// <inheritdoc />
        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var services = context.HttpContext.RequestServices;
            var executor = services.GetRequiredService<ViewComponentResultExecutor>();
            return executor.ExecuteAsync(context, this);
        }
    }
}
