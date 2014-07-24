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
        /// Gets the mutable collection of character encodings supported by
        /// this <see cref="IOutputFormatter"/> instance. The encodings are
        /// used when writing the data.
        /// </summary>
        List<Encoding> SupportedEncodings { get; }

        /// <summary>
        /// Gets the mutable collection of <see cref="MediaTypeHeaderValue"/> elements supported by
        /// this <see cref="IOutputFormatter"/> instance.
        /// </summary>
        List<MediaTypeHeaderValue> SupportedMediaTypes { get; }

        /// <summary>
        /// Determines whether this <see cref="IOutputFormatter"/> can serialize
        /// an object of the specified type.
        /// </summary>
        /// <param name="context">The formatter context associated with the call.</param>
        /// <param name="contentType">The desired contentType on the response.</param>
        /// <remarks>
        /// Subclasses can override this method to determine if the given content can be handled by this formatter.
        ///  Subclasses should call the base implementation.
        /// </remarks>
        /// <returns>True if this <see cref="IOutputFormatter"/> is able to serialize the object
        /// represent by <paramref name="context"/>'s ObjectResult and supports the passed in 
        /// <paramref name="contentType"/>. 
        /// False otherwise.</returns>
        bool CanWriteResult(OutputFormatterContext context, MediaTypeHeaderValue contentType);

        /// <summary>
        /// Writes given <paramref name="value"/> to the HttpResponse <paramref name="response"/> body stream. 
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task that serializes the value to the <paramref name="context"/>'s response message.</returns>
        Task WriteAsync(OutputFormatterContext context, CancellationToken cancellationToken);
    }
}
