// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Runtime;

namespace Microsoft.AspNet.PageExecution
{
    /// <summary>
    /// Specifies the contracts for a execution context that instruments Razor page execution.
    /// </summary>
    [AssemblyNeutral]
    public interface IPageExecutionContext
    {
        /// <summary>
        /// Invoked at the start of a write operation.
        /// </summary>
        /// <param name="position">The position of the expression or text in the Razor file.</param>
        /// <param name="length">The length of the expression or text in the Razor file.</param>
        /// <param name="isLiteral">A flag that indiciates if the operation is for a literal text.</param>
        void BeginContext(int position, int length, bool isLiteral);

        /// <summary>
        /// Invoked at the end of a write operation.
        /// </summary>
        void EndContext();
    }
}