// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Xml;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Mvc.Formatters.Xml.Internal
{
    /// <summary>
    /// Executes a <see cref="XmlResult"/> to write to the response.
    /// </summary>
    public class XmlResultExecutor : IXmlResultExecutor
    {
        private static readonly string DefaultContentType = new MediaTypeHeaderValue("application/xml")
        {
            Encoding = Encoding.UTF8
        }.ToString();

        /// <summary>
        /// Creates a new <see cref="XmlResultExecutor"/>.
        /// </summary>
        /// <param name="writerFactory">The <see cref="IHttpResponseStreamWriterFactory"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public XmlResultExecutor(
            IHttpResponseStreamWriterFactory writerFactory,
            ILoggerFactory loggerFactory)
        {
            if (writerFactory == null)
            {
                throw new ArgumentNullException(nameof(writerFactory));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            WriterFactory = writerFactory;
            Logger = loggerFactory.CreateLogger<XmlResult>();
        }

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the <see cref="IHttpResponseStreamWriterFactory"/>.
        /// </summary>
        protected IHttpResponseStreamWriterFactory WriterFactory { get; }

        /// <summary>
        /// Executes the <see cref="XmlResult"/> and writes the response.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <param name="result">The <see cref="XmlResult"/>.</param>
        /// <returns>A <see cref="Task"/> which will complete when writing has completed.</returns>
        public Task ExecuteAsync(ActionContext context, XmlResult result)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var response = context.HttpContext.Response;

            string resolvedContentType = null;
            Encoding resolvedContentTypeEncoding = null;
            ResponseContentTypeHelper.ResolveContentTypeAndEncoding(
                                                    result.ContentType,
                                                    response.ContentType,
                                                    DefaultContentType,
                                                    out resolvedContentType,
                                                    out resolvedContentTypeEncoding);

            response.ContentType = resolvedContentType;

            if (result.StatusCode != null)
            {
                response.StatusCode = result.StatusCode.Value;
            }

            var serializerSettings = result.XmlSerializerSettings ?? FormattingUtilities.GetDefaultXmlWriterSettings();

            TextOutputFormatter formatter;
            // create the proper formatter

            formatter = new XmlSerializerOutputFormatter(serializerSettings);

            var outputFormatterWriterContext = new OutputFormatterWriteContext(
                                                        context.HttpContext,
                                                        WriterFactory.CreateWriter,
                                                        result.Value.GetType(), result.Value);

            outputFormatterWriterContext.ContentType = new StringSegment(resolvedContentType);

            //  Logger formatter and value of object

            Logger.FormatterSelected(formatter, outputFormatterWriterContext);
            Logger.XmlResultExecuting(result.Value);

            return formatter.WriteAsync(outputFormatterWriterContext);
        }


    }
}

