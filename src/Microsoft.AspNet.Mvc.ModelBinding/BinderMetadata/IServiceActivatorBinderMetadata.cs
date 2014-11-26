// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Metadata interface that indicates model binding should use the service container to get the value for a model.
    /// </summary>
    public interface IServiceActivatorBinderMetadata : IBinderMetadata
    {
    }
}
