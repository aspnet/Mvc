﻿// Copyright (c) .NET Foundation. All rights reserved.
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
            // Load has to happen even if we are not using it. If removed, deserialization will throw an error -
            //"Additional text found in JSON string after finishing deserializing object."
            var obj = JObject.Load(reader);

            var category = new Category();
            category.CategoryName = "CategorySetInConverter";

            return category;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jo = new JObject();

            foreach (var prop in value.GetType().GetTypeInfo().GetProperties())
            {
                if (prop.CanRead)
                {
                    var propertyValue = prop.GetValue(value);
                    if (propertyValue != null)
                    {
                        jo.Add(prop.Name, JToken.FromObject(propertyValue, serializer));
                    }
                }
            }

            jo.WriteTo(writer);
        }
    }
}
