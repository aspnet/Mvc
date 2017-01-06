// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Microsoft.AspNetCore.Mvc.Formatters.Json
{
    public class JsonPatchOperationsArrayProvider : IApiDescriptionProvider
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public JsonPatchOperationsArrayProvider(
            IModelMetadataProvider modelMetadataProvider)
        {
            _modelMetadataProvider = modelMetadataProvider;
        }

        /// <inheritdoc />
        public int Order
        {
            get { return -999; }
        }

        /// <inheritdoc />
        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var result in context.Results)
            {
                foreach (var parameterDescription in result.ParameterDescriptions)
                {
                    if (typeof(IJsonPatchDocument).GetTypeInfo().IsAssignableFrom(parameterDescription.Type))
                    {
                        parameterDescription.OriginalParameterDescriptor  = AssignOriginalParameterDescriptor();
                        parameterDescription.Type = typeof(Operation[]);
                        parameterDescription.ModelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(Operation[]));
                    }
                }
            }
        }

        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
        }

        private static ParameterDescriptor AssignOriginalParameterDescriptor()
        {
            var originalParameterDescriptor = new ParameterDescriptor();
            originalParameterDescriptor.ParameterType = typeof(IJsonPatchDocument);
            return originalParameterDescriptor;
        }
    }
}