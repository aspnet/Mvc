﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Razor.Text;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class StringTextBuffer : ITextBuffer, IDisposable
    {
        private string _buffer;
        public bool Disposed { get; set; }

        public StringTextBuffer(string buffer)
        {
            _buffer = buffer;
        }

        public int Length
        {
            get { return _buffer.Length; }
        }

        public int Position { get; set; }

        public int Read()
        {
            if (Position >= _buffer.Length)
            {
                return -1;
            }
            return _buffer[Position++];
        }

        public int Peek()
        {
            if (Position >= _buffer.Length)
            {
                return -1;
            }
            return _buffer[Position];
        }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}