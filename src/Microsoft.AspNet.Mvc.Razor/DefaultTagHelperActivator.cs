﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Razor
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
                    type, typeof(ActivateAttribute), CreateActivateInfo);
        }

        /// <inheritdoc />
        public void Activate<TTagHelper>([NotNull] TTagHelper tagHelper, [NotNull] ViewContext context)
            where TTagHelper : ITagHelper
        {
            var propertiesToActivate = _injectActions.GetOrAdd(tagHelper.GetType(),
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
            // Run any IInitializeTagHelper<TTagHelper> in the container
            var serviceProvider = context.HttpContext.RequestServices;
            var initializers = serviceProvider.GetService<IEnumerable<IInitializeTagHelper<TTagHelper>>>();

            foreach (var initializer in initializers)
            {
                initializer.Initialize(tagHelper, context);
            }
        }

        private static PropertyActivator<ViewContext> CreateActivateInfo(PropertyInfo property)
        {
            Func<ViewContext, object> valueAccessor;
            var propertyType = property.PropertyType;

            if (propertyType == typeof(ViewContext))
            {
                valueAccessor = viewContext => viewContext;
            }
            else if (propertyType == typeof(ViewDataDictionary))
            {
                valueAccessor = viewContext => viewContext.ViewData;
            }
            else
            {
                valueAccessor = (viewContext) =>
                {
                    var serviceProvider = viewContext.HttpContext.RequestServices;
                    var service = serviceProvider.GetRequiredService(propertyType);

                    var contextable = service as ICanHasViewContext;
                    contextable?.Contextualize(viewContext);

                    return service;
                };
            }

            return new PropertyActivator<ViewContext>(property, valueAccessor);
        }
    }
}