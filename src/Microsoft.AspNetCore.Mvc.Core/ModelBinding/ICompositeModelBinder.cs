// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// Represents an aggregate of <see cref="IModelBinder"/> that delegates to one of the instances for model binding.
    /// </summary>
    public interface ICompositeModelBinder : IModelBinder
    {
    }
}
