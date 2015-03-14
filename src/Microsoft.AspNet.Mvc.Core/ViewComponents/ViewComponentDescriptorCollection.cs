// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ViewComponents
{
    /// <summary>
    /// A cached collection of <see cref="ViewComponentDescriptor" />.
    /// </summary>
    public class ViewComponentDescriptorCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewComponentDescriptorCollection"/>.
        /// </summary>
        /// <param name="items">The result of view component discovery</param>
        /// <param name="version">The unique version of discovered view components.</param>
        public ViewComponentDescriptorCollection([NotNull] IReadOnlyList<ViewComponentDescriptor> items, int version)
        {
            Items = items;
            Version = version;
        }

        /// <summary>
        /// Returns the cached <see cref="IReadOnlyList{ViewComponentDescriptor}"/>.
        /// </summary>
        public IReadOnlyList<ViewComponentDescriptor> Items { get; private set; }

        /// <summary>
        /// Returns the unique version of the currently cached items.
        /// </summary>
        public int Version { get; private set; }
    }
}