// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Formatters.Xml
{
    public class ProblemDetailsWrapperTest
    {
        [Fact]
        public void ReadXml_ReadsProblemDetailsXml()
        {
            // Arrange
            var xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<ProblemDetails>" +
                "<Title>Some title</Title>" +
                "<Status>403</Status>" +
                "<Instance>Some instance</Instance>" +
                "<key1>Test Value 1</key1>" +
                "<_x005B_key2_x005D_>Test Value 2</_x005B_key2_x005D_>" +
                "</ProblemDetails>";
            var serializer = new DataContractSerializer(typeof(ProblemDetailsWrapper));

            // Act
            var value = serializer.ReadObject(
                new MemoryStream(Encoding.UTF8.GetBytes(xml)));

            // Assert
            var problemDetails = Assert.IsType<ProblemDetailsWrapper>(value).ProblemDetails;
            Assert.Equal("Some title", problemDetails.Title);
            Assert.Equal("Some instance", problemDetails.Instance);
            Assert.Equal(403, problemDetails.Status);

            Assert.Collection(
                problemDetails.Extension,
                kvp =>
                {
                    Assert.Equal("key1", kvp.Key);
                    Assert.Equal("Test Value 1", kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal("[key2]", kvp.Key);
                    Assert.Equal("Test Value 2", kvp.Value);
                });
        }


        [Fact]
        public void WriteXml_WritesValidXml()
        {
            // Arrange
            var problemDetails = new ProblemDetails
            {
                Title = "Some title",
                Detail = "Some detail",
                Extension =
                {
                    ["key1"] = "Test Value 1",
                    ["[Key2]"] = "Test Value 2",
                },
            };

            var wrapper = new ProblemDetailsWrapper(problemDetails);
            var outputStream = new MemoryStream();
            var expectedContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<ProblemDetails>" +
                "<Detail>Some detail</Detail>" +
                "<Title>Some title</Title>" +
                "<key1>Test Value 1</key1>" +
                "<_x005B_Key2_x005D_>Test Value 2</_x005B_Key2_x005D_>" +
                "</ProblemDetails>";

            // Act
            using (var xmlWriter = XmlWriter.Create(outputStream))
            {
                var dataContractSerializer = new DataContractSerializer(wrapper.GetType());
                dataContractSerializer.WriteObject(xmlWriter, wrapper);
            }
            outputStream.Position = 0;
            var res = new StreamReader(outputStream, Encoding.UTF8).ReadToEnd();

            // Assert
            Assert.Equal(expectedContent, res);
        }
    }
}
