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
        private readonly LogSelection _logSelection;

        public LoggingMiddleware(RequestDelegate next, TestSink sink, LogSelection logSelection)
        {
            _next = next;
            _sink = sink;
            _logSelection = logSelection;
        }

        public async Task Invoke(HttpContext context)
        {
            var requestId = Guid.NewGuid();
            using (new LoggingContext(requestId)) {
                var stream = context.Response.Body;
                context.Response.Body = new MemoryStream();
                await _next(context);
                var serializer = JsonSerializer.Create();
                var writer = new JsonTextWriter(new StreamWriter(stream));
                if (_logSelection == LogSelection.All)
                {
                    serializer.Serialize(writer, _sink.Writes);
                }
                else if (_logSelection == LogSelection.Startup)
                {
                    serializer.Serialize(writer, _sink.Writes.Where(w => w.RequestId == Guid.Empty));
                }
                else
                {
                    serializer.Serialize(writer, _sink.Writes.Where(w => w.RequestId == requestId));
                }
                writer.Flush();
                context.Response.Body = stream;
            }
        }
    }
}