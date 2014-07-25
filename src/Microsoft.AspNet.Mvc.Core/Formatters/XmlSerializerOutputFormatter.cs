// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// This class handles serialization of objects
    /// to XML using <see cref="XmlSerializer"/>
    /// </summary>
    public class XmlSerializerOutputFormatter : OutputFormatter
    {
        /// <summary>
        /// Initializes a new instance of <see cref="XmlSerializerOutputFormatter"/>
        /// </summary>
        public XmlSerializerOutputFormatter(XmlWriterSettings writerSettings, bool indent)
        {
            SupportedEncodings.Add(Encodings.UTF8EncodingWithoutBOM);
            SupportedEncodings.Add(Encodings.UTF16EncodingLittleEndian);

            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/xml"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/xml"));

            WriterSettings = writerSettings;
            Indent = indent;
        }

        /// <summary>
        /// Gets or sets the settings to be used by the XmlWriter.
        /// </summary>
        public XmlWriterSettings WriterSettings { get; private set; }

        /// <summary>
        /// Gets the default XmlWriterSettings.
        /// </summary>
        /// <returns>Default <see cref="XmlWriterSettings"/></returns>
        public static XmlWriterSettings GetDefaultXmlWriterSettings()
        {
            return new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                CloseOutput = false,
                CheckCharacters = false
            };
        }

        /// <summary>
        /// Gets or sets a value indicating whether to indent elements when writing data. 
        /// </summary>
        public bool Indent
        {
            get
            {
                return WriterSettings.Indent;
            }
            set
            {
                WriterSettings.Indent = value;
            }
        }

        /// <inheritdoc />
        public override bool CanWriteResult(OutputFormatterContext context, MediaTypeHeaderValue contentType)
        {
            return SupportedMediaTypes.Any(supportedMediaType =>
                                            contentType.RawValue.Equals(supportedMediaType.RawValue,
                                                                        StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc />
        public override Task WriteAsync(OutputFormatterContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = context.HttpContext.Response;

            WriterSettings.Encoding = SelectCharacterEncoding(MediaTypeHeaderValue.Parse(response.ContentType));
            using (var xmlWriter = CreateXmlWriter(response.Body))
            {
                var xmlSerializer = CreateXmlSerializer(context.DeclaredType);
                xmlSerializer.Serialize(xmlWriter, context.ObjectResult.Value);
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// Creates a new instance of <see cref="XmlWriter"/> using the given stream and the WriterSettings.
        /// </summary>
        /// <param name="writeStream">The stream on which the XmlWriter should operate on.</param>
        /// <returns>A new instance of <see cref="XmlWriter"/></returns>
        public virtual XmlWriter CreateXmlWriter([NotNull]Stream writeStream)
        {
            return XmlWriter.Create(writeStream, WriterSettings);
        }

        /// <summary>
        /// Create a new instance of <see cref="XmlSerializer"/> for the given object type.
        /// </summary>
        /// <param name="type">The type of object for which the serializer should be created.</param>
        /// <returns>A new instance of <see cref="XmlSerializer"/></returns>
        public virtual XmlSerializer CreateXmlSerializer([NotNull]Type type)
        {
            return new XmlSerializer(type);
        }
    }
}