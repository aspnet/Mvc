// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.HeaderValueAbstractions
{
    public class MediaTypeWithQualityHeaderValue : MediaTypeHeaderValue
    {
        public double? Quality { get; private set; }

        public static new MediaTypeWithQualityHeaderValue Parse(string input)
        {
            var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(input);
            if (mediaTypeHeaderValue == null)
            {
                return null;
            }

            var quality = FormattingUtilities.Match;
            string qualityStringValue = null;
            if (mediaTypeHeaderValue.Parameters.TryGetValue("q", out qualityStringValue))
            {
                quality = Double.Parse(qualityStringValue);
            }

            return
                new MediaTypeWithQualityHeaderValue()
                {
                    MediaType = mediaTypeHeaderValue.MediaType,
                    MediaSubType = mediaTypeHeaderValue.MediaSubType,
                    Charset = mediaTypeHeaderValue.Charset,
                    Parameters = mediaTypeHeaderValue.Parameters,
                    Quality = quality,
                };
        } 
    }
}
