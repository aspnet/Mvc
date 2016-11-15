// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Microsoft.AspNetCore.Mvc.RazorPages
{
    /// <summary>
    /// The context associated with the current request for a Razor page.
    /// </summary>
    public class PageContext : ViewContext
    {
        private PageActionDescriptor _actionDescriptor;

        /// <summary>
        /// Gets or sets the <see cref="PageActionDescriptor"/>.
        /// </summary>
        public new PageActionDescriptor ActionDescriptor
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

        /// <summary>
        /// Gets or sets the <see cref="TypeInfo"/> of the model.
        /// </summary>
        public TypeInfo ModelType { get; set; }
    }
}