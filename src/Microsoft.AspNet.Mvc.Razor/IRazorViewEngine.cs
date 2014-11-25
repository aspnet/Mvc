// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// A <see cref="IViewEngine"/> view engine used to render pages that use the Razor syntax.
    /// </summary>
    public interface IRazorViewEngine : IViewEngine
    {
        /// <summary>
        /// Finds an <see cref="IRazorPage"/> instance using the same view discovery semantics as
        /// <see cref="IViewEngine.FindPartialView(ActionContext, string)"/>.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <param name="page">The name or full path to the page.</param>
        /// <returns>A <see cref="RazorPageResult"/> describing the location operation.</returns>
        RazorPageResult FindPage(ActionContext context, string page);
    }
}