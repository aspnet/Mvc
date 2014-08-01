// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Represents the <see cref="IViewComponentActivator"/> that is registered by default.
    /// </summary>
    public class DefaultViewComponentActivator : IViewComponentActivator
    {
        private readonly Func<Type, PropertyActivator<ViewContext>[]> _getPropertiesToActivate;
        private readonly IReadOnlyDictionary<Type, Func<ViewContext, object>> _valueAccessorLookup;
        private readonly ConcurrentDictionary<Type, PropertyActivator<ViewContext>[]> _injectActions;

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultViewComponentActivator"/> class.
        /// </summary>
        public DefaultViewComponentActivator()
        {
            _valueAccessorLookup = CreateValueAccessorLookup();
            _injectActions = new ConcurrentDictionary<Type, PropertyActivator<ViewContext>[]>();
            _getPropertiesToActivate = type =>
                PropertyActivator<ViewContext>.GetPropertiesToActivate(type,
                                                                        typeof(ActivateAttribute),
                                                                        CreateActivateInfo);
        }

        /// <summary>
        /// Activates the specified ViewComponent using the specified ViewContext.
        /// </summary>
        /// <param name="viewComponent">The ViewComponent which needs to be activated.</param>
        /// <param name="context">The context which should be used to activate the ViewComponent.</param>
        public void Activate(object viewComponent, ViewContext context)
        {
            var propertiesToActivate = _injectActions.GetOrAdd(viewComponent.GetType(),
                                                               _getPropertiesToActivate);

            for (var i = 0; i < propertiesToActivate.Length; i++)
            {
                var activateInfo = propertiesToActivate[i];
                activateInfo.Activate(viewComponent, context);
            }
        }

        protected virtual IReadOnlyDictionary<Type, Func<ViewContext, object>> CreateValueAccessorLookup()
        {
            return new Dictionary<Type, Func<ViewContext, object>>
            {
                { typeof(ViewContext), (context) => context },
                {
                    typeof(ViewDataDictionary),
                    (context) =>
                    {
                        return new ViewDataDictionary(context.ViewData)
                        {
                            Model = null
                        };
                    }
                }
            };
        }

        private PropertyActivator<ViewContext> CreateActivateInfo(
            PropertyInfo property)
        {
            Func<ViewContext, object> valueAccessor;
            if (!_valueAccessorLookup.TryGetValue(property.PropertyType, out valueAccessor))
            {
                valueAccessor = (actionContext) =>
                {
                    var serviceProvider = actionContext.HttpContext.RequestServices;
                    return serviceProvider.GetService(property.PropertyType);
                };
            }

            return new PropertyActivator<ViewContext>(property, valueAccessor);
        }
    }
}