﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc
{
    public class ObjectResult : ActionResult
    {
        public object Value { get; set; }

        public IList<IOutputFormatter> Formatters { get; set; }

        public IList<MediaTypeHeaderValue> ContentTypes { get; set; }

        public Type DeclaredType { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public int? StatusCode { get; set; }

        public ObjectResult(object value)
        {
            Value = value;
            Formatters = new List<IOutputFormatter>();
            ContentTypes = new List<MediaTypeHeaderValue>();
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            var formatters = GetDefaultFormatters(context);
            var formatterContext = new OutputFormatterContext()
            {
                DeclaredType = DeclaredType,
                ActionContext = context,
                Object = Value,
            };

            var selectedFormatter = SelectFormatter(formatterContext, formatters);
            if (selectedFormatter == null)
            {
                // No formatter supports this.
                context.HttpContext.Response.StatusCode = 406;
                return;
            }

            if (StatusCode != null)
            {
                context.HttpContext.Response.StatusCode = (int)StatusCode;
            }

            OnFormatting(context);
            await selectedFormatter.WriteAsync(formatterContext);
        }

        public virtual IOutputFormatter SelectFormatter(OutputFormatterContext formatterContext,
                                                       IEnumerable<IOutputFormatter> formatters)
        {
            var incomingAcceptHeaderMediaTypes = formatterContext.ActionContext.HttpContext.Request.GetTypedHeaders().Accept ??
                                                    new MediaTypeHeaderValue[] { };

            // By default we want to ignore considering accept headers for content negotiation when
            // they have a media type like */* in them. Browsers typically have these media types.
            // In these cases we would want the first formatter in the list of output formatters to
            // write the response. This default behavior can be changed through options, so checking here.
            var options = formatterContext.ActionContext.HttpContext
                                                        .RequestServices
                                                        .GetRequiredService<IOptions<MvcOptions>>()
                                                        .Options;

            var respectAcceptHeader = true;
            if (options.RespectBrowserAcceptHeader == false
                && HasWildCardMediaAndSubMediaType(incomingAcceptHeaderMediaTypes))
            {
                respectAcceptHeader = false;
            }

            IEnumerable<MediaTypeHeaderValue> sortedAcceptHeaderMediaTypes = null;
            if (respectAcceptHeader)
            {
                sortedAcceptHeaderMediaTypes = SortMediaTypeHeaderValues(incomingAcceptHeaderMediaTypes)
                                                .Where(header => header.Quality != HeaderQuality.NoMatch);
            }

            IOutputFormatter selectedFormatter = null;
            if (ContentTypes == null || ContentTypes.Count == 0)
            {
                if (respectAcceptHeader)
                {
                    // Select based on sorted accept headers.
                    selectedFormatter = SelectFormatterUsingSortedAcceptHeaders(
                                                                            formatterContext,
                                                                            formatters,
                                                                            sortedAcceptHeaderMediaTypes);
                }

                if (selectedFormatter == null)
                {
                    var requestContentType = formatterContext.ActionContext.HttpContext.Request.ContentType;

                    // No formatter found based on accept headers, fall back on request contentType.
                    MediaTypeHeaderValue incomingContentType = null;
                    MediaTypeHeaderValue.TryParse(requestContentType, out incomingContentType);

                    // In case the incomingContentType is null (as can be the case with get requests),
                    // we need to pick the first formatter which
                    // can support writing this type.
                    var contentTypes = new[] { incomingContentType };
                    selectedFormatter = SelectFormatterUsingAnyAcceptableContentType(
                                                                                formatterContext,
                                                                                formatters,
                                                                                contentTypes);

                    // This would be the case when no formatter could write the type base on the
                    // accept headers and the request content type. Fallback on type based match.
                    if (selectedFormatter == null)
                    {
                        foreach (var formatter in formatters)
                        {
                            var supportedContentTypes = formatter.GetSupportedContentTypes(
                                                                         formatterContext.DeclaredType,
                                                                         formatterContext.Object?.GetType(),
                                                                         contentType: null);

                            if (formatter.CanWriteResult(formatterContext, supportedContentTypes?.FirstOrDefault()))
                            {
                                return formatter;
                            }
                        }
                    }
                }
            }
            else if (ContentTypes.Count == 1)
            {
                // There is only one value that can be supported.
                selectedFormatter = SelectFormatterUsingAnyAcceptableContentType(
                                                                            formatterContext,
                                                                            formatters,
                                                                            ContentTypes);
            }
            else
            {
                if (respectAcceptHeader)
                {
                    // Filter and remove accept headers which cannot support any of the user specified content types.
                    var filteredAndSortedAcceptHeaders = sortedAcceptHeaderMediaTypes
                                                                    .Where(acceptHeader =>
                                                                            ContentTypes
                                                                                .Any(contentType =>
                                                                                       contentType.IsSubsetOf(acceptHeader)));

                    selectedFormatter = SelectFormatterUsingSortedAcceptHeaders(
                                                                        formatterContext,
                                                                        formatters,
                                                                        filteredAndSortedAcceptHeaders);
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
                                                                        ContentTypes);
                }
            }

            return selectedFormatter;
        }

        public virtual IOutputFormatter SelectFormatterUsingSortedAcceptHeaders(
                                                            OutputFormatterContext formatterContext,
                                                            IEnumerable<IOutputFormatter> formatters,
                                                            IEnumerable<MediaTypeHeaderValue> sortedAcceptHeaders)
        {
            IOutputFormatter selectedFormatter = null;
            foreach (var contentType in sortedAcceptHeaders)
            {
                // Loop through each of the formatters and see if any one will support this
                // mediaType Value.
                selectedFormatter = formatters.FirstOrDefault(
                                                    formatter =>
                                                        formatter.CanWriteResult(formatterContext, contentType));
                if (selectedFormatter != null)
                {
                    // we found our match.
                    break;
                }
            }

            return selectedFormatter;
        }

        public virtual IOutputFormatter SelectFormatterUsingAnyAcceptableContentType(
                                                            OutputFormatterContext formatterContext,
                                                            IEnumerable<IOutputFormatter> formatters,
                                                            IEnumerable<MediaTypeHeaderValue> acceptableContentTypes)
        {
            var selectedFormatter = formatters.FirstOrDefault(
                                            formatter =>
                                                    acceptableContentTypes
                                                    .Any(contentType =>
                                                            formatter.CanWriteResult(formatterContext, contentType)));
            return selectedFormatter;
        }

        private bool HasWildCardMediaAndSubMediaType(IEnumerable<MediaTypeHeaderValue> mediaTypes)
        {
            return mediaTypes.Any(mediaType => mediaType.MatchesAllTypes);
        }

        private static IEnumerable<MediaTypeHeaderValue> SortMediaTypeHeaderValues(IEnumerable<MediaTypeHeaderValue> headerValues)
        {
            // Use OrderBy() instead of Array.Sort() as it performs fewer comparisons. In this case the comparisons
            // are quite expensive so OrderBy() performs better.
            return headerValues.OrderByDescending(headerValue =>
                                                    headerValue,
                                                    MediaTypeHeaderValueComparer.QualityComparer);
        }

        private IEnumerable<IOutputFormatter> GetDefaultFormatters(ActionContext context)
        {
            IEnumerable<IOutputFormatter> formatters = null;
            if (Formatters == null || Formatters.Count == 0)
            {
                formatters = context.HttpContext
                                    .RequestServices
                                    .GetRequiredService<IOutputFormattersProvider>()
                                    .OutputFormatters;
            }
            else
            {
                formatters = Formatters;
            }

            return formatters;
        }

        /// <summary>
        /// This method is called before the formatter writes to the output stream.
        /// </summary>
        protected virtual void OnFormatting([NotNull] ActionContext context)
        {
        }
    }
}