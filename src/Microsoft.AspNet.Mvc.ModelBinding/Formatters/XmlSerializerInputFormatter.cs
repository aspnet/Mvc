// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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
    public class XmlSerializerInputFormatter : IInputFormatter
    {
        private IList<Encoding> _supportedEncodings;
        private IList<string> _supportedMediaTypes;
        private XmlDictionaryReaderQuotas _readerQuotas = FormattingUtilities.GetDefaultReaderQuotas();

        public XmlSerializerInputFormatter()
        {
            _supportedMediaTypes = new List<string>
            {
                "application/xml",
                "text/xml"
            };

            _supportedEncodings = new List<Encoding>
            {
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
                new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true)
            };
        }

        public IList<Encoding> SupportedEncodings
        {
            get { return _supportedEncodings; }
        }

        public IList<string> SupportedMediaTypes
        {
            get { return _supportedMediaTypes; }
        }

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

        public async Task ReadAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            if (request.ContentLength == 0)
            {
                var modelType = context.Metadata.ModelType;
                context.Model = modelType.GetTypeInfo().IsValueType ? Activator.CreateInstance(modelType) :
                                                                      null;
                return;
            }

            var effectiveEncoding = FormattingUtilities.SelectCharacterEncoding(SupportedEncodings,
                request.GetContentType(), typeof(XmlSerializerInputFormatter));
            context.Model = await ReadInternal(context, effectiveEncoding);
        }

        /// <summary>
        /// Called during deserialization to get the <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="readStream">The <see cref="Stream"/> from which to read.</param>
        /// <param name="effectiveEncoding">The <see cref="Encoding"/> to use when reading.</param>
        /// <returns>The <see cref="XmlReader"/> used during deserialization.</returns>
        public virtual XmlReader CreateXmlReader([NotNull] Stream readStream,
                                                 [NotNull] Encoding effectiveEncoding)
        {
            return XmlDictionaryReader.CreateTextReader(
                new DelegatingStream(readStream), effectiveEncoding, _readerQuotas, null);
        }

        /// <summary>
        /// Called during deserialization to get the <see cref="XmlSerializer"/>.
        /// </summary>
        /// <returns>The <see cref="XmlSerializer"/> used during serialization and deserialization.</returns>
        public virtual XmlSerializer CreateXmlSerializer(Type type)
        {
            return new XmlSerializer(type);
        }

        private Task<object> ReadInternal(InputFormatterContext context, Encoding effectiveEncoding)
        {
            var type = context.Metadata.ModelType;
            var request = context.HttpContext.Request;

            using (var xmlReader = CreateXmlReader(request.Body, effectiveEncoding))
            {
                var xmlSerializer = CreateXmlSerializer(type);
                return Task.FromResult(xmlSerializer.Deserialize(xmlReader));
            }
        }
    }
}