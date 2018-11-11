// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;

namespace Microsoft.AspNetCore.Routing
{
    public class DefaultApplicationAssemblyProvider : ApplicationAssemblyProvider
    {
        private readonly IHostingEnvironment _environment;

        public DefaultApplicationAssemblyProvider(IHostingEnvironment environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }

            _environment = environment;
        }

        public override IReadOnlyList<Assembly> GetApplicationAssemblies()
        {
            throw new NotImplementedException();
        }
    }
}
