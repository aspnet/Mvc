// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.RazorPages
{
    /// <summary>
    /// Provides methods to create a Razor page.
    /// </summary>
    public interface IPageActivator
    {
        /// <summary>
        /// Creates a Razor page.
        /// </summary>
        /// <param name="context">The <see cref="PageContext"/>.</param>
        object Create(PageContext context);

        /// <summary>
        /// Releases a Razor page.
        /// </summary>
        /// <param name="context">The <see cref="PageContext"/>.</param>
        /// <param name="page">The page to release.</param>
        void Release(PageContext context, object page);
    }
}