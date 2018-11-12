﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection
{
    public class XmlDataContractSerializerMvcOptionsSetupTest
    {
        [Fact]
        public void AddsFormatterMapping()
        {
            // Arrange
            var optionsSetup = new XmlDataContractSerializerMvcOptionsSetup(Options.Options.Create(new MvcXmlOptions()), NullLoggerFactory.Instance);
            var options = new MvcOptions();

            // Act
            optionsSetup.Configure(options);

            // Assert
            var mappedContentType = options.FormatterMappings.GetMediaTypeMappingForFormat("xml");
            Assert.Equal("application/xml", mappedContentType);
        }

        [Fact]
        public void DoesNotOverrideExistingMapping()
        {
            // Arrange
            var optionsSetup = new XmlDataContractSerializerMvcOptionsSetup(Options.Options.Create(new MvcXmlOptions()), NullLoggerFactory.Instance);
            var options = new MvcOptions();
            options.FormatterMappings.SetMediaTypeMappingForFormat("xml", "text/xml");

            // Act
            optionsSetup.Configure(options);

            // Assert
            var mappedContentType = options.FormatterMappings.GetMediaTypeMappingForFormat("xml");
            Assert.Equal("text/xml", mappedContentType);
        }

        [Fact]
        public void AddsInputFormatter()
        {
            // Arrange
            var optionsSetup = new XmlDataContractSerializerMvcOptionsSetup(Options.Options.Create(new MvcXmlOptions()), NullLoggerFactory.Instance);
            var options = new MvcOptions();

            // Act
            optionsSetup.Configure(options);

            // Assert
            Assert.IsType<XmlDataContractSerializerInputFormatter>(Assert.Single(options.InputFormatters));
        }

        [Fact]
        public void AddsOutputFormatter()
        {
            // Arrange
            var optionsSetup = new XmlDataContractSerializerMvcOptionsSetup(Options.Options.Create(new MvcXmlOptions()), NullLoggerFactory.Instance);
            var options = new MvcOptions();

            // Act
            optionsSetup.Configure(options);

            // Assert
            Assert.IsType<XmlDataContractSerializerOutputFormatter>(Assert.Single(options.OutputFormatters));
        }
    }
}
