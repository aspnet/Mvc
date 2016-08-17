// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.Core.ApplicationParts
{
    /// <summary>
    /// A container for <see cref="PrecompiledViewInfo"/> instances.
    /// </summary>
    public abstract class PrecompiledViews
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ViewInfos"/>.
        /// </summary>
        /// <param name="precompiledViews">The sequence of <see cref="PrecompiledViewInfo"/>.</param>
        protected PrecompiledViews(IReadOnlyCollection<PrecompiledViewInfo> precompiledViews)
        {
            ViewInfos = precompiledViews;
        }

        public IReadOnlyCollection<PrecompiledViewInfo> ViewInfos { get; }
    }
}
