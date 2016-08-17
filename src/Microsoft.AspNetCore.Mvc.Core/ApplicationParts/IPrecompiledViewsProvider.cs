// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Microsoft.AspNetCore.Mvc.Core.ApplicationParts
{
    /// <summary>
    /// Exposes a sequence of precompiled views associated with an <see cref="ApplicationPart"/> .
    /// </summary>
    public interface IPrecompiledViewsProvider
    {
        /// <summary>
        /// Gets the sequence of <see cref="PrecompiledViewInfo"/>.
        /// </summary>
        IReadOnlyCollection<PrecompiledViewInfo> PrecompiledViews { get; }
    }
}
