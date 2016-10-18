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

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// An action result which formats the given object as XML.
    /// </summary>
    public class XmlResult : ActionResult
    {
        /// <summary>
        /// Creates a new <see cref="XmlResult"/> with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to format as xml.</param>
        public XmlResult(object value)
        {
            Value = value;
        }

        /// <summary>
        /// Creates a new <see cref="XmlResult"/> with the given <paramref name="value"/>.
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

        //TODO: have a look at an idea of using
        public FormatterCollection<IOutputFormatter> Formatters { get; set; }

        /// <summary>
        /// Gets or sets the type of used xml serializer.
        /// </summary>
        public bool UseDataContractSerializer { get; set; }

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
            if (UseDataContractSerializer)
            {
                var dcExecutor = services.GetRequiredService<XmlDcResultExecutor>();
                if (dcExecutor == null)
                {
                    throw new ArgumentNullException(nameof(XmlDcResultExecutor));
                }
                return dcExecutor.ExecuteAsync(context, this);
            }
            var executor = services.GetRequiredService<XmlResultExecutor>();
            if (executor == null)
            {
                throw new ArgumentNullException(nameof(XmlResultExecutor));
            }
            return executor.ExecuteAsync(context, this);
        }

    }
}
