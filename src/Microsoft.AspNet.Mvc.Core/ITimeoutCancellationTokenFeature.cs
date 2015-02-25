// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Provides a <see cref="CancellationToken"/> which gets invoked after a specified timeout value.
    /// </summary>
    public interface ITimeoutCancellationTokenFeature
    {
        /// <summary>
        /// Gets a <see cref="CancellationToken"/> which gets invoked 
        /// after a specified timeout value.
        /// </summary>
        CancellationToken TimeoutCancellationToken { get; }
    }
}