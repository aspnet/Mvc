// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Internal;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// An <see cref="ActionResult"/> that when executed will write a virtual file (eg. embedded resource)
    /// to the response using mechanisms provided by the host.
    /// </summary>
    public class VirtualFilePathResult : FileResult
    {
        private const int DefaultBufferSize = 0x1000;
        private string _fileName;

        /// <summary>
        /// Creates a new <see cref="VirtualFilePathResult"/> instance with the provided <paramref name="fileName"/>
        /// and the provided <paramref name="contentType"/>.
        /// </summary>
        /// <param name="fileName">The path to the file. The path must be relative/virtual.</param>
        /// <param name="contentType">The Content-Type header of the response.</param>
        public VirtualFilePathResult([NotNull] string fileName, [NotNull] string contentType)
            : this(fileName, new MediaTypeHeaderValue(contentType))
        {
        }

        /// <summary>
        /// Creates a new <see cref="VirtualFilePathResult"/> instance with
        /// the provided <paramref name="fileName"/> and the
        /// provided <paramref name="contentType"/>.
        /// </summary>
        /// <param name="fileName">The path to the file. The path must be relative/virtual.</param>
        /// <param name="contentType">The Content-Type header of the response.</param>
        public VirtualFilePathResult([NotNull] string fileName, [NotNull] MediaTypeHeaderValue contentType)
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

        /// <summary>
        /// Gets or sets the <see cref="IFileProvider"/> used to resolve paths.
        /// </summary>
        public IFileProvider FileProvider { get; set; }

        /// <inheritdoc />
        protected override Task WriteFileAsync(HttpResponse response, CancellationToken cancellation)
        {
            var fileProvider = GetFileProvider(response.HttpContext.RequestServices);
            var normalizedPath = NormalizePath(FileName);
            var fileInfo = fileProvider.GetFileInfo(normalizedPath);

            if (fileInfo.Exists)
            {
                var sourceStream = fileInfo.CreateReadStream();
                return CopyStreamToResponseAsync(sourceStream, response, cancellation);
            }

            throw new FileNotFoundException(
                Resources.FormatFileResult_InvalidPath(FileName), FileName);
        }

        /// <summary>
        /// Creates a normalized representation of the given <paramref name="path"/>. The default
        /// implementation doesn't support files with '\' in the file name and treats the '\' as
        /// a directory separator. The default implementation will convert all the '\' into '/'
        /// and will remove leading '~' characters.
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        /// <returns>The normalized path.</returns>
        // internal for testing.
        protected internal string NormalizePath(string path)
        {
            // This currently does not go to the approot. We leave it to the file provider to handle it.
            if (path.StartsWith("~"))
            {
                return path.Substring(1).Replace('\\', '/');
            }

            return path.Replace('\\', '/');
        }

        private IFileProvider GetFileProvider(IServiceProvider requestServices)
        {
            if (FileProvider != null)
            {
                return FileProvider;
            }

            var hostingEnvironment = requestServices.GetService<IHostingEnvironment>();
            FileProvider = hostingEnvironment.WebRootFileProvider;

            return FileProvider;
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
