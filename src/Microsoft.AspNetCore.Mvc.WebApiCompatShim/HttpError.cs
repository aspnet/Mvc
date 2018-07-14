// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using ShimResources = Microsoft.AspNetCore.Mvc.WebApiCompatShim.Resources;

namespace System.Web.Http
{
    /// <summary>
    /// Defines a serializable container for storing error information. This information is stored
    /// as key/value pairs. The dictionary keys to look up standard error information are available
    /// on the <see cref="HttpErrorKeys"/> type.
    /// </summary>
    [XmlRoot("Error")]
    public sealed class HttpError : Dictionary<string, object>, IXmlSerializable
    {
        // Silly spelling of empty1111 to avoid collisions with other ModelState entries.
        private static readonly string EmptyKey = "\x00C9m\x00FEt\x00DD1111";

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpError"/> class.
        /// </summary>
        public HttpError()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpError"/> class containing error message
        /// <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The error message to associate with this instance.</param>
        public HttpError(string message)
            : this()
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpError"/> class for <paramref name="exception"/>.
        /// </summary>
        /// <param name="exception">The exception to use for error information.</param>
        /// <param name="includeErrorDetail">
        /// <c>true</c> to include the exception information in the error;<c>false</c> otherwise.
        /// </param>
        public HttpError(Exception exception, bool includeErrorDetail)
            : this()
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            Message = ShimResources.HttpError_GenericError;

            if (includeErrorDetail)
            {
                Add(HttpErrorKeys.ExceptionMessageKey, exception.Message);
                Add(HttpErrorKeys.ExceptionTypeKey, exception.GetType().FullName);
                Add(HttpErrorKeys.StackTraceKey, exception.StackTrace);
                if (exception.InnerException != null)
                {
                    Add(HttpErrorKeys.InnerExceptionKey, new HttpError(exception.InnerException, includeErrorDetail));
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpError"/> class for <paramref name="modelState"/>.
        /// </summary>
        /// <param name="modelState">The invalid model state to use for error information.</param>
        /// <param name="includeErrorDetail">
        /// <c>true</c> to include exception messages in the error; <c>false</c> otherwise.
        /// </param>
        public HttpError(ModelStateDictionary modelState, bool includeErrorDetail)
            : this()
        {
            if (modelState == null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }

            if (modelState.IsValid)
            {
                throw new ArgumentException(ShimResources.HttpError_ValidModelState, nameof(modelState));
            }

            Message = ShimResources.HttpError_BadRequest;

            var modelStateError = new HttpError();
            foreach (var keyModelStatePair in modelState)
            {
                var key = keyModelStatePair.Key;
                var errors = keyModelStatePair.Value.Errors;
                if (errors != null && errors.Count > 0)
                {
                    var errorMessages = errors.Select(error =>
                    {
                        if (includeErrorDetail && error.Exception != null)
                        {
                            return error.Exception.Message;
                        }
                        else
                        {
                            return
                                string.IsNullOrEmpty(error.ErrorMessage) ?
                                    ShimResources.HttpError_GenericError :
                                    error.ErrorMessage;
                        }
                    }).ToArray();

                    modelStateError.Add(key, errorMessages);
                }
            }

            Add(HttpErrorKeys.ModelStateKey, modelStateError);
        }

        /// <summary>
        /// The high-level, user-visible message explaining the cause of the error. Information carried in this field
        /// should be considered public in that it will go over the wire regardless of the value of error detail
        /// policy. As a result care should be taken not to disclose sensitive information about the server or the
        /// application.
        /// </summary>
        public string Message
        {
            get { return GetPropertyValue<string>(HttpErrorKeys.MessageKey); }
            set { this[HttpErrorKeys.MessageKey] = value; }
        }

        /// <summary>
        /// The <see cref="ModelState"/> containing information about the errors that occurred during model binding.
        /// </summary>
        /// <remarks>
        /// The inclusion of <see cref="System.Exception"/> information carried in the <see cref="ModelState"/> is
        /// controlled by the error detail policy. All other information in the <see cref="ModelState"/>
        /// should be considered public in that it will go over the wire. As a result care should be taken not to
        /// disclose sensitive information about the server or the application.
        /// </remarks>
        public HttpError ModelState
        {
            get { return GetPropertyValue<HttpError>(HttpErrorKeys.ModelStateKey); }
        }

        /// <summary>
        /// A detailed description of the error intended for the developer to understand exactly what failed.
        /// </summary>
        /// <remarks>
        /// The inclusion of this field is controlled by the error detail policy. The
        /// field is expected to contain information about the server or the application that should not
        /// be disclosed broadly.
        /// </remarks>
        public string MessageDetail
        {
            get { return GetPropertyValue<string>(HttpErrorKeys.MessageDetailKey); }
            set { this[HttpErrorKeys.MessageDetailKey] = value; }
        }

        /// <summary>
        /// The message of the <see cref="System.Exception"/> if available.
        /// </summary>
        /// <remarks>
        /// The inclusion of this field is controlled by the error detail policy. The
        /// field is expected to contain information about the server or the application that should not
        /// be disclosed broadly.
        /// </remarks>
        public string ExceptionMessage
        {
            get { return GetPropertyValue<string>(HttpErrorKeys.ExceptionMessageKey); }
            set { this[HttpErrorKeys.ExceptionMessageKey] = value; }
        }

        /// <summary>
        /// The type of the <see cref="System.Exception"/> if available.
        /// </summary>
        /// <remarks>
        /// The inclusion of this field is controlled by the error detail policy. The
        /// field is expected to contain information about the server or the application that should not
        /// be disclosed broadly.
        /// </remarks>
        public string ExceptionType
        {
            get { return GetPropertyValue<string>(HttpErrorKeys.ExceptionTypeKey); }
            set { this[HttpErrorKeys.ExceptionTypeKey] = value; }
        }

        /// <summary>
        /// The stack trace information associated with this instance if available.
        /// </summary>
        /// <remarks>
        /// The inclusion of this field is controlled by the error detail policy. The
        /// field is expected to contain information about the server or the application that should not
        /// be disclosed broadly.
        /// </remarks>
        public string StackTrace
        {
            get { return GetPropertyValue<string>(HttpErrorKeys.StackTraceKey); }
            set { this[HttpErrorKeys.StackTraceKey] = value; }
        }

        /// <summary>
        /// The inner <see cref="System.Exception"/> associated with this instance if available.
        /// </summary>
        /// <remarks>
        /// The inclusion of this field is controlled by the error detail policy. The
        /// field is expected to contain information about the server or the application that should not
        /// be disclosed broadly.
        /// </remarks>
        public HttpError InnerException
        {
            get { return GetPropertyValue<HttpError>(HttpErrorKeys.InnerExceptionKey); }
        }

        /// <summary>
        /// Gets a particular property value from this error instance.
        /// </summary>
        /// <typeparam name="TValue">The type of the property.</typeparam>
        /// <param name="key">The name of the error property.</param>
        /// <returns>The value of the error property.</returns>
        public TValue GetPropertyValue<TValue>(string key)
        {
            if (TryGetValue(key, out var value))
            {
                // Special case for round-tripping an HttpError when using JSON.
                if (typeof(TValue) == typeof(HttpError) &&
                    value is JObject jObject)
                {
                    value = GetHttpError(jObject);
                }

                if (value is TValue result)
                {
                    return result;
                }
            }

            return default;
        }

        private HttpError GetHttpError(JObject jObject)
        {
            var httpError = new HttpError();
            foreach (var kvp in jObject)
            {
                var key = kvp.Key;
                if (string.Equals(EmptyKey, key, StringComparison.Ordinal))
                {
                    key = string.Empty;
                }

                // In the ModelState case, HttpError uses string[] values but in the InnerException case, it
                // uses string values. However InnerExceptions can nest...
                object value;
                switch (kvp.Value)
                {
                    case JArray jArray:
                        value = jArray.Values<string>().ToArray();
                        break;

                    case JObject innerObject:
                        value = GetHttpError(innerObject);
                        break;

                    default:
                        value = kvp.Value.Value<string>();
                        break;
                }

                httpError.Add(key, value);
            }

            return httpError;
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            ReadXml(reader, this);
        }

        private void ReadXml(XmlReader reader, HttpError httpError)
        {
            if (reader.IsEmptyElement)
            {
                reader.Read();
                return;
            }

            reader.ReadStartElement();
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                var key = XmlConvert.DecodeName(reader.LocalName);
                object value;
                if (string.Equals(HttpErrorKeys.InnerExceptionKey, key, StringComparison.Ordinal) ||
                    string.Equals(HttpErrorKeys.ModelStateKey, key, StringComparison.Ordinal))
                {
                    var innerReader = reader.ReadSubtree();
                    var innerError = new HttpError();
                    ReadXml(innerReader, innerError);
                    value = innerError;
                }
                else
                {
                    value = reader.ReadInnerXml();
                }

                if (string.Equals(EmptyKey, key, StringComparison.Ordinal))
                {
                    key = string.Empty;
                }

                httpError.Add(key, value);
                reader.MoveToContent();
            }

            reader.ReadEndElement();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            foreach (var keyValuePair in this)
            {
                var key = keyValuePair.Key;
                var value = keyValuePair.Value;
                if (string.IsNullOrEmpty(key))
                {
                    key = EmptyKey;
                }

                writer.WriteStartElement(XmlConvert.EncodeLocalName(key));
                if (value != null)
                {
                    if (!(value is HttpError innerError))
                    {
                        writer.WriteValue(value);
                    }
                    else
                    {
                        ((IXmlSerializable)innerError).WriteXml(writer);
                    }
                }

                writer.WriteEndElement();
            }
        }
    }
}
