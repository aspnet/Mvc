// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.HeaderValueAbstractions
{
    public class MediaTypeWithQualityHeaderValue : MediaTypeHeaderValue
    {
        public double? Quality { get; set; }

        public IDictionary<string, string> AcceptParameters { get; set; }

        public static new MediaTypeWithQualityHeaderValue Parse(string input)
        {
            var mediaTypeString = input;
            var quality = FormattingUtilities.Match;
            Dictionary<string, string> acceptParameters = null;

            // TODO: this does not take care if there was a space between.
            var index = input.IndexOf("q=", System.StringComparison.Ordinal);
            if (index > 0)
            {
                var acceptParameterString = input.Substring(index);
                acceptParameters = ParseParameters(acceptParameterString);
                mediaTypeString = input.Substring(0, index);
                quality = Double.Parse(acceptParameters["q"]);
            }

            var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(mediaTypeString);
            if (mediaTypeHeaderValue == null)
            {
                return null;
            }

            return
                new MediaTypeWithQualityHeaderValue()
                {
                    MediaType = mediaTypeHeaderValue.MediaType,
                    MediaSubType = mediaTypeHeaderValue.MediaSubType,
                    Charset = mediaTypeHeaderValue.Charset,
                    RawValue = input,
                    Parameters = mediaTypeHeaderValue.Parameters,
                    Quality = quality,
                    AcceptParameters = acceptParameters
                };
        } 
    }
}
