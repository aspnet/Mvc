// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// A filter which can be used to enable/disable cors support for a resource.
    /// </summary>
    public interface ICorsAuthorizationFilter : IAsyncAuthorizationFilter, IOrderedFilter
    {
    }
}
