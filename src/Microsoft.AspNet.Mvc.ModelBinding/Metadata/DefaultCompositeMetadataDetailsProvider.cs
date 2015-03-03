// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
{
    public class DefaultCompositeMetadataDetailsProvider : ICompositeMetadataDetailsProvider
    {
        private readonly IEnumerable<IMetadataDetailsProvider> _providers;

        public DefaultCompositeMetadataDetailsProvider(IEnumerable<IMetadataDetailsProvider> providers)
        {
            _providers = providers;
        }

        public virtual void GetBindingMetadata([NotNull] BindingMetadataProviderContext context)
        {
            foreach (var provider in _providers.OfType<IBindingMetadataProvider>())
            {
                provider.GetBindingMetadata(context);
            }
        }

        public virtual void GetDisplayMetadata([NotNull] DisplayMetadataProviderContext context)
        {
            foreach (var provider in _providers.OfType<IDisplayMetadataProvider>())
            {
                provider.GetDisplayMetadata(context);
            }
        }

        public virtual void GetValidationMetadata([NotNull] ValidationMetadataProviderContext context)
        {
            foreach (var provider in _providers.OfType<IValiationMetadataProvider>())
            {
                provider.GetValidationMetadata(context);
            }
        }
    }
}