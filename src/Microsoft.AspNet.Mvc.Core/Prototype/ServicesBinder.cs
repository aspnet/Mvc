// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ServiceBinder : MarkerAwareBinder<ActivateAttribute>
    {
        private IServiceProvider _serviceProvider;

        public ServiceBinder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override Task<bool> BindAsync(ModelBindingContext bindingContext)
        {
            bindingContext.Model = _serviceProvider.GetService(bindingContext.ModelType);
            return Task.FromResult<bool>(true);
        }
    }
}
