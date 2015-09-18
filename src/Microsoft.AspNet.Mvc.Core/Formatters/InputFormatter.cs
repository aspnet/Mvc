// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc.Formatters
{
    /// <summary>
    /// Reads an object from the request body.
    /// </summary>
    public abstract class InputFormatter : IInputFormatter
    {
        /// <summary>
        /// Returns UTF8 Encoding without BOM and throws on invalid bytes.
        /// </summary>
        protected static readonly Encoding UTF8EncodingWithoutBOM
            = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        /// <summary>
        /// Returns UTF16 Encoding which uses littleEndian byte order with BOM and throws on invalid bytes.
        /// </summary>
        protected static readonly Encoding UTF16EncodingLittleEndian
            = new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true);

        /// <summary>
        /// Gets the mutable collection of character encodings supported by
        /// this <see cref="InputFormatter"/>. The encodings are
        /// used when reading the data.
        /// </summary>
        public IList<Encoding> SupportedEncodings { get; } = new List<Encoding>();

        /// <summary>
        /// Gets the mutable collection of <see cref="MediaTypeHeaderValue"/> elements supported by
        /// this <see cref="InputFormatter"/>.
        /// </summary>
        public IList<MediaTypeHeaderValue> SupportedMediaTypes { get; } = new List<MediaTypeHeaderValue>();

        protected object GetDefaultValueForType(Type modelType)
        {
            if (modelType.GetTypeInfo().IsValueType)
            {
                return Activator.CreateInstance(modelType);
            }

            return null;
        }

        /// <inheritdoc />
        public virtual bool CanRead(InputFormatterContext context)
        {
            if (!CanReadType(context.ModelType))
            {
                return false;
            }

            var contentType = context.HttpContext.Request.ContentType;
            MediaTypeHeaderValue requestContentType;
            if (!MediaTypeHeaderValue.TryParse(contentType, out requestContentType))
            {
                return false;
            }

            return SupportedMediaTypes.Any(supportedMediaType => supportedMediaType.IsSubsetOf(requestContentType));
        }

        /// <summary>
        /// Determines whether this <see cref="InputFormatter"/> can deserialize an <c>object</c> of the given
        /// <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of <c>object</c> that will be read.</param>
        /// <returns><c>true</c> if the <paramref name="type"/> can be read; <c>false</c> otherwise.</returns>
        protected virtual bool CanReadType(Type type)
        {
            return true;
        }

        /// <inheritdoc />
        public virtual Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            if (request.ContentLength == 0)
            {
                return InputFormatterResult.SuccessfulAsync(GetDefaultValueForType(context.ModelType));
            }

            return ReadRequestBodyAsync(context);
        }

        /// <summary>
        /// Reads an <c>object</c> from the request body.
        /// </summary>
        /// <param name="context">The <see cref="InputFormatterContext"/>.</param>
        /// <returns>A task that on completion deserializes the request body.</returns>
        public abstract Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context);

        /// <summary>
        /// Returns an <see cref="Encoding"/> based on <paramref name="contentType"/>'s
        /// <see cref="MediaTypeHeaderValue.Charset"/>.
        /// </summary>
        /// <param name="contentType">The <see cref="MediaTypeHeaderValue"/>.</param>
        /// <returns>
        /// An <see cref="Encoding"/> based on <paramref name="contentType"/>'s
        /// <see cref="MediaTypeHeaderValue.Charset"/>. <c>null</c> if no supported encoding was found.
        /// </returns>
        protected Encoding SelectCharacterEncoding(MediaTypeHeaderValue contentType)
        {
            if (contentType != null)
            {
                var charset = contentType.Charset;
                if (!string.IsNullOrWhiteSpace(contentType.Charset))
                {
                    foreach (var supportedEncoding in SupportedEncodings)
                    {
                        if (string.Equals(charset, supportedEncoding.WebName, StringComparison.OrdinalIgnoreCase))
                        {
                            return supportedEncoding;
                        }
                    }
                }
            }

            if (SupportedEncodings.Count > 0)
            {
                return SupportedEncodings[0];
            }

            // No supported encoding was found so there is no way for us to start reading.
            return null;
        }

        protected string GetNoEncodingMessage()
        {
            return Resources.FormatInputFormatterNoEncoding(GetType().FullName);
        }
    }
}