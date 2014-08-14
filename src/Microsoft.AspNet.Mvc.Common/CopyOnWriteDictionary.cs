// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// A <see cref="IDictionary{string, TValue}"/> that defers creating a shallow copy of the source dictionary until
    ///  a mutative operation has been performed on it.
    /// </summary>
    internal class CopyOnWriteDictionary<TValue> : IDictionary<string, TValue>
    {
        private readonly IDictionary<string, TValue> _sourceDictionary;
        private IDictionary<string, TValue> _innerDictionary;

        public CopyOnWriteDictionary([NotNull] IDictionary<string, TValue> sourceDictionary)
        {
            _sourceDictionary = sourceDictionary;
        }

        private IDictionary<string, TValue> ReadDictionary
        {
            get
            {
                return _innerDictionary ?? _sourceDictionary;
            }
        }

        private IDictionary<string, TValue> WriteDictionary
        {
            get
            {
                if (_innerDictionary == null)
                {
                    _innerDictionary = new Dictionary<string, TValue>(_sourceDictionary,
                                                                      StringComparer.OrdinalIgnoreCase);
                }

                return _innerDictionary;
            }
        }

        public virtual ICollection<string> Keys
        {
            get
            {
                return ReadDictionary.Keys;
            }
        }

        public virtual ICollection<TValue> Values
        {
            get
            {
                return ReadDictionary.Values;
            }
        }

        public virtual int Count
        {
            get
            {
                return ReadDictionary.Count;
            }
        }

        public virtual bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public virtual TValue this[[NotNull] string key]
        {
            get
            {
                return ReadDictionary[key];
            }
            set
            {
                WriteDictionary[key] = value;
            }
        }

        public virtual bool ContainsKey([NotNull] string key)
        {
            return ReadDictionary.ContainsKey(key);
        }

        public virtual void Add([NotNull] string key, TValue value)
        {
            WriteDictionary.Add(key, value);
        }

        public virtual bool Remove([NotNull] string key)
        {
            return WriteDictionary.Remove(key);
        }

        public virtual bool TryGetValue([NotNull] string key, out TValue value)
        {
            return ReadDictionary.TryGetValue(key, out value);
        }

        public virtual void Add(KeyValuePair<string, TValue> item)
        {
            WriteDictionary.Add(item);
        }

        public virtual void Clear()
        {
            WriteDictionary.Clear();
        }

        public virtual bool Contains(KeyValuePair<string, TValue> item)
        {
            return ReadDictionary.Contains(item);
        }

        public virtual void CopyTo([NotNull] KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            ReadDictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, TValue> item)
        {
            return WriteDictionary.Remove(item);
        }

        public virtual IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            return ReadDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}