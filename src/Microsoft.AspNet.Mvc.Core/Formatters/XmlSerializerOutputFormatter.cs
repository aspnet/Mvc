﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
    public class XmlSerializerOutputFormatter : XmlOutputFormatter
    {
        /// <summary>
        /// Initializes a new instance of <see cref="XmlSerializerOutputFormatter"/>
        /// with default XmlWriterSettings.
        /// </summary>
        public XmlSerializerOutputFormatter()
            : this(GetDefaultXmlWriterSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="XmlSerializerOutputFormatter"/>
        /// </summary>
        /// <param name="writerSettings">The settings to be used by the <see cref="XmlSerializer"/>.</param>
        public XmlSerializerOutputFormatter([NotNull] XmlWriterSettings writerSettings)
            : base(writerSettings)
        {
        }

        /// <summary>
        /// Create a new instance of <see cref="XmlSerializer"/> for the given object type.
        /// </summary>
        /// <param name="type">The type of object for which the serializer should be created.</param>
        /// <returns>A new instance of <see cref="XmlSerializer"/></returns>
        public virtual XmlSerializer CreateSerializer([NotNull] Type type)
        {
            try
            {
                // If the serializer does not support this type it will throw an exception.
                return new XmlSerializer(type);
            }
            catch (Exception)
            {
                // We do not surface the caught exception because if CanWriteResult returns
                // false, then this Formatter is not picked up at all.
            }

            return null;
        }

        /// <inheritdoc />
        public override bool CanWriteResult([NotNull] OutputFormatterContext context, MediaTypeHeaderValue contentType)
        {
            var savedContentType = context.SelectedContentType;
            if (base.CanWriteResult(context, contentType))
            {
                if (CreateSerializer(base.GetObjectType(context)) != null)
                {
                    return true;
                }

                context.SelectedContentType = savedContentType;
            }

            return false;
        }

        /// <inheritdoc />
        public override Task WriteResponseBodyAsync([NotNull] OutputFormatterContext context)
        {
            var response = context.ActionContext.HttpContext.Response;

            var tempWriterSettings = WriterSettings.Clone();
            tempWriterSettings.Encoding = context.SelectedEncoding;

            var innerStream = context.ActionContext.HttpContext.Response.Body;

            using (var outputStream = new DelegatingStream(innerStream))
            using (var xmlWriter = CreateXmlWriter(outputStream, tempWriterSettings))
            {
                var xmlSerializer = CreateSerializer(GetObjectType(context));
                xmlSerializer.Serialize(xmlWriter, context.Object);
            }

            return Task.FromResult(true);
        }
    }
}