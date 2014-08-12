﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class HttpMethodProviderAttributesTests
    {
        [Theory]
        [MemberData("HttpMethodProviderTestData")]
        public void HttpMethodProviderAttributes_ReturnsCorrectHttpMethodSequence(
            IActionHttpMethodProvider httpMethodProvider,
            IEnumerable<string> expectedHttpMethods)
        {
            // Act & Assert
            Assert.Equal(expectedHttpMethods, httpMethodProvider.HttpMethods);
        }

        public static TheoryData<IActionHttpMethodProvider, IEnumerable<string>> HttpMethodProviderTestData
        {
            get
            {
                var data = new TheoryData<IActionHttpMethodProvider, IEnumerable<string>>();
                data.Add(new HttpGetAttribute(), new[] { "GET" });
                data.Add(new HttpPostAttribute(), new[] { "POST" });
                data.Add(new HttpPutAttribute(), new[] { "PUT" });
                data.Add(new HttpPatchAttribute(), new[] { "PATCH" });
                data.Add(new HttpDeleteAttribute(), new[] { "DELETE" });
                data.Add(new AcceptVerbsAttribute("MERGE", "OPTIONS"), new[] { "MERGE", "OPTIONS" });

                return data;
            }
        }
    }
}