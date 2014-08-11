// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// This class handles serialization of objects
    /// to XML using <see cref="DataContractSerializer"/>
    /// </summary>
    public class XmlDataContractSerializerOutputFormatter : OutputFormatter
    {
        /// <summary>
        /// Initializes a new instance of <see cref="XmlDataContractSerializerOutputFormatter"/>
        /// </summary>
        /// <param name="writerSettings">The settings to be used by the DataContractSerializer.</param>
        public XmlDataContractSerializerOutputFormatter([NotNull] XmlWriterSettings writerSettings)
        {
            SupportedEncodings.Add(Encodings.UTF8EncodingWithoutBOM);
            SupportedEncodings.Add(Encodings.UTF16EncodingLittleEndian);

            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/xml"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/xml"));

            WriterSettings = writerSettings;
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
        /// Creates a new instance of <see cref="XmlWriter"/> using the given stream and the WriterSettings.
        /// </summary>
        /// <param name="writeStream">The stream on which the XmlWriter should operate on.</param>
        /// <returns>A new instance of <see cref="XmlWriter"/></returns>
        public virtual XmlWriter CreateXmlWriter([NotNull] Stream writeStream)
        {
            return XmlWriter.Create(writeStream, WriterSettings);
        }

        /// <summary>
        /// Create a new instance of <see cref="DataContractSerializer"/> for the given object type.
        /// </summary>
        /// <param name="type">The type of object for which the serializer should be created.</param>
        /// <returns>A new instance of <see cref="DataContractSerializer"/></returns>
        public virtual DataContractSerializer CreateDataContractSerializer([NotNull]Type type)
        {
            return new DataContractSerializer(type);
        }

        public override Task WriteResponseBodyAsync([NotNull] OutputFormatterContext context)
        {
            var response = context.ActionContext.HttpContext.Response;

            WriterSettings.Encoding = SelectCharacterEncoding(context);
            using (var xmlWriter = CreateXmlWriter(response.Body))
            {
                var xmlSerializer = CreateDataContractSerializer(context.DeclaredType);
                xmlSerializer.WriteObject(xmlWriter, context.Object);
            }

            return Task.FromResult(true);
        }
    }
}