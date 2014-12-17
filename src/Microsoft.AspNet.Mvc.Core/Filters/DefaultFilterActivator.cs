// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultFilterActivator : IFilterActivator
    {
        private readonly IServiceProvider _provider;

        public DefaultFilterActivator([NotNull] IServiceProvider provider)
        {
            _provider = provider;
        }

        public IFilter CreateInstance([NotNull] Type filterType)
        {
            var filter = ActivatorUtilities.CreateInstance(_provider, filterType) as IFilter;

            if (filter == null)
            {
                throw new InvalidOperationException(Resources.FormatFilterFactoryAttribute_TypeMustImplementIFilter(
                    typeof(TypeFilterAttribute).Name,
                    typeof(IFilter).Name));
            }

            return filter;
        }
    }
}