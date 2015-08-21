// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.Framework.Internal;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// An <see cref="ActionResult"/> that when executed will write a file from disk to the response
    /// using mechanisms provided by the host.
    /// </summary>
    public class PhysicalFilePathResult : FileResult
    {
        private const int DefaultBufferSize = 0x1000;
        private string _fileName;

        /// <summary>
        /// Creates a new <see cref="PhysicalFilePathResult"/> instance with
        /// the provided <paramref name="fileName"/> and the provided <paramref name="contentType"/>.
        /// </summary>
        /// <param name="fileName">The path to the file. The path must be an absolute path.</param>
        /// <param name="contentType">The Content-Type header of the response.</param>
        public PhysicalFilePathResult([NotNull] string fileName, [NotNull] string contentType)
            : this(fileName, new MediaTypeHeaderValue(contentType))
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhysicalFilePathResult"/> instance with
        /// the provided <paramref name="fileName"/> and the provided <paramref name="contentType"/>.
        /// </summary>
        /// <param name="fileName">The path to the file. The path must be an absolute path.</param>
        /// <param name="contentType">The Content-Type header of the response.</param>
        public PhysicalFilePathResult([NotNull] string fileName, [NotNull] MediaTypeHeaderValue contentType)
            : base(contentType)
        {
            FileName = fileName;
        }

        /// <summary>
        /// Gets or sets the path to the file that will be sent back as the response.
        /// </summary>
        public string FileName
        {
            get
            {
                return _fileName;
            }

            [param: NotNull]
            set
            {
                _fileName = value;
            }
        }

        /// <inheritdoc />
        protected override Task WriteFileAsync(HttpResponse response, CancellationToken cancellation)
        {
            var pathWithReplacedSlashes = NormalizePath(FileName);
            if (!Path.IsPathRooted(pathWithReplacedSlashes))
            {
                throw new FileNotFoundException(Resources.FormatFileResult_InvalidPath(FileName), FileName);
            }

            return CopyPhysicalFileToResponseAsync(response, pathWithReplacedSlashes, cancellation);
        }

        /// <summary>
        /// Creates a normalized representation of the given <paramref name="path"/>. The default
        /// implementation doesn't support files with '\' in the file name and treats the '\' as
        /// a directory separator. The default implementation will convert all the '\' into '/'.
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        /// <returns>The normalized path.</returns>
        // internal for testing.
        protected internal string NormalizePath(string path)
        {
            return path.Replace('\\', '/');
        }

        private Task CopyPhysicalFileToResponseAsync(
            HttpResponse response,
            string physicalFilePath,
            CancellationToken cancellationToken)
        {
            var sendFile = response.HttpContext.GetFeature<IHttpSendFileFeature>();
            if (sendFile != null)
            {
                return sendFile.SendFileAsync(
                    physicalFilePath,
                    offset: 0,
                    length: null,
                    cancellation: cancellationToken);
            }
            else
            {
                var fileStream = new FileStream(
                    physicalFilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite,
                    DefaultBufferSize,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);

                return CopyStreamToResponseAsync(fileStream, response, cancellationToken);
            }
        }

        private static async Task CopyStreamToResponseAsync(
            Stream sourceStream,
            HttpResponse response,
            CancellationToken cancellation)
        {
            using (sourceStream)
            {
                await sourceStream.CopyToAsync(response.Body, DefaultBufferSize, cancellation);
            }
        }
    }
}
