// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Framework.Internal;
using Xunit;

namespace Microsoft.AspNet.Mvc.Formatters
{
    public class FormatterCollectionTests
    {
        [Fact]
        public void RemoveType_RemovesAllOfType()
        {
            // Arrange
            var collection = new FormatterCollection<IOutputFormatter>
            {
                new TestOutputFormatter(),
                new AnotherTestOutputFormatter(),
                new TestOutputFormatter()
            };

            // Act
            collection.RemoveType<TestOutputFormatter>();

            // Assert
            var formatter = Assert.Single(collection);
            Assert.IsType(typeof(AnotherTestOutputFormatter), formatter);
        }

        private class TestOutputFormatter : OutputFormatter
        {
            public override Task WriteResponseBodyAsync([NotNull] OutputFormatterContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class AnotherTestOutputFormatter : OutputFormatter
        {
            public override Task WriteResponseBodyAsync([NotNull] OutputFormatterContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}
