// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Represents model binder metadata state during the model binding process.
    /// </summary>
    public enum ModelBinderMetadataState
    {
        /// <summary>
        /// Represents if there has been no metadata found which needs to read the body during the current
        /// model binding process.
        /// </summary>
        NotBodyBased,

        /// <summary>
        /// Represents if there is a <see cref="IFormatterBinderMetadata"/> that
        /// has been found during the current model binding process.
        /// </summary>
        FormatterBased,

        /// <summary>
        /// Represents if there is a <see cref = "IFormDataValueProviderMetadata" /> that
        /// has been found during the current model binding process.
        /// </summary>
        FormBased
    }
}
