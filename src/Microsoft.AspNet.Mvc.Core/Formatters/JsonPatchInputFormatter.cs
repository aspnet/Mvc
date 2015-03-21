// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.JsonPatch;
using Microsoft.Framework.Internal;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc
{
    public class JsonPatchInputFormatter : JsonInputFormatter
    {
        public JsonPatchInputFormatter()
        {
            // Remove json mediatype header and only include json-patch+json value.
            SupportedMediaTypes.Remove(MediaTypeHeaderValue.Parse("application/json"));
            SupportedMediaTypes.Remove(MediaTypeHeaderValue.Parse("text/json"));

            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json-patch+json"));
        }

        /// <inheritdoc />
        public override Task<object> ReadRequestBodyAsync([NotNull] InputFormatterContext context)
        {
            var jsonSerializer = CreateJsonSerializer();
            var jsonPatchDocument = base.ReadRequestBodyAsync(context).Result as IJsonPatchDocument;
            jsonPatchDocument.ContractResolver = jsonSerializer.ContractResolver;

            return Task.FromResult((object)jsonPatchDocument);
        }

        /// <inheritdoc />
        public override bool CanRead(InputFormatterContext context)
        {
            if (!typeof(IJsonPatchDocument).IsAssignableFrom(context.ModelType))
            {
                return false;
            }

            var contentType = context.ActionContext.HttpContext.Request.ContentType;

            if (!string.Equals(contentType, "application/json-patch+json"))
            {
                return false;
            }

            return true;
        }
    }
}