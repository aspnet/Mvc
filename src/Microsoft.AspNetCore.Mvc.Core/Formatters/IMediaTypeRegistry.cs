// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.Formatters
{
    /// <summary>
    /// A service which provides mappings from a <see cref="Type"/> representing a data type to serialize
    /// to a set of possible media types.
    /// </summary>
    /// <remarks>
    /// This service is registered as a singleton, and implementations should cache results to improve
    /// performance.
    /// </remarks>
    public abstract class MediaTypeRegistry
    {
        /// <summary>
        /// Gets a list of applicable media types to which a <see cref="Type"/> can be serialized.
        /// </summary>
        /// <param name="type">The data type.</param>
        /// <returns>A list of applicable media types.</returns>
        public abstract IReadOnlyList<string> GetMediaTypes(Type type);
    }
}
