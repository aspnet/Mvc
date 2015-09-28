﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.Framework.WebEncoders.Testing;
using Xunit;

namespace Microsoft.AspNet.Mvc.ViewFeatures
{
    public class StringHtmlContentTest
    {
        [Fact]
        public void WriteTo_WritesContent()
        {
            // Arrange & Act
            var content = new StringHtmlContent("Hello World");

            // Assert
            using (var writer = new StringWriter())
            {
                content.WriteTo(writer, new CommonTestEncoder());
                Assert.Equal("HtmlEncode[[Hello World]]", writer.ToString());
            }
        }
    }
}
