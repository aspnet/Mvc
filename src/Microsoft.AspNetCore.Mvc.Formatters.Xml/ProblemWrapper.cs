// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Mvc.Formatters.Xml
{
    /// <summary>
    /// Wrapper class for <see cref="Problem"/> to enable it to be serialized by the xml formatters.
    /// </summary>
    [XmlRoot("Error")]
    public sealed class ProblemWrapper : IXmlSerializable, IUnwrappable
    {
        // Note: XmlSerializer requires to have default constructor
        public ProblemWrapper()
        {
            Problem = new Problem();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemWrapper"/> class.
        /// </summary>
        /// <param name="problem">The <see cref="Mvc.Problem"/> object that needs to be wrapped.</param>
        public ProblemWrapper(Problem problem)
        {
            Problem = problem ?? throw new ArgumentNullException(nameof(Problem));
        }

        /// <summary>
        /// Gets the wrapped object which is serialized/deserialized into XML
        /// representation.
        /// </summary>
        public Problem Problem { get; }

        /// <inheritdoc />
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Generates a <see cref="Problem"/> object from its XML representation.
        /// </summary>
        /// <param name="reader">The <see cref="XmlReader"/> stream from which the object is deserialized.</param>
        public void ReadXml(XmlReader reader) => throw new NotSupportedException();

        /// <summary>
        /// Converts the wrapped <see cref="Problem"/> object into its XML representation.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(XmlWriter writer)
        {
            WriteProperty(nameof(Problem.Title), Problem.Title);

            if (Problem.Status != null)
            {
                WriteProperty(nameof(Problem.Status), Problem.Status);
            }

            if (!string.IsNullOrEmpty(Problem.Instance))
            {
                WriteProperty(nameof(Problem.Instance), Problem.Instance);
            }

            if (!string.IsNullOrEmpty(Problem.Type))
            {
                WriteProperty(nameof(Problem.Type), Problem.Type);
            }

            if (!string.IsNullOrEmpty(Problem.Detail))
            {
                WriteProperty(nameof(Problem.Detail), Problem.Detail);

            }

            if (Problem.GetType() != typeof(Problem))
            {
                // Derived type
                var properties = PropertyHelper.GetVisibleProperties(Problem.GetType());
                for (var i = 0; i < properties.Length; i++)
                {
                    var property = properties[i];
                    if (property.Property.DeclaringType != typeof(Problem))
                    {
                        WriteProperty(property.Name, property.GetValue(Problem));
                    }
                }
            }

            foreach (var extendedProperty in Problem.AdditionalProperties)
            {
                WriteProperty(extendedProperty.Key, extendedProperty.Value);
            }

            void WriteProperty<TValue>(string name, TValue value)
            {
                writer.WriteStartElement(XmlConvert.EncodeLocalName(name));
                writer.WriteValue(value);
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

            return Problem;
        }
    }
}