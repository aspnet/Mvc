// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Mvc.Formatters.Internal
{
    public static class AcceptHeaderParser
    {
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

            if (MediaTypeHeaderValue.TryParseList(acceptHeaders, out var output))
            {
                for (int j = 0; j < output.Count; j++)
                {
                    var mediaTypeHeaderValue = output[j];
                    var mediaTypeWithQuality = new MediaTypeSegmentWithQuality(mediaTypeHeaderValue.ToString(), mediaTypeHeaderValue.Quality ?? 1.0);
                    parsedValues.Add(mediaTypeWithQuality);
                }
            }
        }
    }
}
