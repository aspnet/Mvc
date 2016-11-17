// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.RazorPages
{
    /// <summary>
    /// A <see cref="PageActionDescriptor"/> for a compiled Razor page.
    /// </summary>
    public class CompiledPageActionDescriptor : PageActionDescriptor
    {
        /// <summary>
        /// Gets or sets the <see cref="TypeInfo"/> of the page.
        /// </summary>
        public TypeInfo PageTypeInfo { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TypeInfo"/> of the model.
        /// </summary>
        public TypeInfo ModelTypeInfo { get; set; }
    }
}
