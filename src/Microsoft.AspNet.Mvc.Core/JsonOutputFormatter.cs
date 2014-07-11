// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.HeaderValueAbstractions;
using Microsoft.AspNet.Http;
using Newtonsoft.Json;

namespace Microsoft.AspNet.Mvc
{
    public class JsonOutputFormatter : OutputFormatter
    {
        private readonly JsonSerializerSettings _settings;
        private readonly bool _indent;

        public JsonOutputFormatter([NotNull] JsonSerializerSettings settings, bool indent)
            : base()
        {
            _settings = settings;
            _indent = indent;
            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false,
                                                    throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false,
                                                       byteOrderMark: true,
                                                       throwOnInvalidBytes: true));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/json"));
        }

        public static JsonSerializerSettings CreateDefaultSettings()
        {
            return new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,

                // Do not change this setting
                // Setting this to None prevents Json.NET from loading malicious, unsafe, or security-sensitive types.
                TypeNameHandling = TypeNameHandling.None
            };
        }

        public void WriteObject([NotNull] TextWriter writer, object value)
        {
            using (var jsonWriter = CreateJsonWriter(writer))
            {
                var jsonSerializer = CreateJsonSerializer();
                jsonSerializer.Serialize(jsonWriter, value);

                // We're explicitly calling flush here to simplify the debugging experience because the
                // underlying TextWriter might be long-lived. If this method ends up being called repeatedly
                // for a request, we should revisit.
                jsonWriter.Flush();
            }
        }

        private JsonWriter CreateJsonWriter([NotNull] TextWriter writer)
        {
            var jsonWriter = new JsonTextWriter(writer);
            if (_indent)
            {
                jsonWriter.Formatting = Formatting.Indented;
            }

            jsonWriter.CloseOutput = false;

            return jsonWriter;
        }

        private JsonSerializer CreateJsonSerializer()
        {
            var jsonSerializer = JsonSerializer.Create(_settings);
            return jsonSerializer;
        }

        public override bool CanWriteResult(ObjectResult result, Type declaredType, HttpContext context)
        {
            return true;
        }

        public override Task WriteAsync(object value,
                                        Type declaredType,
                                        HttpContext context,
                                        CancellationToken cancellationToken)
        {
            // The content type including the encoding should have been written already. 
            // In case it was not present, a default will be selected. 
            var selectedEncoding = SelectCharacterEncoding(MediaTypeHeaderValue.Parse(context.Response.ContentType));
            using (var writer = new StreamWriter(context.Response.Body, selectedEncoding))
            {
                using (var jsonWriter = CreateJsonWriter(writer))
                {
                    var jsonSerializer = CreateJsonSerializer();
                    jsonSerializer.Serialize(jsonWriter, value);

                    // We're explicitly calling flush here to simplify the debugging experience because the
                    // underlying TextWriter might be long-lived. If this method ends up being called repeatedly
                    // for a request, we should revisit.
                    jsonWriter.Flush();
                }
            }

            return Task.FromResult(true);
        }
    }
}
