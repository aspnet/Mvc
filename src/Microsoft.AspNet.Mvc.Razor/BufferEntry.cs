// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Represents an entry in <see cref="BufferEntryCollection"/> that references either the string
    /// value to be printed or another BufferEntryCollection..
    /// </summary>
    public sealed class BufferEntry
    {
        /// <summary>
        /// Represents a string entry in the buffer.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Represents a reference to another <see cref="BufferEntryCollection"/>.
        /// </summary>
        public BufferEntryCollection Buffer { get; set; }
    }
}