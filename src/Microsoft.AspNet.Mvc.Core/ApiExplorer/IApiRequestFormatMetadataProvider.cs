// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc.ApiExplorer
{
    /// <summary>
    /// Provides metadata information about the request format to an <c>IApiDescriptionProvider</c>.
    /// </summary>
    /// <remarks>
    /// An <see cref="Formatters.IInputFormatter"/> should implement this interface to expose metadata information
    /// to an <c>IApiDescriptionProvider</c>.
    /// </remarks>
    public interface IApiRequestFormatMetadataProvider
    {
        /// <summary>
        /// Gets a filtered list of content types which are supported by the <see cref="Formatters.IInputFormatter"/>
        /// for the <paramref name="objectType"/> and <paramref name="contentType"/>.
        /// </summary>
        /// <param name="contentType">
        /// The content type for which the supported content types are desired, or <c>null</c> if any content
        /// type can be used.
        /// </param>
        /// <param name="objectType">
        /// The <see cref="Type"/> for which the supported content types are desired.
        /// </param>
        /// <returns>Content types which are supported by the <see cref="Formatters.IInputFormatter"/>.</returns>
        IReadOnlyList<MediaTypeHeaderValue> GetSupportedContentTypes(
            MediaTypeHeaderValue contentType,
            Type objectType);
    }
}