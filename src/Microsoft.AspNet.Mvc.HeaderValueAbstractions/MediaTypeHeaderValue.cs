// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.HeaderValueAbstractions
{
    public class MediaTypeHeaderValue
    {
        public string Charset { get; set; }

        public string MediaType { get; set; }

        public string MediaSubType { get; set; }

        public string RawValue { get; set; }

        public IDictionary<string, string> Parameters { get; set; }

        public static MediaTypeHeaderValue Parse(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            var inputArray = input.Split(new[] { ';' }, 2);
            var mediaTypeParts = inputArray[0].Split('/');
            if (mediaTypeParts.Length != 2)
            {
                return null;
            }

            // TODO: throw if the media type and subtypes are invalid.
            var mediaType = mediaTypeParts[0].Trim();
            var mediaSubType = mediaTypeParts[1].Trim();

            Dictionary<string, string> parameters = null;
            string charset = null;
            if (inputArray.Length == 2)
            {
                parameters = ParseParameters(inputArray[1]);
                if (parameters.ContainsKey("charset"))
                {
                    charset = parameters["charset"];
                }
            }

            var mediaTypeHeader = new MediaTypeHeaderValue()
            {
                MediaType = mediaType,
                MediaSubType = mediaSubType,
                Charset = charset,
                Parameters = parameters,
                RawValue = input
            };

            return mediaTypeHeader;
        }

        protected static Dictionary<string, string> ParseParameters(string inputString)
        {
            var acceptParameters = inputString.Split(';');
            var parameterNameValue = new Dictionary<string, string>();
            foreach (var parameter in acceptParameters)
            {
                var index = parameter.Split('=');
                if (index.Length == 2)
                {
                    // TODO: throw exception if this is not the case.
                    parameterNameValue.Add(index[0].Trim(), index[1].Trim());
                }
            }

            return parameterNameValue;
        }
    }
}
