﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Core.Collections;
using Microsoft.Net.Http.Headers;
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

        [DataContract(Name = "SomeDummyClass", Namespace = "")]
        public class SomeDummyClass : DummyClass
        {
            [DataMember]
            public string SampleString { get; set; }
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

        [DataContract(Name = "Child", Namespace = "")]
        public class Child
        {
            [DataMember]
            public int Id { get; set; }
            [DataMember]
            public Parent Parent { get; set; }
        }

        [DataContract(Name = "Parent", Namespace = "")]
        public class Parent
        {
            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public List<Child> Children { get; set; }
        }

        public static IEnumerable<object[]> BasicTypeValues
        {
            get
            {
                yield return new object[] { "sampleString",
                    "<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">sampleString</string>" };
                yield return new object[] { 5,
                    "<int xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">5</int>" };
                yield return new object[] { 5.43,
                    "<double xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">5.43</double>" };
                yield return new object[] { 'a',
                    "<char xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">97</char>" };
                yield return new object[] { new DummyClass { SampleInt = 10 },
                    "<DummyClass xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                    "<SampleInt>10</SampleInt></DummyClass>" };
                yield return new object[] { new Dictionary<string, string>() { { "Hello", "World" } },
                    "<ArrayOfKeyValueOfstringstring xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                    "xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\"><KeyValueOfstringstring>" +
                    "<Key>Hello</Key><Value>World</Value></KeyValueOfstringstring></ArrayOfKeyValueOfstringstring>" };
            }
        }

        [Theory]
        [MemberData(nameof(BasicTypeValues))]
        public async Task XmlDataContractSerializerOutputFormatterCanWriteBasicTypes(object input, string expectedOutput)
        {
            // Arrange
            var formatter = new XmlDataContractSerializerOutputFormatter();
            var outputFormatterContext = GetOutputFormatterContext(input, typeof(object));

            // Act
            await formatter.WriteAsync(outputFormatterContext);

            // Assert
            Assert.NotNull(outputFormatterContext.ActionContext.HttpContext.Response.Body);
            outputFormatterContext.ActionContext.HttpContext.Response.Body.Position = 0;
            Assert.Equal(expectedOutput,
                new StreamReader(outputFormatterContext.ActionContext.HttpContext.Response.Body, Encoding.UTF8)
                        .ReadToEnd());
            Assert.True(outputFormatterContext.ActionContext.HttpContext.Response.Body.CanRead);
        }

        [Fact]
        public void DefaultConstructor_ExpectedWriterSettings_Created()
        {
            // Arrange and Act
            var formatter = new XmlDataContractSerializerOutputFormatter();

            // Assert
            var writerSettings = formatter.WriterSettings;
            Assert.NotNull(writerSettings);
            Assert.True(writerSettings.OmitXmlDeclaration);
            Assert.False(writerSettings.CloseOutput);
            Assert.False(writerSettings.CheckCharacters);
        }

        [Fact]
        public async Task SuppliedWriterSettings_TakeAffect()
        {
            // Arrange
            var writerSettings = FormattingUtilities.GetDefaultXmlWriterSettings();
            writerSettings.OmitXmlDeclaration = false;
            var sampleInput = new DummyClass { SampleInt = 10 };
            var formatterContext = GetOutputFormatterContext(sampleInput, sampleInput.GetType());
            var formatter = new XmlDataContractSerializerOutputFormatter(writerSettings);
            var expectedOutput = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"+
                                "<DummyClass xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                                "<SampleInt>10</SampleInt></DummyClass>";

            // Act
            await formatter.WriteAsync(formatterContext);

            // Assert
            Assert.Same(writerSettings, formatter.WriterSettings);
            var responseStream = formatterContext.ActionContext.HttpContext.Response.Body;
            Assert.NotNull(responseStream);
            responseStream.Position = 0;
            var actualOutput = new StreamReader(responseStream, Encoding.UTF8).ReadToEnd();
            Assert.Equal(expectedOutput, actualOutput);
        }

        [Fact]
        public async Task XmlDataContractSerializerOutputFormatterWritesSimpleTypes()
        {
            // Arrange
            var sampleInput = new DummyClass { SampleInt = 10 };
            var formatter = new XmlDataContractSerializerOutputFormatter();
            var outputFormatterContext = GetOutputFormatterContext(sampleInput, sampleInput.GetType());

            // Act
            await formatter.WriteAsync(outputFormatterContext);

            // Assert
            Assert.NotNull(outputFormatterContext.ActionContext.HttpContext.Response.Body);
            outputFormatterContext.ActionContext.HttpContext.Response.Body.Position = 0;
            Assert.Equal("<DummyClass xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                "<SampleInt>10</SampleInt></DummyClass>",
                new StreamReader(outputFormatterContext.ActionContext.HttpContext.Response.Body, Encoding.UTF8)
                        .ReadToEnd());
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
            var formatter = new XmlDataContractSerializerOutputFormatter();
            var outputFormatterContext = GetOutputFormatterContext(sampleInput, sampleInput.GetType());

            // Act
            await formatter.WriteAsync(outputFormatterContext);

            // Assert
            Assert.NotNull(outputFormatterContext.ActionContext.HttpContext.Response.Body);
            outputFormatterContext.ActionContext.HttpContext.Response.Body.Position = 0;
            Assert.Equal("<TestLevelTwo xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                            "<SampleString>TestString</SampleString>" +
                            "<TestOne><SampleInt>10</SampleInt><sampleString>TestLevelOne string</sampleString>" +
                            "</TestOne></TestLevelTwo>",
                new StreamReader(outputFormatterContext.ActionContext.HttpContext.Response.Body, Encoding.UTF8)
                        .ReadToEnd());
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
                });

            // Act
            await formatter.WriteAsync(outputFormatterContext);

            // Assert
            Assert.NotNull(outputFormatterContext.ActionContext.HttpContext.Response.Body);
            outputFormatterContext.ActionContext.HttpContext.Response.Body.Position = 0;
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                            "<DummyClass xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                            "<SampleInt>10</SampleInt></DummyClass>",
                        new StreamReader(outputFormatterContext.ActionContext.HttpContext.Response.Body, Encoding.UTF8).ReadToEnd());
        }

        [Fact]
        public async Task XmlDataContractSerializerOutputFormatterWritesUTF16Output()
        {
            // Arrange
            var sampleInput = new DummyClass { SampleInt = 10 };
            var outputFormatterContext = GetOutputFormatterContext(sampleInput, sampleInput.GetType(),
                "application/xml; charset=utf-16");
            var formatter = new XmlDataContractSerializerOutputFormatter();
            formatter.WriterSettings.OmitXmlDeclaration = false;

            // Act
            await formatter.WriteAsync(outputFormatterContext);

            // Assert
            Assert.NotNull(outputFormatterContext.ActionContext.HttpContext.Response.Body);
            outputFormatterContext.ActionContext.HttpContext.Response.Body.Position = 0;
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
                            "<DummyClass xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                            "<SampleInt>10</SampleInt></DummyClass>",
                        new StreamReader(outputFormatterContext.ActionContext.HttpContext.Response.Body,
                                Encodings.UTF16EncodingLittleEndian).ReadToEnd());
        }

        [Fact]
        public async Task XmlDataContractSerializerOutputFormatterWritesIndentedOutput()
        {
            // Arrange
            var sampleInput = new DummyClass { SampleInt = 10 };
            var formatter = new XmlDataContractSerializerOutputFormatter();
            formatter.WriterSettings.Indent = true;
            var outputFormatterContext = GetOutputFormatterContext(sampleInput, sampleInput.GetType());

            // Act
            await formatter.WriteAsync(outputFormatterContext);

            // Assert
            Assert.NotNull(outputFormatterContext.ActionContext.HttpContext.Response.Body);
            outputFormatterContext.ActionContext.HttpContext.Response.Body.Position = 0;
            var outputString = new StreamReader(outputFormatterContext.ActionContext.HttpContext.Response.Body,
                Encoding.UTF8).ReadToEnd();
            Assert.Equal("<DummyClass xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                "\r\n  <SampleInt>10</SampleInt>\r\n</DummyClass>",
                outputString);
        }

        [Fact]
        public async Task VerifyBodyIsNotClosedAfterOutputIsWritten()
        {
            // Arrange
            var sampleInput = new DummyClass { SampleInt = 10 };
            var formatter = new XmlDataContractSerializerOutputFormatter();
            var outputFormatterContext = GetOutputFormatterContext(sampleInput, sampleInput.GetType());

            // Act
            await formatter.WriteAsync(outputFormatterContext);

            // Assert
            Assert.NotNull(outputFormatterContext.ActionContext.HttpContext.Response.Body);
            Assert.True(outputFormatterContext.ActionContext.HttpContext.Response.Body.CanRead);
        }

        [Fact]
        public async Task XmlSerializerOutputFormatterDoesntFlushOutputStream()
        {
            // Arrange
            var sampleInput = new DummyClass { SampleInt = 10 };
            var formatter = new XmlDataContractSerializerOutputFormatter();
            var outputFormatterContext = GetOutputFormatterContext(sampleInput, sampleInput.GetType());

            var response = outputFormatterContext.ActionContext.HttpContext.Response;
            response.Body = FlushReportingStream.GetThrowingStream();

            // Act & Assert
            await formatter.WriteAsync(outputFormatterContext);
        }

        public static IEnumerable<object[]> TypesForCanWriteResult
        {
            get
            {
                yield return new object[] { null, typeof(string), true };
                yield return new object[] { null, null, false };
                yield return new object[] { new DummyClass { SampleInt = 5 }, null, true };
                yield return new object[] { new DummyClass { SampleInt = 5 }, typeof(object), true };
                yield return new object[] { null, typeof(object), true };
                yield return new object[] {
                    new Dictionary<string, string> { { "Hello", "world" } }, typeof(object), true };
                yield return new object[] {
                    new Dictionary<string, string> { { "Hello", "world" } }, typeof(Dictionary<string,string>), true };
            }
        }

        [Theory]
        [MemberData(nameof(TypesForCanWriteResult))]
        public void XmlDataContractSerializer_CanWriteResult(object input, Type declaredType, bool expectedOutput)
        {
            // Arrange
            var formatter = new XmlDataContractSerializerOutputFormatter();
            var outputFormatterContext = GetOutputFormatterContext(input, declaredType);

            // Act
            var result = formatter.CanWriteResult(outputFormatterContext, MediaTypeHeaderValue.Parse("application/xml"));

            // Assert
            Assert.Equal(expectedOutput, result);
        }

        public static IEnumerable<object[]> TypesForGetSupportedContentTypes
        {
            get
            {
                yield return new object[] { typeof(DummyClass), typeof(DummyClass), "application/xml" };
                yield return new object[] { typeof(DummyClass), typeof(object), "application/xml" };
                yield return new object[] { null, typeof(DummyClass), "application/xml" };
                yield return new object[] { typeof(DummyClass), null, "application/xml" };
                yield return new object[] { typeof(object), null, "application/xml" };
                yield return new object[] { null, null, null };
            }
        }

        [Theory]
        [MemberData(nameof(TypesForGetSupportedContentTypes))]
        public void XmlDataContractSerializer_GetSupportedContentTypes_Returns_SupportedTypes(Type declaredType, Type runtimeType, object expectedOutput)
        {
            // Arrange
            var formatter = new XmlDataContractSerializerOutputFormatter();

            // Act
            var result = formatter.GetSupportedContentTypes(
                declaredType, runtimeType, MediaTypeHeaderValue.Parse("application/xml"));

            // Assert
            if (expectedOutput != null)
            {
                Assert.Equal(expectedOutput, Assert.Single(result).ToString());
            }
            else
            {
                Assert.Equal(expectedOutput, result);
            }
        }

        [Fact]
        public async Task XmlDataContractSerializerOutputFormatterThrowsWhenNotConfiguredWithKnownTypes()
        {
            // TODO: Test on Mono platform

            // Arrange
            var sampleInput = new SomeDummyClass { SampleInt = 1, SampleString = "TestString" };
            var formatter = new XmlDataContractSerializerOutputFormatter();
            var outputFormatterContext = GetOutputFormatterContext(sampleInput, typeof(DummyClass));

            // Act & Assert
            await Assert.ThrowsAsync(typeof(SerializationException), async () => await formatter.WriteAsync(outputFormatterContext));
        }

        [Fact]
        public async Task XmlDataContractSerializerOutputFormatterThrowsWhenNotConfiguredWithPreserveReferences()
        {
            // TODO: Test on Mono platform

            // Arrange
            var child = new Child { Id = 1 };
            var parent = new Parent { Name = "Parent", Children = new List<Child> { child } };
            child.Parent = parent;

            var formatter = new XmlDataContractSerializerOutputFormatter();
            var outputFormatterContext = GetOutputFormatterContext(parent, parent.GetType());

            // Act & Assert
            await Assert.ThrowsAsync(typeof(SerializationException), async () => await formatter.WriteAsync(outputFormatterContext));
        }

        private OutputFormatterContext GetOutputFormatterContext(object outputValue, Type outputType,
                                                        string contentType = "application/xml; charset=utf-8")
        {
            return new OutputFormatterContext
            {
                Object = outputValue,
                DeclaredType = outputType,
                ActionContext = GetActionContext(contentType)
            };
        }

        private static ActionContext GetActionContext(string contentType)
        {
            var request = new Mock<HttpRequest>();
            var headers = new HeaderDictionary(new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase));
            headers["Accept-Charset"] = MediaTypeHeaderValue.Parse(contentType).Charset;
            request.Setup(r => r.ContentType).Returns(contentType);
            request.SetupGet(r => r.Headers).Returns(headers);
            var response = new Mock<HttpResponse>();
            response.SetupGet(f => f.Body).Returns(new MemoryStream());
            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(c => c.Request).Returns(request.Object);
            httpContext.SetupGet(c => c.Response).Returns(response.Object);
            return new ActionContext(httpContext.Object, routeData: null, actionDescriptor: null);
        }
    }
}