// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNet.Mvc.Formatters
{
    /// <summary>
    /// A parser for media types that parses the media type without performing memory allocations.
    /// </summary>
    public struct MediaTypeParser : IEnumerable<MediaTypeComponent>
    {
        private readonly string _mediaType;
        private readonly int _offset;
        private readonly int? _length;

        /// <summary>
        /// Initializes a <see cref="MediaTypeParser"/> instance.
        /// </summary>
        /// <param name="mediaType">The <see cref="StringSegment"/> with the media type.</param>
        public MediaTypeParser(StringSegment mediaType)
            : this(mediaType.Buffer, mediaType.Offset, mediaType.Length)
        {
        }

        /// <summary>
        /// Initializes a <see cref="MediaTypeParser"/> instance.
        /// </summary>
        /// <param name="mediaType">The <see cref="string"/> with the media type.</param>
        /// <param name="offset">The offset in the <paramref name="mediaType"/> where the parsing starts.</param>
        /// <param name="length">The of the media type to parse if provided.</param>
        public MediaTypeParser(string mediaType, int offset, int? length)
        {
            if (mediaType == null)
            {
                throw new ArgumentNullException(nameof(mediaType));
            }

            if (offset < 0 || offset >= mediaType.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (length != null && offset + length > mediaType.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            _mediaType = mediaType;
            _offset = offset;
            _length = length;
        }

        /// <summary>
        /// Gets the parameter <paramref name="parameterName"/> of the media type.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to retrieve.</param>
        /// <returns>The <see cref="MediaTypeComponent"/>for the given <paramref name="parameterName"/> if found; otherwise<code>null</code>.</returns>
        public MediaTypeComponent? GetParameter(string parameterName)
        {
            return GetParameter(new StringSegment(parameterName));
        }

        /// <summary>
        /// Gets the parameter <paramref name="parameterName"/> of the media type.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to retrieve.</param>
        /// <returns>The <see cref="MediaTypeComponent"/>for the given <paramref name="parameterName"/> if found; otherwise<code>null</code>.</returns>
        public MediaTypeComponent? GetParameter(StringSegment parameterName)
        {
            var componentsEnumerator = GetEnumerator();

            if (!(componentsEnumerator.MoveNext() || componentsEnumerator.MoveNext()))
            {
                // Failed to parse media type.
                return null;
            }

            while (componentsEnumerator.MoveNext())
            {
                if (componentsEnumerator.Current.HasName(parameterName))
                {
                    return componentsEnumerator.Current;
                }
            }

            return null;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator<MediaTypeComponent> IEnumerable<MediaTypeComponent>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets an <see cref="Enumerator"/> that can be used to iterate
        /// over the <see cref="MediaTypeComponent"/> of the media type.
        /// </summary>
        /// <returns></returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(_mediaType, _offset, _length);
        }

        /// <summary>
        /// An iterator for media types that doesn't allocate memory.
        /// </summary>
        public struct Enumerator : IEnumerator<MediaTypeComponent>
        {
            private ParsingStatus _parsingStatus;
            private string _mediaType;
            private int _initialOffset;
            private int _currentOffset;
            private int? _length;
            private MediaTypeComponent _current;

            internal Enumerator(string mediaType, int offset, int? length)
            {
                _parsingStatus = ParsingStatus.NotStarted;
                _mediaType = mediaType;
                _initialOffset = offset;
                _length = length;
                _currentOffset = _initialOffset;
                _current = default(MediaTypeComponent);
            }

            /// <inheritdoc />
            public MediaTypeComponent Current
            {
                get
                {
                    if (_parsingStatus == ParsingStatus.NotStarted)
                    {
                        throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");
                    }
                    return _current;
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

            /// <summary>
            /// Gets the offset of the iteration over the current media type.
            /// </summary>
            public int CurrentOffset => _currentOffset;

            /// <summary>
            /// Gets whether the parsing failed or not.
            /// </summary>
            public bool ParsingFailed => _parsingStatus == ParsingStatus.Failed;

            /// <inheritdoc />
            public bool MoveNext()
            {
               switch (_parsingStatus)
                {
                    case ParsingStatus.NotStarted:
                        return ParseType();
                    case ParsingStatus.TypeParsed:
                        return ParseSubType();
                    case ParsingStatus.SubtypeParsed:
                        return ParseParameter();
                    case ParsingStatus.Finished:
                    case ParsingStatus.Failed:
                    default:
                        return false;
                }
            }

            /// <inheritdoc />
            public void Reset()
            {
                _currentOffset = _initialOffset;
                _parsingStatus = ParsingStatus.NotStarted;
                _current = default(MediaTypeComponent);
            }

            /// <inheritdoc />
            public void Dispose()
            {
            }

            private bool ParseType()
            {
                StringSegment type;
                var typeLength = GetTypeLength(
                    _mediaType,
                    _currentOffset,
                    out type);

                if (FailedToParse(typeLength, _length != null ? _initialOffset + _length : null))
                {
                    _parsingStatus = ParsingStatus.Failed;
                    return false;
                }
                else
                {
                    _current = new MediaTypeComponent(MediaTypeComponent.Type, type);
                    _currentOffset += typeLength;
                    _parsingStatus = ParsingStatus.TypeParsed;
                    return true;
                }
            }

            private bool ParseSubType()
            {
                StringSegment subType;
                var subTypeLength = GetSubtypeLength(
                    _mediaType,
                    _currentOffset,
                    out subType);

                if (FailedToParse(subTypeLength, _length != null ? _initialOffset + _length : null))
                {
                    _parsingStatus = ParsingStatus.Failed;
                    return false;
                }
                else
                {
                    _current = new MediaTypeComponent(MediaTypeComponent.Subtype, subType);
                    _currentOffset += subTypeLength;
                    _parsingStatus = ParsingStatus.SubtypeParsed;
                    return true;
                }
            }

            private bool ParseParameter()
            {
                if (_currentOffset < _mediaType.Length)
                {
                    MediaTypeComponent parameter;
                    int parameterLength = GetParameterLength(_mediaType, _currentOffset, out parameter);

                    _current = parameter;
                    _currentOffset = _currentOffset + parameterLength;

                    if (parameterLength == 0)
                    {
                        _parsingStatus = ParsingStatus.Failed;
                    }

                    return _parsingStatus != ParsingStatus.Failed;
                }
                else
                {
                    var outOfBoundary = _length != null && _currentOffset - _initialOffset != _length;
                    _parsingStatus = outOfBoundary ? ParsingStatus.Failed : ParsingStatus.Finished;

                    return false;
                }
            }

            private static bool FailedToParse(int mediaTypeLength, int? parsingBoundary)
            {
                return mediaTypeLength == 0 ||
                    (parsingBoundary != null && mediaTypeLength > parsingBoundary);
            }

            // All GetXXXLength methods work in the same way. They expect to be on the right position for
            // the token they are parsing, for example, the beginning of the media type or the delimiter
            // from a previous token, like '/', ';' or '='.
            // Each method consumes the delimiter token if any, the leading whitespace, then the given token
            // itself, and finally the trailing whitespace.
            private static int GetTypeLength(string input, int offset, out StringSegment type)
            {
                if (string.IsNullOrEmpty(input) || OffsetIsOutOfRange(offset, input.Length))
                {
                    type = default(StringSegment);
                    return 0;
                }

                // Parse the type, i.e. <type> in media type string "<type>/<subtype>; param1=value1; param2=value2"
                var typeLength = HttpTokenParsingRules.GetTokenLength(input, offset);

                if (typeLength == 0)
                {
                    type = default(StringSegment);
                    return 0;
                }

                type = new StringSegment(input, offset, typeLength);

                var current = offset + typeLength;
                current = current + HttpTokenParsingRules.GetWhitespaceLength(input, current);

                return current - offset;
            }

            private static int GetSubtypeLength(string input, int offset, out StringSegment subType)
            {
                var current = offset;
                // Parse the separator between type and subtype
                if (string.IsNullOrEmpty(input) || OffsetIsOutOfRange(current, input.Length) ||
                    (input[current] != '/'))
                {
                    subType = default(StringSegment);
                    return 0;
                }

                current++; // skip delimiter.
                current = current + HttpTokenParsingRules.GetWhitespaceLength(input, current);

                var subtypeLength = HttpTokenParsingRules.GetTokenLength(input, current);

                if (subtypeLength == 0)
                {
                    subType = default(StringSegment);
                    return 0;
                }

                subType = new StringSegment(input, current, subtypeLength);

                current = current + subtypeLength;
                current = current + HttpTokenParsingRules.GetWhitespaceLength(input, current);

                return current - offset;
            }

            private static int GetParameterLength(string input, int startIndex, out MediaTypeComponent parsedValue)
            {
                StringSegment name;
                var nameLength = GetNameLength(input, startIndex, out name);

                if (nameLength == 0)
                {
                    parsedValue = default(MediaTypeComponent);
                    return 0;
                }

                var current = startIndex + nameLength;

                StringSegment value;
                var valueLength = GetValueLength(input, current, out value);

                parsedValue = new MediaTypeComponent(name, value);

                current = current + valueLength;
                return current - startIndex;
            }

            private static int GetNameLength(string input, int startIndex, out StringSegment name)
            {
                if (string.IsNullOrEmpty(input) || OffsetIsOutOfRange(startIndex, input.Length) ||
                    input[startIndex] != ';')
                {
                    name = default(StringSegment);
                    return 0;
                }

                var current = startIndex;

                current++; // skip delimiter
                current = current + HttpTokenParsingRules.GetWhitespaceLength(input, current);

                var nameLength = HttpTokenParsingRules.GetTokenLength(input, current);

                if (nameLength == 0)
                {
                    name = default(StringSegment);
                    return 0;
                }

                name = new StringSegment(input, current, nameLength);

                current = current + nameLength;
                current = current + HttpTokenParsingRules.GetWhitespaceLength(input, current);
                return current - startIndex;
            }

            private static int GetValueLength(string input, int startIndex, out StringSegment value)
            {
                if (string.IsNullOrEmpty(input) || OffsetIsOutOfRange(startIndex, input.Length) ||
                    input[startIndex] != '=')
                {
                    value = default(StringSegment);
                    return 0;
                }

                var current = startIndex;

                current++; // skip delimiter.
                current = current + HttpTokenParsingRules.GetWhitespaceLength(input, current);

                var valueLength = HttpTokenParsingRules.GetTokenLength(input, current);

                if (valueLength == 0)
                {
                    // A value can either be a token or a quoted string. Check if it is a quoted string.
                    if (HttpTokenParsingRules.GetQuotedStringLength(input, current, out valueLength) != HttpParseResult.Parsed)
                    {
                        // We have an invalid value. Reset the name and return.
                        value = default(StringSegment);
                        return 0;
                    }
                }

                value = new StringSegment(input, current, valueLength);

                current = current + valueLength;
                current = current + HttpTokenParsingRules.GetWhitespaceLength(input, current);

                return current - startIndex;
            }

            private static bool OffsetIsOutOfRange(int offset, int length)
            {
                return offset < 0 || offset >= length;
            }

            private enum ParsingStatus
            {
                Failed,
                NotStarted,
                TypeParsed,
                SubtypeParsed,
                Finished
            }
        }
    }
}
