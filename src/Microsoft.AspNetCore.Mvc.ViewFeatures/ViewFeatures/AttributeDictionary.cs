// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    /// <summary>
    /// A dictionary for HTML attributes.
    /// </summary>
    public class AttributeDictionary : IDictionary<string, string>, IReadOnlyDictionary<string, string>
    {
        private readonly AttributeValuesDictionary _innerDictionary;

        // Legacy constructor for tests.
        public AttributeDictionary()
            : this(new AttributeValuesDictionary())
        {
        }

        public AttributeDictionary(AttributeValuesDictionary innerDictionary)
        {
            if (innerDictionary == null)
            {
                throw new ArgumentNullException(nameof(innerDictionary));
            }

            _innerDictionary = innerDictionary;
        }

        /// <inheritdoc />
        public string this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                var value = _innerDictionary[key];
                return value.ToString();
            }

            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                _innerDictionary[key] = value;
            }
        }

        /// <inheritdoc />
        public int Count => _innerDictionary.Count;

        /// <inheritdoc />
        public bool IsReadOnly { get; } = false;

        /// <inheritdoc />
        public ICollection<string> Keys
        {
            get
            {
                return _innerDictionary.Keys;
            }
        }

        /// <inheritdoc />
        public ICollection<string> Values
        {
            get
            {
                return new ValueCollection(_innerDictionary);
            }
        }

        /// <inheritdoc />
        IEnumerable<string> IReadOnlyDictionary<string, string>.Keys
        {
            get
            {
                return _innerDictionary.Keys;
            }
        }

        /// <inheritdoc />
        IEnumerable<string> IReadOnlyDictionary<string, string>.Values
        {
            get
            {
                return new ValueCollection(_innerDictionary);
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            _innerDictionary.Clear();
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<string, string> item)
        {
            if (item.Key == null)
            {
                throw new ArgumentException(
                    Resources.FormatPropertyOfTypeCannotBeNull(
                        nameof(KeyValuePair<string, string>.Key),
                        nameof(KeyValuePair<string, string>)),
                    nameof(item));
            }

            _innerDictionary.Add(item.Key, item.Value);
        }

        /// <inheritdoc />
        public void Add(string key, string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _innerDictionary.Add(key, value);
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, string> item)
        {
            if (item.Key == null)
            {
                throw new ArgumentException(
                    Resources.FormatPropertyOfTypeCannotBeNull(
                        nameof(KeyValuePair<string, string>.Key),
                        nameof(KeyValuePair<string, string>)),
                    nameof(item));
            }

            var innerItem = new KeyValuePair<string, StringValuesTutu>(item.Key, item.Value);
            return _innerDictionary.Contains(innerItem);
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _innerDictionary.ContainsKey(key);
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0 || arrayIndex >= array.Length)
            {
                throw new IndexOutOfRangeException();
            }

            foreach (var innerItem in _innerDictionary)
            {
                var item = new KeyValuePair<string, string>(innerItem.Key, innerItem.Value.ToString());
                array[arrayIndex++] = item;
            }
        }

        /// <inheritdoc />
        public Enumerator GetEnumerator()
        {
            return new Enumerator(_innerDictionary);
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, string> item)
        {
            if (item.Key == null)
            {
                throw new ArgumentException(
                    Resources.FormatPropertyOfTypeCannotBeNull(
                        nameof(KeyValuePair<string, string>.Key),
                        nameof(KeyValuePair<string, string>)),
                    nameof(item));
            }

            var innerItem = new KeyValuePair<string, StringValuesTutu>(item.Key, item.Value);
            return _innerDictionary.Remove(innerItem);
        }

        /// <inheritdoc />
        public bool Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _innerDictionary.Remove(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            StringValuesTutu stringValues;
            if (_innerDictionary.TryGetValue(key, out stringValues))
            {
                value = stringValues.ToString();
                return true;
            }

            value = null;
            return false;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// An enumerator for <see cref="AttributeDictionary"/>.
        /// </summary>
        public struct Enumerator : IEnumerator<KeyValuePair<string, string>>
        {
            private AttributeValuesDictionary.Enumerator _innerEnumerator;

            /// <summary>
            /// Creates a new <see cref="Enumerator"/>.
            /// </summary>
            /// <param name="attributes">The <see cref="AttributeValuesDictionary"/>.</param>
            public Enumerator(AttributeValuesDictionary attributes)
            {
                _innerEnumerator = attributes.GetEnumerator();
            }

            /// <inheritdoc />
            public KeyValuePair<string, string> Current
            {
                get
                {
                    var innerItem = _innerEnumerator.Current;
                    return new KeyValuePair<string, string>(innerItem.Key, innerItem.Value.ToString());
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
                _innerEnumerator.Dispose();
            }

            /// <inheritdoc />
            public bool MoveNext()
            {
                return _innerEnumerator.MoveNext();
            }

            /// <inheritdoc />
            public void Reset()
            {
                _innerEnumerator.Reset();
            }
        }

        private class ValueCollection : ICollection<string>
        {
            private readonly AttributeValuesDictionary _attributes;

            public ValueCollection(AttributeValuesDictionary attributes)
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
                foreach (var value in _attributes.Values)
                {
                    if (string.Equals(item, value.ToString(), StringComparison.OrdinalIgnoreCase))
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

                foreach (var value in _attributes.Values)
                {
                    array[arrayIndex++] = value.ToString();
                }
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(_attributes);
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
                private IEnumerator<StringValuesTutu> _innerEnumerator;

                public Enumerator(AttributeValuesDictionary attributes)
                {
                    _innerEnumerator = attributes.Values.GetEnumerator();
                }

                public string Current
                {
                    get
                    {
                        var innerValue = _innerEnumerator.Current;
                        return innerValue.ToString();
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
                    _innerEnumerator.Dispose();
                }

                public bool MoveNext()
                {
                    return _innerEnumerator.MoveNext();
                }

                public void Reset()
                {
                    _innerEnumerator.Reset();
                }
            }
        }
    }
}
