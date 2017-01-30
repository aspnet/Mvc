﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Microsoft.AspNetCore.Mvc
{
    public class HostingApplicationNameProvider : IApplicationNameProvider
    {
        private IHostingEnvironment _environment;

        public HostingApplicationNameProvider(IServiceCollection services)
        {
            _environment = GetServiceFromCollection<IHostingEnvironment>(services);
        }

        public string ApplicationName
        {
            get
            {
                return _environment.ApplicationName;
            }
        }

        private static T GetServiceFromCollection<T>(IServiceCollection services)
        {
            return (T)services
                .LastOrDefault(d => d.ServiceType == typeof(T))
                ?.ImplementationInstance;
        }
    }
}
