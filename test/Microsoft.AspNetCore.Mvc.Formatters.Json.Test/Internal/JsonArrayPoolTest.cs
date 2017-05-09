// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Buffers;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Formatters.Json.Internal
{
    public class JsonArrayPoolTest
    {
        [Fact]
        public void Return_DoesNotCallInner_IfReturnedArrayWasNotRented()
        {
            // Arrange
            var mockArrayPool = new Mock<ArrayPool<char>>();
            var pool = new JsonArrayPool<char>(mockArrayPool.Object);
            var myarray = new char[50];

            // Act
            var array = pool.Rent(35);
            pool.Return(myarray);

            //Assert
            mockArrayPool.Verify(v => v.Return(It.IsAny<char[]>(), false), Times.Never());
        }
    }
}
