// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.AspNet.Mvc.HeaderValueAbstractions
{
    public class MediaTypeHeaderValueParsingTests
    {
        public static IEnumerable<object[]> GetValidMediaTypeWithQualityHeaderValues
        {
            get
            {
                yield return new object[]
                {
                    "application",
                    "json",
                    null,
                    null,
                    null,
                    FormattingUtilities.Match,
                    "application/json"
                };

                yield return new object[]
                {
                    "text",
                    "plain",
                    "us-ascii",
                    new Dictionary<string, string>() { { "charset", "us-ascii" } },
                    new Dictionary<string, string>()
                                        { { "q", "0.8"  }, { "acceptParam", "foo" } },
                    0.8,
                    "text/plain;charset=us-ascii;q=0.8;acceptParam=foo",
                };

                yield return new object[]
                {
                    "text",
                    "plain",
                    "utf-8",
                    new Dictionary<string, string>() { { "charset", "utf-8" }, { "foo", "bar" } },
                    null,
                    FormattingUtilities.Match,
                    "text/plain;charset=utf-8;foo=bar",
                };

                yield return new object[]
                {
                    "text",
                    "plain",
                    null,
                    new Dictionary<string, string>() { { "foo", "bar" } },
                    new Dictionary<string, string>() { { "q", "0.0" }},
                    FormattingUtilities.NoMatch,
                    "text/plain;foo=bar;q=0.0",
                };

                yield return new object[]
                {
                    "text",
                    "plain",
                    "utf-8",
                    new Dictionary<string, string>() { { "charset", "utf-8" }, { "foo", "bar" } },
                    new Dictionary<string, string>() { { "q", "0.0" }},
                    FormattingUtilities.NoMatch,
                    "text/plain;charset=utf-8;foo=bar;q=0.0",
                };
            }
        }

        [Theory]
        [MemberData("GetValidMediaTypeWithQualityHeaderValues")]
        public void MediaTypeWithQualityHeaderValue_ParseSuccessfully(string mediaType,
                                         string mediaSubType,
                                         string charset,
                                         IDictionary<string, string> parameters,
                                         IDictionary<string, string> acceptParameters,
                                         double quality,
                                         string rawValue)
        {
            var parsedValue = MediaTypeWithQualityHeaderValue.Parse(rawValue);
            // Act and Assert
            Assert.Equal(rawValue, parsedValue.RawValue);
            Assert.Equal(mediaType, parsedValue.MediaType);
            Assert.Equal(mediaSubType, parsedValue.MediaSubType);
            Assert.Equal(charset, parsedValue.Charset);
            ValidateParametes(parameters, parsedValue.Parameters);
            ValidateParametes(acceptParameters, parsedValue.AcceptParameters);
        }

        private static void ValidateParametes(IDictionary<string, string> expectedParameters,
                                              IDictionary<string, string> actualParameters)
        {
            if (expectedParameters == null)
            {
                Assert.Null(expectedParameters);
            }
            else
            {
                foreach (var key in expectedParameters.Keys)
                {
                    Assert.Equal(expectedParameters[key], actualParameters[key]);
                }
            }
        }
    }
}
