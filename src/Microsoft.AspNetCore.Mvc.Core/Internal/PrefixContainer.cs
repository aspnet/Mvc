// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    /// <summary>
    /// This is a container for prefix values. It normalizes all the values into dotted-form and then stores
    /// them in a sorted array. All queries for prefixes are also normalized to dotted-form, and searches
    /// for ContainsPrefix are done with a binary search.
    /// </summary>
    public class PrefixContainer
    {
        private readonly ICollection<string> _originalValues;
        private readonly string[] _sortedValues;

        public PrefixContainer(ICollection<string> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            _originalValues = values;
            _sortedValues = ToArrayWithoutNulls(_originalValues);
            Array.Sort(_sortedValues, StringComparer.OrdinalIgnoreCase);
        }

        public bool ContainsPrefix(string prefix)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            if (prefix.Length == 0)
            {
                return _sortedValues.Length > 0; // only match empty string when we have some value
            }

            return BinarySearch(prefix) > -1;
        }

        // Given "foo.bar", "foo.hello", "something.other", foo[abc].baz and asking for prefix "foo" will return:
        // - "bar"/"foo.bar"
        // - "hello"/"foo.hello"
        // - "abc"/"foo[abc]"
        public IDictionary<string, string> GetKeysFromPrefix(string prefix)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in _originalValues)
            {
                if (entry != null)
                {
                    if (entry.Length == prefix.Length)
                    {
                        // No key in this entry
                        continue;
                    }

                    if (prefix.Length == 0)
                    {
                        GetKeyFromEmptyPrefix(entry, result);
                    }
                    else if (entry.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        GetKeyFromNonEmptyPrefix(prefix, entry, result);
                    }
                }
            }

            return result;
        }

        private static void GetKeyFromEmptyPrefix(string entry, IDictionary<string, string> results)
        {
            string key;
            string fullName;
            var delimiterPosition = IndexOfDelimiter(entry, 0);

            if (delimiterPosition == 0 && entry[0] == '[')
            {
                // Handle an entry such as "[key]".
                var bracketPosition = entry.IndexOf(']', 1);
                if (bracketPosition == -1)
                {
                    // Malformed for dictionary.
                    return;
                }

                key = entry.Substring(1, bracketPosition - 1);
                fullName = entry.Substring(0, bracketPosition + 1);
            }
            else
            {
                // Handle an entry such as "key", "key.property" and "key[index]".
                key = delimiterPosition == -1 ? entry : entry.Substring(0, delimiterPosition);
                fullName = key;
            }

            if (!results.ContainsKey(key))
            {
                results.Add(key, fullName);
            }
        }

        private static void GetKeyFromNonEmptyPrefix(string prefix, string entry, IDictionary<string, string> results)
        {
            string key;
            string fullName;
            var keyPosition = prefix.Length + 1;

            switch (entry[prefix.Length])
            {
                case '.':
                    // Handle an entry such as "prefix.key", "prefix.key.property" and "prefix.key[index]".
                    var delimiterPosition = IndexOfDelimiter(entry, keyPosition);
                    if (delimiterPosition == -1)
                    {
                        // Neither '.' nor '[' found later in the name. Use rest of the string.
                        key = entry.Substring(keyPosition);
                        fullName = entry;
                    }
                    else
                    {
                        key = entry.Substring(keyPosition, delimiterPosition - keyPosition);
                        fullName = entry.Substring(0, delimiterPosition);
                    }
                    break;

                case '[':
                    // Handle an entry such as "prefix[key]".
                    var bracketPosition = entry.IndexOf(']', keyPosition);
                    if (bracketPosition == -1)
                    {
                        // Malformed for dictionary
                        return;
                    }

                    key = entry.Substring(keyPosition, bracketPosition - keyPosition);
                    fullName = entry.Substring(0, bracketPosition + 1);
                    break;

                default:
                    // Ignore an entry such as "prefixA".
                    return;
            }

            if (!results.ContainsKey(key))
            {
                results.Add(key, fullName);
            }
        }

        public static bool IsPrefixMatch(string prefix, string testString)
        {
            if (testString == null)
            {
                return false;
            }

            if (prefix.Length == 0)
            {
                return true; // shortcut - non-null testString matches empty prefix
            }

            if (prefix.Length > testString.Length)
            {
                return false; // not long enough
            }

            if (!testString.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return false; // prefix doesn't match
            }

            if (testString.Length == prefix.Length)
            {
                return true; // exact match
            }

            // invariant: testString.Length > prefix.Length
            switch (testString[prefix.Length])
            {
                case '.':
                case '[':
                    return true; // known delimiters

                default:
                    return false; // not known delimiter
            }
        }

        private static int IndexOfDelimiter(string entry, int startIndex)
        {
            int delimiterPosition;
            var bracketPosition = entry.IndexOf('[', startIndex);
            var dotPosition = entry.IndexOf('.', startIndex);

            if (dotPosition == -1)
            {
                delimiterPosition = bracketPosition;
            }
            else if (bracketPosition == -1)
            {
                delimiterPosition = dotPosition;
            }
            else
            {
                delimiterPosition = Math.Min(dotPosition, bracketPosition);
            }

            return delimiterPosition;
        }

        /// <summary>
        /// Convert an ICollection to an array, removing null values. Fast path for case where
        /// there are no null values.
        /// </summary>
        private static TElement[] ToArrayWithoutNulls<TElement>(ICollection<TElement> collection) where TElement : class
        {
            Debug.Assert(collection != null);

            var result = new TElement[collection.Count];
            var count = 0;
            foreach (TElement value in collection)
            {
                if (value != null)
                {
                    result[count] = value;
                    count++;
                }
            }
            if (count == collection.Count)
            {
                return result;
            }
            else
            {
                var trimmedResult = new TElement[count];
                Array.Copy(result, trimmedResult, count);
                return trimmedResult;
            }
        }

        private int BinarySearch(string prefix)
        {
            var start = 0;
            var end = _sortedValues.Length - 1;

            while (start <= end)
            {
                var pivot = start + (end - start >> 1);
                var candidate = _sortedValues[pivot];
                var compare = string.Compare(
                    prefix,
                    0,
                    candidate,
                    0,
                    prefix.Length,
                    StringComparison.OrdinalIgnoreCase);
                if (compare == 0)
                {
                    // Ok, so now we have a candiate that for which candidate.StartsWith(prefix) is
                    // at least true. If the candidate is longer than the prefix, we need to look 
                    // at the next character and see if it's a delimiter.
                    if (candidate.Length == prefix.Length)
                    {
                        // Exact match
                        return pivot;
                    }

                    var c = candidate[prefix.Length];
                    if (c == '.' || c == '[')
                    {
                        // Match, followed by delimiter
                        return pivot;
                    }

                    // Ok, so the candidate has some extra text. We need to keep searching, but we know
                    // the candidate string is considered "greater" than the prefix, so treat it as-if
                    // the comparer returned a negative number.
                    //
                    // Ex:
                    //  prefix: product
                    //  candidate: productId
                    //
                    compare = -1;
                }

                if (compare > 0)
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
    }
}
