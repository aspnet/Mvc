// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Extensions methods for <see cref="IModelMetadataProvider"/>.
    /// </summary>
    public static class ModelMetadataProviderExtensions
    {
        /// <summary>
        /// Gets a <see cref="ModelExplorer"/> for the provided <paramref name="modelType"/> and
        /// <paramref name="model"/>.
        /// </summary>
        /// <param name="provider">The <see cref="IModelMetadataProvider"/>.</param>
        /// <param name="modelType">The declared <see cref="Type"/> of the model object.</param>
        /// <param name="model">The model object.</param>
        /// <returns></returns>
        public static ModelExplorer GetModelExplorerForType(
            [NotNull] this IModelMetadataProvider provider,
            [NotNull] Type modelType,
            object model)
        {
            var modelMetadata = provider.GetMetadataForType(modelType);
            return new ModelExplorer(provider, modelMetadata, model);
        }
    }
}