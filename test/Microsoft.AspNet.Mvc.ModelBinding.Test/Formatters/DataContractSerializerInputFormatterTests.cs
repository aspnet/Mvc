using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNet.Http;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding.Test
{
    public class DataContractSerializerInputFormatterTests
    {
        [DataContract(Name = "DummyClass", Namespace = "")]
        public class DummyClass
        {
            [DataMember]
            public int sampleInt;
        }

        [DataContract(Name = "TestLevelOne", Namespace = "")]
        public class TestLevelOne
        {
            [DataMember]
            public int sampleInt;
            [DataMember]
            public string sampleString;
            public DateTime date;
        }

        [DataContract(Name = "TestLevelTwo", Namespace = "")]
        public class TestLevelTwo
        {
            [DataMember]
            public string sampleString;
            [DataMember]
            public TestLevelOne testOne;
        }

        [Fact]
        public void DataContractSerializerFormatterHasProperSuppportedMediaTypes()
        {
            // Arrange & Act
            var formatter = new DataContractSerializerInputFormatter();

            // Assert
            Assert.True(formatter.SupportedMediaTypes.Contains("application/xml"));
            Assert.True(formatter.SupportedMediaTypes.Contains("text/xml"));
        }

        [Fact]
        public void DataContractSerializerFormatterHasProperSuppportedEncodings()
        {
            // Arrange & Act
            var formatter = new DataContractSerializerInputFormatter();

            // Assert
            Assert.True(formatter.SupportedEncodings.Any(i => i.WebName == "utf-8"));
            Assert.True(formatter.SupportedEncodings.Any(i => i.WebName == "utf-16"));
        }

        [Fact]
        public async Task XmlFormatterReadsSimpleTypes()
        {
            // Arrange
            var expectedInt = 10;
            var expectedString = "TestString";

            var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                                "<TestLevelOne><sampleInt>" + expectedInt + "</sampleInt>" +
                                "<sampleString>" + expectedString + "</sampleString></TestLevelOne>";

            var formatter = new DataContractSerializerInputFormatter();
            var contentBytes = Encoding.UTF8.GetBytes(input);
            var context = GetInputFormatterContext(contentBytes, typeof(TestLevelOne));

            // Act
            await formatter.ReadAsync(context);

            // Assert
            Assert.NotNull(context.Model);
            Assert.IsType<TestLevelOne>(context.Model);

            var model = context.Model as TestLevelOne;
            Assert.Equal(expectedInt, model.sampleInt);
            Assert.Equal(expectedString, model.sampleString);
        }

        [Fact]
        public async Task XmlFormatterReadsComplexTypes()
        {
            // Arrange
            var expectedInt = 10;
            var expectedString = "TestString";
            var expectedLevelTwoString = "102";

            var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                        "<TestLevelTwo><sampleString>" + expectedLevelTwoString + "</sampleString>" +
                        "<testOne><sampleInt>" + expectedInt + "</sampleInt>" +
                        "<sampleString>" + expectedString + "</sampleString></testOne></TestLevelTwo>";

            var formatter = new DataContractSerializerInputFormatter();
            var contentBytes = Encoding.UTF8.GetBytes(input);
            var context = GetInputFormatterContext(contentBytes, typeof(TestLevelTwo));

            // Act
            await formatter.ReadAsync(context);

            // Assert
            Assert.NotNull(context.Model);
            Assert.IsType<TestLevelTwo>(context.Model);

            var model = context.Model as TestLevelTwo;
            Assert.Equal(expectedLevelTwoString, model.sampleString);
            Assert.Equal(expectedInt, model.testOne.sampleInt);
            Assert.Equal(expectedString, model.testOne.sampleString);
        }

        [Fact]
        public async Task XmlFormatterReadsWhenMaxDepthIsModified()
        {
            // Arrange
            var expectedInt = 10;

            var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                "<DummyClass><sampleInt>" + expectedInt + "</sampleInt></DummyClass>";
            var formatter = new DataContractSerializerInputFormatter();
            formatter.MaxDepth = 10;
            var contentBytes = Encoding.UTF8.GetBytes(input);
            var context = GetInputFormatterContext(contentBytes, typeof(DummyClass));


            // Act
            await formatter.ReadAsync(context);

            // Assert
            Assert.NotNull(context.Model);
            Assert.IsType<DummyClass>(context.Model);
            var model = context.Model as DummyClass;
            Assert.Equal(expectedInt, model.sampleInt);
        }

        [Fact]
        public async Task XmlFormatterThrowsOnExceededMaxDepth()
        {
            // Arrange
            var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                        "<TestLevelTwo><sampleString>test</sampleString>" +
                        "<testOne><sampleInt>10</sampleInt>" +
                        "<sampleString>test</sampleString></testOne></TestLevelTwo>";
            var formatter = new DataContractSerializerInputFormatter();
            formatter.MaxDepth = 1;
            var contentBytes = Encoding.UTF8.GetBytes(input);
            var context = GetInputFormatterContext(contentBytes, typeof(TestLevelTwo));

            // Act & Assert
            await Assert.ThrowsAsync(typeof(SerializationException), async () => await formatter.ReadAsync(context));
        }

        [Fact]
        public void DataContractSerializerThrowsWhenMaxDepthIsBelowOne()
        {
            // Arrange
            var formatter = new XmlSerializerInputFormatter();

            // Act & Assert
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => { formatter.MaxDepth = 0; });
        }

        [Fact]
        public async Task VerifyStreamIsOpenAfterRead()
        {
            // Arrange
            var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                "<DummyClass><sampleInt>10</sampleInt></DummyClass>";
            var formatter = new DataContractSerializerInputFormatter();
            var contentBytes = Encoding.UTF8.GetBytes(input);
            var context = GetInputFormatterContext(contentBytes, typeof(DummyClass));

            // Act
            await formatter.ReadAsync(context);

            // Assert
            Assert.NotNull(context.Model);
            Assert.True(context.HttpContext.Request.Body.CanRead);
        }

        private InputFormatterContext GetInputFormatterContext(byte[] contentBytes, Type modelType)
        {
            var httpContext = GetHttpContext(contentBytes);
            var modelState = new ModelStateDictionary();
            var metadata = new EmptyModelMetadataProvider().GetMetadataForType(null, modelType);
            return new InputFormatterContext(httpContext, metadata, modelState);
        }

        private static HttpContext GetHttpContext(byte[] contentBytes,
                                                        string contentType = "application/xml")
        {
            var request = new Mock<HttpRequest>();
            var headers = new Mock<IHeaderDictionary>();
            headers.SetupGet(h => h["Content-Type"]).Returns(contentType);
            request.SetupGet(r => r.Headers).Returns(headers.Object);
            request.SetupGet(f => f.Body).Returns(new MemoryStream(contentBytes));

            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(c => c.Request).Returns(request.Object);
            return httpContext.Object;
        }
    }
}