// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Internal;
using System.Reflection;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public class TempDataApplicationModelProvider : IApplicationModelProvider
    {
        /// <inheritdoc />
        public int Order { get { return -1000 + 10; } }

        /// <inheritdoc />
        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
        }

        /// <inheritdoc />
        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var controllerModel in context.Result.Controllers)
            {
                SaveTempDataPropertyFilterProvider provider = null;
                var properties = PropertyHelper.GetVisibleProperties(controllerModel.ControllerType.AsType());
                for (int i = 0; i < properties.Length; i++)
                {
                    if (properties[i].Property.GetCustomAttribute<TempDataAttribute>() != null)
                    {
                        if (provider == null)
                        {
                            provider = new SaveTempDataPropertyFilterProvider()
                            {
                                TempDataProperties = new List<PropertyHelper>()
                            };
                        }

                        provider.TempDataProperties.Add(properties[i]);
                    }
                }

                if (provider != null)
                {
                    controllerModel.Filters.Add(provider);
                }
            }
        }
    }
}
