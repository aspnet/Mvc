﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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
            // Clear all values and only include json-patch+json value.
            SupportedMediaTypes.Clear();

            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json-patch+json"));
        }

        /// <inheritdoc />
        public override Task<object> ReadRequestBodyAsync([NotNull] InputFormatterContext context)
        {
            var jsonPatchDocument = (IJsonPatchDocument)base.ReadRequestBodyAsync(context).Result;
            if (jsonPatchDocument != null)
            {
                jsonPatchDocument.ContractResolver = SerializerSettings.ContractResolver;
            }

            return Task.FromResult((object)jsonPatchDocument);
        }

        /// <inheritdoc />
        public override bool CanRead(InputFormatterContext context)
        {
            if (!typeof(IJsonPatchDocument).IsAssignableFrom(context.ModelType) ||
                !context.ModelType.IsGenericType)
            {
                return false;
            }

            return base.CanRead(context);
        }
    }
}