// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Internal;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Represents an <see cref="ActionResult"/> that when executed will
    /// write a stream to the response.
    /// </summary>
    public class StreamResult : FileResult
    {
        // default buffer size as defined in BufferedStream type
        private const int BufferSize = 0x1000;

        private Stream _stream;

        /// <summary>
        /// Creates a new <see cref="StreamResult"/> instance with
        /// the provided <paramref name="fileStream"/> and the
        /// provided <paramref name="contentType"/>.
        /// </summary>
        /// <param name="stream">The stream with the content.</param>
        /// <param name="contentType">The Content-Type header of the response.</param>
        public StreamResult([NotNull] Stream stream, [NotNull] string contentType)
            : this(stream, new MediaTypeHeaderValue(contentType))
        {
        }

        /// <summary>
        /// Creates a new <see cref="StreamResult"/> instance with
        /// the provided <paramref name="fileStream"/> and the
        /// provided <paramref name="contentType"/>.
        /// </summary>
        /// <param name="stream">The stream with the content.</param>
        /// <param name="contentType">The Content-Type header of the response.</param>
        public StreamResult([NotNull] Stream stream, [NotNull] MediaTypeHeaderValue contentType)
            : base(contentType)
        {
            Stream = stream;
        }

        /// <summary>
        /// Gets or sets the stream that will be sent back as the response.
        /// </summary>
        public Stream Stream
        {
            get
            {
                return _stream;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _stream = value;
            }
        }

        /// <inheritdoc />
        protected async override Task WriteFileAsync(HttpResponse response, CancellationToken cancellation)
        {
            var outputStream = response.Body;

            using (Stream)
            {
                await Stream.CopyToAsync(outputStream, BufferSize, cancellation);
            }
        }
    }
}