// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using Newtonsoft.Json;

namespace Microsoft.AspNet.Mvc.Formatters.Json.Internal
{
    public class JsonBufferPool<T> : IJsonBufferPool<T>
    {
        private readonly ArrayPool<T> _inner;

        public JsonBufferPool(ArrayPool<T> inner)
        {
            if (inner == null)
            {
                throw new ArgumentNullException(nameof(inner));
            }

            _inner = inner;
        }

        public T[] RentBuffer(int minSize)
        {
            return _inner.Rent(minSize);
        }

        public void ReturnBuffer(ref T[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            _inner.Return(buffer);
            buffer = null;
        }
    }
}
