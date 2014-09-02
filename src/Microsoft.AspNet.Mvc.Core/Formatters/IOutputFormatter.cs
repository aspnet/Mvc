// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Writes an object to the output stream.
    /// </summary>
    public interface IOutputFormatter
    {
        /// <summary>
        /// Determines whether this <see cref="IOutputFormatter"/> can serialize
        /// an object of the specified type.
        /// </summary>
        /// <param name="context">The formatter context associated with the call.</param>
        /// <param name="contentType">The desired contentType on the response.</param>
        /// <returns>True if this <see cref="IOutputFormatter"/> supports the passed in 
        /// <paramref name="contentType"/> and is able to serialize the object
        /// represent by <paramref name="context"/>'s Object property.
        /// False otherwise.</returns>
        bool CanWriteResult(OutputFormatterContext context, MediaTypeHeaderValue contentType);

        /// <summary>
        /// Gets all possible values of <see cref="MediaTypeHeaderValue"/> for the provided combination of 
        /// <paramref name="declaredType"/>, <paramref name="actualType"/>, and <paramref name="contentType"/>.
        /// </summary>
        /// <param name="declaredType">The declared return type of the action method, may be null.</param>
        /// <param name="actualType">The actual return type of the action method, may be null.</param>
        /// <param name="contentType">The desired content type, may be null.</param>
        /// <returns>A collection of supported media types, or null.</returns>
        /// <remarks>
        /// If <paramref name="contentType"/> is null, then a formatter should return an ordered list of
        /// preferred content types.
        /// </remarks>
        IEnumerable<MediaTypeHeaderValue> GetAllPossibleContentTypes(
            Type declaredType,
            Type actualType,
            MediaTypeHeaderValue contentType);

        /// <summary>
        /// Writes the object represented by <paramref name="context"/>'s Object property.
        /// </summary>
        /// <param name="context">The formatter context associated with the call.</param>
        /// <returns>A Task that serializes the value to the <paramref name="context"/>'s response message.</returns>
        Task WriteAsync(OutputFormatterContext context);
    }
}
