﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
{
    /// <summary>
    /// Marker interface for a provider of metadata details about model objects. Implementations should
    /// implement one or more of <see cref="IBindingMetadataProvider"/>, <see cref="IDisplayMetadataProvider"/>, 
    /// and <see cref="IModelValidatorProvider"/>.
    /// </summary>
    public interface IMetadataDetailsProvider
    {
    }
}