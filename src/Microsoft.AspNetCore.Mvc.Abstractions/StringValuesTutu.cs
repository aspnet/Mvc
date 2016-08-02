// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Represents zero/null, one, or many strings in an efficient way.
    /// </summary>
    /// <remarks>
    /// A variant of <see cref="StringValues"/> that converts back to a <see cref="string"/> without additional commas
    /// between values. In addition, this <c>struct</c> lacks an implicit conversion to <see cref="string"/> and the
    /// explicit and implicit conversions it has never return <c>null</c>.
    /// </remarks>
    public struct StringValuesTutu
        : IList<string>, IReadOnlyList<string>, IEquatable<StringValuesTutu>, IEquatable<string>, IEquatable<string[]>
    {
        private static readonly string[] EmptyArray = new string[0];
        public static readonly StringValuesTutu Empty = new StringValuesTutu(EmptyArray);

        private readonly string _value;
        private readonly string[] _values;

        ////private string _concatenatedValue;

        public StringValuesTutu(string value)
        {
            _value = value;
            _values = null;

            ////_concatenatedValue = null;
        }

        public StringValuesTutu(string[] values)
        {
            _value = null;
            _values = values;

            ////_concatenatedValue = null;
        }

        public static implicit operator StringValuesTutu(string value)
        {
            return new StringValuesTutu(value);
        }

        public static implicit operator StringValuesTutu(string[] values)
        {
            return new StringValuesTutu(values);
        }

        public int Count => _value != null ? 1 : (_values?.Length ?? 0);

        bool ICollection<string>.IsReadOnly => true;

        string IList<string>.this[int index]
        {
            get { return this[index]; }
            set { throw new NotSupportedException(); }
        }

        public string this[int index]
        {
            get
            {
                if (_values != null)
                {
                    return _values[index]; // may throw
                }

                if (index == 0 && _value != null)
                {
                    return _value;
                }

                return EmptyArray[index]; // throws
            }
        }

        public override string ToString()
        {
            return GetStringValue() ?? string.Empty;
        }

        private string GetStringValue()
        {
            if (_values == null)
            {
                return _value;
            }

            switch (_values.Length)
            {
                case 0:
                    return null;
                case 1:
                    return _values[0];
                default:
                    ////if (_concatenatedValue == null)
                    ////{
                    ////    _concatenatedValue = string.Concat(_values);
                    ////}

                    return string.Concat(_values);
            }
        }

        public string[] ToArray()
        {
            return GetArrayValue() ?? EmptyArray;
        }

        private string[] GetArrayValue()
        {
            if (_value != null)
            {
                return new[] { _value };
            }

            return _values;
        }

        int IList<string>.IndexOf(string item)
        {
            return IndexOf(item, startIndex: 0);
        }

        public int IndexOf(string item, int startIndex)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (startIndex < 0 || startIndex > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), "ArgumentOutOfRange_Index");
            }

            if (_values != null)
            {
                var values = _values;
                for (int i = startIndex; i < values.Length; i++)
                {
                    if (string.Equals(values[i], item, StringComparison.Ordinal))
                    {
                        return i;
                    }
                }

                return -1;
            }

            if (_value != null && startIndex == 0)
            {
                return string.Equals(_value, item, StringComparison.Ordinal) ? 0 : -1;
            }

            return -1;
        }

        bool ICollection<string>.Contains(string item)
        {
            return IndexOf(item, startIndex: 0) >= 0;
        }

        void ICollection<string>.CopyTo(string[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex);
        }

        private void CopyTo(string[] array, int arrayIndex)
        {
            if (_values != null)
            {
                Array.Copy(_values, 0, array, arrayIndex, _values.Length);
                return;
            }

            if (_value != null)
            {
                if (array == null)
                {
                    throw new ArgumentNullException(nameof(array));
                }

                if (arrayIndex < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(arrayIndex));
                }

                if (array.Length - arrayIndex < 1)
                {
                    throw new ArgumentException(
                        $"'{nameof(array)}' is not long enough to copy all the items in the collection. Check '{nameof(arrayIndex)}' and '{nameof(array)}' length.");
                }

                array[arrayIndex] = _value;
            }
        }

        void ICollection<string>.Add(string item)
        {
            throw new NotSupportedException();
        }

        void IList<string>.Insert(int index, string item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<string>.Remove(string item)
        {
            throw new NotSupportedException();
        }

        void IList<string>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        void ICollection<string>.Clear()
        {
            throw new NotSupportedException();
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(ref this);
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static bool IsNullOrEmpty(StringValuesTutu value)
        {
            if (value._values == null)
            {
                return string.IsNullOrEmpty(value._value);
            }

            switch (value._values.Length)
            {
                case 0: return true;
                case 1: return string.IsNullOrEmpty(value._values[0]);
                default: return false;
            }
        }

        public static StringValuesTutu Concat(string value1, StringValuesTutu values)
        {
            if (string.IsNullOrEmpty(value1))
            {
                return values;
            }

            var count = values.Count;
            if (count == 0)
            {
                return new StringValuesTutu(value1);
            }

            var combined = new string[1 + count];
            combined[0] = value1;
            values.CopyTo(combined, arrayIndex: 1);

            return new StringValuesTutu(combined);
        }

        public static StringValuesTutu Concat(string value1, string value2, StringValuesTutu values)
        {
            if (string.IsNullOrEmpty(value1))
            {
                return Concat(value2, values);
            }

            var count = values.Count;
            if (count == 0)
            {
                return new StringValuesTutu(new[] { value1, value2 });
            }

            var combined = new string[2 + count];
            combined[0] = value1;
            combined[1] = value2;
            values.CopyTo(combined, arrayIndex: 2);

            return new StringValuesTutu(combined);
        }

        public static StringValuesTutu Concat(string value1, string value2, string value3, StringValuesTutu values)
        {
            if (string.IsNullOrEmpty(value1))
            {
                return Concat(value2, value3, values);
            }

            var count = values.Count;
            if (count == 0)
            {
                return new StringValuesTutu(new[] { value1, value2, value3 });
            }

            var combined = new string[3 + count];
            combined[0] = value1;
            combined[1] = value2;
            combined[2] = value3;
            values.CopyTo(combined, arrayIndex: 3);

            return new StringValuesTutu(combined);
        }

        public static StringValuesTutu Concat(StringValuesTutu values, string value1)
        {
            if (string.IsNullOrEmpty(value1))
            {
                return values;
            }

            var count = values.Count;
            if (count == 0)
            {
                return new StringValuesTutu(value1);
            }

            var combined = new string[count + 1];
            combined[0] = value1;
            values.CopyTo(combined, arrayIndex: 1);

            return new StringValuesTutu(combined);
        }

        public static StringValuesTutu Concat(StringValuesTutu values, string value1, string value2)
        {
            if (string.IsNullOrEmpty(value1))
            {
                return Concat(values, value2);
            }

            var count = values.Count;
            if (count == 0)
            {
                return new StringValuesTutu(new[] { value1, value2 });
            }

            var combined = new string[count + 2];
            combined[0] = value1;
            combined[1] = value2;
            values.CopyTo(combined, arrayIndex: 2);

            return new StringValuesTutu(combined);
        }

        public static StringValuesTutu Concat(StringValuesTutu values, string value1, string value2, string value3)
        {
            if (string.IsNullOrEmpty(value1))
            {
                return Concat(values, value2, value3);
            }

            var count = values.Count;
            if (count == 0)
            {
                return new StringValuesTutu(new[] { value1, value2, value3 });
            }

            var combined = new string[count + 3];
            combined[0] = value1;
            combined[1] = value2;
            combined[2] = value3;
            values.CopyTo(combined, arrayIndex: 3);

            return new StringValuesTutu(combined);
        }

        public static StringValuesTutu Concat(StringValuesTutu values, params string[] paramValues)
        {
            return Concat(values, new StringValuesTutu(paramValues));
        }

        public static StringValuesTutu Concat(StringValuesTutu values1, StringValuesTutu values2)
        {
            var count1 = values1.Count;
            if (count1 == 0)
            {
                return values2;
            }

            var count2 = values2.Count;
            if (count2 == 0)
            {
                return values1;
            }

            var combined = new string[count1 + count2];
            values1.CopyTo(combined, 0);
            values2.CopyTo(combined, count1);

            return new StringValuesTutu(combined);
        }

        public static StringValuesTutu Concat(StringValuesTutu values1, string value1, StringValuesTutu values2)
        {
            if (string.IsNullOrEmpty(value1))
            {
                return Concat(values1, values2);
            }

            var count1 = values1.Count;
            if (count1 == 0)
            {
                return Concat(value1, values2);
            }

            var count2 = values2.Count;
            if (count2 == 0)
            {
                return Concat(values1, value1);
            }

            var combined = new string[count1 + 1 + count2];
            values1.CopyTo(combined, 0);
            combined[count1] = value1;
            values2.CopyTo(combined, count1 + 1);

            return new StringValuesTutu(combined);
        }

        public StringValuesTutu Substring(int startIndex)
        {
            return Substring(startIndex, Count - startIndex);
        }

        public StringValuesTutu Substring(int startIndex, int length)
        {
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), "ArgumentOutOfRange_StartIndex");
            }

            if (startIndex > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), "ArgumentOutOfRange_StartIndexLargerThanLength");
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "ArgumentOutOfRange_NegativeLength");
            }

            if (startIndex > Count - length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "ArgumentOutOfRange_IndexLength");
            }

            if (length == 0)
            {
                return Empty;
            }

            if (startIndex == 0 && length == Count)
            {
                return this;
            }

            Debug.Assert(_values != null, $"Attempt to Substring an uninitialized {typeof(StringValuesTutu)}.");

            var substring = new string[length];
            var i = 0;
            for (var j = startIndex; length > 0; i++, j++, length--)
            {
                substring[i] = _values[j];
            }

            return new StringValuesTutu(substring);
        }

        public static bool Equals(StringValuesTutu left, StringValuesTutu right)
        {
            return Equals(left, right, StringComparison.Ordinal);
        }

        // Ignore value boundaries to determine equality.
        public static bool Equals(StringValuesTutu left, StringValuesTutu right, StringComparison comparisonType)
        {
            var leftIndex = 0;
            var rightIndex = 0;
            var leftOffset = 0;
            var rightOffset = 0;
            while (leftIndex < left.Count && rightIndex < right.Count)
            {
                var leftString = left[leftIndex];
                var rightString = right[rightIndex];
                var length = Math.Min(leftString.Length - leftOffset, rightString.Length - rightOffset);
                if (string.Compare(leftString, leftOffset, rightString, rightOffset, length, comparisonType) != 0)
                {
                    return false;
                }

                if (leftOffset + length == leftString.Length)
                {
                    leftIndex++;
                    leftOffset = 0;
                }
                else
                {
                    leftOffset += length;
                }

                if (rightOffset + length == rightString.Length)
                {
                    rightIndex++;
                    rightOffset = 0;
                }
                else
                {
                    rightOffset += length;
                }
            }

            if (leftIndex != left.Count || leftOffset != 0)
            {
                // Something left in left.
                return false;
            }

            if (rightIndex != right.Count || rightOffset != 0)
            {
                // Something left in right.
                return false;
            }

            return true;
        }

        public static bool operator ==(StringValuesTutu left, StringValuesTutu right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(StringValuesTutu left, StringValuesTutu right)
        {
            return !Equals(left, right);
        }

        public bool Equals(StringValuesTutu other)
        {
            return Equals(this, other);
        }

        public static bool Equals(string left, StringValuesTutu right)
        {
            return Equals(new StringValuesTutu(left), right);
        }

        public static bool Equals(StringValuesTutu left, string right)
        {
            return Equals(left, new StringValuesTutu(right));
        }

        public bool Equals(string other)
        {
            return Equals(this, new StringValuesTutu(other));
        }

        public static bool Equals(string[] left, StringValuesTutu right)
        {
            return Equals(new StringValuesTutu(left), right);
        }

        public static bool Equals(StringValuesTutu left, string[] right)
        {
            return Equals(left, new StringValuesTutu(right));
        }

        public bool Equals(string[] other)
        {
            return Equals(this, new StringValuesTutu(other));
        }

        public static bool operator ==(StringValuesTutu left, string right)
        {
            return Equals(left, new StringValuesTutu(right));
        }

        public static bool operator !=(StringValuesTutu left, string right)
        {
            return !Equals(left, new StringValuesTutu(right));
        }

        public static bool operator ==(string left, StringValuesTutu right)
        {
            return Equals(new StringValuesTutu(left), right);
        }

        public static bool operator !=(string left, StringValuesTutu right)
        {
            return !Equals(new StringValuesTutu(left), right);
        }

        public static bool operator ==(StringValuesTutu left, string[] right)
        {
            return Equals(left, new StringValuesTutu(right));
        }

        public static bool operator !=(StringValuesTutu left, string[] right)
        {
            return !Equals(left, new StringValuesTutu(right));
        }

        public static bool operator ==(string[] left, StringValuesTutu right)
        {
            return Equals(new StringValuesTutu(left), right);
        }

        public static bool operator !=(string[] left, StringValuesTutu right)
        {
            return !Equals(new StringValuesTutu(left), right);
        }

        public static bool operator ==(StringValuesTutu left, object right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StringValuesTutu left, object right)
        {
            return !left.Equals(right);
        }
        public static bool operator ==(object left, StringValuesTutu right)
        {
            return right.Equals(left);
        }

        public static bool operator !=(object left, StringValuesTutu right)
        {
            return !right.Equals(left);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return Equals(this, StringValuesTutu.Empty);
            }

            if (obj is string)
            {
                return Equals(this, (string)obj);
            }

            if (obj is string[])
            {
                return Equals(this, (string[])obj);
            }

            if (obj is StringValuesTutu)
            {
                return Equals(this, (StringValuesTutu)obj);
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (_values == null)
            {
                return _value == null ? 0 : _value.GetHashCode();
            }

            var hashCodeCombiner = HashCodeCombiner.Start();
            for (var i = 0; i < _values.Length; i++)
            {
                hashCodeCombiner.Add(_values[i]);
            }

            return hashCodeCombiner.CombinedHash;
        }

        public struct Enumerator : IEnumerator<string>
        {
            private readonly string[] _values;
            private string _current;
            private int _index;

            public Enumerator(ref StringValuesTutu values)
            {
                _values = values._values;
                _current = values._value;
                _index = 0;
            }

            public bool MoveNext()
            {
                if (_index < 0)
                {
                    return false;
                }

                if (_values != null)
                {
                    if (_index < _values.Length)
                    {
                        _current = _values[_index];
                        _index++;
                        return true;
                    }

                    _index = -1;
                    return false;
                }

                _index = -1; // sentinel value
                return _current != null;
            }

            public string Current => _current;

            object IEnumerator.Current => _current;

            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
            }
        }
    }
}
