// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.Formatters
{
    /// <summary>
    /// A default implementation of <see cref="MediaTypeRegistry"/>.
    /// </summary>
    public class DefaultMediaTypeRegistry : MediaTypeRegistry
    {
        /// <summary>
        /// Creates a new instance of <see cref="DefaultMediaTypeRegistry"/>.
        /// </summary>
        public DefaultMediaTypeRegistry()
        {
            KnownMappings = new Dictionary<Type, string[]>
            {
                { typeof(ProblemDetails), new string[]{ "application/problem+json", "application/problem+xml", } },
            };
        }

        /// <summary>
        /// Gets a dictionary of known mappings between types and media types.
        /// </summary>
        /// <remarks>
        /// This property is not thread-safe. Consumers should not mutate this property while requests are served.
        /// </remarks>
        public IDictionary<Type, string[]> KnownMappings { get; }

        /// <summary>
        /// Gets a list of media types that used as default media types when a <see cref="Type"/> does not have
        /// a specific mapping.
        /// </summary>
        /// <remarks>
        /// This property is not thread-safe. Consumers should not mutate this property while requests are served.
        /// </remarks>
        public string[] FallbackMediaTypes { get; set; }

        /// <inheritdoc />
        public sealed override IReadOnlyList<string> GetMediaTypes(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (KnownMappings.TryGetValue(type, out var mediaTypes))
            {
                return mediaTypes;
            }

            return FallbackMediaTypes;
        }
    }
}
