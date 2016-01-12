// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc.Formatters
{
    public static class MediaTypeComparisons
    {
        /// <summary>
        /// Determines if the <paramref name="subset" /> media type is a subset of the <paramref name="set" /> media type
        /// without taking into account the quality parameter.
        /// </summary>
        /// <param name="set">The more general media type.</param>
        /// <param name="subset">The more specific media type.</param>
        /// <returns><code>true</code> if <paramref name="set" />is a more general media type than <paramref name="subset"/>
        /// except for the quality parameter; otherwise <code>false</code>.</returns>
        public static bool IsSubsetOf(StringSegment set, StringSegment subset)
        {
            return IsSubsetOf(set, subset, ignoreQuality: true);
        }

        /// <summary>
        /// Determines if the <paramref name="subset" /> media type is a subset of the <paramref name="set" /> media type.
        /// Two media types are compatible if one is a subset of the other ignoring any charset
        /// parameter.
        /// </summary>
        /// <param name="set">The more general media type.</param>
        /// <param name="subset">The more specific media type.</param>
        /// <param name="ignoreQuality">Whether or not we should skip checking the quality parameter.</param>
        /// <returns><code>true</code> if <paramref name="set" />is a more general media type than <paramref name="subset"/>;
        /// otherwise <code>false</code>.</returns>
        public static bool IsSubsetOf(StringSegment set, StringSegment subset, bool ignoreQuality)
        {
            if (!set.HasValue || !subset.HasValue)
            {
                return false;
            }

            var setParser = new MediaTypeParser(set);
            var subSetParser = new MediaTypeParser(subset);

            var setEnumerator = setParser.GetEnumerator();
            var subSetEnumerator = subSetParser.GetEnumerator();

            if (!(setEnumerator.MoveNext() && subSetEnumerator.MoveNext()))
            {
                return false;
            }

            var setType = setEnumerator.Current;
            var subsetType = subSetEnumerator.Current;

            if (!setType.IsMatchesAll() && !setType.HasValue(subsetType.Value))
            {
                return false;
            }

            if (!(setEnumerator.MoveNext() && subSetEnumerator.MoveNext()))
            {
                return false;
            }

            var setSubType = setEnumerator.Current;
            var subsetSubType = subSetEnumerator.Current;

            if (!setSubType.IsMatchesAll() && !setSubType.HasValue(subsetSubType.Value))
            {
                return false;
            }

            var parameterFound = true;
            while (setEnumerator.MoveNext() && parameterFound)
            {
                if (setEnumerator.Current.HasName("q") && ignoreQuality)
                {
                    continue;
                }

                // Copy the enumerator as we need to iterate multiple times over it.
                // We can do this because it's a struct
                var subsetParameterEnumerator = subSetEnumerator;
                parameterFound = false;
                while (subsetParameterEnumerator.MoveNext() && !parameterFound)
                {
                    parameterFound = subsetParameterEnumerator.Current.Equals(setEnumerator.Current);
                }
            }

            return parameterFound;
        }

        /// <summary>
        /// Determines if two media types are equal except for the quality parameter.
        /// </summary>
        /// <param name="left">The first media type.</param>
        /// <param name="right">The second media type.</param>
        /// <returns><code>true</code> if <paramref name="set" />is equal to <paramref name="subset"/>
        /// except for quality parameter; otherwise <code>false</code>.</returns>
        public static bool AreEqual(StringSegment left, StringSegment right)
        {
            return AreEqual(left, right, ignoreQuality: true);
        }

        /// <summary>
        /// Determines if two media types are equal except for the quality parameter.
        /// </summary>
        /// <param name="left">The first media type.</param>
        /// <param name="right">The second media type.</param>
        /// <param name="ignoreQuality">Whether or not we should avoid comparing the quality parameter.</param>
        /// <returns><code>true</code> if <paramref name="set" />is equal to <paramref name="subset"/>;
        /// otherwise <code>false</code>.</returns>
        public static bool AreEqual(StringSegment left, StringSegment right, bool ignoreQuality)
        {
            if (!left.HasValue && !right.HasValue)
            {
                return true;
            }
            else if (!left.HasValue || !right.HasValue)
            {
                return false;
            }

            var leftParser = new MediaTypeParser(left);
            var rightParser = new MediaTypeParser(right);

            var leftEnumerator = leftParser.GetEnumerator();
            var rightEnumerator = rightParser.GetEnumerator();

            if (!(leftEnumerator.MoveNext() && rightEnumerator.MoveNext()))
            {
                return false;
            }

            var leftType = leftEnumerator.Current;
            var rightType = rightEnumerator.Current;

            if (!leftType.Equals(rightType))
            {
                return false;
            }

            if (!(leftEnumerator.MoveNext() && rightEnumerator.MoveNext()))
            {
                return false;
            }

            var leftSubType = leftEnumerator.Current;
            var rightSubType = rightEnumerator.Current;

            if (!leftSubType.Equals(rightSubType))
            {
                return false;
            }

            var leftParametersCount = CountParameters(leftEnumerator, ignoreQuality);
            var rightParametersCount = CountParameters(rightEnumerator, ignoreQuality);

            if (leftParametersCount != rightParametersCount)
            {
                return false;
            }

            bool parameterFound = true;
            while (leftEnumerator.MoveNext() && parameterFound)
            {
                if (leftEnumerator.Current.HasName("q") && ignoreQuality)
                {
                    continue;
                }

                parameterFound = false;
                var rightParameter = rightEnumerator;
                while (rightParameter.MoveNext() && !parameterFound)
                {
                    if (leftEnumerator.Current.HasName("q") && ignoreQuality)
                    {
                        continue;
                    }

                    parameterFound = leftEnumerator.Current.Equals(rightParameter.Current);
                }
            }

            return parameterFound;
        }

        /// <summary>
        /// Determines if the type of a given <paramref name="mediaType" /> matches all types, E.g, */*.
        /// </summary>
        /// <param name="mediaType">The mediaType to check</param>
        /// <returns><code>true</code> if the <paramref name="mediaType" /> matches all subtypes; otherwise <code>false</code>.</returns>
        public static bool MatchesAllTypes(StringSegment mediaType)
        {
            var parser = new MediaTypeParser(mediaType);
            var enumerator = parser.GetEnumerator();

            return enumerator.MoveNext() && enumerator.Current.IsMatchesAll();
        }

        /// <summary>
        /// Determines if the given <paramref name="mediaType" /> matches all subtypes, E.g, text/*.
        /// </summary>
        /// <param name="mediaType">The mediaType to check</param>
        /// <returns><code>true</code> if the <paramref name="mediaType" /> matches all subtypes; otherwise <code>false</code>.</returns>
        public static bool MatchesAllSubtypes(StringSegment mediaType)
        {
            var parser = new MediaTypeParser(mediaType);
            var enumerator = parser.GetEnumerator();

            return enumerator.MoveNext() &&
                enumerator.MoveNext() &&
                enumerator.Current.IsMatchesAll();
        }

        /// <summary>
        /// Determines if two media types are compatible.
        /// Two media types are compatible if one is a subset of the other ignoring any charset
        /// parameter.
        /// </summary>
        /// <param name="set">The more general media type.</param>
        /// <param name="subset">The more specific media type.</param>
        /// <returns><code>true</code> if <paramref name="set" />is a more general mediaType than <paramref name="subset"/>
        /// except for the charset and quality parameters; otherwise <code>false</code>.</returns>
        public static bool IsCompatible(StringSegment set, StringSegment subset)
        {
            if (!set.HasValue || !subset.HasValue)
            {
                return false;
            }

            var setParser = new MediaTypeParser(set);
            var subSetParser = new MediaTypeParser(subset);

            var setEnumerator = setParser.GetEnumerator();
            var subSetEnumerator = subSetParser.GetEnumerator();

            if (!(setEnumerator.MoveNext() && subSetEnumerator.MoveNext()))
            {
                return false;
            }

            var setType = setEnumerator.Current;
            var subsetType = subSetEnumerator.Current;

            if (!setType.IsMatchesAll() && !setType.HasValue(subsetType.Value))
            {
                return false;
            }

            if (!(setEnumerator.MoveNext() && subSetEnumerator.MoveNext()))
            {
                return false;
            }

            var setSubType = setEnumerator.Current;
            var subsetSubType = subSetEnumerator.Current;

            if (!setSubType.IsMatchesAll() && !setSubType.HasValue(subsetSubType.Value))
            {
                return false;
            }

            var parameterFound = true;
            while (setEnumerator.MoveNext() && parameterFound)
            {
                if (setEnumerator.Current.HasName("q") || setEnumerator.Current.HasName("charset"))
                {
                    continue;
                }

                // Copy the enumerator as we need to iterate multiple times over it.
                // We can do this because it's a struct
                var subsetParameterEnumerator = subSetEnumerator;
                parameterFound = false;
                while (subsetParameterEnumerator.MoveNext() && !parameterFound)
                {
                    parameterFound = subsetParameterEnumerator.Current.Equals(setEnumerator.Current);
                }
            }

            return parameterFound;
        }

        private static int CountParameters(MediaTypeParser.Enumerator enumerator, bool ignoreQuality)
        {
            var count = 0;
            while (enumerator.MoveNext())
            {
                if (!enumerator.Current.HasName("q") || !ignoreQuality)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
