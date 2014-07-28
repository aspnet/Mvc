// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Represents a message encountered during compilation.
    /// </summary>
    public class CompilationMessage
    {
        /// <summary>
        /// Initializes a <see cref="CompilationMessage"/> with the specified message.
        /// </summary>
        /// <param name="message">A message produced from compilation.</param>
        public CompilationMessage(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Gets a message produced from compilation.
        /// </summary>
        public string Message { get; private set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Message;
        }
    }
}
