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
        private readonly Func<Type, PropertyActivator<ControllerContext>[]> _getPropertiesToActivate;
        private readonly ControllerViewDataDictionaryFactory _viewDataDictionaryFactory;
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly ConcurrentDictionary<Type, PropertyActivator<ControllerContext>[]> _activateActions
            = new ConcurrentDictionary<Type, PropertyActivator<ControllerContext>[]>();

        [Obsolete("This constructor is obsolete and will be removed in a future version")]
        // Note: Remove Legacy code path from GetViewDataDictionary whence this constructor is removed.
        public ViewDataDictionaryControllerPropertyActivator(IModelMetadataProvider modelMetadataProvider)
        {
            _modelMetadataProvider = modelMetadataProvider;
            _getPropertiesToActivate = GetPropertiesToActivate;
        }

        public ViewDataDictionaryControllerPropertyActivator(
            IModelMetadataProvider modelMetadataProvider,
            ControllerViewDataDictionaryFactory viewDataDictionaryFactory)
        {
            _modelMetadataProvider = modelMetadataProvider ?? throw new ArgumentNullException(nameof(modelMetadataProvider));
            _viewDataDictionaryFactory = viewDataDictionaryFactory ?? throw new ArgumentNullException(nameof(viewDataDictionaryFactory));
            _getPropertiesToActivate = GetPropertiesToActivate;
        }

        public void Activate(ControllerContext actionContext, object controller)
        {
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
            if (_viewDataDictionaryFactory != null)
            {
                return _viewDataDictionaryFactory.GetViewDataDictionary(context);
            }

            var viewData = new ViewDataDictionary(
                _modelMetadataProvider,
                context.ModelState);

            return viewData;
        }
    }
}
