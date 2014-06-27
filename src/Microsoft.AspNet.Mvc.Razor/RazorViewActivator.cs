// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <inheritdoc />
    public class RazorViewActivator : IRazorViewActivator
    {
        private readonly Func<Type, PropertyActivator<ViewContext>[]> _getPropertiesToActivate;
        private readonly IReadOnlyDictionary<Type, Func<ViewContext, object>> _valueAccessorLookup;
        private readonly ConcurrentDictionary<Type, PropertyActivator<ViewContext>[]> _activators;

        /// <summary>
        /// Initializes a new instance of the DefaultViewActivator class.
        /// </summary>
        public RazorViewActivator()
        {
            _valueAccessorLookup = CreateValueAccessorLookup();
            _activators = new ConcurrentDictionary<Type, PropertyActivator<ViewContext>[]>();
            _getPropertiesToActivate = type =>
                PropertyActivator<ViewContext>.GetPropertiesToActivate(type, 
                                                                       typeof(ActivateAttribute),
                                                                       CreateActivateInfo);
        }

        /// <summary>
        /// Activates the specified view by using the specified ViewContext.
        /// </summary>
        /// <param name="view">The view to activate.</param>
        /// <param name="context">The ViewContext for the executing view.</param>
        public void Activate([NotNull] RazorView view, [NotNull] ViewContext context)
        {
            var propertiesToActivate = _activators.GetOrAdd(view.GetType(),
                                                            _getPropertiesToActivate);
            
            for (var i = 0; i < propertiesToActivate.Length; i++)
            {
                var activateInfo = propertiesToActivate[i];

                var value = activateInfo.Activate(view, context);
                var canHasViewContext = value as ICanHasViewContext;
                if (canHasViewContext != null)
                {
                    canHasViewContext.Contextualize(context);
                }
            }
        }

        protected virtual IReadOnlyDictionary<Type, Func<ViewContext, object>> CreateValueAccessorLookup()
        {
            return new Dictionary<Type, Func<ViewContext, object>>();
        }

        private PropertyActivator<ViewContext> CreateActivateInfo(PropertyInfo property)
        {
            Func<ViewContext, object> valueAccessor;
            if (!_valueAccessorLookup.TryGetValue(property.PropertyType, out valueAccessor))
            {
                valueAccessor = (context) =>
                {
                    var serviceProvider = context.HttpContext.RequestServices;
                    return serviceProvider.GetService(property.PropertyType);
                };
            }

            return new PropertyActivator<ViewContext>(property, valueAccessor);
        }
    }
}