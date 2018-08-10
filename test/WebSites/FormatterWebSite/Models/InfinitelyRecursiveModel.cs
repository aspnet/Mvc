// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Principal;
using Newtonsoft.Json;

namespace FormatterWebSite
{
    public class InfinitelyRecursiveModel
    {
        [JsonConverter(typeof(StringSecurityConverter))]
        public SecurityIdentifier Id { get; set; } = new SecurityIdentifier("S-1-5-21-1004336348-1177238915-682003330-512");

        private class StringSecurityConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => objectType == typeof(SecurityIdentifier);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return new SecurityIdentifier(reader.Value.ToString());
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}
