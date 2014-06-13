// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultControllerActivator : IControllerActivator
    {
        private static readonly Dictionary<Type, Func<ActionContext, object>> _valueAccessorLookup
            = CreateValueAccessorLookup();
        private static readonly Func<Type, ActivateInfo[]> _getPropertiesToActivate =
            GetPropertiesToActivate;
        private static readonly Func<PropertyInfo, ActivateInfo> _createActivateInfo =
            CreateActivateInfo;

        private readonly ConcurrentDictionary<Type, ActivateInfo[]> _injectActions
            = new ConcurrentDictionary<Type, ActivateInfo[]>();

        public void Activate([NotNull] ActionContext context)
        {
            if (context.Controller == null)
            {
                throw new InvalidOperationException(Resources.ControllerActivator_ControllerRequired);
            }

            var controllerType = context.Controller.GetType();
            var controllerTypeInfo = controllerType.GetTypeInfo();
            var propertiesToActivate = _injectActions.GetOrAdd(controllerType,
                                                               _getPropertiesToActivate);

            // Get a boxed version of the instance in the event we're dealing with a value type.
            var instance = RuntimeHelpers.GetObjectValue(context.Controller);
            for (var i = 0; i < propertiesToActivate.Length; i++)
            {
                var activateInfo = propertiesToActivate[i];
                activateInfo.Activate(instance, context);
            }

            context.Controller = instance;
        }

        private static Dictionary<Type, Func<ActionContext, object>> CreateValueAccessorLookup()
        {
            return new Dictionary<Type, Func<ActionContext, object>>
            {
                { typeof(ActionContext), (context) => context },
                { typeof(HttpContext), (context) => context.HttpContext },
                { typeof(HttpRequest), (context) => context.HttpContext.Request },
                { typeof(HttpResponse), (context) => context.HttpContext.Response },
                {
                    typeof(ViewDataDictionary),
                    (context) =>
                    {
                        var serviceProvider = context.HttpContext.RequestServices;
                        return new ViewDataDictionary(
                            serviceProvider.GetService<IModelMetadataProvider>(),
                            context.ModelState);
                    }
                }
            };
        }

        private static ActivateInfo[] GetPropertiesToActivate(Type controllerType)
        {
            return controllerType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(property => property.IsDefined(typeof(ActivateAttribute)) &&
                                                    property.GetSetMethod() != null)
                                 .Select(_createActivateInfo)
                                 .ToArray();
        }

        private static ActivateInfo CreateActivateInfo(PropertyInfo property)
        {
            Func<ActionContext, object> valueAccessor;
            if (!_valueAccessorLookup.TryGetValue(property.PropertyType, out valueAccessor))
            {
                valueAccessor = (actionContext) =>
                {
                    var serviceProvider = actionContext.HttpContext.RequestServices;
                    return serviceProvider.GetService(property.PropertyType);
                };
            }

            return new ActivateInfo(property,
                                    valueAccessor);
        }

        private sealed class ActivateInfo
        {
            private readonly PropertyInfo _propertyInfo;
            private readonly Func<ActionContext, object> _valueAccessor;
            private readonly Action<object, object> _fastPropertySetter;

            public ActivateInfo(PropertyInfo propertyInfo,
                                Func<ActionContext, object> valueAccessor)
            {
                _propertyInfo = propertyInfo;
                _valueAccessor = valueAccessor;

                // The fast property setter does not work with value types - casting it from object type to 
                // TDeclaringType creates a new value type and we end up setting the property value on this new
                // instance. We'll use the slow reflection code path to activate value types.
                if (!propertyInfo.DeclaringType.GetTypeInfo().IsValueType)
                {
                    _fastPropertySetter = PropertyHelper.MakeFastPropertySetter(propertyInfo);
                }
            }

            public void Activate(object instance, ActionContext context)
            {
                var value = _valueAccessor(context);
                if (_fastPropertySetter != null)
                {
                    _fastPropertySetter(instance, value);
                }
                else
                {
                    _propertyInfo.SetValue(instance, value);
                }
            }
        }
    }
}
