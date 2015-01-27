// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Provides methods for discovery of controller types.
    /// </summary>
    public interface IControllerTypeProvider
    {
        /// <summary>
        /// Gets a sequence of controller <see cref="Type"/>.
        /// </summary>
        /// <returns>A sequence of controller types.</returns>
        IEnumerable<TypeInfo> GetControllerTypes();
    }
}