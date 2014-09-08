// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;

namespace FormatterWebSite
{
    /// <summary>
    /// This formatter sets the headers after writing the body.
    /// The functional test makes sure that these headers are not present in the response.
    /// </summary>
    public class RogueFormatter : OutputFormatter
    {
        public RogueFormatter()
        {
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/xml"));
        }

        public override Task WriteResponseBodyAsync(OutputFormatterContext context)
        {
            var message = "HelloWorld";
            var response = context.ActionContext.HttpContext.Response;
            response.Body.Write(Encoding.UTF8.GetBytes(message), 0, message.Length);
            response.Headers.Set("TestHeader", "TestValue");

            return Task.FromResult(true);
        }
    }
}