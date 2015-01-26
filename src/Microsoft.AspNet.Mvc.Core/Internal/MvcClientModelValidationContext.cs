// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc.Internal
{
    /// <summary>
    /// <see cref="ClientModelValidationContext"/> that provides additional context for use in some
    /// <see cref="IClientModelValidator"/> implementations.
    /// </summary>
    /// <remarks>
    /// <see cref="RemoteAttribute"/> is one <see cref="IClientModelValidator"/> implementation that needs this
    /// additional context.
    /// </remarks>
    public class MvcClientModelValidationContext : ClientModelValidationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MvcClientModelValidationContext"/> class.
        /// </summary>
        /// <param name="metadata">
        /// The <see cref="ModelMetadata"/> for the property the <see cref="IClientModelValidator"/> targets.
        /// </param>
        /// <param name="metadataProvider">
        /// The <see cref="IModelMetadataProvider"/> used to get additional <see cref="ModelMetadata"/> instances.
        /// </param>
        /// <param name="urlHelper">The <see cref="IUrlHelper"/> used to generate URLs.</param>
        public MvcClientModelValidationContext(
            [NotNull] ModelMetadata metadata,
            [NotNull] IModelMetadataProvider metadataProvider,
            [NotNull] IUrlHelper urlHelper)
            : base(metadata, metadataProvider)
        {
            UrlHelper = urlHelper;
        }

        /// <summary>
        /// Gets the <see cref="IUrlHelper"/> used to generate URLs.
        /// </summary>
        public IUrlHelper UrlHelper { get; }
    }
}