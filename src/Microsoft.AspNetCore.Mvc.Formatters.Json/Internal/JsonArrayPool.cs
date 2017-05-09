// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.Mvc.Formatters.Json.Internal
{
    public class JsonArrayPool<T> : IArrayPool<T>
    {
        private readonly ArrayPool<T> _inner;
        private readonly List<T[]> _arrayTracker;

        public JsonArrayPool(ArrayPool<T> inner)
        {
            if (inner == null)
            {
                throw new ArgumentNullException(nameof(inner));
            }

            _inner = inner;
            _arrayTracker = new List<T[]>();
        }

        public T[] Rent(int minimumLength)
        {
            var array = _inner.Rent(minimumLength);
            _arrayTracker.Add(array);

            return array;
        }

        public void Return(T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (_arrayTracker.Contains(array))
            {
                _arrayTracker.Remove(array);
                _inner.Return(array);
            }
        }
    }
}
