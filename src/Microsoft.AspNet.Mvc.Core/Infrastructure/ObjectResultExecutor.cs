// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.Formatters;
using Microsoft.AspNet.Mvc.Internal;
using Microsoft.AspNet.Mvc.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc.Infrastructure
{
    /// <summary>
    /// Executes an <see cref="ObjectResult"/> to write to the response.
    /// </summary>
    public class ObjectResultExecutor
    {
        /// <summary>
        /// Creates a new <see cref="ObjectResultExecutor"/>.
        /// </summary>
        /// <param name="options">An accessor to <see cref="MvcOptions"/>.</param>
        /// <param name="writerFactory">The <see cref="IHttpResponseStreamWriterFactory"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public ObjectResultExecutor(
            IOptions<MvcOptions> options,
            IHttpResponseStreamWriterFactory writerFactory,
            ILoggerFactory loggerFactory)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            OptionsFormatters = options.Value.OutputFormatters;
            RespectBrowserAcceptHeader = options.Value.RespectBrowserAcceptHeader;
            Logger = loggerFactory.CreateLogger<ObjectResultExecutor>();
            WriterFactory = writerFactory.CreateWriter;
        }

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the list of <see cref="IOutputFormatter"/> instances from <see cref="MvcOptions"/>.
        /// </summary>
        protected FormatterCollection<IOutputFormatter> OptionsFormatters { get; }

        /// <summary>
        /// Gets the value of <see cref="MvcOptions.RespectBrowserAcceptHeader"/>.
        /// </summary>
        protected bool RespectBrowserAcceptHeader { get; }

        /// <summary>
        /// Gets the writer factory delegate.
        /// </summary>
        protected Func<Stream, Encoding, TextWriter> WriterFactory { get; }

        /// <summary>
        /// Executes the <see cref="ObjectResult"/>.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/> for the current request.</param>
        /// <param name="result">The <see cref="ObjectResult"/>.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete once the <see cref="ObjectResult"/> is written to the response.
        /// </returns>
        public virtual Task ExecuteAsync(ActionContext context, ObjectResult result)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            // If the user sets the content type both on the ObjectResult (example: by Produces) and Response object,
            // then the one set on ObjectResult takes precedence over the Response object
            if (result.ContentTypes == null || result.ContentTypes.Count == 0)
            {
                var responseContentType = context.HttpContext.Response.ContentType;
                if (!string.IsNullOrEmpty(responseContentType))
                {
                    if (result.ContentTypes == null)
                    {
                        result.ContentTypes = new MediaTypeCollection();
                    }

                    result.ContentTypes.Add(new StringSegment(responseContentType));
                }
            }

            ValidateContentTypes(result.ContentTypes);

            var formatters = result.Formatters;
            if (formatters == null || formatters.Count == 0)
            {
                formatters = OptionsFormatters;
            }

            var objectType = result.DeclaredType;
            if (objectType == null || objectType == typeof(object))
            {
                objectType = result.Value?.GetType();
            }

            var formatterContext = new OutputFormatterWriteContext(
                context.HttpContext,
                WriterFactory,
                objectType,
                result.Value);

            var selectedFormatter = SelectFormatter(formatterContext, result.ContentTypes, formatters);
            if (selectedFormatter == null)
            {
                // No formatter supports this.
                Logger.NoFormatter(formatterContext);

                context.HttpContext.Response.StatusCode = StatusCodes.Status406NotAcceptable;
                return TaskCache.CompletedTask;
            }

            Logger.FormatterSelected(selectedFormatter, formatterContext);
            Logger.ObjectResultExecuting(context);

            result.OnFormatting(context);
            return selectedFormatter.WriteAsync(formatterContext);
        }

        /// <summary>
        /// Selects the <see cref="IOutputFormatter"/> to write the response.
        /// </summary>
        /// <param name="formatterContext">The <see cref="OutputFormatterWriteContext"/>.</param>
        /// <param name="contentTypes">
        /// The list of content types provided by <see cref="ObjectResult.ContentTypes"/>.
        /// </param>
        /// <param name="formatters">
        /// The list of <see cref="IOutputFormatter"/> instances to consider.
        /// </param>
        /// <returns>
        /// The selected <see cref="IOutputFormatter"/> or <c>null</c> if no formatter can write the response.
        /// </returns>
        protected virtual IOutputFormatter SelectFormatter(
            OutputFormatterWriteContext formatterContext,
            IList<StringSegment> contentTypes,
            IList<IOutputFormatter> formatters)
        {
            if (formatterContext == null)
            {
                throw new ArgumentNullException(nameof(formatterContext));
            }

            if (contentTypes == null)
            {
                throw new ArgumentNullException(nameof(contentTypes));
            }

            if (formatters == null)
            {
                throw new ArgumentNullException(nameof(formatters));
            }

            // Check if any content-type was explicitly set (for example, via ProducesAttribute
            // or URL path extension mapping). If yes, then ignore content-negotiation and use this content-type.
            if (contentTypes.Count == 1)
            {
                Logger.SkippedContentNegotiation(contentTypes[0]);

                return SelectFormatterUsingAnyAcceptableContentType(formatterContext, formatters, contentTypes);
            }

            var request = formatterContext.HttpContext.Request;

            var mediaTypes = GetMediaTypes(contentTypes, request);
            IOutputFormatter selectedFormatter = null;
            if (contentTypes.Count == 0)
            {
                // Check if we have enough information to do content-negotiation, otherwise get the first formatter
                // which can write the type. Let the formatter choose the Content-Type.
                if (!(mediaTypes.Count() > 0))
                {
                    Logger.NoAcceptForNegotiation();

                    return SelectFormatterNotUsingAcceptHeaders(formatterContext, formatters);
                }

                //
                // Content-Negotiation starts from this point on.
                //

                // 1. Select based on sorted accept headers.
                selectedFormatter = SelectFormatterUsingAcceptHeader(
                    formatterContext,
                    formatters,
                    mediaTypes);

                // 2. No formatter was found based on Accept header. Fallback to the first formatter which can write
                // the type. Let the formatter choose the Content-Type.
                if (selectedFormatter == null)
                {
                    Logger.NoFormatterFromNegotiation(mediaTypes);

                    // Set this flag to indicate that content-negotiation has failed to let formatters decide
                    // if they want to write the response or not.
                    formatterContext.FailedContentNegotiation = true;

                    return SelectFormatterNotUsingAcceptHeaders(formatterContext, formatters);
                }
            }
            else
            {
                if (mediaTypes.Count() > 0)
                {
                    selectedFormatter = SelectFormatterUsingAcceptHeader(
                        formatterContext,
                        formatters,
                        mediaTypes);
                }

                if (selectedFormatter == null)
                {
                    // Either there were no acceptHeaders that were present OR
                    // There were no accept headers which matched OR
                    // There were acceptHeaders which matched but there was no formatter
                    // which supported any of them.
                    // In any of these cases, if the user has specified content types,
                    // do a last effort to find a formatter which can write any of the user specified content type.
                    selectedFormatter = SelectFormatterUsingAnyAcceptableContentType(
                        formatterContext,
                        formatters,
                        contentTypes);
                }
            }

            return selectedFormatter;
        }

        private IList<MediaTypeSegmentWithQuality> GetMediaTypes(
            IList<StringSegment> contentTypes,
            HttpRequest request)
        {
            // Will fix the list with object pooling
            var result = new List<MediaTypeSegmentWithQuality>();
            AcceptHeaderParser.ParseAcceptHeader(request.Headers[HeaderNames.Accept], result);
            for (int i = 0; i < result.Count; i++)
            {
                if (!RespectBrowserAcceptHeader &&
                    IsAcceptAllMediaTypesMediaType(result[i]))
                {
                    result.Clear();
                    return result;
                }

                RemoveMediaTypeFromResultsIfNotAcceptable(result, i, contentTypes);
            }

            SortMediaTypesByQuality(result);

            return result;
        }

        private static bool IsAcceptAllMediaTypesMediaType(MediaTypeSegmentWithQuality mediaTypeWithQuality)
        {
            return mediaTypeWithQuality.MediaType.StartsWith("*/*", StringComparison.OrdinalIgnoreCase);
        }

        private static void RemoveMediaTypeFromResultsIfNotAcceptable(
            IList<MediaTypeSegmentWithQuality> mediaTypes,
            int index,
            IList<StringSegment> acceptableMediaTypes)
        {
            if (!InAcceptableMediaTypes(mediaTypes[index].MediaType, acceptableMediaTypes))
            {
                mediaTypes.RemoveAt(index);
            }
        }

        private static void SortMediaTypesByQuality(List<MediaTypeSegmentWithQuality> result)
        {
            result.Sort((left, right) => left.Quality > right.Quality ? 1 : (left.Quality == right.Quality ? 0 : -1));
        }

        private static bool InAcceptableMediaTypes(StringSegment mediaType, IList<StringSegment> acceptableMediaTypes)
        {
            if (acceptableMediaTypes.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < acceptableMediaTypes.Count; i++)
            {
                if (MediaTypeComparisons.IsSubsetOf(mediaType, acceptableMediaTypes[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Selects the <see cref="IOutputFormatter"/> to write the response. The first formatter which
        /// can write the response should be chosen without any consideration for content type.
        /// </summary>
        /// <param name="formatterContext">The <see cref="OutputFormatterWriteContext"/>.</param>
        /// <param name="formatters">
        /// The list of <see cref="IOutputFormatter"/> instances to consider.
        /// </param>
        /// <returns>
        /// The selected <see cref="IOutputFormatter"/> or <c>null</c> if no formatter can write the response.
        /// </returns>
        protected virtual IOutputFormatter SelectFormatterNotUsingAcceptHeaders(
            OutputFormatterWriteContext formatterContext,
            IList<IOutputFormatter> formatters)
        {
            if (formatterContext == null)
            {
                throw new ArgumentNullException(nameof(formatterContext));
            }

            if (formatters == null)
            {
                throw new ArgumentNullException(nameof(formatters));
            }

            foreach (var formatter in formatters)
            {
                formatterContext.ContentType = new StringSegment(); // Equivalent to null MediaType
                if (formatter.CanWriteResult(formatterContext))
                {
                    return formatter;
                }
            }

            return null;
        }

        /// <summary>
        /// Selects the <see cref="IOutputFormatter"/> to write the response based on the content type values
        /// present in <paramref name="sortedAcceptHeaders"/>.
        /// </summary>
        /// <param name="formatterContext">The <see cref="OutputFormatterWriteContext"/>.</param>
        /// <param name="formatters">
        /// The list of <see cref="IOutputFormatter"/> instances to consider.
        /// </param>
        /// <param name="sortedAcceptHeaders">
        /// The ordered content types from the <c>Accept</c> header, sorted by descending q-value.
        /// </param>
        /// <returns>
        /// The selected <see cref="IOutputFormatter"/> or <c>null</c> if no formatter can write the response.
        /// </returns>
        protected virtual IOutputFormatter SelectFormatterUsingAcceptHeader(
            OutputFormatterWriteContext formatterContext,
            IList<IOutputFormatter> formatters,
            IList<MediaTypeSegmentWithQuality> acceptHeader)
        {
            if (formatterContext == null)
            {
                throw new ArgumentNullException(nameof(formatterContext));
            }

            if (formatters == null)
            {
                throw new ArgumentNullException(nameof(formatters));
            }

            if (acceptHeader == null)
            {
                throw new ArgumentNullException(nameof(acceptHeader));
            }

            foreach (var mediaTpe in acceptHeader)
            {
                formatterContext.ContentType = mediaTpe.MediaType;
                foreach (var formatter in formatters)
                {
                    if (formatter.CanWriteResult(formatterContext))
                    {
                        return formatter;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Selects the <see cref="IOutputFormatter"/> to write the response based on the content type values
        /// present in <paramref name="acceptableContentTypes"/>.
        /// </summary>
        /// <param name="formatterContext">The <see cref="OutputFormatterWriteContext"/>.</param>
        /// <param name="formatters">
        /// The list of <see cref="IOutputFormatter"/> instances to consider.
        /// </param>
        /// <param name="acceptableContentTypes">
        /// The ordered content types from <see cref="ObjectResult.ContentTypes"/> in descending priority order.
        /// </param>
        /// <returns>
        /// The selected <see cref="IOutputFormatter"/> or <c>null</c> if no formatter can write the response.
        /// </returns>
        protected virtual IOutputFormatter SelectFormatterUsingAnyAcceptableContentType(
            OutputFormatterWriteContext formatterContext,
            IList<IOutputFormatter> formatters,
            IList<StringSegment> acceptableContentTypes)
        {
            if (formatterContext == null)
            {
                throw new ArgumentNullException(nameof(formatterContext));
            }

            if (formatters == null)
            {
                throw new ArgumentNullException(nameof(formatters));
            }

            if (acceptableContentTypes == null)
            {
                throw new ArgumentNullException(nameof(acceptableContentTypes));
            }

            foreach (var formatter in formatters)
            {
                foreach (var contentType in acceptableContentTypes)
                {
                    formatterContext.ContentType = contentType;
                    if (formatter.CanWriteResult(formatterContext))
                    {
                        return formatter;
                    }
                }
            }

            return null;
        }

        // There's no allocation-free way to sort an IList so we're going to have to live with the
        // copy + insertion sort.
        private IList<MediaTypeHeaderValue> PrepareAcceptValues(IList<MediaTypeHeaderValue> values)
        {
            if (values == null || values.Count == 0)
            {
                return null;
            }

            // By default we want to ignore considering accept headers for content negotiation when
            // they have a media type like */* in them. Browsers typically have these media types.
            // In these cases we would want the first formatter in the list of output formatters to
            // write the response. This default behavior can be changed through options, so checking here.
            if (!RespectBrowserAcceptHeader)
            {
                for (var i = 0; i < values.Count; i++)
                {
                    if (values[i].MatchesAllTypes)
                    {
                        return null;
                    }
                }
            }

            // Degenerate case, we can avoid copying anything.
            if (values.Count == 1)
            {
                return values;
            }

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

            var sorted = new List<MediaTypeHeaderValue>(count);
            for (var i = 0; i < values.Count; i++)
            {
                var value = values[i];
                if (value.Quality == HeaderQuality.NoMatch)
                {
                    // Exclude this one
                }
                else
                {
                    var position = sorted.BinarySearch(value, MediaTypeHeaderValueComparer.QualityComparer);
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

        private void ValidateContentTypes(IList<StringSegment> contentTypes)
        {
            if (contentTypes == null)
            {
                return;
            }

            for (var i = 0; i < contentTypes.Count; i++)
            {
                var contentType = contentTypes[i];
                if (MediaTypeComparisons.MatchesAllTypes(contentType) ||
                    MediaTypeComparisons.MatchesAllSubtypes(contentType))
                {
                    var message = Resources.FormatObjectResult_MatchAllContentType(
                        contentType,
                        nameof(ObjectResult.ContentTypes));
                    throw new InvalidOperationException(message);
                }
            }
        }
    }
}
