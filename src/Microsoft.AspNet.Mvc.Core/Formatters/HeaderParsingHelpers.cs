// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;

namespace Microsoft.AspNet.Mvc
{
    public static class HeaderParsingHelpers
    {
        public static IList<MediaTypeWithQualityHeaderValue> GetAcceptHeaders(string acceptHeader)
        {
            if (string.IsNullOrEmpty(acceptHeader))
            {
                return null;
            }

            var acceptHeaderCollection = new List<MediaTypeWithQualityHeaderValue>();
            foreach (var item in acceptHeader.Split(','))
            {
                var parsedAcceptHeader = MediaTypeWithQualityHeaderValue.Parse(item);
                // If we are unable to parse even one of the Accept Headers, we ignore them completely.
                if (parsedAcceptHeader == null)
                {
                    return null;
                }

                acceptHeaderCollection.Add(parsedAcceptHeader);
            }

            return acceptHeaderCollection;
        }

        public static IList<StringWithQualityHeaderValue> GetAcceptCharsetHeaders(string acceptCharsetHeader)
        {
            if (string.IsNullOrEmpty(acceptCharsetHeader))
            {
                return null;
            }

            var acceptCharsetHeaderCollection = new List<StringWithQualityHeaderValue>();
            foreach (var item in acceptCharsetHeader.Split(','))
            {
                var parsedAcceptCharsetHeader = StringWithQualityHeaderValue.Parse(item);
                // If we are unable to parse even one of the Accept-Charset Headers, we ignore them completely.
                if (parsedAcceptCharsetHeader == null)
                {
                    return null;
                }

                acceptCharsetHeaderCollection.Add(parsedAcceptCharsetHeader);
            }

            return acceptCharsetHeaderCollection;
        }
    }
}
