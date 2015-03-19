// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNet.JsonPatch.Helpers
{
    /// <summary>
    /// Metadata for JsonProperty.
    /// </summary>
    public class JsonPropertyMetadata
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public JsonPropertyMetadata(JsonProperty property, object parent)
        {
            Property = property ?? null;
            Parent = parent ?? null;
        }

        /// <summary>
        /// Gets or sets JsonProperty.
        /// </summary>
        public JsonProperty Property { get; set; }

        /// <summary>
        /// Gets or sets Parent.
        /// </summary>
        public object Parent { get; set; }
    }
}