// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Used to set the timeout value, in milliseconds, 
    /// which when elapsed will cause the current request to be aborted.
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
            var httpContext = context.HttpContext;

            // Get a task that will complete after a time delay.
            var timeDelayTask = Task.Delay(DurationInMilliseconds, cancellationToken: httpContext.RequestAborted);

            // Task representing later stages of the pipeline.
            var pipelineTask = next();

            // Get the first task which completed.
            var completedTask = await Task.WhenAny(pipelineTask, timeDelayTask);

            if (completedTask == pipelineTask)
            {
                // Task completed within timeout, but it could be in faulted or canceled state.
                // Allow the following line to throw exception and be handled somewhere else.
                await completedTask;
            }
            else
            {
                // Pipeline task did not complete within timeout, so abort the request.
                httpContext.Abort();
            }
        }
    }
}