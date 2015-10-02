﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;

namespace Microsoft.AspNet.Mvc.Infrastructure
{
    /// <summary>
    /// Creates <see cref="TextWriter"/> instances for writing to <see cref="Http.HttpResponse.Body"/>.
    /// </summary>
    public interface IHttpResponseStreamWriterFactory
    {
        /// <summary>
        /// Creates a new <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/>, usually <see cref="Http.HttpResponse.Body"/>.</param>
        /// <param name="encoding">The <see cref="Encoding"/>, usually <see cref="Encoding.UTF8"/>.</param>
        /// <returns>A <see cref="TextWriter"/>.</returns>
        TextWriter CreateWriter(Stream stream, Encoding encoding);
    }
}
