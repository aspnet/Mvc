// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Mvc.Formatters
{
    /// <summary>
    /// Writes an object to the output stream.
    /// </summary>
    public abstract class OutputFormatter : IOutputFormatter, IApiResponseFormatMetadataProvider
    {
        /// <summary>
        /// Gets the mutable collection of media type elements supported by
        /// this <see cref="OutputFormatter"/>.
        /// </summary>
        public MediaTypeCollection SupportedMediaTypes { get; } = new MediaTypeCollection();

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
        public virtual IReadOnlyList<string> GetSupportedContentTypes(
            string contentType,
            Type objectType)
        {
            if (SupportedMediaTypes.Count == 0)
            {
                var message = Resources.FormatFormatter_NoMediaTypes(
                    GetType().FullName,
                    nameof(SupportedMediaTypes));

                throw new InvalidOperationException(message);
            }

            if (!CanWriteType(objectType))
            {
                return null;
            }

            if (contentType == null)
            {
                // If contentType is null, then any type we support is valid.
                return SupportedMediaTypes;
            }
            else
            {
                List<string> mediaTypes = null;

                var parsedContentType = new MediaType(contentType);

                // Confirm this formatter supports a more specific media type than requested e.g. OK if "text/*"
                // requested and formatter supports "text/plain". Treat contentType like it came from an Accept header.
                foreach (var mediaType in SupportedMediaTypes)
                {
                    var parsedMediaType = new MediaType(mediaType);
                    if (parsedMediaType.IsSubsetOf(parsedContentType))
                    {
                        if (mediaTypes == null)
                        {
                            mediaTypes = new List<string>();
                        }

                        mediaTypes.Add(mediaType);
                    }
                }

                return mediaTypes;
            }
        }

        /// <inheritdoc />
        public virtual bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (SupportedMediaTypes.Count == 0)
            {
                var message = Resources.FormatFormatter_NoMediaTypes(
                    GetType().FullName,
                    nameof(SupportedMediaTypes));

                throw new InvalidOperationException(message);
            }

            if (!CanWriteType(context.ObjectType))
            {
                return false;
            }

            if (!context.ContentType.HasValue)
            {
                // If the desired content type is set to null, then the current formatter can write anything
                // it wants.
                context.ContentType = new StringSegment(SupportedMediaTypes[0]);
                return true;
            }
            else
            {
                // Confirm this formatter supports a more specific media type than requested e.g. OK if "text/*"
                // requested and formatter supports "text/plain". contentType is typically what we got in an Accept
                // header.
                var parsedContentType = new MediaType(context.ContentType);
                for (var i = 0; i < SupportedMediaTypes.Count; i++)
                {
                    var supportedMediaType = new MediaType(SupportedMediaTypes[i]);
                    if (supportedMediaType.IsSubsetOf(parsedContentType))
                    {
                        context.ContentType = new StringSegment(SupportedMediaTypes[i]);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <inheritdoc />
        public virtual Task WriteAsync(OutputFormatterWriteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            WriteResponseHeaders(context);
            return WriteResponseBodyAsync(context);
        }

        /// <summary>
        /// Sets the headers on <see cref="Microsoft.AspNetCore.Http.HttpResponse"/> object.
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
    }
}
