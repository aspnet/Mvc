// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.RazorPages.ApplicationModels
{
    /// <summary>
    /// Allows customization of the of the <see cref="PageModel"/>.
    /// </summary>
    /// <remarks>
    /// Implementaions of this interface can be registered in <see cref="RazorPagesOptions.Conventions"/>
    /// to customize metadata about the application.
    /// </remarks>
    public interface IPageModelConvention
    {
        /// <summary>
        /// Called to apply the convention to the <see cref="PageModel"/>.
        /// </summary>
        /// <param name="model">The <see cref="PageModel"/>.</param>
        void Apply(PageModel model);
    }
}
