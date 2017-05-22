// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.TagHelpers
{
    /// <summary>
    /// An implementation of this interface provides the list of <see cref="ITagHelperComponent"/>s
    /// that will be used by the <see cref="TagHelperComponentTagHelper"/>.
    /// </summary>
    public interface ITagHelperComponentManager
    {
        /// <summary>
        /// Gets the list of <see cref="ITagHelperComponent"/>s that will be used by the 
        /// <see cref="TagHelperComponentTagHelper"/>.
        /// </summary>
        ICollection<ITagHelperComponent> Components { get; }
    }
}
