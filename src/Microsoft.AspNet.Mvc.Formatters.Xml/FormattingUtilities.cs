// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if DNX451
using System.Runtime.Serialization;
#endif
using System.Xml;

namespace Microsoft.AspNet.Mvc.Formatters.Xml
{
    /// <summary>
    /// Contains methods which are used by Xml input formatters.
    /// </summary>
    public static class FormattingUtilities
    {
        public static readonly int DefaultMaxDepth = 32;

#if DNX451
        public static readonly XsdDataContractExporter XsdDataContractExporter = new XsdDataContractExporter();
#endif

        /// <summary>
        /// Gets the default Reader Quotas for XmlReader.
        /// </summary>
        /// <returns>XmlReaderQuotas with default values</returns>
        public static XmlDictionaryReaderQuotas GetDefaultXmlReaderQuotas()
        {
            return new XmlDictionaryReaderQuotas()
            {
                MaxArrayLength = int.MaxValue,
                MaxBytesPerRead = int.MaxValue,
                MaxDepth = DefaultMaxDepth,
                MaxNameTableCharCount = int.MaxValue,
                MaxStringContentLength = int.MaxValue
            };
        }

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
    }
}
