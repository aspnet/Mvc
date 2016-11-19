// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class DefaultPageFactory : IPageFactoryProvider
    {
        private readonly IPageActivator _pageActivator;
        private readonly RazorPagePropertyActivator _razorPagePropertyActivator;

        public DefaultPageFactory(
            IPageActivator pageActivator,
            IModelMetadataProvider metadataProvider,
            IUrlHelperFactory urlHelperFactory,
            IJsonHelper jsonHelper,
            DiagnosticSource diagnosticSource,
            HtmlEncoder htmlEncoder,
            IModelExpressionProvider modelExpressionProvider)
        {
            _pageActivator = pageActivator;
            _razorPagePropertyActivator = new RazorPagePropertyActivator(
                metadataProvider,
                urlHelperFactory,
                jsonHelper,
                diagnosticSource,
                htmlEncoder,
                modelExpressionProvider);
        }

        public virtual Func<PageContext, object> CreatePage(CompiledPageActionDescriptor actionDescriptor)
        {
            if (!typeof(Page).GetTypeInfo().IsAssignableFrom(actionDescriptor.PageTypeInfo))
            {
                throw new InvalidOperationException(Resources.FormatActivatedInstance_MustBeAnInstanceOf(
                    _pageActivator.GetType().FullName,
                    typeof(Page).FullName));
            }

            var activatorFactory = _pageActivator.Create(actionDescriptor);
            var modelType = actionDescriptor.ModelTypeInfo?.AsType() ?? actionDescriptor.PageTypeInfo.GetType();

            return (context) =>
            {
                var page = activatorFactory(context) as Page;
                page.PageContext = context;
                _razorPagePropertyActivator.Activate(page, context, modelType);
                return page;
            };
        }

        public void ReleasePage(PageContext context, object page)
        {
            _pageActivator.Release(context, page);
        }
    }
}