// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.Core
{
    /// <summary>
    /// Helper class which contains MvcServices related helpers.
    /// </summary>
    public static class MvcServicesHelper
    {
        /// <summary>
        /// Throws InvalidOperationException when the given type of service is not present
        /// in the list of services.
        /// </summary>
        /// <param name="services">The list of services.</param>
        /// <param name="serviceType">The type of service which needs to be searched for.</param>
        public static object ThrowIfServiceDoesNotExist(IServiceProvider services, Type serviceType)
        {
            var markerService = services.GetServiceOrNull(typeof(MvcMarkerService));
            if (markerService == null)
            {
                throw new InvalidOperationException(Resources.FormatUnableToFindServices(
                    "IServiceCollection.AddMvc()", "IBuilder.UseServices(...)", "IBuilder.UseMvc(...)"));
            }

            return markerService;
        }
    }
}