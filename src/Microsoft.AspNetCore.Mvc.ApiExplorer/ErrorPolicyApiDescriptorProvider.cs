// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    public class ErrorPolicyApiDescriptorProvider : IApiDescriptionProvider
    {
        private readonly IModelMetadataProvider _metadataProvider;

        public ErrorPolicyApiDescriptorProvider(IModelMetadataProvider modelMetadataProvider)
        {
            _metadataProvider = modelMetadataProvider;
        }

        public int Order => -1000 + 10;

        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
            
        }

        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
            for (var i = 0; i < context.Results.Count; i++)
            {
                var apiDescription = context.Results[i];
                var policy = FindErrorPolicy(apiDescription.ActionDescriptor);
                if (policy != null)
                {
                    var errorPolicyContext = new ErrorPolicyContext(_metadataProvider) { Description = apiDescription };
                    policy.Apply(errorPolicyContext);

                    context.Results[i] = errorPolicyContext.Description;
                }
            }
        }

        private IErrorPolicy FindErrorPolicy(ActionDescriptor action)
        {
            for (var i = action.FilterDescriptors.Count - 1; i >= 0; i--)
            {
                if (action.FilterDescriptors[i].Filter is IErrorPolicy policy)
                {
                    return policy;
                }
            }

            return null;
        }
    }
}
