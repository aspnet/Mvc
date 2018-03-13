// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public static class HttpClientExtensions
    {
        public static async Task<IHtmlDocument> GetHtmlDocumentAsync(this HttpClient client, string url)
        {
            var content = await client.GetStringAsync(url);
            var parser = new HtmlParser();
            return parser.Parse(content);
        }
    }
}