// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace RoutingWebSite
{
    public class RemoveControllerActionDescriptorProvider : IActionDescriptorProvider
    {
        private readonly Type[] _controllerTypes;

        public RemoveControllerActionDescriptorProvider(params Type[] controllerTypes)
        {
            _controllerTypes = controllerTypes;
        }

        public int Order => int.MaxValue;

        public void OnProvidersExecuted(ActionDescriptorProviderContext context)
        {
        }

        public void OnProvidersExecuting(ActionDescriptorProviderContext context)
        {
            foreach (var item in context.Results.ToList())
            {
                if (item is ControllerActionDescriptor controllerActionDescriptor)
                {
                    if (_controllerTypes.Contains(controllerActionDescriptor.ControllerTypeInfo))
                    {
                        context.Results.Remove(item);
                    }
                }
            }
        }
    }
}