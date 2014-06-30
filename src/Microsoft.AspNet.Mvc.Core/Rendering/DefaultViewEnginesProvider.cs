// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <inheritdoc />
    public class DefaultViewEnginesProvider : IViewEnginesProvider
    {
        private readonly IList<ViewEngineDescriptor> _descriptors;
        private readonly ITypeActivator _typeActivator;
        private readonly IServiceProvider _serviceProvider;
        private List<IViewEngine> _viewEngines;

        public DefaultViewEnginesProvider(
            ITypeActivator typeActivator,
            IServiceProvider serviceProvider,
            IOptionsAccessor<MvcOptions> options)
        {
            _typeActivator = typeActivator;
            _serviceProvider = serviceProvider;
            _descriptors = options.Options.ViewEngines;
        }

        /// <inheritdoc />
        public IReadOnlyList<IViewEngine> ViewEngines
        {
            get
            {
                if (_viewEngines == null)
                {
                    _viewEngines = new List<IViewEngine>(_descriptors.Count);
                    foreach (var descriptor in _descriptors)
                    {
                        var viewEngine = descriptor.ViewEngine;
                        if (viewEngine == null)
                        {
                            viewEngine = (IViewEngine)_typeActivator.CreateInstance(_serviceProvider, descriptor.ViewEngineType);
                        }

                        _viewEngines.Add(viewEngine);
                    }
                }

                return _viewEngines;
            }
        }
    }
}