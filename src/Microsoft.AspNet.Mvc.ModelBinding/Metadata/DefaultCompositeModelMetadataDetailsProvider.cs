// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
{
    public class DefaultCompositeModelMetadataDetailsProvider : ICompositeModelMetadataDetailsProvider
    {
        private readonly IEnumerable<IModelMetadataDetailsProvider> _providers;

        public DefaultCompositeModelMetadataDetailsProvider(IEnumerable<IModelMetadataDetailsProvider> providers)
        {
            _providers = providers;
        }

        public virtual void GetBindingDetails([NotNull] ModelMetadataBindingDetailsContext context)
        {
            foreach (var provider in _providers.OfType<IModelMetadataBindingDetailsProvider>())
            {
                provider.GetBindingDetails(context);
            }
        }

        public virtual void GetDisplayDetails([NotNull] ModelMetadataDisplayDetailsContext context)
        {
            foreach (var provider in _providers.OfType<IModelMetadataDisplayDetailsProvider>())
            {
                provider.GetDisplayDetails(context);
            }
        }

        public virtual void GetValidationDetails([NotNull] ModelMetadataValidationDetailsContext context)
        {
            foreach (var provider in _providers.OfType<IModelMetadataValidationDetailsProvider>())
            {
                provider.GetValidationDetails(context);
            }
        }
    }
}