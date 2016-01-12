// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.ApiExplorer;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc.Formatters
{
    /// <summary>
    /// Writes an object to the output stream.
    /// </summary>
    public abstract class OutputFormatter : IOutputFormatter, IApiResponseFormatMetadataProvider
    {
        // using a field so we can return it as both IList and IReadOnlyList
        private readonly MediaTypeCollection _supportedMediaTypes;
        private IDictionary<StringSegment, StringSegment> _outputMediaTypeCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputFormatter"/> class.
        /// </summary>
        protected OutputFormatter()
        {
            SupportedEncodings = new List<Encoding>();
            _supportedMediaTypes = new MediaTypeCollection();
        }

        /// <summary>
        /// Gets the mutable collection of character encodings supported by
        /// this <see cref="OutputFormatter"/>. The encodings are
        /// used when writing the data.
        /// </summary>
        public IList<Encoding> SupportedEncodings { get; }

        /// <summary>
        /// Gets the mutable collection of <see cref="StringSegment"/> elements that represent the media types
        /// supported by this <see cref="OutputFormatter"/>.
        /// </summary>
        public MediaTypeCollection SupportedMediaTypes
        {
            get { return _supportedMediaTypes; }
        }

        /// <summary>
        /// Gets a cache with of the <see cref="SupportedMediaTypes"/> with utf-8 encoding
        /// that this formatter uses when it writes the Content-Type header of the response.
        /// </summary>
        protected virtual IDictionary<StringSegment, StringSegment> OutputMediaTypeCache
        {
            get
            {
                if (_outputMediaTypeCache == null)
                {
                    var cache = new Dictionary<StringSegment, StringSegment>();
                    foreach (var mediaType in SupportedMediaTypes)
                    {
                        cache.Add(mediaType, CreateMediaTypeWithEncoding(mediaType, Encoding.UTF8));
                    }

                    // Safe race condition, worst case scenario we initialize the field multiple times with dictionaries containing
                    // the same values.
                    _outputMediaTypeCache = cache;
                }

                return _outputMediaTypeCache;
            }
        }


        /// <summary>
        /// Returns a value indicating whether or not the given type can be written by this serializer.
        /// </summary>
        /// <param name="type">The object type.</param>
        /// <returns><c>true</c> if the type can be written, otherwise <c>false</c>.</returns>
        protected virtual bool CanWriteType(Type type)
        {
            return true;
        }

        /// <inheritdoc />
        public virtual IReadOnlyList<StringSegment> GetSupportedContentTypes(
            StringSegment contentType,
            Type objectType)
        {
            if (!CanWriteType(objectType))
            {
                return null;
            }

            if (!contentType.HasValue)
            {
                // If contentType is null, then any type we support is valid.
                return _supportedMediaTypes.Count > 0 ? _supportedMediaTypes : null;
            }
            else
            {
                List<StringSegment> mediaTypes = null;

                // Confirm this formatter supports a more specific media type than requested e.g. OK if "text/*"
                // requested and formatter supports "text/plain". Treat contentType like it came from an Accept header.
                foreach (var mediaType in _supportedMediaTypes)
                {
                    if (MediaTypeComparisons.IsSubsetOf(contentType, mediaType))
                    {
                        if (mediaTypes == null)
                        {
                            mediaTypes = new List<StringSegment>();
                        }

                        mediaTypes.Add(mediaType);
                    }
                }

                return mediaTypes;
            }
        }

        /// <summary>
        /// Determines the best <see cref="Encoding"/> amongst the supported encodings
        /// for reading or writing an HTTP entity body based on the provided <paramref name="contentTypeHeader"/>.
        /// </summary>
        /// <param name="context">The formatter context associated with the call.
        /// </param>
        /// <returns>The <see cref="Encoding"/> to use when reading the request or writing the response.</returns>
        public virtual Encoding SelectCharacterEncoding(OutputFormatterWriteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var request = context.HttpContext.Request;
            var encoding = MatchAcceptCharacterEncoding(request.GetTypedHeaders().AcceptCharset);
            if (encoding != null)
            {
                return encoding;
            }

            if (context.ContentType.HasValue)
            {
                var contentTypeEncoding = GetEncoding(context.ContentType);
                if (contentTypeEncoding != null)
                {
                    for (var i = 0; i < SupportedEncodings.Count; i++)
                    {
                        if (contentTypeEncoding.Equals(SupportedEncodings[i]))
                        {
                            // This is supported.
                            return SupportedEncodings[i];
                        }
                    }
                }
            }

            // A formatter for a non-text media-type won't have any supported encodings.
            return SupportedEncodings.Count > 0 ? SupportedEncodings[0] : null;
        }

        /// <inheritdoc />
        public virtual bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!CanWriteType(context.ObjectType))
            {
                return false;
            }

            if (!context.ContentType.HasValue)
            {
                // If the desired content type is set to null, then the current formatter can write anything
                // it wants.
                if (SupportedMediaTypes.Count > 0)
                {
                    context.ContentType = SupportedMediaTypes[0];
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // Confirm this formatter supports a more specific media type than requested e.g. OK if "text/*"
                // requested and formatter supports "text/plain". contentType is typically what we got in an Accept
                // header.
                var contentType = context.ContentType;
                for (var i = 0; i < SupportedMediaTypes.Count; i++)
                {
                    var supportedMediaType = SupportedMediaTypes[i];
                    if (MediaTypeComparisons.IsSubsetOf(contentType, supportedMediaType))
                    {
                        context.ContentType = SupportedMediaTypes[i];
                        return true;
                    }
                }
            }

            return false;
        }

        /// <inheritdoc />
        public Task WriteAsync(OutputFormatterWriteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var selectedMediaType = context.ContentType;
            if (!selectedMediaType.HasValue)
            {
                // If content type is not set then set it based on supported media types.
                if (SupportedEncodings.Count > 0)
                {
                    selectedMediaType = SupportedMediaTypes[0];
                }
                else
                {
                    throw new InvalidOperationException(Resources.FormatOutputFormatterNoMediaType(GetType().FullName));
                }
            }

            // Note: Text-based media types will use an encoding/charset - binary formats just ignore it. We want to
            // make this class work with media types that use encodings, and those that don't.
            //
            // The default implementation of SelectCharacterEncoding will read from the list of SupportedEncodings
            // and will always choose a default encoding if any are supported. So, the only cases where the
            // selectedEncoding can be null are:
            //
            // 1). No supported encodings - we assume this is a non-text format
            // 2). Custom implementation of SelectCharacterEncoding - trust the user and give them what they want.
            var selectedEncoding = SelectCharacterEncoding(context);
            if (selectedEncoding != null)
            {
                // Override the content type value even if one already existed.
                selectedMediaType = AddOrOverrideEncoding(selectedMediaType, selectedEncoding);
            }

            context.ContentType = selectedMediaType;

            WriteResponseHeaders(context);
            return WriteResponseBodyAsync(context);
        }

        /// <summary>
        /// Sets the headers on <see cref="Microsoft.AspNet.Http.HttpResponse"/> object.
        /// </summary>
        /// <param name="context">The formatter context associated with the call.</param>
        public virtual void WriteResponseHeaders(OutputFormatterWriteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var response = context.HttpContext.Response;
            response.ContentType = context.ContentType.Value;
        }

        /// <summary>
        /// Writes the response body.
        /// </summary>
        /// <param name="context">The formatter context associated with the call.</param>
        /// <returns>A task which can write the response body.</returns>
        public abstract Task WriteResponseBodyAsync(OutputFormatterWriteContext context);

        /// <summary>
        /// Adds or replaces the charset parameter in a given <paramref name="mediaType"/> with the
        /// given <paramref name="encoding"/>.
        /// </summary>
        /// <param name="mediaType">The <see cref="StringSegment"/> with the media type.</param>
        /// <param name="encoding">
        /// The <see cref="Encoding"/> to add or replace in the <paramref name="mediaType"/>.
        /// </param>
        /// <returns>The mediaType with the given encoding.</returns>
        protected StringSegment AddOrOverrideEncoding(StringSegment mediaType, Encoding encoding)
        {
            var mediaTypeEncoding = GetEncoding(mediaType);
            if (mediaTypeEncoding == encoding)
            {
                return mediaType;
            }
            else if (mediaTypeEncoding == null)
            {
                if (encoding == Encoding.UTF8)
                {
                    return OutputMediaTypeCache[mediaType];
                }
                else
                {
                    return CreateMediaTypeWithEncoding(mediaType, encoding);
                }
            }
            else
            {
                // This can happen if the user has overriden SelectCharacterEncoding
                var result = GetFromCacheIfCompatible(mediaType, encoding);
                return result.HasValue ? result : ReplaceEncoding(mediaType, encoding);
            }
        }

        /// <summary>
        /// Gets the <see cref="Encoding"/> for the given <see cref="mediaType"/> if it exists.
        /// </summary>
        /// <param name="mediaType">The mediaType from which to get the charset parameter.</param>
        /// <returns>The <see cref="Encoding"/> of the mediaType if it exists or a <see cref="StringSegment"/> without value if not.</returns>
        protected Encoding GetEncoding(StringSegment mediaType)
        {
            var charset = GetCharset(mediaType);
            if (charset.Equals("utf-8", StringComparison.OrdinalIgnoreCase))
            {
                // This is an optimization for utf-8 that prevents the Substring caused by
                // charset.Value
                return Encoding.UTF8;
            }

            return charset.HasValue ? Encoding.GetEncoding(charset.Value) : null;
        }

        /// <summary>
        /// Gets the charset parameter of the given <paramref name="mediaType"/> if it exists.
        /// </summary>
        /// <param name="mediaType">The mediaType from which to get the charset parameter.</param>
        /// <returns>The charset of the mediaType if it exists or a <see cref="StringSegment"/> without value if not.</returns>
        protected StringSegment GetCharset(StringSegment mediaType)
        {
            var parser = new MediaTypeParser(
                mediaType.Buffer,
                mediaType.Offset,
                mediaType.Length);

            var parameter = parser.GetParameter("charset");

            if (parameter.HasValue)
            {
                return parameter.Value.Value;
            }
            else
            {
                return new StringSegment(buffer: null);
            }
        }

        private Encoding MatchAcceptCharacterEncoding(IList<StringWithQualityHeaderValue> acceptCharsetHeaders)
        {
            if (acceptCharsetHeaders != null && acceptCharsetHeaders.Count > 0)
            {
                var acceptValues = Sort(acceptCharsetHeaders);
                for (var i = 0; i < acceptValues.Count; i++)
                {
                    var charset = acceptValues[i].Value;
                    if (!string.IsNullOrEmpty(charset))
                    {
                        for (var j = 0; j < SupportedEncodings.Count; j++)
                        {
                            var encoding = SupportedEncodings[j];
                            if (charset.Equals(encoding.WebName, StringComparison.OrdinalIgnoreCase) ||
                                charset.Equals("*", StringComparison.Ordinal))
                            {
                                return encoding;
                            }
                        }
                    }
                }
            }

            return null;
        }

        // There's no allocation-free way to sort an IList and we may have to filter anyway,
        // so we're going to have to live with the copy + insertion sort.
        private IList<StringWithQualityHeaderValue> Sort(IList<StringWithQualityHeaderValue> values)
        {
            var sortNeeded = false;
            var count = 0;

            for (var i = 0; i < values.Count; i++)
            {
                var value = values[i];
                if (value.Quality == HeaderQuality.NoMatch)
                {
                    // Exclude this one
                }
                else if (value.Quality != null)
                {
                    count++;
                    sortNeeded = true;
                }
                else
                {
                    count++;
                }
            }

            if (!sortNeeded)
            {
                return values;
            }

            var sorted = new List<StringWithQualityHeaderValue>();
            for (var i = 0; i < values.Count; i++)
            {
                var value = values[i];
                if (value.Quality == HeaderQuality.NoMatch)
                {
                    // Exclude this one
                }
                else
                {
                    // Doing an insertion sort.
                    var position = sorted.BinarySearch(value, StringWithQualityHeaderValueComparer.QualityComparer);
                    if (position >= 0)
                    {
                        sorted.Insert(position + 1, value);
                    }
                    else
                    {
                        sorted.Insert(~position, value);
                    }
                }
            }

            // We want a descending sort, but BinarySearch does ascending
            sorted.Reverse();
            return sorted;
        }

        private StringSegment ReplaceEncoding(StringSegment mediaType, Encoding encoding)
        {
            var charset = GetCharset(mediaType);
            var charsetOffset = charset.Offset - mediaType.Offset;
            var restOffset = charsetOffset + charset.Length;
            var mediaTypeString = mediaType.Substring(0, charsetOffset);
            var restString = mediaType.Substring(restOffset, mediaType.Length - restOffset);

            return new StringSegment($"{mediaTypeString}{encoding.WebName}{restString}");
        }

        private StringSegment GetFromCacheIfCompatible(StringSegment mediaType, Encoding encoding)
        {
            if (encoding != Encoding.UTF8)
            {
                return new StringSegment();
            }

            foreach (var supportedMediaType in SupportedMediaTypes)
            {
                if (MediaTypeComparisons.IsCompatible(mediaType, supportedMediaType))
                {
                    return OutputMediaTypeCache[supportedMediaType];
                }
            }

            return new StringSegment();
        }

        private static StringSegment CreateMediaTypeWithEncoding(StringSegment mediaType, Encoding encoding)
        {
            return new StringSegment($"{mediaType.Value}; charset={encoding.WebName}");
        }
    }
}
