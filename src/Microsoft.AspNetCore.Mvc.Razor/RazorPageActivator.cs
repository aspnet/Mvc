// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Microsoft.AspNetCore.Mvc.Razor
{
    /// <inheritdoc />
    public class RazorPageActivator : IRazorPageActivator
    {
        // Name of the "public TModel Model" property on RazorPage<TModel>
        private const string ModelPropertyName = "Model";
        private readonly RazorPagePropertyActivator _razorPagePropertyActivator;
        private readonly ConcurrentDictionary<Type, Type> _modelTypeLookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="RazorPageActivator"/> class.
        /// </summary>
        public RazorPageActivator(
            IModelMetadataProvider metadataProvider,
            IUrlHelperFactory urlHelperFactory,
            IJsonHelper jsonHelper,
            DiagnosticSource diagnosticSource,
            HtmlEncoder htmlEncoder,
            IModelExpressionProvider modelExpressionProvider)
        {
            _razorPagePropertyActivator = new RazorPagePropertyActivator(
                metadataProvider,
                urlHelperFactory,
                jsonHelper,
                diagnosticSource,
                htmlEncoder,
                modelExpressionProvider);

            _modelTypeLookup = new ConcurrentDictionary<Type, Type>();
        }

        /// <inheritdoc />
        public void Activate(IRazorPage page, ViewContext context)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var type = page.GetType();
            Type modelType;
            if (!_modelTypeLookup.TryGetValue(type, out modelType))
            {
                // Look for a property named "Model". If it is non-null, we'll assume this is
                // the equivalent of TModel Model property on RazorPage<TModel>
                var modelProperty = type.GetRuntimeProperty(ModelPropertyName);
                if (modelProperty == null)
                {
                    var message = Resources.FormatViewCannotBeActivated(type.FullName, GetType().FullName);
                    throw new InvalidOperationException(message);
                }

                modelType = _modelTypeLookup.GetOrAdd(type, modelProperty.PropertyType);
            }

            _razorPagePropertyActivator.Activate(page, context, modelType);
        }
    }
}