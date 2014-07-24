﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Provides an activated collection of <see cref="IModelBinder"/> instances.
    /// </summary>
    public interface IModelBinderProvider
    {
        /// <summary>
        /// Gets a collection of activated ModelBinders instances.
        /// </summary>
        IReadOnlyList<IModelBinder> ModelBinders { get; }
    }
}