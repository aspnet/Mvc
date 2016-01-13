// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.ViewFeatures.Buffer
{
    public class ViewBufferPage
    {
        public ViewBufferPage(ViewBufferValue[] buffer)
        {
            Buffer = buffer;
        }

        public void Append(ViewBufferValue value)
        {
            if (IsFull)
            {
                throw new InvalidOperationException();
            }

            Buffer[Count++] = value;
        }

        public ViewBufferValue[] Buffer { get; }

        public int Capacity => Buffer.Length;

        public int Count { get; set; }

        public bool IsFull => Count == Capacity;
    }
}
