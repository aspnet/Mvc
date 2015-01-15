﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc
{
    public static class MvcOptionsExtensions
    {
        /// <summary>
        /// Adds <see cref="XmlDataContractSerializerInputFormatter"/> and <see cref="XmlDataContractSerializerOutputFormatter"/>
        /// to the input and output formatter collections respectively.
        /// </summary>
        /// <param name="options">The MvcOptions</param>
        public static void AddXmlDataContractSerializerFormatter(this MvcOptions options)
        {
            options.OutputFormatters.Add(
                new XmlDataContractSerializerOutputFormatter(XmlOutputFormatter.GetDefaultXmlWriterSettings()));

            options.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());
        }
    }
}