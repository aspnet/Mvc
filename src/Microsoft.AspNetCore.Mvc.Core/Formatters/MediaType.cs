// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Formatters.Internal;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Mvc.Formatters
{
    /// <summary>
    /// A media type value.
    /// </summary>
    public struct MediaType
    {
        private static readonly StringSegment QualityParameter = new StringSegment("q");
        private MediaTypeHeaderValue _mediaTypeHeaderValue;

        /// <summary>
        /// Initializes a <see cref="MediaType"/> instance.
        /// </summary>
        /// <param name="mediaType">The <see cref="string"/> with the media type.</param>
        public MediaType(string mediaType)
            : this(mediaType, 0, mediaType.Length)
        {
        }

        /// <summary>
        /// Initializes a <see cref="MediaType"/> instance.
        /// </summary>
        /// <param name="mediaType">The <see cref="StringSegment"/> with the media type.</param>
        public MediaType(StringSegment mediaType)
            : this(mediaType.Buffer, mediaType.Offset, mediaType.Length)
        {
        }

        /// <param name="mediaType">The <see cref="string"/> with the media type.</param>
        /// <param name="offset">The offset in the <paramref name="mediaType"/> where the parsing starts.</param>
        /// <param name="length">The length of the media type to parse if provided.</param>
        public MediaType(string mediaType, int offset, int? length)
        {
            _mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(mediaType);
            SubType = _mediaTypeHeaderValue.SubType;
            Type = _mediaTypeHeaderValue.Type;
            SubTypeSuffix = _mediaTypeHeaderValue.Suffix;
            SubTypeWithoutSuffix = _mediaTypeHeaderValue.SubTypeWithoutSuffix;
        }

        /// <summary>
        /// Gets the type of the <see cref="MediaType"/>.
        /// </summary>
        /// <example>
        /// For the media type <c>"application/json"</c>, this property gives the value <c>"application"</c>.
        /// </example>
        public StringSegment Type { get; }

        /// <summary>
        /// Gets whether this <see cref="MediaType"/> matches all types.
        /// </summary>
        public bool MatchesAllTypes => Type.Equals("*", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the subtype of the <see cref="MediaType"/>.
        /// </summary>
        /// <example>
        /// For the media type <c>"application/vnd.example+json"</c>, this property gives the value
        /// <c>"vnd.example+json"</c>.
        /// </example>
        public StringSegment SubType { get; }

        /// <summary>
        /// Gets the subtype of the <see cref="MediaType"/>, excluding any structured syntax suffix.
        /// </summary>
        /// <example>
        /// For the media type <c>"application/vnd.example+json"</c>, this property gives the value
        /// <c>"vnd.example"</c>.
        /// </example>
        public StringSegment SubTypeWithoutSuffix { get; }

        /// <summary>
        /// Gets the structured syntax suffix of the <see cref="MediaType"/> if it has one.
        /// </summary>
        /// <example>
        /// For the media type <c>"application/vnd.example+json"</c>, this property gives the value
        /// <c>"json"</c>.
        /// </example>
        public StringSegment SubTypeSuffix { get; }

        /// <summary>
        /// Gets whether this <see cref="MediaType"/> matches all subtypes.
        /// </summary>
        /// <example>
        /// For the media type <c>"application/*"</c>, this property is <c>true</c>.
        /// </example>
        /// <example>
        /// For the media type <c>"application/json"</c>, this property is <c>false</c>.
        /// </example>
        public bool MatchesAllSubTypes => SubType.Equals("*", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets whether this <see cref="MediaType"/> matches all subtypes, ignoring any structured syntax suffix.
        /// </summary>
        /// <example>
        /// For the media type <c>"application/*+json"</c>, this property is <c>true</c>.
        /// </example>
        /// <example>
        /// For the media type <c>"application/vnd.example+json"</c>, this property is <c>false</c>.
        /// </example>
        public bool MatchesAllSubTypesWithoutSuffix => SubTypeWithoutSuffix.Equals("*", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the <see cref="System.Text.Encoding"/> of the <see cref="MediaType"/> if it has one.
        /// </summary>
        public Encoding Encoding => GetEncodingFromCharset(GetParameter("charset"));

        /// <summary>
        /// Gets the charset parameter of the <see cref="MediaType"/> if it has one.
        /// </summary>
        public StringSegment Charset => GetParameter("charset");

        /// <summary>
        /// Determines whether the current <see cref="MediaType"/> contains a wildcard.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this <see cref="MediaType"/> contains a wildcard; otherwise <c>false</c>.
        /// </returns>
        public bool HasWildcard
        {
            get
            {
                return MatchesAllTypes ||
                    MatchesAllSubTypesWithoutSuffix ||
                    GetParameter("*").Equals("*", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Determines whether the current <see cref="MediaType"/> is a subset of the <paramref name="set"/>
        /// <see cref="MediaType"/>.
        /// </summary>
        /// <param name="set">The set <see cref="MediaType"/>.</param>
        /// <returns>
        /// <c>true</c> if this <see cref="MediaType"/> is a subset of <paramref name="set"/>; otherwise <c>false</c>.
        /// </returns>
        public bool IsSubsetOf(MediaType set)
        {
            return _mediaTypeHeaderValue.IsSubsetOf(set._mediaTypeHeaderValue);
        }

        /// <summary>
        /// Gets the parameter <paramref name="parameterName"/> of the media type.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to retrieve.</param>
        /// <returns>
        /// The <see cref="StringSegment"/>for the given <paramref name="parameterName"/> if found; otherwise
        /// <c>null</c>.
        /// </returns>
        public StringSegment GetParameter(string parameterName)
        {
            return GetParameter(new StringSegment(parameterName));
        }

        /// <summary>
        /// Gets the parameter <paramref name="parameterName"/> of the media type.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to retrieve.</param>
        /// <returns>
        /// The <see cref="StringSegment"/>for the given <paramref name="parameterName"/> if found; otherwise
        /// <c>null</c>.
        /// </returns>
        public StringSegment GetParameter(StringSegment parameterName)
        {
            foreach (var nameValue in _mediaTypeHeaderValue.Parameters)
            {
                if (nameValue.Name == parameterName)
                {
                    return nameValue.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Replaces the encoding of the given <paramref name="mediaType"/> with the provided
        /// <paramref name="encoding"/>.
        /// </summary>
        /// <param name="mediaType">The media type whose encoding will be replaced.</param>
        /// <param name="encoding">The encoding that will replace the encoding in the <paramref name="mediaType"/>.
        /// </param>
        /// <returns>A media type with the replaced encoding.</returns>
        public static string ReplaceEncoding(string mediaType, Encoding encoding)
        {
            return ReplaceEncoding(new StringSegment(mediaType), encoding);
        }

        /// <summary>
        /// Replaces the encoding of the given <paramref name="mediaType"/> with the provided
        /// <paramref name="encoding"/>.
        /// </summary>
        /// <param name="mediaType">The media type whose encoding will be replaced.</param>
        /// <param name="encoding">The encoding that will replace the encoding in the <paramref name="mediaType"/>.
        /// </param>
        /// <returns>A media type with the replaced encoding.</returns>
        public static string ReplaceEncoding(StringSegment mediaType, Encoding encoding)
        {
            var parsedMediaType = new MediaType(mediaType);
            var charset = parsedMediaType.GetParameter("charset");

            if (charset.HasValue && charset.Equals(encoding.WebName, StringComparison.OrdinalIgnoreCase))
            {
                return mediaType.Value;
            }

            if (!charset.HasValue)
            {
                return CreateMediaTypeWithEncoding(mediaType, encoding);
            }

            var charsetOffset = charset.Offset - mediaType.Offset;
            var restOffset = charsetOffset + charset.Length;
            var restLength = mediaType.Length - restOffset;
            var finalLength = charsetOffset + encoding.WebName.Length + restLength;

            var builder = new StringBuilder(mediaType.Buffer, mediaType.Offset, charsetOffset, finalLength);
            builder.Append(encoding.WebName);
            builder.Append(mediaType.Buffer, restOffset, restLength);

            return builder.ToString();
        }

        public static Encoding GetEncoding(string mediaType)
        {
            return GetEncoding(new StringSegment(mediaType));
        }

        public static Encoding GetEncoding(StringSegment mediaType)
        {
            var parsedMediaType = new MediaType(mediaType);
            return parsedMediaType.Encoding;
        }

        /// <summary>
        /// Creates an <see cref="MediaTypeSegmentWithQuality"/> containing the media type in <paramref name="mediaType"/>
        /// and its associated quality.
        /// </summary>
        /// <param name="mediaType">The media type to parse.</param>
        /// <param name="start">The position at which the parsing starts.</param>
        /// <returns>The parsed media type with its associated quality.</returns>
        public static MediaTypeSegmentWithQuality CreateMediaTypeSegmentWithQuality(string mediaType, int start)
        {
            var parsedMediaType = new MediaType(mediaType, start, length: null);
            // Short-circuit use of the MediaTypeParameterParser if constructor detected an invalid type or subtype.
            // Parser would set ParsingFailed==true in this case. But, we handle invalid parameters as a separate case.
            if (parsedMediaType.Type.Equals(default(StringSegment)) ||
                parsedMediaType.SubType.Equals(default(StringSegment)))
            {
                return default(MediaTypeSegmentWithQuality);
            }

            var quality = 1.0d;
            foreach (var nameValue in parsedMediaType._mediaTypeHeaderValue.Parameters)
            {
                if (nameValue.Name == QualityParameter)
                {
                    // If media type contains two `q` values i.e. it's invalid in an uncommon way, pick last value.
                    quality = double.Parse(
                        nameValue.Value.Value, NumberStyles.AllowDecimalPoint,
                        NumberFormatInfo.InvariantInfo);
                }
            }

            // We check if the parsed media type has a value at this stage when we have iterated
            // over all the parameters and we know if the parsing was successful.

            return new MediaTypeSegmentWithQuality(
                parsedMediaType._mediaTypeHeaderValue.MediaType,
                quality);
        }

        private static Encoding GetEncodingFromCharset(StringSegment charset)
        {
            if (charset.Equals("utf-8", StringComparison.OrdinalIgnoreCase))
            {
                // This is an optimization for utf-8 that prevents the Substring caused by
                // charset.Value
                return Encoding.UTF8;
            }

            try
            {
                // charset.Value might be an invalid encoding name as in charset=invalid.
                // For that reason, we catch the exception thrown by Encoding.GetEncoding
                // and return null instead.
                return charset.HasValue ? Encoding.GetEncoding(charset.Value) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string CreateMediaTypeWithEncoding(StringSegment mediaType, Encoding encoding)
        {
            return $"{mediaType.Value}; charset={encoding.WebName}";
        }
    }
}
