// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.Description
{
    /// <summary>
    /// A cached collection of <see cref="ApiDescription" />.
    /// </summary>
    public class ApiDescriptionCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiDescriptionCollection"/>.
        /// </summary>
        /// <param name="items">The result of action discovery</param>
        /// <param name="version">The unique version of discovered actions.</param>
        public ApiDescriptionCollection([NotNull] IReadOnlyList<ApiDescription> items, int version)
        {
            Items = items;
            Version = version;
        }

        /// <summary>
        /// Returns the cached <see cref="IReadOnlyList{ApiDescription}"/>.
        /// </summary>
        public IReadOnlyList<ApiDescription> Items { get; private set; }

        /// <summary>
        /// Returns the unique version of the currently cached items.
        /// </summary>
        public int Version { get; private set; }
    }
}