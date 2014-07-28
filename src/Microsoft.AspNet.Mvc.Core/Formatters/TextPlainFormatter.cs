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
    public class TextPlainFormatter : IOutputFormatter
    {
        public bool CanWriteResult(OutputFormatterContext context, MediaTypeHeaderValue contentType)
        {
            // Ignore the passed in content type, if the object is string 
            // always return it as a text/plain format.
            var valueAsString = context.Object as string;
            if (valueAsString != null)
            {
                return true;
            }

            return false;
        }

        public async Task WriteAsync(OutputFormatterContext context)
        {
            // Ignore the accept-charset field, as this will always write utf-8.
            var response = context.ActionContext.HttpContext.Response;
            response.ContentType = "text/plain;charset=utf-8";
            var valueAsString = context.Object as string;
            await response.WriteAsync(valueAsString);
        }
    }
}