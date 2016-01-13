// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.IO;

namespace Microsoft.AspNet.Mvc.ViewFeatures.Buffer
{
    public class TestViewBufferScope : IViewBufferScope
    {
        public const int DefaultBufferSize = 128;
        private readonly int _bufferSize;

        public TestViewBufferScope(int bufferSize = DefaultBufferSize)
        {
            _bufferSize = bufferSize;
        }

        public ViewBufferValue[] GetSegment() => new ViewBufferValue[_bufferSize];

        public void ReturnSegment(ViewBufferValue[] segment)
        {
        }

        public ViewBufferTextWriter CreateWriter(TextWriter writer)
        {
            return new ViewBufferTextWriter(ArrayPool<char>.Shared, writer);
        }
    }
}
