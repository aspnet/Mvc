﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Deserialize input using XmlSerializer.
    /// </summary>
    public class XmlSerializerInputFormatter : IInputFormatter
    {
        private IList<Encoding> _supportedEncodings;
        private IList<string> _supportedMediaTypes;
        private XmlDictionaryReaderQuotas _readerQuotas = FormattingUtilities.GetDefaultXmlReaderQuotas();

        /// <summary>
        /// Initializes a new instance of XmlSerializerInputFormatter.
        /// </summary>
        public XmlSerializerInputFormatter()
        {
            _supportedMediaTypes = new List<string>
            {
                "application/xml",
                "text/xml"
            };

            _supportedEncodings = new List<Encoding>
            {
                Encodings.UTF8EncodingWithoutBOM,
                Encodings.UnicodeEncodingWithBOM
            };
        }

        /// <summary>
        /// Returns the list of supported encodings.
        /// </summary>
        public IList<Encoding> SupportedEncodings
        {
            get { return _supportedEncodings; }
        }

        /// <summary>
        /// Returns the list of supported Media Types.
        /// </summary>
        public IList<string> SupportedMediaTypes
        {
            get { return _supportedMediaTypes; }
        }

        /// <summary>
        /// Indicates the acceptable input XML depth.
        /// </summary>
        public int MaxDepth
        {
            get
            {
                return _readerQuotas.MaxDepth;
            }
            set
            {
                if (value < FormattingUtilities.DefaultMinDepth)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _readerQuotas.MaxDepth = value;
            }
        }

        /// <summary>
        /// Reads the input XML.
        /// </summary>
        /// <param name="context">The input formatter context which contains the body to be read.</param>
        /// <returns>Task which reads the input.</returns>
        public async Task ReadAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            if (request.ContentLength == 0)
            {
                context.Model = GetDefaultValueForType(context.Metadata.ModelType);
                return;
            }

            context.Model = await ReadInternal(context);
        }

        /// <summary>
        /// Called during deserialization to get the <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="readStream">The <see cref="Stream"/> from which to read.</param>
        /// <returns>The <see cref="XmlReader"/> used during deserialization.</returns>
        public virtual XmlReader CreateXmlReader([NotNull] Stream readStream)
        {
            return XmlDictionaryReader.CreateTextReader(
                readStream, _readerQuotas);
        }

        /// <summary>
        /// Called during deserialization to get the <see cref="XmlSerializer"/>.
        /// </summary>
        /// <returns>The <see cref="XmlSerializer"/> used during serialization and deserialization.</returns>
        public virtual XmlSerializer CreateXmlSerializer(Type type)
        {
            return new XmlSerializer(type);
        }

        private object GetDefaultValueForType(Type modelType)
        {
            return modelType.GetTypeInfo().IsValueType ? Activator.CreateInstance(modelType) :
                                                                      null;
        }

        private Task<object> ReadInternal(InputFormatterContext context)
        {
            var type = context.Metadata.ModelType;
            var request = context.HttpContext.Request;

            using (var xmlReader = CreateXmlReader(new DelegatingStream(request.Body)))
            {
                var xmlSerializer = CreateXmlSerializer(type);
                return Task.FromResult(xmlSerializer.Deserialize(xmlReader));
            }
        }
    }
}