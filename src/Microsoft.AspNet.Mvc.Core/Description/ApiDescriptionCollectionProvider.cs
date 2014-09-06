// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.Description
{
    public class ApiDescriptionCollectionProvider : IApiDescriptionCollectionProvider
    {
        private readonly IActionDescriptorsCollectionProvider _actionDescriptorCollectionProvider;
        private readonly INestedProviderManager<ApiDescriptionProviderContext> _apiDescriptionProvider;

        private ApiDescriptionCollection _apiDescriptions;

        public ApiDescriptionCollectionProvider(
            IActionDescriptorsCollectionProvider actionDescriptorCollectionProvider,
            INestedProviderManager<ApiDescriptionProviderContext> apiDescriptionProvider)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            _apiDescriptionProvider = apiDescriptionProvider;
        }

        public ApiDescriptionCollection ApiDescriptions
        {
            get
            {
                var actionDescriptors = _actionDescriptorCollectionProvider.ActionDescriptors;
                if (_apiDescriptions == null || _apiDescriptions.Version != actionDescriptors.Version)
                {
                    _apiDescriptions = GetCollection(actionDescriptors);
                }

                return _apiDescriptions;
            }
        }

        private ApiDescriptionCollection GetCollection(ActionDescriptorsCollection actionDescriptors)
        {
            var context = new ApiDescriptionProviderContext(actionDescriptors.Items);
            _apiDescriptionProvider.Invoke(context);

            return new ApiDescriptionCollection(context.Results, actionDescriptors.Version);
        }
    }
}