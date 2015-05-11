﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Internal;
using Newtonsoft.Json;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// Base JSON helpers.
    /// </summary>
    public interface IJsonHelper
    {
        /// <summary>
        /// Returns serialized JSON for the <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to serialize as JSON.</param>
        /// <returns>A new <see cref="HtmlString"/> containing the serialized JSON.</returns>
        HtmlString Serialize(object value);

        /// <summary>
        /// Returns serialized JSON for the <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to serialize as JSON.</param>
        /// <param name="serializerSettings">The <see cref="JsonSerializerSettings"/> to be used by the serializer.</param>
        /// <returns>A new <see cref="HtmlString"/> containing the serialized JSON.</returns>
        HtmlString Serialize(object value, [NotNull] JsonSerializerSettings serializerSettings);
    }
}
