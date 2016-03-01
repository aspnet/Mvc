// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    /// <summary>
    /// Creates and manages the lifetime of <see cref="T:ViewBufferValue[]"/> instances.
    /// </summary>
    public interface IViewBufferScope
    {
        /// <summary>
        /// Gets a <see cref="T:ViewBufferValue[]"/>.
        /// <param name="size">The minimum size of the returned segment.</param>
        /// <returns>The <see cref="T:ViewBufferValue[]"/>.</returns>
        ViewBufferValue[] GetSegment(int size);

        /// <summary>
        /// Returns a <see cref="T:ViewBufferValue[]"/> that can be reused.
        /// </summary>
        /// <param name="segment">The <see cref="T:ViewBufferValue[]"/>.</param>
        void ReturnSegment(ViewBufferValue[] segment);

        /// <summary>
        /// Creates a <see cref="PagedBufferedTextWriter"/> that will delegate to the provided
        /// <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/>.</param>
        /// <returns>A <see cref="PagedBufferedTextWriter"/>.</returns>
        PagedBufferedTextWriter CreateWriter(TextWriter writer);
    }
}
