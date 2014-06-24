// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Represents an aggregate of <see cref="IModelBinder"/> that delegates to one of the instances for model binding.
    /// </summary>
    public interface ICompositeModelBinder : IModelBinder
    {
        /// <summary>
        /// Gets a collection of <see cref="IModelBinder"/> that the composite binder delegates to.
        /// </summary>
        IReadOnlyList<IModelBinder> ModelBinders { get; }
    }
}
