// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JsonPatchWebSite.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonPatchWebSite
{
    public class ProductCategoryConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var category = new Category();
            category.CategoryName = "CategorySetInConverter";

            return category;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject jo = new JObject();

            foreach (var prop in value.GetType().GetTypeInfo().GetProperties())
            {
                if (prop.CanRead)
                {
                    object propValue = prop.GetValue(value);
                    if (propValue != null)
                    {
                        jo.Add(prop.Name, JToken.FromObject(propValue, serializer));
                    }
                }
            }

            jo.WriteTo(writer);
        }
    }
}
