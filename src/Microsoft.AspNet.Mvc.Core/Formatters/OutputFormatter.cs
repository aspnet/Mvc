// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.HeaderValueAbstractions;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Core;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Writes an object to the output stream.
    /// </summary>
    public abstract class OutputFormatter
    {
        /// <summary>
        /// Gets the mutable collection of character encodings supported by
        /// this <see cref="OutputFormatter"/> instance. The encodings are
        /// used when writing the data.
        /// </summary>
        public List<Encoding> SupportedEncodings { get; private set; }

        /// <summary>
        /// Gets the mutable collection of <see cref="MediaTypeHeaderValue"/> elements supported by
        /// this <see cref="OutputFormatter"/> instance.
        /// </summary>
        public List<MediaTypeHeaderValue> SupportedMediaTypes { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputFormatter"/> class.
        /// </summary>
        protected OutputFormatter()
        {
            SupportedEncodings = new List<Encoding>();
            SupportedMediaTypes = new List<MediaTypeHeaderValue>();
        }

        /// <summary>
        /// Determines the best <see cref="Encoding"/> amongst the supported encodings
        /// for reading or writing an HTTP entity body based on the provided <paramref name="contentTypeHeader"/>.
        /// </summary>
        /// <param name="contentTypeHeader">The content type header provided as part of the request or response.</param>
        /// <returns>The <see cref="Encoding"/> to use when reading the request or writing the response.</returns>
        public virtual Encoding SelectCharacterEncoding(MediaTypeHeaderValue contentTypeHeader)
        {
            // Performance-sensitive
            Encoding encoding = null;
            if (contentTypeHeader != null)
            {
                // Find encoding based on content type charset parameter
                var charset = contentTypeHeader.Charset;
                if (!String.IsNullOrWhiteSpace(charset))
                {
                    encoding = SupportedEncodings.FirstOrDefault(
                                                    supportedEncoding => 
                                                        charset.Equals(supportedEncoding.WebName, 
                                                                       StringComparison.OrdinalIgnoreCase));
                }
            }

            if (encoding == null)
            {
                // We didn't find a character encoding match based on the content headers.
                // Instead we try getting the default character encoding.
                if (SupportedEncodings.Count > 0)
                {
                    encoding = SupportedEncodings[0];
                }
            }

            if (encoding == null)
            {
                // No supported encoding was found so there is no way for us to start writing.
                throw new InvalidOperationException(Resources.FormatMediaTypeFormatterNoEncoding(GetType().Name));
            }

            return encoding;
        }

        /// <summary>
        /// Determines whether this <see cref="OutputFormatter"/> can serialize
        /// an object of the specified type.
        /// </summary>
        /// <param name="result">ObjectResult which represents the result of the action.</param>
        /// <param name="declaredType">The compile time type of the returned value.</param>
        /// <param name="context">The http context associated with the call.</param>
        /// <returns>True if this <see cref="OutputFormatter"/> is able to serialize the object
        /// represent by <paramref name="result"/>. False otherwise.</returns>
        public abstract bool CanWriteResult(ObjectResult result,
                                            Type declaredType,
                                            HttpContext context);

        /// <summary>
        /// Writes given <paramref name="value"/> to the HttpResponse <paramref name="response"/> body stream. 
        /// </summary>
        /// <param name="value">The object value to write.</param>
        /// <param name="declaredType">The compile time type of the object.</param>
        /// <param name="response">The <see cref="HttpResponse"/> which should be written to.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> that serializes the given <paramref name="value"/> 
        /// of the given <paramref name="type"/> to the given <paramref name="response"/>.</returns>
        public abstract Task WriteAsync(object value,
                                        Type declaredType,
                                        HttpResponse response,
                                        CancellationToken cancellationToken);
    }
}
