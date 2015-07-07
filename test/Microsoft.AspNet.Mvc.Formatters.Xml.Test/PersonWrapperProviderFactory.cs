// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.Formatters.Xml
{
    public class PersonWrapperProviderFactory : IWrapperProviderFactory
    {
        public IWrapperProvider GetProvider(WrapperProviderContext context)
        {
            if (context.DeclaredType == typeof(Person))
            {
                return new PersonWrapperProvider();
            }

            return null;
        }
    }
}