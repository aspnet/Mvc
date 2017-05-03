// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    public class ViewDataDictionaryControllerPropertyActivator : IControllerPropertyActivator
    {
        private readonly object _initializeLock = new object();
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private bool _initialized;
        private ConcurrentDictionary<Type, PropertyActivator<ControllerContext>[]> _activateActions;
        private Func<Type, PropertyActivator<ControllerContext>[]> _getPropertiesToActivate;

        public ViewDataDictionaryControllerPropertyActivator(IModelMetadataProvider modelMetadataProvider)
        {
            _modelMetadataProvider = modelMetadataProvider;
        }

        public void Activate(ControllerContext actionContext, object controller)
        {
            EnsureInitialized();
            var controllerType = controller.GetType();
            var propertiesToActivate = _activateActions.GetOrAdd(
                controllerType,
                _getPropertiesToActivate);

            for (var i = 0; i < propertiesToActivate.Length; i++)
            {
                var activateInfo = propertiesToActivate[i];
                activateInfo.Activate(controller, actionContext);
            }
        }

        public Action<ControllerContext, object> GetActivatorDelegate(ControllerActionDescriptor actionDescriptor)
        {
            var controllerType = actionDescriptor.ControllerTypeInfo?.AsType();
            if (controllerType == null)
            {
                throw new ArgumentException(Resources.FormatPropertyOfTypeCannotBeNull(
                    nameof(actionDescriptor.ControllerTypeInfo),
                    nameof(actionDescriptor)),
                    nameof(actionDescriptor));
            }

            var propertiesToActivate = GetPropertiesToActivate(controllerType);

            void Activate(ControllerContext controllerContext, object controller)
            {
                for (var i = 0; i < propertiesToActivate.Length; i++)
                {
                    var activateInfo = propertiesToActivate[i];
                    activateInfo.Activate(controller, controllerContext);
                }
            }

            return Activate;
        }

        private void EnsureInitialized()
        {
            lock (_initializeLock)
            {
                if (!_initialized)
                {
                    _activateActions = new ConcurrentDictionary<Type, PropertyActivator<ControllerContext>[]>();
                    _getPropertiesToActivate = GetPropertiesToActivate;
                    _initialized = true;
                }
            }
        }

        private PropertyActivator<ControllerContext>[] GetPropertiesToActivate(Type type)
        {
            var activators = PropertyActivator<ControllerContext>.GetPropertiesToActivate(
                type,
                typeof(ViewDataDictionaryAttribute),
                p => new PropertyActivator<ControllerContext>(p, GetViewDataDictionary));

            return activators;
        }

        private ViewDataDictionary GetViewDataDictionary(ControllerContext context)
        {
            return new ViewDataDictionary(
                _modelMetadataProvider,
                context.ModelState);
        }
    }
}
