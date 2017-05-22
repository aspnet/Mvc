// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.TagHelpers
{
    /// <summary>
    /// Contract used to manage <see cref="ITagHelperComponent"/>s.
    /// </summary>
    public interface ITagHelperComponentManager
    {
        /// <summary>
        /// The collection of <see cref="ITagHelperComponent"/>s to manage.
        /// </summary>
        ICollection<ITagHelperComponent> Components { get; }
    }
}
