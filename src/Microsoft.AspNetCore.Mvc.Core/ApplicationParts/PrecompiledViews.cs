// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.Core.ApplicationParts
{
    /// <summary>
    /// Provides information for precompiled views.
    /// </summary>
    public abstract class PrecompiledViews : IEnumerable<PrecompiledViewInfo>
    {
        private IEnumerable<PrecompiledViewInfo> _precompiledViews;

        protected PrecompiledViews(IEnumerable<PrecompiledViewInfo> precompiledViews)
        {
            _precompiledViews = precompiledViews;
        }

        public IEnumerator<PrecompiledViewInfo> GetEnumerator() => _precompiledViews.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
