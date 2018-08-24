// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.AspNetCore.Mvc.Formatters.Xml
{
    /// <summary>
    /// Wrapper class for <see cref="Mvc.ProblemDetails"/> to enable it to be serialized by the xml formatters.
    /// </summary>
    [XmlRoot(nameof(ProblemDetails))]
    public sealed class ProblemDetailsWrapper : IXmlSerializable, IUnwrappable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ProblemDetailsWrapper"/>.
        /// </summary>
        public ProblemDetailsWrapper()
            : this(new ProblemDetails())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ProblemDetailsWrapper"/>.
        /// </summary>
        public ProblemDetailsWrapper(ProblemDetails problemDetails)
        {
            ProblemDetails = problemDetails;
        }

        /// <summary>
        /// Gets the wrapped <see cref="Mvc.ProblemDetails"/>.
        /// </summary>
        public ProblemDetails ProblemDetails { get; }

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
                ReadProperty(reader, ProblemDetails, key);

                reader.MoveToContent();
            }

            reader.ReadEndElement();
        }

        internal static void ReadProperty(XmlReader reader, ProblemDetails problemDetails, string key)
        {
            var value = reader.ReadInnerXml();

            if (key == nameof(problemDetails.Detail))
            {
                problemDetails.Detail = value;
            }
            else if (key == nameof(problemDetails.Instance))
            {
                problemDetails.Instance = value;
            }
            else if (key == nameof(problemDetails.Status))
            {
                problemDetails.Status = string.IsNullOrEmpty(value) ?
                    (int?)null :
                    int.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (key == nameof(problemDetails.Title))
            {
                problemDetails.Title = value;
            }
            else if (key == nameof(problemDetails.Type))
            {
                problemDetails.Type = value;
            }
            else
            {
                problemDetails.ExtensionMembers.Add(key, value);
            }
        }

        /// <inheritdoc />
        public void WriteXml(XmlWriter writer) => WriteProblemDetails(writer, ProblemDetails);
        
        internal static void WriteProblemDetails(XmlWriter writer, ProblemDetails problemDetails)
        {
            if (!string.IsNullOrEmpty(problemDetails.Detail))
            {
                writer.WriteElementString(
                    XmlConvert.EncodeLocalName(nameof(problemDetails.Detail)),
                    problemDetails.Detail);
            }

            if (!string.IsNullOrEmpty(problemDetails.Instance))
            {
                writer.WriteElementString(
                    XmlConvert.EncodeLocalName(nameof(problemDetails.Instance)),
                    problemDetails.Instance);
            }

            if (problemDetails.Status.HasValue)
            {
                writer.WriteStartElement(XmlConvert.EncodeLocalName(nameof(problemDetails.Status)));
                writer.WriteValue(problemDetails.Status.Value);
                writer.WriteEndElement();
            }

            if (!string.IsNullOrEmpty(problemDetails.Title))
            {
                writer.WriteElementString(
                    XmlConvert.EncodeLocalName(nameof(problemDetails.Title)),
                    problemDetails.Title);
            }

            if (!string.IsNullOrEmpty(problemDetails.Type))
            {
                writer.WriteElementString(
                    XmlConvert.EncodeLocalName(nameof(problemDetails.Type)),
                    problemDetails.Type);
            }

            foreach (var keyValuePair in problemDetails.ExtensionMembers)
            {
                var key = keyValuePair.Key;
                var value = keyValuePair.Value;

                writer.WriteStartElement(XmlConvert.EncodeLocalName(key));
                if (value != null)
                {
                    writer.WriteValue(value);
                }

                writer.WriteEndElement();
            }
        }

        /// <inheritdoc />
        public object Unwrap(Type declaredType)
        {
            if (declaredType == null)
            {
                throw new ArgumentNullException(nameof(declaredType));
            }

            return ProblemDetails;
        }
    }
}
