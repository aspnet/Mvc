// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    /// <summary>
    /// <see cref="IPageActivator"/> that uses type activation to create Pages.
    /// </summary>
    public class DefaultPageActivator : IPageActivator
    {
        private readonly ConcurrentDictionary<Type, Func<object>> _typeActivatorCache;

        /// <summary>
        /// Creates a new <see cref="DefaultPageActivator"/>.
        /// </summary>
        public DefaultPageActivator()
        {
            _typeActivatorCache = new ConcurrentDictionary<Type, Func<object>>();
        }

        /// <inheritdoc />
        public virtual object Create(PageContext pageContext)
        {
            if (pageContext == null)
            {
                throw new ArgumentNullException(nameof(pageContext));
            }

            if (pageContext.ActionDescriptor == null)
            {
                throw new ArgumentException(Resources.FormatPropertyOfTypeCannotBeNull(
                    nameof(pageContext.ActionDescriptor),
                    nameof(pageContext)),
                    nameof(pageContext));
            }

            var pageTypeInfo = pageContext.ActionDescriptor.PageTypeInfo?.AsType();
            if (pageTypeInfo == null)
            {
                throw new ArgumentException(Resources.FormatPropertyOfTypeCannotBeNull(
                    nameof(pageContext.ActionDescriptor.PageTypeInfo),
                    nameof(pageContext.ActionDescriptor)),
                    nameof(pageContext));
            }

            Func<object> pageFactory;
            if (!_typeActivatorCache.TryGetValue(pageTypeInfo, out pageFactory))
            {
                pageFactory = _typeActivatorCache.GetOrAdd(pageTypeInfo, CreatePageFactory(pageTypeInfo));
            }

            return pageFactory();
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

        private static Func<object> CreatePageFactory(Type pageTypeInfo)
        {
            // new Page();
            var newExpression = Expression.New(pageTypeInfo);
            // () => new Page();
            var pageFactory = Expression
                .Lambda<Func<object>>(newExpression)
                .Compile();
            return pageFactory;
        }
    }
}
