// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Used to create a <see cref="CancellationToken"/> which gets invoked when the 
    /// supplied timeout value, in milliseconds, elapses.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method,
        AllowMultiple = false,
        Inherited = true)]
    public class AsyncTimeoutAttribute : Attribute, IAsyncResourceFilter
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AsyncTimeoutAttribute"/>.
        /// </summary>
        /// <param name="durationInMilliseconds">The duration in milliseconds.</param>
        public AsyncTimeoutAttribute(int durationInMilliseconds)
        {
            if (durationInMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(durationInMilliseconds), 
                    durationInMilliseconds, 
                    Resources.AsyncTimeoutAttribute_InvalidTimeout);
            }

            DurationInMilliseconds = durationInMilliseconds;
        }

        /// <summary>
        /// The timeout duration in milliseconds.
        /// </summary>
        public int DurationInMilliseconds { get; }

        public async Task OnResourceExecutionAsync(
            [NotNull] ResourceExecutingContext context,
            [NotNull] ResourceExecutionDelegate next)
        {
            // Set the feature which provides the cancellation token to later stages
            // in the pipeline. This cancellation token gets invoked when the timeout value
            // elapses. One can register to this cancellation token to get notified when the
            // timeout occurs to take any action.
            context.HttpContext.SetFeature<ITimeoutCancellationTokenFeature>(
                new TimeoutCancellationTokenFeature(DurationInMilliseconds));

            await next();
        }
    }
}