// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc.Formatters
{
    /// <summary>
    /// Used to specify mapping between the URL Format and corresponding <see cref="MediaTypeHeaderValue"/>.
    /// </summary>
    public class FormatterMappings
    {
        private readonly Dictionary<string, StringSegment> _map =
            new Dictionary<string, StringSegment>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Sets mapping for the format to specified <see cref="MediaTypeHeaderValue"/>. 
        /// If the format already exists, the <see cref="MediaTypeHeaderValue"/> will be overwritten with the new value.
        /// </summary>
        /// <param name="format">The format value.</param>
        /// <param name="contentType">The <see cref="MediaTypeHeaderValue"/> for the format value.</param>
        public void SetMediaTypeMappingForFormat(string format, MediaTypeHeaderValue contentType)
        {
            if (format == null)
            {
                throw new ArgumentNullException(nameof(format));
            }

            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            ValidateContentType(contentType);
            format = RemovePeriodIfPresent(format);
            _map[format] = new StringSegment(contentType.ToString());
        }

        /// <summary>
        /// Gets <see cref="MediaTypeHeaderValue"/> for the specified format.
        /// </summary>
        /// <param name="format">The format value.</param>
        /// <returns>The <see cref="MediaTypeHeaderValue"/> for input format.</returns>
        public StringSegment GetMediaTypeMappingForFormat(string format)
        {
            if (format == null)
            {
                throw new ArgumentNullException(nameof(format));
            }

            format = RemovePeriodIfPresent(format);

            StringSegment value = default(StringSegment);
            _map.TryGetValue(format, out value);

            return value;
        }

        /// <summary>
        /// Clears the <see cref="MediaTypeHeaderValue"/> mapping for the format.
        /// </summary>
        /// <param name="format">The format value.</param>
        /// <returns><c>true</c> if the format is successfully found and cleared; otherwise, <c>false</c>.</returns>
        public bool ClearMediaTypeMappingForFormat(string format)
        {
            if (format == null)
            {
                throw new ArgumentNullException(nameof(format));
            }

            format = RemovePeriodIfPresent(format);
            return _map.Remove(format);
        }

        private void ValidateContentType(MediaTypeHeaderValue contentType)
        {
            if (contentType.Type == "*" || contentType.SubType == "*")
            {
                throw new ArgumentException(string.Format(Resources.FormatterMappings_NotValidMediaType, contentType));
            }
        }

        private string RemovePeriodIfPresent(string format)
        {
            if (string.IsNullOrEmpty(format))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(format));
            }

            if (format.StartsWith(".", StringComparison.Ordinal))
            {
                if (format == ".")
                {
                    throw new ArgumentException(string.Format(Resources.Format_NotValid, format));
                }

                format = format.Substring(1);
            }

            return format;
        }
    }
}