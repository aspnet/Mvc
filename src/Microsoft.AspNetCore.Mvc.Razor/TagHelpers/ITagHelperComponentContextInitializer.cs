// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.TagHelpers
{
    /// <summary>
    /// Provides methods to initialize <see cref="ViewContext"/> for <see cref="ITagHelperComponent"/>s.
    /// </summary>
    public interface ITagHelperComponentContextInitializer
    {
        /// <summary>
        /// Creates an <see cref="ITagHelperComponent"/> with <see cref="ViewContext"/> initialized.
        /// </summary>
        /// <param name="tagHelperComponent">The <see cref="ITagHelperComponent"/> to intialize <see cref="ViewContext"/> for.</param>
        /// <param name="context">The <see cref="ViewContext"/> for the executing view.</param>
        /// <returns>The tag helper component.</returns>
        ITagHelperComponent InitializeViewContext(ITagHelperComponent tagHelperComponent, ViewContext context);
    }
}
