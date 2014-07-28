// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Represents a sequence of <see cref="BufferEntry"/> items and provides
    /// an enumerator that iterates over the sequence.
    /// </summary>
    public class BufferEntryCollection : IEnumerable<string>
    {
        private readonly List<BufferEntry> _buffer = new List<BufferEntry>();

        public List<BufferEntry> BufferEntries
        {
            get { return _buffer; }
        }

        /// <summary>
        /// Adds a string value to the buffer.
        /// </summary>
        /// <param name="value">The value to add.</param>
        public void Add(string value)
        {
            _buffer.Add(new BufferEntry { Value = value });
        }

        /// <summary>
        /// Adds a subarray of characters to the buffer.
        /// </summary>
        /// <param name="value">The array to add.</param>
        /// <param name="start">The character position in the array at which to start copying data.</param>
        /// <param name="length">The number of characters to copy.</param>
        public void Add([NotNull] char[] value, int start, int length)
        {
            var stringValue = new string(value, start, length);
            Add(stringValue);
        }

        /// <summary>
        /// Adds an instance of <see cref="BufferEntryCollection"/> to the buffer.
        /// </summary>
        /// <param name="buffer">The buffer collection to add.</param>
        public void Add([NotNull] BufferEntryCollection buffer)
        {
            _buffer.Add(new BufferEntry { Buffer = buffer });
        }

        /// <inheritdoc />
        public IEnumerator<string> GetEnumerator()
        {
            return new BufferEntryEnumerator(_buffer);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private sealed class BufferEntryEnumerator : IEnumerator<string>
        {
            private readonly Stack<IEnumerator<BufferEntry>> _enumerators = new Stack<IEnumerator<BufferEntry>>();
            private readonly List<BufferEntry> _initialBuffer;

            public BufferEntryEnumerator(List<BufferEntry> buffer)
            {
                _initialBuffer = buffer;
                Reset();
            }

            public IEnumerator<BufferEntry> CurrentEnumerator
            {
                get
                {
                    return _enumerators.Peek();
                }
            }

            public string Current
            {
                get
                {
                    var currentEnumerator = CurrentEnumerator;
                    Debug.Assert(currentEnumerator != null);

                    return currentEnumerator.Current.Value;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public bool MoveNext()
            {
                var currentEnumerator = CurrentEnumerator;
                if (currentEnumerator.MoveNext())
                {
                    // If the next item is a collection, recursively call in to it.
                    var buffer = currentEnumerator.Current.Buffer;
                    if (buffer != null)
                    {
                        var enumerator = buffer.BufferEntries.GetEnumerator();
                        _enumerators.Push(enumerator);
                        return MoveNext();
                    }

                    return true;
                }
                else if (_enumerators.Count > 1)
                {
                    // The current enumerator is exhausted and we have a parent.
                    // Pop the current enumerator out and continue with it's parent.
                    var enumerator = _enumerators.Pop();
                    enumerator.Dispose();

                    return MoveNext();
                }

                // We've exactly one element in our stack which cannot move next.
                return false;
            }

            public void Reset()
            {
                DisposeEnumerators();

                _enumerators.Clear();
                _enumerators.Push(_initialBuffer.GetEnumerator());
            }

            private void DisposeEnumerators()
            {
                foreach (var buffer in _enumerators)
                {
                    buffer.Dispose();
                }
            }

            public void Dispose()
            {
                DisposeEnumerators();
            }
        }
    }
}