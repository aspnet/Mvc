﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.Internal
{
    /// <summary>
    /// Utility methods for dealing with <see cref="Task"/>.
    /// </summary>
    public static class TaskHelper
    {
        /// <summary>
        /// Throws the first faulting exception for a task which is faulted. It preserves the original stack trace when
        /// throwing the exception.
        /// </summary>
        public static void ThrowIfFaulted(Task task)
        {
            task.GetAwaiter().GetResult();
        }
    }
}