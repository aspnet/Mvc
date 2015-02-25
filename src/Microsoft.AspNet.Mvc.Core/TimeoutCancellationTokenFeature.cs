// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.AspNet.Mvc.Core;

namespace Microsoft.AspNet.Mvc
{
    /// <inheritdoc />
    public class TimeoutCancellationTokenFeature : ITimeoutCancellationTokenFeature
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TimeoutCancellationTokenFeature"/>.
        /// </summary>
        /// <param name="durationInMilliseconds">The duration in milliseconds.</param>
        public TimeoutCancellationTokenFeature(int durationInMilliseconds)
        {
            if (durationInMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(durationInMilliseconds),
                    durationInMilliseconds,
                    Resources.AsyncTimeoutAttribute_InvalidTimeout);
            }

            var timeoutCancellationTokenSource = new CancellationTokenSource();
            timeoutCancellationTokenSource.CancelAfter(millisecondsDelay: durationInMilliseconds);

            TimeoutCancellationToken = timeoutCancellationTokenSource.Token;
        }

        /// <inheritdoc />
        public CancellationToken TimeoutCancellationToken { get; }
    }
}