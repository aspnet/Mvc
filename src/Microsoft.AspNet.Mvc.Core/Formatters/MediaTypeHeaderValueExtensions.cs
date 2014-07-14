// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Extension methods for <see cref="MediaTypeHeaderValue"/>.
    /// </summary>
    public static class MediaTypeHeaderValueExtensions
    {
        /// <summary>
        /// Determines whether two <see cref="MediaTypeHeaderValue"/> instances match. The instance
        /// <paramref name="mediaType1"/> is said to match <paramref name="mediaType2"/> if and only if
        /// <paramref name="mediaType1"/> is a strict subset of the values and parameters of 
        /// <paramref name="mediaType2"/>. 
        /// That is, if the media type and media type parameters of <paramref name="mediaType1"/> are all present 
        /// and match those of <paramref name="mediaType2"/> then it is a match even though 
        /// <paramref name="mediaType2"/> may have additional parameters.
        /// </summary>
        /// <param name="mediaType1">The first media type.</param>
        /// <param name="mediaType2">The second media type.</param>
        /// <returns><c>true</c> if this is a subset of <paramref name="mediaType2"/>; false otherwise.</returns>
        public static bool IsSubsetOf(this MediaTypeHeaderValue mediaType1, MediaTypeHeaderValue mediaType2)
        { 
            Contract.Assert(mediaType1 != null);

            if (mediaType2 == null)
            {
                return false;
            }

            var mediaType2Range = GetMediaTypeRange(mediaType2);
            if (!mediaType1.MediaType.Equals(mediaType2.MediaType, StringComparison.OrdinalIgnoreCase))
            {
                if (mediaType2Range != MediaTypeHeaderValueRange.AllMediaRange)
                {
                    return false;
                }
            }
            else if (!mediaType1.MediaSubType.Equals(mediaType2.MediaSubType, StringComparison.OrdinalIgnoreCase))
            {
                if (mediaType2Range != MediaTypeHeaderValueRange.SubtypeMediaRange)
                {
                    return false;
                }
            }

            // TODO: Stop gap, this should really be initialized in the parser. 
            if (mediaType1.Parameters != null)
            {
                if (mediaType1.Parameters.Count != 0  && 
                    (mediaType2.Parameters == null || mediaType1.Parameters.Count == 0))
                {
                    return false;
                }

                // So far we either have a full match or a subset match. Now check that all of 
                // mediaType1's parameters are present and equal in mediatype2
                // TODO: we also need to do it for accept parameter values. 
                MatchParameters(mediaType1.Parameters, mediaType2.Parameters);
            }

            return true;
        }

        public static MediaTypeHeaderValueRange GetMediaTypeRange(this MediaTypeHeaderValue mediaType)
        {
            var mediaTypeRange = MediaTypeHeaderValueRange.None;
            if (mediaType.MediaType == "*" && mediaType.MediaSubType == "*")
            {
                mediaTypeRange = MediaTypeHeaderValueRange.AllMediaRange;
            }
            else if (mediaType.MediaSubType == "*")
            {
                mediaTypeRange = MediaTypeHeaderValueRange.SubtypeMediaRange;
            }

            return mediaTypeRange;
        }

        private static bool MatchParameters(IDictionary<string, string> parameters1,
                                            IDictionary<string, string> parameters2)
        {
            foreach (var parameterKey in parameters1.Keys)
            {
                string parameterValue2 = null;
                if (!parameters2.TryGetValue(parameterKey, out parameterValue2))
                {
                    return false;
                }

                if (parameterValue2 == null || !parameterValue2.Equals(parameters1[parameterKey]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
