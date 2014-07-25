// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core
{
    public class XmlDataContractSerializerOutputFormatterTests
    {
        [DataContract(Name = "DummyClass", Namespace = "")]
        public class DummyClass
        {
            [DataMember]
            public int SampleInt { get; set; }
        }

        [DataContract(Name = "TestLevelOne", Namespace = "")]
        public class TestLevelOne
        {
            [DataMember]
            public int SampleInt { get; set; }
            [DataMember]
            public string sampleString;
        }

        [DataContract(Name = "TestLevelTwo", Namespace = "")]
        public class TestLevelTwo
        {
            [DataMember]
            public string SampleString { get; set; }
            [DataMember]
            public TestLevelOne TestOne { get; set; }
        }

        [Fact]
        public async Task XmlDataContractSerializerOutputFormatterWritesSimpleTypes()
        {
            // Arrange
            var sampleInput = new DummyClass { SampleInt = 10 };
            var formatter = new XmlDataContractSerializerOutputFormatter(
                XmlSerializerOutputFormatter.GetDefaultXmlWriterSettings(),
                indent:false);
            var outputFormatterContext = GetOutputFormatterContext(sampleInput, sampleInput.GetType());

            // Act
            await formatter.WriteAsync(outputFormatterContext, CancellationToken.None);

            // Assert
            Assert.NotNull(outputFormatterContext.HttpContext.Response.Body);
            outputFormatterContext.HttpContext.Response.Body.Position = 0;
            Assert.Equal("<DummyClass xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                "<SampleInt>10</SampleInt></DummyClass>",
                new StreamReader(outputFormatterContext.HttpContext.Response.Body, Encoding.UTF8).ReadToEnd());
        }

        [Fact]
        public async Task XmlDataContractSerializerOutputFormatterWritesComplexTypes()
        {
            // Arrange
            var sampleInput = new TestLevelTwo
            {
                SampleString = "TestString",
                TestOne = new TestLevelOne
                {
                    SampleInt = 10,
                    sampleString = "TestLevelOne string"
                }
            };
            var formatter = new XmlDataContractSerializerOutputFormatter(
                XmlSerializerOutputFormatter.GetDefaultXmlWriterSettings(),
                indent: false);
            var outputFormatterContext = GetOutputFormatterContext(sampleInput, sampleInput.GetType());

            // Act
            await formatter.WriteAsync(outputFormatterContext, CancellationToken.None);

            // Assert
            Assert.NotNull(outputFormatterContext.HttpContext.Response.Body);
            outputFormatterContext.HttpContext.Response.Body.Position = 0;
            Assert.Equal("<TestLevelTwo xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                            "<SampleString>TestString</SampleString>" +
                            "<TestOne><SampleInt>10</SampleInt><sampleString>TestLevelOne string</sampleString>" +
                            "</TestOne></TestLevelTwo>",
                new StreamReader(outputFormatterContext.HttpContext.Response.Body, Encoding.UTF8).ReadToEnd());
        }

        [Fact]
        public async Task XmlDataContractSerializerOutputFormatterWritesOnModifiedWriterSettings()
        {
            // Arrange
            var sampleInput = new DummyClass { SampleInt = 10 };
            var outputFormatterContext = GetOutputFormatterContext(sampleInput, sampleInput.GetType());
            var formatter = new XmlDataContractSerializerOutputFormatter(
                new System.Xml.XmlWriterSettings
                {
                    OmitXmlDeclaration = false,
                    CloseOutput = false
                },
                indent: false);

            // Act
            await formatter.WriteAsync(outputFormatterContext, CancellationToken.None);

            // Assert
            Assert.NotNull(outputFormatterContext.HttpContext.Response.Body);
            outputFormatterContext.HttpContext.Response.Body.Position = 0;
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                            "<DummyClass xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                            "<SampleInt>10</SampleInt></DummyClass>",
                        new StreamReader(outputFormatterContext.HttpContext.Response.Body, Encoding.UTF8).ReadToEnd());
        }

        [Fact]
        public async Task XmlDataContractSerializerOutputFormatterWritesUTF16Output()
        {
            // Arrange
            var sampleInput = new DummyClass { SampleInt = 10 };
            var outputFormatterContext = GetOutputFormatterContext(sampleInput, sampleInput.GetType(),
                "application/xml; charset=utf-16");
            var formatter = new XmlDataContractSerializerOutputFormatter(
                XmlSerializerOutputFormatter.GetDefaultXmlWriterSettings(),
                indent: false);
            formatter.WriterSettings.OmitXmlDeclaration = false;

            // Act
            await formatter.WriteAsync(outputFormatterContext, CancellationToken.None);

            // Assert
            Assert.NotNull(outputFormatterContext.HttpContext.Response.Body);
            outputFormatterContext.HttpContext.Response.Body.Position = 0;
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
                            "<DummyClass xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                            "<SampleInt>10</SampleInt></DummyClass>",
                        new StreamReader(outputFormatterContext.HttpContext.Response.Body,
                                Encodings.UTF16EncodingLittleEndian).ReadToEnd());
        }

        [Fact]
        public async Task XmlDataContractSerializerOutputFormatterWritesIndentedOutput()
        {
            // Arrange
            var sampleInput = new DummyClass { SampleInt = 10 };
            var formatter = new XmlDataContractSerializerOutputFormatter(
                XmlSerializerOutputFormatter.GetDefaultXmlWriterSettings(),
                indent: true);
            var outputFormatterContext = GetOutputFormatterContext(sampleInput, sampleInput.GetType());

            // Act
            await formatter.WriteAsync(outputFormatterContext, CancellationToken.None);

            // Assert
            Assert.NotNull(outputFormatterContext.HttpContext.Response.Body);
            outputFormatterContext.HttpContext.Response.Body.Position = 0;
            var outputString = new StreamReader(outputFormatterContext.HttpContext.Response.Body,
                Encoding.UTF8).ReadToEnd();
            Assert.Equal("<DummyClass xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                "\r\n  <SampleInt>10</SampleInt>\r\n</DummyClass>",
                outputString);
        }

        private OutputFormatterContext GetOutputFormatterContext(object outputValue, Type outputType,
                                                        string contentType = "application/xml; charset=utf-8")
        {
            return new OutputFormatterContext
            {
                ObjectResult = new ObjectResult(outputValue),
                DeclaredType = outputType,
                HttpContext = GetHttpContext(contentType)
            };
        }

        private static HttpContext GetHttpContext(string contentType)
        {
            var response = new Mock<HttpResponse>();
            var headers = new Mock<IHeaderDictionary>();
            response.Setup(r => r.ContentType).Returns(contentType);
            response.SetupGet(r => r.Headers).Returns(headers.Object);
            response.SetupGet(f => f.Body).Returns(new MemoryStream());
            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(c => c.Response).Returns(response.Object);
            return httpContext.Object;
        }
    }
}