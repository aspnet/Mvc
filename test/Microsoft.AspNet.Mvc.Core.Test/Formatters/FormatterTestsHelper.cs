// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using Microsoft.AspNet.Http;
using Moq;

namespace Microsoft.AspNet.Mvc
{
    internal static class FormatterTestsHelper
    {
        internal static Mock<HttpResponse> GetMockResponseWhichThrowsOnHeadersBeingSetAfterWritingBody()
        {
            var wasStreamWritten = false;
            var response = new Mock<HttpResponse>();
            var mockStream = new Mock<Stream>();
            mockStream.Setup(m => m.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Callback(() =>
                {
                    wasStreamWritten = true;
                });
            mockStream.Setup(
                m => m.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Callback(() =>
                {
                    wasStreamWritten = true;
                });
            response.SetupGet(f => f.Body).Returns(mockStream.Object);
            response.Setup(r => r.Headers.Set(It.IsAny<string>(), It.IsAny<string>())).Callback(() =>
            {
                if (wasStreamWritten)
                {
                    throw new InvalidOperationException("Headers cannot be set after body is written.");
                }
            });

            return response;
        }
    }
}