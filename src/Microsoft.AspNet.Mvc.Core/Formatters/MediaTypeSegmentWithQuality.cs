// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Represents a media type with its associated quality.
    /// </summary>
    public struct MediaTypeSegmentWithQuality
    {
        /// <summary>
        /// Gets or sets the media type of this <see cref="MediaTypeSegmentWithQuality"/>.
        /// </summary>
        public StringSegment MediaType { get; set; }

        /// <summary>
        /// Gets or sets the quality of this <see cref="MediaTypeSegmentWithQuality"/>.
        /// </summary>
        public double Quality { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            // For logging purposes
            return MediaType.ToString();
        }
    }
}
