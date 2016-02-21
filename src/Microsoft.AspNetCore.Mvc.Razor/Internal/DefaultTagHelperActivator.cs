// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    /// <inheritdoc />
    public class DefaultTagHelperActivator : ITagHelperActivator
    {
        private readonly ConcurrentDictionary<Type, PropertyActivator<ViewContext>[]> _injectActions;
        private readonly Func<Type, PropertyActivator<ViewContext>[]> _getPropertiesToActivate;

        /// <summary>
        /// Instantiates a new <see cref="DefaultTagHelperActivator"/> instance.
        /// </summary>
        public DefaultTagHelperActivator()
        {
            _injectActions = new ConcurrentDictionary<Type, PropertyActivator<ViewContext>[]>();
            _getPropertiesToActivate = type =>
                PropertyActivator<ViewContext>.GetPropertiesToActivate(
                    type,
                    typeof(ViewContextAttribute),
                    CreateActivateInfo);
        }

        /// <inheritdoc />
        public void Activate<TTagHelper>(TTagHelper tagHelper, ViewContext context)
            where TTagHelper : ITagHelper
        {
            if (tagHelper == null)
            {
                throw new ArgumentNullException(nameof(tagHelper));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var propertiesToActivate = _injectActions.GetOrAdd(
                tagHelper.GetType(),
                _getPropertiesToActivate);

            for (var i = 0; i < propertiesToActivate.Length; i++)
            {
                var activateInfo = propertiesToActivate[i];
                activateInfo.Activate(tagHelper, context);
            }

            InitializeTagHelper(tagHelper, context);
        }

        private static void InitializeTagHelper<TTagHelper>(TTagHelper tagHelper, ViewContext context)
            where TTagHelper : ITagHelper
        {
            // Run any tag helper initializers in the container
            var serviceProvider = context.HttpContext.RequestServices;
            var initializers = serviceProvider.GetService<IEnumerable<ITagHelperInitializer<TTagHelper>>>();

            foreach (var initializer in initializers)
            {
                initializer.Initialize(tagHelper, context);
            }
        }

        private static PropertyActivator<ViewContext> CreateActivateInfo(PropertyInfo property)
        {
            return new PropertyActivator<ViewContext>(property, viewContext => viewContext);
        }
    }
}