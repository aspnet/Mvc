// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultControllerActivator : IControllerActivator
    {
        public void Activate([NotNull] ActionContext context)
        {
            if (context.Controller == null)
            {
                throw new InvalidOperationException("A controller instance must be specified by the ActionContext for Activate to be invoked.");
            }

            var controllerType = context.Controller.GetType().GetTypeInfo();
            var propertiesToActivate = controllerType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                     .Where(p => p.IsDefined(typeof(ActivateAttribute)) &&
                                                                 p.GetSetMethod() != null);

            foreach (var property in propertiesToActivate)
            {
                ActivateProperty(context, property);
            }
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
