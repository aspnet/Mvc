// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Html;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// Extension methods for <see cref="IViewComponentHelper"/>.
    /// </summary>
    public static class ViewComponentHelperExtensions
    {
        /// <summary>
        /// Invokes a View Component with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the View Component.</param>
        /// <returns>A <see cref="Task"/> that on completion returns the rendered <see cref="IHtmlContent" />.
        /// </returns>
        public static Task<IHtmlContent> InvokeAsync(this IViewComponentHelper helper, string name)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            return helper.InvokeAsync(name, arguments: null);
        }

        /// <summary>
        /// Invokes a View Component of type <paramref name="componentType" />.
        /// </summary>
        /// <param name="componentType">The View Component <see cref="Type"/>.</param>
        /// <returns>A <see cref="Task"/> that on completion returns the rendered <see cref="IHtmlContent" />.
        /// </returns>
        public static Task<IHtmlContent> InvokeAsync(this IViewComponentHelper helper, Type componentType)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            return helper.InvokeAsync(componentType, arguments: null);
        }

        /// <summary>
        /// Invokes a View Component of type <typeparam name="TComponent"/>.
        /// </summary>
        /// <param name="helper">The <see cref="IViewComponentHelper"/>.</param>
        /// <param name="arguments">Arguments to be passed to tbe invoked View Component method.</param>
        /// <typeparam name="TComponent">The <see cref="Type"/> of the View Component.</typeparam>
        /// <returns>A <see cref="Task"/> that on completion returns the rendered <see cref="IHtmlContent" />.
        /// </returns>
        public static Task<IHtmlContent> InvokeAsync<TComponent>(this IViewComponentHelper helper, object arguments)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            return helper.InvokeAsync(typeof(TComponent), arguments);
        }

        /// <summary>
        /// Invokes a View Component of type <typeparam name="TComponent"/>.
        /// </summary>
        /// <param name="helper">The <see cref="IViewComponentHelper"/>.</param>
        /// <typeparam name="TComponent">The <see cref="Type"/> of the View Component.</typeparam>
        /// <returns>A <see cref="Task"/> that on completion returns the rendered <see cref="IHtmlContent" />.
        /// </returns>
        public static Task<IHtmlContent> InvokeAsync<TComponent>(this IViewComponentHelper helper)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            return helper.InvokeAsync(typeof(TComponent), arguments: null);
        }
    }
}
