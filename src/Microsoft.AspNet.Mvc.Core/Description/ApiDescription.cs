// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc.Description
{
    /// <summary>
    /// Represents an API exposed by this application.
    /// </summary>
    public class ApiDescription
    {
        /// <summary>
        /// Creates a new instance of <see cref="ApiDescription"/>.
        /// </summary>
        public ApiDescription()
        {
            ExtensionData = new Dictionary<Type, object>();
            ParameterDescriptions = new List<ApiParameterDescription>();
            SupportedResponseFormats = new List<ApiResponseFormat>();
        }

        /// <summary>
        /// The <see cref="ActionDescriptor"/> for this api.
        /// </summary>
        public ActionDescriptor ActionDescriptor { get; set; }

        /// <summary>
        /// Stores arbitrary extension metadata associated with the <see cref="ApiDescription"/>.
        /// </summary>
        public IDictionary<Type, object> ExtensionData { get; private set; }

        /// <summary>
        /// The group name for this api.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// The supported HTTP method for this api, or null if all HTTP methods are supported.
        /// </summary>
        public string HttpMethod { get; set; }

        /// <summary>
        /// The list of <see cref="ApiParameterDescription"/> for this api.
        /// </summary>
        public List<ApiParameterDescription> ParameterDescriptions { get; private set; }

        /// <summary>
        /// The relative url path template (relative to application root) for this api.
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// The <see cref="ModelMetadata"/> for the <see cref="ResponseType"/> or null.
        /// </summary>
        /// <remarks>
        /// Will be null if <see cref="ResponseType"/> is null.
        /// </remarks>
        public ModelMetadata ResponseModelMetadata { get; set; }

        /// <summary>
        /// The CLR data type of the response or null.
        /// </summary>
        /// <remarks>
        /// Will be null if the action returns no response, or if the response type is unclear. Use 
        /// <see cref="ProducesAttribute"/> on an action method to specify a response type.
        /// </remarks>
        public Type ResponseType { get; set; }

        /// <summary>
        /// A list of possible formats for a response.
        /// </summary>
        /// <remarks>
        /// Will be empty if the action returns no response, or if the response type is unclear. Use
        /// <see cref="ProducesAttribute"/> on an action method to specify a response type.
        /// </remarks>
        public IList<ApiResponseFormat> SupportedResponseFormats { get; private set; }

        /// <summary>
        /// Gets the value of an extension data from the <see cref="ExtensionData"/> collection.
        /// </summary>
        /// <typeparam name="T">The type of the extension data.</typeparam>
        /// <returns>The extension data or the default value of <typeparamref name="T"/>.</returns>
        public T GetExtension<T>()
        {
            object value;
            if (ExtensionData.TryGetValue(typeof(T), out value))
            {
                return (T)value;
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Sets the value of an extension data in the <see cref="ExtensionData"/> collection.
        /// </summary>
        /// <typeparam name="T">The type of the extension data.</typeparam>
        /// <param name="value">The value of an extension data.</param>
        public void SetExtension<T>([NotNull] T value)
        {
            ExtensionData[typeof(T)] = value;
        }
    }
}