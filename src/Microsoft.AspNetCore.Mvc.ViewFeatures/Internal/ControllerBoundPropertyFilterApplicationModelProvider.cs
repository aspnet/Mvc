// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    internal class ControllerBoundPropertyFilterApplicationModelProvider : IApplicationModelProvider
    {
        private readonly MvcViewOptions _mvcViewOptions;
        private readonly ITempDataDictionaryFactory _tempDataFactory;
        private readonly ControllerViewDataDictionaryFactory _viewDataFactory;

        public ControllerBoundPropertyFilterApplicationModelProvider(
            IOptions<MvcViewOptions> mvcViewOptions,
            ITempDataDictionaryFactory tempDataFactory,
            ControllerViewDataDictionaryFactory viewDataFactory)
        {
            _mvcViewOptions = mvcViewOptions?.Value ?? throw new ArgumentNullException(nameof(mvcViewOptions));
            _tempDataFactory = tempDataFactory ?? throw new ArgumentNullException(nameof(tempDataFactory));
            _viewDataFactory = viewDataFactory ?? throw new ArgumentNullException(nameof(viewDataFactory));
        }

        /// <remarks>This order ensures that <see cref="ControllerBoundPropertyFilterApplicationModelProvider"/> runs after
        /// the <see cref="DefaultApplicationModelProvider"/>.</remarks>
        public int Order => -1000 + 10;

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var controllerModel in context.Result.Controllers)
            {
                var controllerType = controllerModel.ControllerType.AsType();
                var propertyManager = BoundPropertyManager.Create(_mvcViewOptions, controllerType);

                controllerModel.Filters.Add(new ControllerBoundPropertyFilter(_tempDataFactory, _viewDataFactory, propertyManager));
            }
        }
    }
}
