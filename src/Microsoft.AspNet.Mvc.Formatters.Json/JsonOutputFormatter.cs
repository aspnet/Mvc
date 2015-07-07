// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Internal;
using Microsoft.Framework.Internal;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// An output formatter that specializes in writing JSON content.
    /// </summary>
    public class JsonOutputFormatter : OutputFormatter
    {
        private JsonSerializerSettings _serializerSettings;

        public JsonOutputFormatter()
            : this(SerializerSettingsProvider.CreateSerializerSettings())
        {
        }

        public JsonOutputFormatter([NotNull] JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings;

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationJson);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.TextJson);
        }

        /// <summary>
        /// Gets or sets the <see cref="JsonSerializerSettings"/> used to configure the <see cref="JsonSerializer"/>.
        /// </summary>
        public JsonSerializerSettings SerializerSettings
        {
            get
            {
                return _serializerSettings;
            }
            [param: NotNull]
            set
            {
                _serializerSettings = value;
            }
        }

        public void WriteObject([NotNull] TextWriter writer, object value)
        {
            using (var jsonWriter = CreateJsonWriter(writer))
            {
                var jsonSerializer = CreateJsonSerializer();
                jsonSerializer.Serialize(jsonWriter, value);
            }
        }

        /// <summary>
        /// Called during serialization to create the <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> used to write.</param>
        /// <returns>The <see cref="JsonWriter"/> used during serialization.</returns>
        protected virtual JsonWriter CreateJsonWriter([NotNull] TextWriter writer)
        {
            var jsonWriter = new JsonTextWriter(writer);
            jsonWriter.CloseOutput = false;

            return jsonWriter;
        }

        /// <summary>
        /// Called during serialization to create the <see cref="JsonSerializer"/>.
        /// </summary>
        /// <returns>The <see cref="JsonSerializer"/> used during serialization and deserialization.</returns>
        protected virtual JsonSerializer CreateJsonSerializer()
        {
            return JsonSerializer.Create(SerializerSettings);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterContext context)
        {
            var response = context.HttpContext.Response;
            var selectedEncoding = context.SelectedEncoding;

            using (var writer = new HttpResponseStreamWriter(response.Body, selectedEncoding))
            {
                WriteObject(writer, context.Object);
            }

            return Task.FromResult(true);
        }
    }
}
