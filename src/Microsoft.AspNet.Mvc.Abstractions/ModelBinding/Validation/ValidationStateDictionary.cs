﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.AspNet.Mvc.ModelBinding.Validation
{
    /// <summary>
    /// Used for tracking validation state to customize validation behavior for a model object.
    /// </summary>
    public class ValidationStateDictionary : 
        IDictionary<object, ValidationStateEntry>, 
        IReadOnlyDictionary<object, ValidationStateEntry>
    {
        private readonly Dictionary<object, ValidationStateEntry> _inner;
        
        /// <summary>
        /// Creates a new <see cref="ValidationStateDictionary"/>.
        /// </summary>
        public ValidationStateDictionary()
        {
            _inner = new Dictionary<object, ValidationStateEntry>(ReferenceEqualityComparer.Instance);
        }

        /// <inheritdoc />
        public ValidationStateEntry this[object key]
        {
            get
            {
                ValidationStateEntry entry;
                TryGetValue(key, out entry);
                return entry;
            }

            set
            {
                _inner[key] = value;
            }
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                return _inner.Count;
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get
            {
                return ((IDictionary<object, ValidationStateEntry>)_inner).IsReadOnly;
            }
        }

        /// <inheritdoc />
        public ICollection<object> Keys
        {
            get
            {
                return ((IDictionary<object, ValidationStateEntry>)_inner).Keys;
            }
        }

        /// <inheritdoc />
        public ICollection<ValidationStateEntry> Values
        {
            get
            {
                return ((IDictionary<object, ValidationStateEntry>)_inner).Values;
            }
        }

        /// <inheritdoc />
        IEnumerable<object> IReadOnlyDictionary<object, ValidationStateEntry>.Keys
        {
            get
            {
                return ((IReadOnlyDictionary<object, ValidationStateEntry>)_inner).Keys;
            }
        }

        /// <inheritdoc />
        IEnumerable<ValidationStateEntry> IReadOnlyDictionary<object, ValidationStateEntry>.Values
        {
            get
            {
                return ((IReadOnlyDictionary<object, ValidationStateEntry>)_inner).Values;
            }
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<object, ValidationStateEntry> item)
        {
            ((IDictionary<object, ValidationStateEntry>)_inner).Add(item);
        }

        /// <inheritdoc />
        public void Add(object key, ValidationStateEntry value)
        {
            _inner.Add(key, value);
        }

        /// <inheritdoc />
        public void Clear()
        {
            _inner.Clear();
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<object, ValidationStateEntry> item)
        {
            return ((IDictionary<object, ValidationStateEntry>)_inner).Contains(item);
        }

        /// <inheritdoc />
        public bool ContainsKey(object key)
        {
            return _inner.ContainsKey(key);
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<object, ValidationStateEntry>[] array, int arrayIndex)
        {
            ((IDictionary<object, ValidationStateEntry>)_inner).CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<object, ValidationStateEntry>> GetEnumerator()
        {
            return ((IDictionary<object, ValidationStateEntry>)_inner).GetEnumerator();
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<object, ValidationStateEntry> item)
        {
            return _inner.Remove(item);
        }

        /// <inheritdoc />
        public bool Remove(object key)
        {
            return _inner.Remove(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(object key, out ValidationStateEntry value)
        {
            return _inner.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<object, ValidationStateEntry>)_inner).GetEnumerator();
        }
        
        private class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

            public new bool Equals(object x, object y)
            {
                return Object.ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}