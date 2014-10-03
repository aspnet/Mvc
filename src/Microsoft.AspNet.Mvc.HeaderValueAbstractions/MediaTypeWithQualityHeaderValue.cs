// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;

namespace Microsoft.AspNet.Mvc.HeaderValueAbstractions
{
    public class MediaTypeWithQualityHeaderValue : MediaTypeHeaderValue
    {
        public double? Quality { get; private set; }

        public static bool TryParse(string input, out MediaTypeWithQualityHeaderValue headerValue)
        {
            MediaTypeHeaderValue mediaTypeHeaderValue;
            if (!TryParse(input, out mediaTypeHeaderValue))
            {
                headerValue = null;
                return false;
            }

            var quality = HttpHeaderUtilitites.Match;
            string qualityStringValue = null;
            if (mediaTypeHeaderValue.Parameters.TryGetValue("q", out qualityStringValue))
            {
                if (!double.TryParse(
                        qualityStringValue,
                        NumberStyles.AllowLeadingWhite | NumberStyles.AllowDecimalPoint |
                            NumberStyles.AllowTrailingWhite,
                        NumberFormatInfo.InvariantInfo,
                        out quality))
                {
                    headerValue = null;
                    return false;
                }
            }

            headerValue = new MediaTypeWithQualityHeaderValue()
                {
                    MediaType = mediaTypeHeaderValue.MediaType,
                    MediaSubType = mediaTypeHeaderValue.MediaSubType,
                    MediaTypeRange = mediaTypeHeaderValue.MediaTypeRange,
                    Charset = mediaTypeHeaderValue.Charset,
                    Parameters = mediaTypeHeaderValue.Parameters,
                    Quality = quality,
                };

            return true;
        } 
    }
}
