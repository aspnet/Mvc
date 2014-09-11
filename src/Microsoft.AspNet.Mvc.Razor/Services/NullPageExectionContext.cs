// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.PageExecution;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Default implementation for <see cref="IPageExecutionContext" />.
    /// </summary>
    public sealed class NullPageExectionContext : IPageExecutionContext
    {
        public static NullPageExectionContext Instance { get; } = new NullPageExectionContext();

        public void BeginContext(int startIndex, int length, bool isLiteral)
        {
            // Do nothing.
        }

        public void EndContext()
        {
            // Do nothing.
        }
    }
}