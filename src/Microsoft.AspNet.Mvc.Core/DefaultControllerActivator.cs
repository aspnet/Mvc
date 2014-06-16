// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly Func<Type, ActivateInfo[]> _getPropertiesToActivate;
        private readonly Func<PropertyInfo, ActivateInfo> _createActivateInfo;
        private readonly ReadOnlyDictionary<Type, Func<ActionContext, object>> _valueAccessorLookup;
        private readonly ConcurrentDictionary<Type, ActivateInfo[]> _injectActions;

        public DefaultControllerActivator()
        {
            _valueAccessorLookup = CreateValueAccessorLookup();
            _getPropertiesToActivate = GetPropertiesToActivate;
            _createActivateInfo = CreateActivateInfo;
            _injectActions = new ConcurrentDictionary<Type, ActivateInfo[]>();
        }

        public void Activate([NotNull] object controller, [NotNull] ActionContext context)
        {
            var controllerType = controller.GetType();
            var controllerTypeInfo = controllerType.GetTypeInfo();
            if (controllerTypeInfo.IsValueType)
            {
                var message = Resources.FormatValueTypesCannotBeActivated(GetType().FullName);
                throw new InvalidOperationException(message);
            }
            var propertiesToActivate = _injectActions.GetOrAdd(controllerType,
                                                               _getPropertiesToActivate);

            for (var i = 0; i < propertiesToActivate.Length; i++)
            {
                var activateInfo = propertiesToActivate[i];
                activateInfo.Activate(controller, context);
            }
        }

        protected virtual ReadOnlyDictionary<Type, Func<ActionContext, object>> CreateValueAccessorLookup()
        {
            var dictionary = new Dictionary<Type, Func<ActionContext, object>>
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
            return new ReadOnlyDictionary<Type, Func<ActionContext, object>>(dictionary);
        }

        private ActivateInfo[] GetPropertiesToActivate(Type controllerType)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            return controllerType.GetProperties(bindingFlags)
                                 .Where(property => property.IsDefined(typeof(ActivateAttribute)) &&
                                                    property.GetSetMethod(nonPublic: true) != null)
                                 .Select(_createActivateInfo)
                                 .ToArray();
        }

        private ActivateInfo CreateActivateInfo(PropertyInfo property)
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
                _fastPropertySetter = PropertyHelper.MakeFastPropertySetter(propertyInfo);
            }

            public void Activate(object instance, ActionContext context)
            {
                var value = _valueAccessor(context);
                _fastPropertySetter(instance, value);
            }
        }
    }
}
