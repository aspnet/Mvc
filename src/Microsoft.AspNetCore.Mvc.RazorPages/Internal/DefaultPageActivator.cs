// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Internal;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    /// <summary>
    /// <see cref="IPageActivator"/> that uses type activation to create Pages.
    /// </summary>
    public class DefaultPageActivator : IPageActivator
    {
        private readonly ITypeActivatorCache _typeActivatorCache;

        /// <summary>
        /// Creates a new <see cref="DefaultPageActivator"/>.
        /// </summary>
        /// <param name="typeActivatorCache">The <see cref="ITypeActivatorCache"/>.</param>
        public DefaultPageActivator(ITypeActivatorCache typeActivatorCache)
        {
            if (typeActivatorCache == null)
            {
                throw new ArgumentNullException(nameof(typeActivatorCache));
            }

            _typeActivatorCache = typeActivatorCache;
        }

        /// <inheritdoc />
        public virtual object Create(PageContext PageContext)
        {
            if (PageContext == null)
            {
                throw new ArgumentNullException(nameof(PageContext));
            }

            if (PageContext.ActionDescriptor == null)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture, 
                    Resources.PropertyOfTypeCannotBeNull, 
                    nameof(PageContext.ActionDescriptor),
                    nameof(PageContext)));
            }

            var PageTypeInfo = PageContext.ActionDescriptor.PageTypeInfo;

            if (PageTypeInfo == null)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.PropertyOfTypeCannotBeNull,
                    nameof(PageContext.ActionDescriptor.PageTypeInfo),
                    nameof(PageContext.ActionDescriptor)));
            }

            var serviceProvider = PageContext.HttpContext.RequestServices;
            return _typeActivatorCache.CreateInstance<object>(serviceProvider, PageTypeInfo.AsType());
        }

        /// <inheritdoc />
        public virtual void Release(PageContext context, object page)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            (page as IDisposable)?.Dispose();
        }
    }
}
