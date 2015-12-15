// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Html;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Supports the rendering of View Components in a view.
    /// </summary>
    public interface IViewComponentHelper
    {
        /// <summary>
        /// Invokes a View Component with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the View Component.</param>
        /// <param name="arguments">Arguments to be passed to tbe invoked View Component method.</param>
        /// <returns>A <see cref="Task"/> that on completion returns the rendered <see cref="IHtmlContent" />.
        /// </returns>
        Task<IHtmlContent> InvokeAsync(string name, object arguments);

        /// <summary>
        /// Invokes a View Component of type <paramref name="componentType" />.
        /// </summary>
        /// <param name="componentType">The View Component <see cref="Type"/>.</param>
        /// <param name="arguments">Arguments to be passed to tbe invoked View Component method.</param>
        /// <returns>A <see cref="Task"/> that on completion returns the rendered <see cref="IHtmlContent" />.
        /// </returns>
        Task<IHtmlContent> InvokeAsync(Type componentType, object arguments);
    }
}
