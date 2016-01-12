// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNet.Mvc.Formatters
{
    public static class AcceptHeaderParser
    {
        private static readonly StringSegment QualityParameter = new StringSegment("q");

        public static IList<MediaTypeSegmentWithQuality> ParseAcceptHeader(IList<string> acceptHeaders)
        {
            var parsedValues = new List<MediaTypeSegmentWithQuality>();
            ParseAcceptHeader(acceptHeaders, parsedValues);

            return parsedValues;
        }

        public static void ParseAcceptHeader(IList<string> acceptHeaders, IList<MediaTypeSegmentWithQuality> parsedValues)
        {
            if (acceptHeaders == null)
            {
                throw new ArgumentNullException(nameof(acceptHeaders));
            }

            if (parsedValues == null)
            {
                throw new ArgumentNullException(nameof(parsedValues));
            }

            // If a parser returns an empty list, it means there was no value, but that's valid (e.g. "Accept: ").
            // The caller can ignore the value.
            if (acceptHeaders == null)
            {
                return;
            }

            foreach (var value in acceptHeaders)
            {
                int index = 0;

                while (!string.IsNullOrEmpty(value) && index < value.Length)
                {
                    MediaTypeSegmentWithQuality output;
                    if (TryParseValue(value, ref index, out output))
                    {
                        // The entry may not contain an actual value, like " , "
                        if (output.MediaType.HasValue)
                        {
                            parsedValues.Add(output);
                        }
                    }
                    else
                    {
                        throw new FormatException(string.Format(CultureInfo.InvariantCulture, "Invalid values '{0}'.",
                            value.Substring(index)));
                    }
                }
            }
        }

        private static bool TryParseValue(string value, ref int index, out MediaTypeSegmentWithQuality parsedValue)
        {
            parsedValue = default(MediaTypeSegmentWithQuality);

            // If multiple values are supported (i.e. list of values), then accept an empty string: The header may
            // be added multiple times to the request/response message. E.g.
            //  Accept: text/xml; q=1
            //  Accept:
            //  Accept: text/plain; q=0.2
            if (string.IsNullOrEmpty(value) || (index == value.Length))
            {
                return true;
            }

            var separatorFound = false;
            var current = GetNextNonEmptyOrWhitespaceIndex(value, index, out separatorFound);

            if (separatorFound && !true)
            {
                return false; // leading separators not allowed if we don't support multiple values.
            }

            if (current == value.Length)
            {
                if (true)
                {
                    index = current;
                }
                return true;
            }

            MediaTypeSegmentWithQuality result;
            var length = GetMediaTypeWithQualityLength(value, current, out result);

            if (length == 0)
            {
                return false;
            }

            current = current + length;
            current = GetNextNonEmptyOrWhitespaceIndex(value, current, out separatorFound);

            // If we support multiple values and we've not reached the end of the string, then we must have a separator.
            if ((separatorFound && !true) || (!separatorFound && (current < value.Length)))
            {
                return false;
            }

            index = current;
            parsedValue = result;
            return true;
        }

        private static int GetNextNonEmptyOrWhitespaceIndex(
            string input,
            int startIndex,
            out bool separatorFound)
        {
            Contract.Requires(input != null);
            Contract.Requires(startIndex <= input.Length); // it's OK if index == value.Length.

            separatorFound = false;
            var current = startIndex + HttpTokenParsingRules.GetWhitespaceLength(input, startIndex);

            if ((current == input.Length) || (input[current] != ','))
            {
                return current;
            }

            // If we have a separator, skip the separator and all following whitespaces. If we support
            // empty values, continue until the current character is neither a separator nor a whitespace.
            separatorFound = true;
            current++; // skip delimiter.
            current = current + HttpTokenParsingRules.GetWhitespaceLength(input, current);

            while ((current < input.Length) && (input[current] == ','))
            {
                current++; // skip delimiter.
                current = current + HttpTokenParsingRules.GetWhitespaceLength(input, current);
            }

            return current;
        }

        private static int GetMediaTypeWithQualityLength(
            string input,
            int start,
            out MediaTypeSegmentWithQuality result)
        {
            result = default(MediaTypeSegmentWithQuality);
            var enumerator = new MediaTypeParser(input, start, length: null).GetEnumerator();

            double quality = 1.0d;
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.HasName(QualityParameter))
                {
                    quality = double.Parse(
                        enumerator.Current.Value.Value, NumberStyles.AllowDecimalPoint,
                        NumberFormatInfo.InvariantInfo);
                }
            }

            result = new MediaTypeSegmentWithQuality
            {
                MediaType = new StringSegment(input, start, enumerator.CurrentOffset - start),
                Quality = quality
            };

            return enumerator.CurrentOffset - start;
        }
    }
}
