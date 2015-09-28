// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding.Validation
{
    /// <summary>
    /// Aggregate of <see cref="IModelValidatorProvider"/>s that delegates to its underlying providers.
    /// </summary>
    public class CompositeModelValidatorProvider : IModelValidatorProvider
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CompositeModelValidatorProvider"/>.
        /// </summary>
        /// <param name="providers">
        /// A collection of <see cref="IModelValidatorProvider"/> instances.
        /// </param>
        public CompositeModelValidatorProvider([NotNull] IEnumerable<IModelValidatorProvider> providers)
        {
            ValidatorProviders = new List<IModelValidatorProvider>(providers);
        }

        /// <summary>
        /// Gets the list of <see cref="IModelValidatorProvider"/> instances.
        /// </summary>
        public IReadOnlyList<IModelValidatorProvider> ValidatorProviders { get; }

        /// <inheritdoc />
        public void GetValidators(ModelValidatorProviderContext context)
        {
            // Perf: Avoid allocations
            for (var i = 0; i < ValidatorProviders.Count; i++)
            {
                ValidatorProviders[i].GetValidators(context);
            }
        }
    }
}