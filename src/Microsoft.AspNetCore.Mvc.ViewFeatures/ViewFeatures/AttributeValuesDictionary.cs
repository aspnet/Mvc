// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    /// <summary>
    /// A dictionary for HTML attributes.
    /// </summary>
    public class AttributeValuesDictionary
        : IDictionary<string, StringValuesTutu>, IReadOnlyDictionary<string, StringValuesTutu>
    {
        private List<KeyValuePair<string, StringValuesTutu>> _items;

        /// <inheritdoc />
        public StringValuesTutu this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                var index = Find(key);
                if (index < 0)
                {
                    throw new KeyNotFoundException();
                }
                else
                {
                    return Get(index).Value;
                }
            }

            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                var item = new KeyValuePair<string, StringValuesTutu>(key, value);
                var index = Find(key);
                if (index < 0)
                {
                    Insert(~index, item);
                }
                else
                {
                    Set(index, item);
                }
            }
        }

        /// <inheritdoc />
        public int Count => _items == null ? 0 : _items.Count;

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc />
        public ICollection<string> Keys
        {
            get
            {
                return new KeyCollection(this);
            }
        }

        /// <inheritdoc />
        public ICollection<StringValuesTutu> Values
        {
            get
            {
                return new ValueCollection(this);
            }
        }

        /// <inheritdoc />
        IEnumerable<string> IReadOnlyDictionary<string, StringValuesTutu>.Keys
        {
            get
            {
                return new KeyCollection(this);
            }
        }

        /// <inheritdoc />
        IEnumerable<StringValuesTutu> IReadOnlyDictionary<string, StringValuesTutu>.Values
        {
            get
            {
                return new ValueCollection(this);
            }
        }

        private KeyValuePair<string, StringValuesTutu> Get(int index)
        {
            Debug.Assert(index >= 0 && index < Count);
            return _items[index];
        }

        private void Set(int index, KeyValuePair<string, StringValuesTutu> value)
        {
            Debug.Assert(index >= 0 && index <= Count);
            Debug.Assert(value.Key != null);

            if (_items == null)
            {
                _items = new List<KeyValuePair<string, StringValuesTutu>>();
            }

            _items[index] = value;
        }

        private void Insert(int index, KeyValuePair<string, StringValuesTutu> value)
        {
            Debug.Assert(index >= 0 && index <= Count);
            Debug.Assert(value.Key != null);

            if (_items == null)
            {
                _items = new List<KeyValuePair<string, StringValuesTutu>>();
            }

            _items.Insert(index, value);
        }

        private void Remove(int index)
        {
            Debug.Assert(index >= 0 && index < Count);

            _items.RemoveAt(index);
        }

        // This API is a lot like List<T>.BinarySearch https://msdn.microsoft.com/en-us/library/3f90y839(v=vs.110).aspx
        // If an item is not found, we return the compliment of where it belongs. Then we don't need to search again
        // to do something with it.
        private int Find(string key)
        {
            Debug.Assert(key != null);

            if (Count == 0)
            {
                return ~0;
            }

            var start = 0;
            var end = Count - 1;

            while (start <= end)
            {
                var pivot = start + (end - start >> 1);

                var compare = StringComparer.OrdinalIgnoreCase.Compare(Get(pivot).Key, key);
                if (compare == 0)
                {
                    return pivot;
                }
                if (compare < 0)
                {
                    start = pivot + 1;
                }
                else
                {
                    end = pivot - 1;
                }
            }

            return ~start;
        }

        /// <inheritdoc />
        public void Clear()
        {
            if (_items != null)
            {
                _items.Clear();
            }
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<string, StringValuesTutu> item)
        {
            if (item.Key == null)
            {
                throw new ArgumentException(
                    Resources.FormatPropertyOfTypeCannotBeNull(
                        nameof(KeyValuePair<string, StringValuesTutu>.Key),
                        nameof(KeyValuePair<string, StringValuesTutu>)),
                    nameof(item));
            }

            var index = Find(item.Key);
            if (index < 0)
            {
                Insert(~index, item);
            }
            else
            {
                throw new InvalidOperationException(Resources.FormatDictionary_DuplicateKey(item.Key));
            }
        }

        /// <inheritdoc />
        public void Add(string key, StringValuesTutu value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Add(new KeyValuePair<string, StringValuesTutu>(key, value));
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, StringValuesTutu> item)
        {
            if (item.Key == null)
            {
                throw new ArgumentException(
                    Resources.FormatPropertyOfTypeCannotBeNull(
                        nameof(KeyValuePair<string, StringValuesTutu>.Key),
                        nameof(KeyValuePair<string, StringValuesTutu>)),
                    nameof(item));
            }

            var index = Find(item.Key);
            if (index < 0)
            {
                return false;
            }
            else
            {
                return StringValuesTutu.Equals(item.Value, Get(index).Value, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (Count == 0)
            {
                return false;
            }

            return Find(key) >= 0;
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, StringValuesTutu>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0 || arrayIndex >= array.Length)
            {
                throw new IndexOutOfRangeException();
            }

            for (var i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = Get(i);
            }
        }

        /// <inheritdoc />
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, StringValuesTutu> item)
        {
            if (item.Key == null)
            {
                throw new ArgumentException(
                    Resources.FormatPropertyOfTypeCannotBeNull(
                        nameof(KeyValuePair<string, StringValuesTutu>.Key),
                        nameof(KeyValuePair<string, StringValuesTutu>)),
                    nameof(item));
            }

            var index = Find(item.Key);
            if (index < 0)
            {
                return false;
            }
            else if (StringValuesTutu.Equals(item.Value, Get(index).Value, StringComparison.OrdinalIgnoreCase))
            {
                Remove(index);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var index = Find(key);
            if (index < 0)
            {
                return false;
            }
            else
            {
                Remove(index);
                return true;
            }
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out StringValuesTutu value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var index = Find(key);
            if (index < 0)
            {
                value = StringValuesTutu.Empty;
                return false;
            }
            else
            {
                value = Get(index).Value;
                return true;
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator<KeyValuePair<string, StringValuesTutu>> IEnumerable<KeyValuePair<string, StringValuesTutu>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// An enumerator for <see cref="AttributeValuesDictionary"/>.
        /// </summary>
        public struct Enumerator : IEnumerator<KeyValuePair<string, StringValuesTutu>>
        {
            private AttributeValuesDictionary _attributes;
            private int _index;

            /// <summary>
            /// Creates a new <see cref="Enumerator"/>.
            /// </summary>
            /// <param name="attributes">The <see cref="AttributeValuesDictionary"/>.</param>
            public Enumerator(AttributeValuesDictionary attributes)
            {
                _attributes = attributes;
                _index = -1;
            }

            /// <inheritdoc />
            public KeyValuePair<string, StringValuesTutu> Current
            {
                get
                {
                    return _attributes.Get(_index);
                }
            }

            /// <inheritdoc />
            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {
            }

            /// <inheritdoc />
            public bool MoveNext()
            {
                _index++;
                return _index < _attributes.Count;
            }

            /// <inheritdoc />
            public void Reset()
            {
                _index = -1;
            }
        }

        private class KeyCollection : ICollection<string>
        {
            private readonly AttributeValuesDictionary _attributes;

            public KeyCollection(AttributeValuesDictionary attributes)
            {
                _attributes = attributes;
            }

            public int Count => _attributes.Count;

            public bool IsReadOnly => true;

            public void Add(string item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(string item)
            {
                if (item == null)
                {
                    throw new ArgumentNullException(nameof(item));
                }

                for (var i = 0; i < _attributes.Count; i++)
                {
                    if (string.Equals(item, _attributes.Get(i).Key, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                if (array == null)
                {
                    throw new ArgumentNullException(nameof(array));
                }

                if (arrayIndex < 0 || arrayIndex >= array.Length)
                {
                    throw new IndexOutOfRangeException();
                }

                for (var i = 0; i < _attributes.Count; i++)
                {
                    array[arrayIndex + i] = _attributes.Get(i).Key;
                }
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(this._attributes);
            }

            public bool Remove(string item)
            {
                throw new NotSupportedException();
            }

            IEnumerator<string> IEnumerable<string>.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public struct Enumerator : IEnumerator<string>
            {
                private AttributeValuesDictionary _attributes;
                private int _index;

                public Enumerator(AttributeValuesDictionary attributes)
                {
                    _attributes = attributes;
                    _index = -1;
                }

                public string Current
                {
                    get
                    {
                        return _attributes.Get(_index).Key;
                    }
                }

                object IEnumerator.Current
                {
                    get
                    {
                        return Current;
                    }
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    _index++;
                    return _index < _attributes.Count;
                }

                public void Reset()
                {
                    _index = -1;
                }
            }
        }

        private class ValueCollection : ICollection<StringValuesTutu>
        {
            private readonly AttributeValuesDictionary _attributes;

            public ValueCollection(AttributeValuesDictionary attributes)
            {
                _attributes = attributes;
            }

            public int Count => _attributes.Count;

            public bool IsReadOnly => true;

            public void Add(StringValuesTutu item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(StringValuesTutu item)
            {
                for (var i = 0; i < _attributes.Count; i++)
                {
                    if (StringValuesTutu.Equals(item, _attributes.Get(i).Value, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }

            public void CopyTo(StringValuesTutu[] array, int arrayIndex)
            {
                if (array == null)
                {
                    throw new ArgumentNullException(nameof(array));
                }

                if (arrayIndex < 0 || arrayIndex >= array.Length)
                {
                    throw new IndexOutOfRangeException();
                }

                for (var i = 0; i < _attributes.Count; i++)
                {
                    array[arrayIndex + i] = _attributes.Get(i).Value;
                }
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(this._attributes);
            }

            public bool Remove(StringValuesTutu item)
            {
                throw new NotSupportedException();
            }

            IEnumerator<StringValuesTutu> IEnumerable<StringValuesTutu>.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public struct Enumerator : IEnumerator<StringValuesTutu>
            {
                private AttributeValuesDictionary _attributes;
                private int _index;

                public Enumerator(AttributeValuesDictionary attributes)
                {
                    _attributes = attributes;
                    _index = -1;
                }

                public StringValuesTutu Current
                {
                    get
                    {
                        return _attributes.Get(_index).Value;
                    }
                }

                object IEnumerator.Current
                {
                    get
                    {
                        return Current;
                    }
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    _index++;
                    return _index < _attributes.Count;
                }

                public void Reset()
                {
                    _index = -1;
                }
            }
        }
    }
}
