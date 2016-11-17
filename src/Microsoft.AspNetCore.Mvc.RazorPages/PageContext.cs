// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.Rendering;

namespace Microsoft.AspNetCore.Mvc.RazorPages
{
    /// <summary>
    /// The context associated with the current request for a Razor page.
    /// </summary>
    public class PageContext : ViewContext
    {
        private CompiledPageActionDescriptor _actionDescriptor;

        /// <summary>
        /// Gets or sets the <see cref="PageActionDescriptor"/>.
        /// </summary>
        public new CompiledPageActionDescriptor ActionDescriptor
        {
            get
            {
                return _actionDescriptor;
            }
            set
            {
                _actionDescriptor = value;
                base.ActionDescriptor = value;
            }
        }
    }
}