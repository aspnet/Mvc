﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public class TempDataSerializer
    {
        private readonly JsonSerializer _jsonSerializer =
            JsonSerializer.Create(JsonSerializerSettingsProvider.CreateSerializerSettings());

        private static readonly MethodInfo _convertArrayMethodInfo = typeof(TempDataSerializer).GetMethod(
            nameof(ConvertArray), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _convertDictionaryMethodInfo = typeof(TempDataSerializer).GetMethod(
            nameof(ConvertDictionary), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly ConcurrentDictionary<Type, Func<JArray, object>> _arrayConverters =
            new ConcurrentDictionary<Type, Func<JArray, object>>();
        private static readonly ConcurrentDictionary<Type, Func<JObject, object>> _dictionaryConverters =
            new ConcurrentDictionary<Type, Func<JObject, object>>();

        private static readonly Dictionary<JTokenType, Type> _tokenTypeLookup = new Dictionary<JTokenType, Type>
        {
            { JTokenType.String, typeof(string) },
            { JTokenType.Integer, typeof(int) },
            { JTokenType.Boolean, typeof(bool) },
            { JTokenType.Float, typeof(float) },
            { JTokenType.Guid, typeof(Guid) },
            { JTokenType.Date, typeof(DateTime) },
            { JTokenType.TimeSpan, typeof(TimeSpan) },
            { JTokenType.Uri, typeof(Uri) },
        };

        public IDictionary<string, object> Deserialize(byte[] value)
        {
            Dictionary<string, object> tempDataDictionary;

            using (var memoryStream = new MemoryStream(value))
            using (var reader = new BsonDataReader(memoryStream))
            {
                tempDataDictionary = _jsonSerializer.Deserialize<Dictionary<string, object>>(reader);
                if (tempDataDictionary == null)
                {
                    return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                }
            }

            var convertedDictionary = new Dictionary<string, object>(
                tempDataDictionary,
                StringComparer.OrdinalIgnoreCase);
            foreach (var item in tempDataDictionary)
            {
                var jArrayValue = item.Value as JArray;
                var jObjectValue = item.Value as JObject;
                if (jArrayValue != null && jArrayValue.Count > 0)
                {
                    var arrayType = jArrayValue[0].Type;
                    if (_tokenTypeLookup.TryGetValue(arrayType, out var returnType))
                    {
                        var arrayConverter = _arrayConverters.GetOrAdd(returnType, type =>
                        {
                            return (Func<JArray, object>)_convertArrayMethodInfo
                                .MakeGenericMethod(type)
                                .CreateDelegate(typeof(Func<JArray, object>));
                        });
                        var result = arrayConverter(jArrayValue);

                        convertedDictionary[item.Key] = result;
                    }
                    else
                    {
                        var message = Resources.FormatTempData_CannotDeserializeToken(nameof(JToken), arrayType);
                        throw new InvalidOperationException(message);
                    }
                }
                else if (jObjectValue != null)
                {
                    if (!jObjectValue.HasValues)
                    {
                        convertedDictionary[item.Key] = null;
                        continue;
                    }

                    var jTokenType = jObjectValue.Properties().First().Value.Type;
                    if (_tokenTypeLookup.TryGetValue(jTokenType, out var valueType))
                    {
                        var dictionaryConverter = _dictionaryConverters.GetOrAdd(valueType, type =>
                        {
                            return (Func<JObject, object>)_convertDictionaryMethodInfo
                                .MakeGenericMethod(type)
                                .CreateDelegate(typeof(Func<JObject, object>));
                        });
                        var result = dictionaryConverter(jObjectValue);

                        convertedDictionary[item.Key] = result;
                    }
                    else
                    {
                        var message = Resources.FormatTempData_CannotDeserializeToken(nameof(JToken), jTokenType);
                        throw new InvalidOperationException(message);
                    }
                }
                else if (item.Value is long longValue)
                {
                    if (longValue >= int.MinValue && longValue <= int.MaxValue)
                    {
                        // BsonReader casts all ints to longs. We'll attempt to work around this by force converting
                        // longs to ints when there's no loss of precision.
                        convertedDictionary[item.Key] = (int)longValue;
                    }
                }
            }

            return convertedDictionary;
        }

        public byte[] Serialize(IDictionary<string, object> values)
        {
            var hasValues = (values != null && values.Count > 0);
            if (hasValues)
            {
                foreach (var item in values.Values)
                {
                    if (item != null)
                    {
                        // We want to allow only simple types to be serialized.
                        EnsureObjectCanBeSerialized(item);
                    }
                }

                using (var memoryStream = new MemoryStream())
                {
                    using (var writer = new BsonDataWriter(memoryStream))
                    {
                        _jsonSerializer.Serialize(writer, values);
                        return memoryStream.ToArray();
                    }
                }
            }
            else
            {
                return new byte[0];
            }
        }

        public void EnsureObjectCanBeSerialized(object item)
        {
            var itemType = item.GetType();
            Type actualType = null;

            if (itemType.IsArray)
            {
                itemType = itemType.GetElementType();
            }
            else if (itemType.GetTypeInfo().IsGenericType)
            {
                if (ClosedGenericMatcher.ExtractGenericInterface(itemType, typeof(IList<>)) != null)
                {
                    var genericTypeArguments = itemType.GenericTypeArguments;
                    Debug.Assert(genericTypeArguments.Length == 1, "IList<T> has one generic argument");
                    actualType = genericTypeArguments[0];
                }
                else if (ClosedGenericMatcher.ExtractGenericInterface(itemType, typeof(IDictionary<,>)) != null)
                {
                    var genericTypeArguments = itemType.GenericTypeArguments;
                    Debug.Assert(
                        genericTypeArguments.Length == 2,
                        "IDictionary<TKey, TValue> has two generic arguments");

                    // Throw if the key type of the dictionary is not string.
                    if (genericTypeArguments[0] != typeof(string))
                    {
                        var message = Resources.FormatTempData_CannotSerializeDictionary(
                            typeof(TempDataSerializer).FullName, genericTypeArguments[0]);
                        throw new InvalidOperationException(message);
                    }
                    else
                    {
                        actualType = genericTypeArguments[1];
                    }
                }
            }

            actualType = actualType ?? itemType;
            if (!IsSimpleType(actualType))
            {
                var underlyingType = Nullable.GetUnderlyingType(actualType) ?? actualType;
                var message = Resources.FormatTempData_CannotSerializeType(
                    typeof(TempDataSerializer).FullName, underlyingType);
                throw new InvalidOperationException(message);
            }
        }

        private static IList<TVal> ConvertArray<TVal>(JArray array)
        {
            return array.Values<TVal>().ToArray();
        }

        private static IDictionary<string, TVal> ConvertDictionary<TVal>(JObject jObject)
        {
            var convertedDictionary = new Dictionary<string, TVal>(StringComparer.Ordinal);
            foreach (var item in jObject)
            {
                convertedDictionary.Add(item.Key, jObject.Value<TVal>(item.Key));
            }
            return convertedDictionary;
        }

        private static bool IsSimpleType(Type type)
        {
            var typeInfo = type.GetTypeInfo();

            return typeInfo.IsPrimitive ||
                typeInfo.IsEnum ||
                type.Equals(typeof(decimal)) ||
                type.Equals(typeof(string)) ||
                type.Equals(typeof(DateTime)) ||
                type.Equals(typeof(Guid)) ||
                type.Equals(typeof(DateTimeOffset)) ||
                type.Equals(typeof(TimeSpan)) ||
                type.Equals(typeof(Uri));
        }
    }
}
