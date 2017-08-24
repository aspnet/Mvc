// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    public class ErrorPolicyContext
    {
        public ErrorPolicyContext(IModelMetadataProvider metadataProvider)
        {
            MetadataProvider = metadataProvider;
        }

        public ApiDescription Description { get; set; }

        public IModelMetadataProvider MetadataProvider { get; }
    }
}