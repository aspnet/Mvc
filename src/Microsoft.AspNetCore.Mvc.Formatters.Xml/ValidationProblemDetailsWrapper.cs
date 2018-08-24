// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.AspNetCore.Mvc.Formatters.Xml
{
    /// <summary>
    /// Wrapper class for <see cref="Mvc.ValidationProblemDetails"/> to enable it to be serialized by the xml formatters.
    /// </summary>
    [XmlRoot(nameof(Mvc.ValidationProblemDetails))]
    public sealed class ValidationProblemDetailsWrapper : IXmlSerializable, IUnwrappable
    {
        private static readonly string EmptyKey = SerializableErrorWrapper.EmptyKey;

        /// <summary>
        /// Initializes a new instance of <see cref="ValidationProblemDetailsWrapper"/>.
        /// </summary>
        public ValidationProblemDetailsWrapper()
            : this(new ValidationProblemDetails())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ValidationProblemDetailsWrapper"/> for the specified
        /// <paramref name="problemDetails"/>.
        /// </summary>
        /// <param name="problemDetails">The <see cref="ValidationProblemDetails"/>.</param>
        public ValidationProblemDetailsWrapper(ValidationProblemDetails problemDetails)
        {
            ValidationProblemDetails = problemDetails;
        }

        /// <summary>
        /// Gets the wrapped <see cref="Mvc.ValidationProblemDetails"/>.
        /// </summary>
        public ValidationProblemDetails ValidationProblemDetails { get; }

        /// <inheritdoc />
        public XmlSchema GetSchema() => null;

        /// <inheritdoc />
        public void ReadXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                reader.Read();
                return;
            }

            reader.ReadStartElement();
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                var key = XmlConvert.DecodeName(reader.LocalName);
                if (key == nameof(ValidationProblemDetails.Errors))
                {
                    reader.Read();
                    ReadErrorProperty(reader);
                }
                else
                {
                    ProblemDetailsWrapper.ReadProperty(reader, ValidationProblemDetails, key);
                }

                reader.MoveToContent();
            }

            reader.ReadEndElement();
        }

        private void ReadErrorProperty(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                return;
            }

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                var key = XmlConvert.DecodeName(reader.LocalName);
                var value = reader.ReadInnerXml();
                if (string.Equals(EmptyKey, key, StringComparison.Ordinal))
                {
                    key = string.Empty;
                }

                ValidationProblemDetails.Errors.Add(key, new[] { value });
                reader.MoveToContent();
            }
        }

        /// <inheritdoc />
        public void WriteXml(XmlWriter writer)
        {
            ProblemDetailsWrapper.WriteProblemDetails(writer, ValidationProblemDetails);
            writer.WriteStartElement(XmlConvert.EncodeLocalName(nameof(ValidationProblemDetails.Errors)));

            foreach (var keyValuePair in ValidationProblemDetails.Errors)
            {
                var key = keyValuePair.Key;
                var value = keyValuePair.Value;
                if (string.IsNullOrEmpty(key))
                {
                    key = EmptyKey;
                }

                writer.WriteStartElement(XmlConvert.EncodeLocalName(key));
                if (value != null)
                {
                    writer.WriteValue(value);
                }

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        /// <inheritdoc />
        public object Unwrap(Type declaredType)
        {
            if (declaredType == null)
            {
                throw new ArgumentNullException(nameof(declaredType));
            }

            return ValidationProblemDetails;
        }
    }
}
