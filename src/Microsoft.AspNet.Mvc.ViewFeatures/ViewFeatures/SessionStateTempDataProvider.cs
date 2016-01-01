// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.Mvc.ViewFeatures
{
    /// <summary>
    /// Provides session-state data to the current <see cref="ITempDataDictionary"/> object.
    /// </summary>
    public class SessionStateTempDataProvider : ITempDataProvider
    {
        private const string TempDataSessionStateKey = "__ControllerTempData";
        private readonly JsonSerializer _jsonSerializer = JsonSerializer.Create(
            new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.None
            });

        private static readonly MethodInfo _convertArrayMethodInfo = typeof(SessionStateTempDataProvider).GetMethod(
            nameof(ConvertArray), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _convertDictMethodInfo = typeof(SessionStateTempDataProvider).GetMethod(
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

        /// <inheritdoc />
        public virtual IDictionary<string, object> LoadTempData(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Accessing Session property will throw if the session middleware is not enabled.
            var session = context.Session;

            var tempDataDictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            byte[] value;

            if (session.TryGetValue(TempDataSessionStateKey, out value))
            {
                using (var memoryStream = new MemoryStream(value))
                using (var writer = new BsonReader(memoryStream))
                {
                    tempDataDictionary = _jsonSerializer.Deserialize<Dictionary<string, object>>(writer);
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
                        Type returnType;
                        if (_tokenTypeLookup.TryGetValue(arrayType, out returnType))
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
                        Type valueType;
                        if (_tokenTypeLookup.TryGetValue(jTokenType, out valueType))
                        {
                            var dictionaryConverter = _dictionaryConverters.GetOrAdd(valueType, type =>
                            {
                                return (Func<JObject, object>)_convertDictMethodInfo
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
                    else if (item.Value is long)
                    {
                        var longValue = (long)item.Value;
                        if (longValue >= int.MinValue && longValue <= int.MaxValue)
                        {
                            // BsonReader casts all ints to longs. We'll attempt to work around this by force converting
                            // longs to ints when there's no loss of precision.
                            convertedDictionary[item.Key] = (int)longValue;
                        }
                    }
                }

                tempDataDictionary = convertedDictionary;

                // If we got it from Session, remove it so that no other request gets it
                session.Remove(TempDataSessionStateKey);
            }
            else
            {
                // Since we call Save() after the response has been sent, we need to initialize an empty session
                // so that it is established before the headers are sent.
                session.Set(TempDataSessionStateKey, new byte[] { });
            }

            return tempDataDictionary;
        }

        /// <inheritdoc />
        public virtual void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Accessing Session property will throw if the session middleware is not enabled.
            var session = context.Session;

            var hasValues = (values != null && values.Count > 0);
            if (hasValues)
            {
                foreach (var item in values.Values)
                {
                    // We want to allow only simple types to be serialized in session.
                    EnsureObjectCanBeSerialized(item);
                }

                using (var memoryStream = new MemoryStream())
                {
                    using (var writer = new BsonWriter(memoryStream))
                    {
                        _jsonSerializer.Serialize(writer, values);
                        session.Set(TempDataSessionStateKey, memoryStream.ToArray());
                    }
                }
            }
            else
            {
                session.Remove(TempDataSessionStateKey);
            }
        }

        internal void EnsureObjectCanBeSerialized(object item)
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
                            typeof(SessionStateTempDataProvider).FullName, genericTypeArguments[0]);
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
                var message = Resources.FormatTempData_CannotSerializeToSession(
                    typeof(SessionStateTempDataProvider).FullName, underlyingType);
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