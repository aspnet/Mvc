// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class TagHelperContentWrapperTextWriterTest
    {
        [Fact]
        public void CreatesANewTagHelperContentAndWrapsIt()
        {
            // Arrange & Act
            var wrapper = new RazorPage.TagHelperContentWrapperTextWriter(Encoding.UTF8);

            // Assert
            Assert.Equal(new DefaultTagHelperContent(), wrapper.Content);
        }

        [Fact]
        public void WriteToTheTextWriterAppendsTagHelperContent()
        {
            // Arrange
            var wrapper = new RazorPage.TagHelperContentWrapperTextWriter(Encoding.UTF8);
            var expected = "Hello World!";

            // Act
            wrapper.Write(expected);

            // Assert
            Assert.Equal(expected, wrapper.Content.GetContent());
            Assert.Equal(expected, wrapper.ToString());
        }
    }
}