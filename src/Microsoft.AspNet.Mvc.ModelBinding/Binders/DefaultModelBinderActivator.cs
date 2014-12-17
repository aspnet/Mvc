// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class DefaultModelBinderActivator : IModelBinderActivator
    {
        private readonly IServiceProvider _provider;

        public DefaultModelBinderActivator([NotNull] IServiceProvider provider)
        {
            _provider = provider;
        }

        public object CreateInstance([NotNull] Type binderType)
        {
            return ActivatorUtilities.CreateInstance(_provider, binderType);
        }
    }
}