// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.HeaderValueAbstractions
{
    public class StringWithQualityHeaderValue
    {
        public double? Quality { get; set; }

        public string RawValue { get; set; }

        public string Value { get; set; }

        public static StringWithQualityHeaderValue Parse(string input)
        {
            var inputArray = input.Split(new[] { ';' }, 2);
            var value = inputArray[0].Trim();

            // By default, an uspecified q factor value is equal to a match.
            var quality = FormattingUtilities.Match;
            if (inputArray.Length > 1)
            {
                var parameter = inputArray[1].Trim();                
                var index = parameter.IndexOf("=", System.StringComparison.Ordinal);
                if (index > 0 && parameter.StartsWith("q"))
                {
                    quality = Double.Parse(parameter.Substring(index + 1).Trim());
                }
            }

            var stringWithQualityHeader = new StringWithQualityHeaderValue()
            {
                Quality = quality,
                Value = value,
                RawValue = input
            };

            return stringWithQualityHeader;
        }
    }
}
