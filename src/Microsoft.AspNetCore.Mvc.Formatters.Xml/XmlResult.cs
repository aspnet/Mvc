// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using Microsoft.AspNetCore.Mvc.Formatters.Xml.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// An action result which formats the given object as XML.
    /// </summary>
    public class XmlResult : ActionResult
    {
        /// <summary>
        /// Creates a new <see cref="XmlResult"/> with the given <paramref name="value"/>.
        /// Requires the XML DataContractSerializer formatters or/and the XML Serializer formatters to be add to MVC.
        /// </summary>
        /// <param name="value">The value to format as xml.</param>
        public XmlResult(object value)
        {
            Value = value;
        }

        /// <summary>
        /// Creates a new <see cref="XmlResult"/> with the given <paramref name="value"/>.
        /// Requires the XML DataContractSerializer formatters or/and the XML Serializer formatters to be add to MVC.
        /// </summary>
        /// <param name="value">The value to format as XML.</param>
        /// <param name="serializerSettings">The <see cref="XmlWriterSettings"/> to be used by
        /// the formatter.</param>
        public XmlResult(object value, XmlWriterSettings serializerSettings)
        {
            if (serializerSettings == null)
            {
                throw new ArgumentNullException(nameof(serializerSettings));
            }

            Value = value;
            XmlSerializerSettings = serializerSettings;
        }

        /// <summary>
        /// Gets or sets the type of used xml serializer.
        /// </summary>
        public XmlSerializerType XmlSerializerType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Net.Http.Headers.MediaTypeHeaderValue"/> representing the Content-Type header of the response.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="XmlWriterSettings"/>.
        /// </summary>
        public XmlWriterSettings XmlSerializerSettings { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the value to be formatted.
        /// </summary>
        public object Value { get; set; }

        /// <inheritdoc />
        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            var services = context.HttpContext.RequestServices;
            IXmlResultExecutor executor = null;
            string serviceName = string.Empty;

            switch (XmlSerializerType)
            {
                case XmlSerializerType.XmlSeriralizer:
                    executor = services.GetService<XmlResultExecutor>();
                    serviceName = "XmlSerializerFormatterServices";
                    break;
                case XmlSerializerType.DataContractSerializer:
                    executor = services.GetService<XmlDcResultExecutor>();
                    serviceName = "XmlDataContractSerializerFormatterServices";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(XmlSerializerType));
            }
            if (executor == null)
            {
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<XmlResult>();
                // No formatter supports this.
                logger.NoExecutor(XmlSerializerType.ToString());
                context.HttpContext.Response.StatusCode = StatusCodes.Status406NotAcceptable;
#if DEBUG
                var msg = Resources.XmlFromater_WasNotSetup_To_Mvc(serviceName);
                context.HttpContext.Response.ContentType = "text/html";
                MvcXmlLoggerExtensions.StringToHttpContext(context.HttpContext, msg);
#endif
                return Task.FromResult(0);
            }

            return executor.ExecuteAsync(context, this);
        }

    }
}
