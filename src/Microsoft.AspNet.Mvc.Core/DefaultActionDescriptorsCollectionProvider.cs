// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultActionDescriptorsCollectionProvider : IActionDescriptorsCollectionProvider
    {
        private IServiceProvider _serviceProvider;
        private ActionDescriptorsCollection _collection;

        public DefaultActionDescriptorsCollectionProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ActionDescriptorsCollection ActionDescriptors 
        {
            get
            {
                if (_collection == null)
                {
                    _collection = GetCollection();
                }

                return _collection;
            }
        }

        private ActionDescriptorsCollection GetCollection()
        {
            var actionDescriptorProvider = _serviceProvider.GetService<INestedProviderManager<ActionDescriptorProviderContext>>();            
            var actionDescriptorProviderContext = new ActionDescriptorProviderContext();

            actionDescriptorProvider.Invoke(actionDescriptorProviderContext);

            return new ActionDescriptorsCollection(actionDescriptorProviderContext.Results, 0);
        }
    }
}