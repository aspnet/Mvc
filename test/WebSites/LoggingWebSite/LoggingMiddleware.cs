// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Logging;
using Newtonsoft.Json;

namespace LoggingWebSite
{
    public class LoggingMiddleware
    {
        private readonly TestSink _sink;
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next, TestSink sink)
        {
            _next = next;
            _sink = sink;
        }

        public async Task Invoke(HttpContext context)
        {
            var requestId = Guid.NewGuid();
            using (new LoggingContext(requestId))
            {
                Stream stream = null;
                try
                {
                    stream = context.Response.Body;
                    context.Response.Body = new MemoryStream();

                    await _next(context);

                    var serializer = JsonSerializer.Create();
                    using (var writer = new JsonTextWriter(new StreamWriter(stream)))
                    {
                        if (string.Equals(context.Request.Headers["Startup"], "true"))
                        {
                            serializer.Serialize(writer, _sink.Writes.Where(w => w.RequestId == Guid.Empty));
                        }
                        else
                        {
                            serializer.Serialize(writer, _sink.Writes.Where(w => w.RequestId == requestId));
                        }
                    }
                }
                finally
                {
                    context.Response.Body = stream;
                }
            }
        }
    }
}