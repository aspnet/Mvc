﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace Microsoft.AspNetCore.Mvc.TagHelpers.Cache
{
    /// <summary>
    /// An implementation of this interface provides a service to
    /// serialize html fragments for being store by <see cref="IDistributedCacheTagHelperStorage" />
    /// </summary>
    public interface IDistributedCacheTagHelperFormatter
    {
        /// <summary>
        /// Serializes some html content.
        /// </summary>
        /// <param name="context">The <see cref="DistributedCacheTagHelperFormattingContext" /> to serialize.</param>
        /// <returns>The serialized result.</returns>
        Task<byte[]> SerializeAsync(DistributedCacheTagHelperFormattingContext context);

        /// <summary>
        /// Deserialize some html content.
        /// </summary>
        /// <param name="value">The value to deserialize.</param>
        /// <returns>The deserialized content, <value>null</value> otherwise.</returns>
        Task<HtmlString> DeserializeAsync(byte[] value);
    }
}
