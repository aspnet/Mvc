// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Features;
using Microsoft.Framework.Internal;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc.Formatters
{
    /// <summary>
    /// Always copies the stream to the response, regardless of requested content type.
    /// </summary>
    public class StreamOutputFormatter : IOutputFormatter
    {
        /// <inheritdoc />
        public bool CanWriteResult([NotNull] OutputFormatterContext context, MediaTypeHeaderValue contentType)
        {
            // Ignore the passed in content type, if the object is a Stream.
            if (context.Object is Stream)
            {
                context.SelectedContentType = contentType;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public async Task WriteAsync([NotNull] OutputFormatterContext context)
        {
            using (var valueAsStream = ((Stream)context.Object))
            {
                var response = context.HttpContext.Response;

                if (context.SelectedContentType != null)
                {
                    response.ContentType = context.SelectedContentType.ToString();
                }

                var bufferingFeature = context.HttpContext.Features.Get<IHttpBufferingFeature>();
                bufferingFeature?.DisableResponseBuffering();

                await valueAsStream.CopyToAsync(response.Body);
            }
        }
    }
}
