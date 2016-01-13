// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc.ApiExplorer
{
    /// <summary>
    /// Provides a a set of possible content types than can be consumed by the action.
    /// </summary>
    public interface IApiRequestMetadataProvider
    {
        /// <summary>
        /// Configures a collection of allowed content types which can be consumed by the action.
        /// </summary>
        void SetContentTypes(IList<MediaTypeHeaderValue> contentTypes);
    }
}