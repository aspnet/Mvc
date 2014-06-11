// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultControllerActivator : IControllerActivator
    {
        private readonly ConcurrentDictionary<Type, List<PropertyInfo>> _propertyLookup
            = new ConcurrentDictionary<Type, List<PropertyInfo>>();

        public void Activate([NotNull] ActionContext context)
        {
            if (context.Controller == null)
            {
                throw new InvalidOperationException(Resources.ControllerActivator_ControllerRequired);
            }


            var propertiesToActivate = _propertyLookup.GetOrAdd(
                                            context.Controller.GetType(),
                                            GetPropertiesToActivate);

            for (var i = 0; i < propertiesToActivate.Count; i++)
            {
                ActivateProperty(context, propertiesToActivate[i]);
            }
        }

        private static List<PropertyInfo> GetPropertiesToActivate(Type controllerType)
        {
            return controllerType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(p => p.IsDefined(typeof(ActivateAttribute)) &&
                                             p.GetSetMethod() != null)
                                 .ToList();
        }

        private static void ActivateProperty(ActionContext context, PropertyInfo property)
        {
            var serviceProvider = context.HttpContext.RequestServices;

            if (typeof(ActionContext).IsAssignableFrom(property.PropertyType))
            {
                property.SetValue(context.Controller, context);
            }
            else if (typeof(HttpContext).IsAssignableFrom(property.PropertyType))
            {
                property.SetValue(context.Controller, context.HttpContext);
            }
            else if (typeof(HttpRequest).IsAssignableFrom(property.PropertyType))
            {
                property.SetValue(context.Controller, context.HttpContext.Request);
            }
            else if (typeof(HttpResponse).IsAssignableFrom(property.PropertyType))
            {
                property.SetValue(context.Controller, context.HttpContext.Response);
            }
            else if (typeof(ViewDataDictionary).IsAssignableFrom(property.PropertyType))
            {
                var viewData = new ViewDataDictionary(
                    serviceProvider.GetService<IModelMetadataProvider>(),
                    context.ModelState);
                property.SetValue(context.Controller, viewData);
            }
            else
            {
                var value = serviceProvider.GetService(property.PropertyType);
                property.SetValue(context.Controller, value);
            }
        }
    }
}
