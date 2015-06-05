// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using LocalizationWebSite;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Xunit;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class LocalizationTest
    {
        private const string SiteName = nameof(LocalizationWebSite);
        private static readonly Assembly _assembly = typeof(LocalizationTest).GetTypeInfo().Assembly;

        private readonly Action<IApplicationBuilder> _app = new Startup().Configure;
        private readonly Action<IServiceCollection> _configureServices = new Startup().ConfigureServices;

        public static IEnumerable<object[]> LocalizationData
        {
            get
            {
                var expected1 =
 @"en-gb-index
partial
mypartial";

                yield return new[] { "en-GB", expected1 };

                var expected2 =
 @"fr-index
fr-partial
mypartial";
                yield return new[] { "fr", expected2 };

                var expected3 =
 @"index
partial
mypartial";
                yield return new[] { "na", expected3 };

            }
        }

        [Theory]
        [MemberData(nameof(LocalizationData))]
        public async Task Localization_SuffixViewName(string value, string expected)
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();
            var cultureCookie = "c=" + value + "|uic=" + value;
            client.DefaultRequestHeaders.Add(
                "Cookie",
                new CookieHeaderValue("ASPNET_CULTURE", cultureCookie).ToString());

            System.Diagnostics.Debugger.Launch();

            // Act
            var body = await client.GetStringAsync("http://localhost/");

            // Assert
            Assert.Equal(expected, body.Trim());
        }
    }
}
