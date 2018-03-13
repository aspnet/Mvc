// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    internal class PageBoundPropertyFilterApplicationModelProvider : IPageApplicationModelProvider
    {
        private readonly MvcViewOptions _mvcViewOptions;
        private readonly ITempDataDictionaryFactory _tempDataFactory;

        public PageBoundPropertyFilterApplicationModelProvider(
            IOptions<MvcViewOptions> mvcViewOptions,
            ITempDataDictionaryFactory tempDataFactory)
        {
            _mvcViewOptions = mvcViewOptions?.Value ?? throw new ArgumentNullException(nameof(mvcViewOptions));
            _tempDataFactory = tempDataFactory ?? throw new ArgumentNullException(nameof(tempDataFactory));
        }

        /// <summary>
        /// Ordered to execute after <see cref="DefaultPageApplicationModelProvider"/>.
        /// </summary>
        public int Order => -1000 + 10;

        public void OnProvidersExecuted(PageApplicationModelProviderContext context)
        {
        }

        public void OnProvidersExecuting(PageApplicationModelProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var propertyManager = BoundPropertyManager.Create(
                _mvcViewOptions,
                context.PageApplicationModel.HandlerType.AsType());
            var filter = new PageBoundPropertyFilter(_tempDataFactory, propertyManager);
            context.PageApplicationModel.Filters.Add(filter);
        }
    }
}
