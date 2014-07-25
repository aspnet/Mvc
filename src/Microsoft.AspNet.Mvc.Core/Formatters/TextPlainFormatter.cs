// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Writes a string value to the response.
    /// </summary>
    public class TextPlainFormatter : OutputFormatter
    {
        public TextPlainFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/plain"));
            SupportedEncodings.Add(Encoding.GetEncoding("utf-8"));
        }
        
        public override bool CanWriteResult(OutputFormatterContext context, MediaTypeHeaderValue contentType)
        {
            if (base.CanWriteResult(context, contentType))
            {
                var valueAsString = context.Object as string;
                if (valueAsString != null)
                {
                    return true;
                }
            }

            return false;
        }

        public override void WriteResponseContentHeaders(OutputFormatterContext context)
        {
            // Ignore the accept-charset field, as this will always write utf-8.
            var response = context.ActionContext.HttpContext.Response;            
            response.ContentType = "text/plain;charset=utf-8";
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterContext context)
        {
            var response = context.ActionContext.HttpContext.Response;
            var valueAsString = context.Object as string;
            await response.WriteAsync(valueAsString);
        }
    }
}