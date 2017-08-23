// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNetCore.Mvc.Formatters.Json.Internal
{
    public class ProblemJsonConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType) => typeof(Problem).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var resolver = serializer.ContractResolver as DefaultContractResolver;

            var problem = (Problem)value;
            writer.WriteStartObject();

            WriteProperty(nameof(Problem.Title), problem.Title);

            if (problem.Status != null)
            {
                WriteProperty(nameof(Problem.Status), problem.Status);
            }

            if (!string.IsNullOrEmpty(problem.Instance))
            {
                WriteProperty(nameof(Problem.Instance), problem.Instance);
            }

            if (!string.IsNullOrEmpty(problem.Type))
            {
                WriteProperty(nameof(Problem.Type), problem.Type);
            }

            if (!string.IsNullOrEmpty(problem.Detail))
            {
                WriteProperty(nameof(Problem.Detail), problem.Detail);

            }

            if (value.GetType() != typeof(Problem))
            {
                // Derived type
                var properties = PropertyHelper.GetVisibleProperties(value.GetType());
                for (var i = 0; i < properties.Length; i++)
                {
                    var property = properties[i];
                    if (property.Property.DeclaringType != typeof(Problem))
                    {
                        WriteProperty(property.Name, property.GetValue(problem));
                    }
                }
            }

            foreach (var extendedProperty in problem.AdditionalProperties)
            {
                writer.WritePropertyName(extendedProperty.Key);
                serializer.Serialize(writer, extendedProperty.Value);
            }

            writer.WriteEndObject();

            void WriteProperty<TValue>(string name, TValue propertyValue)
            {
                writer.WritePropertyName(resolver?.GetResolvedPropertyName(name) ?? name);
                serializer.Serialize(writer, propertyValue);
            }
        }
    }
}
