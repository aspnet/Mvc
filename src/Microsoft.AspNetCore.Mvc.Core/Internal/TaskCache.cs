// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public static class TaskCache
    {
#if NET451
        /// <summary>Gets a task that's already been completed successfully.</summary> 
        public static readonly Task CompletedTask = Task.FromResult(0);
#else
        /// <summary>Gets a task that's already been completed successfully.</summary> 
        public static readonly Task CompletedTask = Task.CompletedTask;
#endif       
    }

}
