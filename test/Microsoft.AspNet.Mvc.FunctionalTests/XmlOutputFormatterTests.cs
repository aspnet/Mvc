// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc.Logging;
using Microsoft.AspNet.TestHost;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class XmlOutputFormatterTests
    {
        private HttpClient GetClient(TestSink sink)
        {
            var factory = new TestLoggerFactory(sink, true);
            var services = TestHelper.CreateServices("FormatterWebSite", factory);
            var app = (Action<IApplicationBuilder>)new FormatterWebSite.Startup(factory).Configure;
            var server = TestServer.Create(services, app);
            return server.CreateClient();
        }

        [Fact]
        public async Task XmlDataContractSerializerOutputFormatterIsCalled()
        {
            // Arrange
            var sink = new TestSink();
            var client = GetClient(sink);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/Home/GetDummyClass?sampleInput=10");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml;charset=utf-8"));

            // Act
            var response = await client.SendAsync(request);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("<DummyClass xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                "xmlns=\"http://schemas.datacontract.org/2004/07/FormatterWebSite\">" +
                "<SampleInt>10</SampleInt></DummyClass>",
                await response.Content.ReadAsStringAsync());
            var logs = sink.Writes.Where(w => string.Equals(w.LoggerName, "Microsoft.AspNet.Mvc.ObjectResult"));
            Assert.Single(logs);
            Assert.Equal(typeof(XmlDataContractSerializerOutputFormatter), 
                ((ObjectResultValues)logs.First().State).SelectedFormatter.OutputFormatterType);
        }

        [Fact]
        public async Task XmlSerializerOutputFormatterIsCalled()
        {
            // Arrange
            var sink = new TestSink();
            var client = GetClient(sink);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/XmlSerializer/GetDummyClass?sampleInput=10");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml;charset=utf-8"));

            // Act
            var response = await client.SendAsync(request);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("<DummyClass xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><SampleInt>10</SampleInt></DummyClass>",
                await response.Content.ReadAsStringAsync());
            var logs = sink.Writes.Where(w => string.Equals(w.LoggerName, "Microsoft.AspNet.Mvc.ObjectResult"));
            Assert.Single(logs);
            Assert.Equal(typeof(XmlSerializerOutputFormatter), 
                ((ObjectResultValues)logs.First().State).SelectedFormatter.OutputFormatterType);
        }

        [Fact]
        public async Task XmlSerializerFailsAndDataContractSerializerIsCalled()
        {
            // Arrange
            var sink = new TestSink();
            var client = GetClient(sink);
            var request = new HttpRequestMessage(HttpMethod.Post,
                                                 "http://localhost/DataContractSerializer/GetPerson?name=HelloWorld");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml;charset=utf-8"));

            // Act
            var response = await client.SendAsync(request);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("<Person xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                "xmlns=\"http://schemas.datacontract.org/2004/07/FormatterWebSite\">" +
                "<Name>HelloWorld</Name></Person>",
                await response.Content.ReadAsStringAsync());
            var logs = sink.Writes.Where(w => string.Equals(w.LoggerName, "Microsoft.AspNet.Mvc.ObjectResult"));
            Assert.Single(logs);
            Assert.Equal(typeof(XmlDataContractSerializerOutputFormatter),
                ((ObjectResultValues)logs.First().State).SelectedFormatter.OutputFormatterType);
        }

        [Fact]
        public async Task XmlSerializerOutputFormatter_WhenDerivedClassIsReturned()
        {
            // Arrange
            var sink = new TestSink();
            var client = GetClient(sink);
            var request = new HttpRequestMessage(
                HttpMethod.Post, "http://localhost/XmlSerializer/GetDerivedDummyClass?sampleInput=10");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml;charset=utf-8"));

            // Act
            var response = await client.SendAsync(request);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("<DummyClass xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xsi:type=\"DerivedDummyClass\">" +
                "<SampleInt>10</SampleInt><SampleIntInDerived>50</SampleIntInDerived></DummyClass>",
                await response.Content.ReadAsStringAsync());
            var logs = sink.Writes.Where(w => string.Equals(w.LoggerName, "Microsoft.AspNet.Mvc.ObjectResult"));
            Assert.Single(logs);
            Assert.Equal(typeof(XmlSerializerOutputFormatter),
                ((ObjectResultValues)logs.First().State).SelectedFormatter.OutputFormatterType);
        }

        [Fact]
        public async Task XmlDataContractSerializerOutputFormatter_WhenDerivedClassIsReturned()
        {
            // Arrange
            var sink = new TestSink();
            var client = GetClient(sink);
            var request = new HttpRequestMessage(
                HttpMethod.Post, "http://localhost/Home/GetDerivedDummyClass?sampleInput=10");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml;charset=utf-8"));

            // Act
            var response = await client.SendAsync(request);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("<DummyClass xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                "i:type=\"DerivedDummyClass\" xmlns=\"http://schemas.datacontract.org/2004/07/FormatterWebSite\"" +
                "><SampleInt>10</SampleInt><SampleIntInDerived>50</SampleIntInDerived></DummyClass>",
                await response.Content.ReadAsStringAsync());
            var logs = sink.Writes.Where(w => string.Equals(w.LoggerName, "Microsoft.AspNet.Mvc.ObjectResult"));
            Assert.Single(logs);
            Assert.Equal(typeof(XmlDataContractSerializerOutputFormatter), 
                ((ObjectResultValues)logs.First().State).SelectedFormatter.OutputFormatterType);
        }

        [Fact]
        public async Task XmlSerializerFormatter_DoesNotWriteDictionaryObjects()
        {
            // Arrange
            var sink = new TestSink();
            var client = GetClient(sink);
            var request = new HttpRequestMessage(
                HttpMethod.Post, "http://localhost/XmlSerializer/GetDictionary");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml;charset=utf-8"));

            // Act
            var response = await client.SendAsync(request);

            //Assert
            Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode);
            var logs = sink.Writes.Where(w => string.Equals(w.LoggerName, "Microsoft.AspNet.Mvc.ObjectResult"));
            Assert.Single(logs);
            Assert.Equal(null, ((ObjectResultValues)logs.First().State).SelectedFormatter.OutputFormatterType);
        }
    }
}