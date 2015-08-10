// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Interface for model binding collections.
    /// </summary>
    public interface ICollectionModelBinder : IModelBinder
    {
        /// <summary>
        /// Create an <see cref="object"/> assignable to <paramref name="targetType"/>.
        /// </summary>
        /// <param name="targetType"><see cref="Type"/> of the model.</param>
        /// <returns>An <see cref="object"/> assignable to <paramref name="targetType"/>.</returns>
        object CreateEmptyCollection(Type targetType);
    }
}
