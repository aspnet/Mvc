// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.RazorPages
{
    /// <summary>
    /// Provides methods for creation and disposal of Razor pages.
    /// </summary>
    public interface IPageFactory
    {
        /// <summary>
        /// Creates a new Razor page for the specified <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The <see cref="PageContext"/>.</param>
        /// <returns>The Razor page.</returns>
        object CreatePage(PageContext context);

        /// <summary>
        /// Releases a Razor page.
        /// </summary>
        /// <param name="context">The <see cref="PageContext"/>.</param>
        /// <param name="page">The Razor page to release.</param>
        void ReleasePage(PageContext context, object page);
    }
}