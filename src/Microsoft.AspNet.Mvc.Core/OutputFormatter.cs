// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.ApiExplorer;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.Framework.Internal;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Writes an object to the output stream.
    /// </summary>
    public abstract class OutputFormatter : IOutputFormatter, IApiResponseFormatMetadataProvider
    {
        // using a field so we can return it as both IList and IReadOnlyList
        private readonly List<MediaTypeHeaderValue> _supportedMediaTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputFormatter"/> class.
        /// </summary>
        protected OutputFormatter()
        {
            SupportedEncodings = new List<Encoding>();
            _supportedMediaTypes = new List<MediaTypeHeaderValue>();
        }

        /// <summary>
        /// Gets the mutable collection of character encodings supported by
        /// this <see cref="OutputFormatter"/>. The encodings are
        /// used when writing the data.
        /// </summary>
        public IList<Encoding> SupportedEncodings { get; }

        /// <summary>
        /// Gets the mutable collection of <see cref="MediaTypeHeaderValue"/> elements supported by
        /// this <see cref="OutputFormatter"/>.
        /// </summary>
        public IList<MediaTypeHeaderValue> SupportedMediaTypes
        {
            get { return _supportedMediaTypes; }
        }

        /// <summary>
        /// Returns a value indicating whether or not the given type can be written by this serializer.
        /// </summary>
        /// <param name="declaredType">The declared type.</param>
        /// <param name="runtimeType">The runtime type.</param>
        /// <returns><c>true</c> if the type can be written, otherwise <c>false</c>.</returns>
        protected virtual bool CanWriteType(Type declaredType, Type runtimeType)
        {
            return true;
        }

        /// <inheritdoc />
        public virtual IReadOnlyList<MediaTypeHeaderValue> GetSupportedContentTypes(
            Type declaredType,
            Type runtimeType,
            MediaTypeHeaderValue contentType)
        {
            if (!CanWriteType(declaredType, runtimeType))
            {
                return null;
            }

            if (contentType == null)
            {
                // If contentType is null, then any type we support is valid.
                return _supportedMediaTypes.Count > 0 ? _supportedMediaTypes : null;
            }
            else
            {
                List<MediaTypeHeaderValue> mediaTypes = null;

                foreach (var mediaType in _supportedMediaTypes)
                {
                    if (mediaType.IsSubsetOf(contentType))
                    {
                        if (mediaTypes == null)
                        {
                            mediaTypes = new List<MediaTypeHeaderValue>();
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
        public virtual Encoding SelectCharacterEncoding([NotNull] OutputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            var encoding = MatchAcceptCharacterEncoding(request.GetTypedHeaders().AcceptCharset);
            if (encoding == null)
            {
                // Match based on request acceptHeader.
                MediaTypeHeaderValue requestContentType = null;
                if (MediaTypeHeaderValue.TryParse(request.ContentType, out requestContentType) &&
                    !string.IsNullOrEmpty(requestContentType.Charset))
                {
                    var requestCharset = requestContentType.Charset;
                    encoding = SupportedEncodings.FirstOrDefault(
                                                            supportedEncoding =>
                                                                requestCharset.Equals(supportedEncoding.WebName));
                }
            }

            encoding = encoding ?? SupportedEncodings.FirstOrDefault();
            return encoding;
        }

        /// <inheritdoc />
        public virtual bool CanWriteResult([NotNull] OutputFormatterContext context, MediaTypeHeaderValue contentType)
        {
            var runtimeType = context.Object == null ? null : context.Object.GetType();
            if (!CanWriteType(context.DeclaredType, runtimeType))
            {
                return false;
            }

            MediaTypeHeaderValue mediaType = null;
            if (contentType == null)
            {
                // If the desired content type is set to null, the current formatter is free to choose the
                // response media type.
                mediaType = SupportedMediaTypes.FirstOrDefault();
            }
            else
            {
                // Since supportedMedia Type is going to be more specific check if supportedMediaType is a subset
                // of the content type which is typically what we get on acceptHeader.
                mediaType = SupportedMediaTypes
                                  .FirstOrDefault(supportedMediaType => supportedMediaType.IsSubsetOf(contentType));
            }

            if (mediaType != null)
            {
                context.SelectedContentType = mediaType;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public Task WriteAsync([NotNull] OutputFormatterContext context)
        {
            WriteResponseHeaders(context);
            return WriteResponseBodyAsync(context);
        }

        /// <summary>
        /// Sets the headers on <see cref="Microsoft.AspNet.Http.HttpResponse"/> object.
        /// </summary>
        /// <param name="context">The formatter context associated with the call.</param>
        public virtual void WriteResponseHeaders([NotNull] OutputFormatterContext context)
        {
            var selectedMediaType = context.SelectedContentType;

            // If content type is not set then set it based on supported media types.
            selectedMediaType = selectedMediaType ?? SupportedMediaTypes.FirstOrDefault();
            if (selectedMediaType == null)
            {
                throw new InvalidOperationException(Resources.FormatOutputFormatterNoMediaType(GetType().FullName));
            }

            // Copy the media type as we don't want it to affect the next request
            selectedMediaType = selectedMediaType.Copy();

            // Not text-based media types will use an encoding/charset - binary formats just ignore it. We want to
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
                context.SelectedEncoding = selectedEncoding;

                // Override the content type value even if one already existed.
                selectedMediaType.Charset = selectedEncoding.WebName;
            }

            context.SelectedContentType = context.SelectedContentType ?? selectedMediaType;

            var response = context.HttpContext.Response;
            response.ContentType = selectedMediaType.ToString();
        }

        /// <summary>
        /// Writes the response body.
        /// </summary>
        /// <param name="context">The formatter context associated with the call.</param>
        /// <returns>A task which can write the response body.</returns>
        public abstract Task WriteResponseBodyAsync([NotNull] OutputFormatterContext context);

        private Encoding MatchAcceptCharacterEncoding(IList<StringWithQualityHeaderValue> acceptCharsetHeaders)
        {
            if (acceptCharsetHeaders != null && acceptCharsetHeaders.Count > 0)
            {
                var sortedAcceptCharsetHeaders = acceptCharsetHeaders
                                                    .Where(acceptCharset =>
                                                                acceptCharset.Quality != HeaderQuality.NoMatch)
                                                    .OrderByDescending(
                                                        m => m, StringWithQualityHeaderValueComparer.QualityComparer);

                foreach (var acceptCharset in sortedAcceptCharsetHeaders)
                {
                    var charset = acceptCharset.Value;
                    if (!string.IsNullOrWhiteSpace(charset))
                    {
                        var encoding = SupportedEncodings.FirstOrDefault(
                                                        supportedEncoding =>
                                                            charset.Equals(supportedEncoding.WebName,
                                                                           StringComparison.OrdinalIgnoreCase) ||
                                                            charset.Equals("*", StringComparison.Ordinal));
                        if (encoding != null)
                        {
                            return encoding;
                        }
                    }
                }
            }

            return null;
        }
    }
}
