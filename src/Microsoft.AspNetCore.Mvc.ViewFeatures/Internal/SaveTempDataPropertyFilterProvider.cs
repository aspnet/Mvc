// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public class SaveTempDataPropertyFilterFactory : IFilterFactory
    {
        internal IList<PropertyHelper> TempDataProperties { get; set; }

        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var service = serviceProvider.GetRequiredService<SaveTempDataPropertyFilter>();
            service.PropertyHelpers = TempDataProperties;
            return service;
        }
    }
}
