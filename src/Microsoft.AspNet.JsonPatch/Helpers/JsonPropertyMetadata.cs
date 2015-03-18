// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNet.JsonPatch.Helpers
{
    public class JsonPropertyMetadata
    {
        public JsonPropertyMetadata(JsonProperty property, object parent)
        {
            Property = property ?? null;
            Parent = parent ?? null;
        }

        public JsonProperty Property { get; set; }

        public object Parent { get; set; }
    }
}