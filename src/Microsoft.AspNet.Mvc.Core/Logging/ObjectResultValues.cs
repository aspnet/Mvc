// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Logging representation of an <see cref="ObjectResult"/>. Logged during content negotiation.
    /// If the selected formatter is null, no valid formatter was found.
    /// </summary>
    public class ObjectResultValues : LoggerStructureBase
    {
        public ObjectResultValues([NotNull] ObjectResult inner, [NotNull] HttpContext context, IOutputFormatter selected)
        {
            ValueType = inner.Value?.GetType();
            Formatters = inner.Formatters.Select(f => new OutputFormatterValues(f));
            ContentTypes = inner.ContentTypes.Select(c => new MediaTypeHeaderValueValues(c));
            DeclaredType = inner.DeclaredType;
            SelectedFormatter = new OutputFormatterValues(selected);
            AcceptHeader = context.Request.Headers["Accept"];
            ContentTypeHeader = context.Request.Headers["Content-Type"];
        }

        /// <summary>
        /// The <see cref="Type"/> of <see cref="ObjectResult.Value"/>.
        /// </summary>
        public Type ValueType { get; }

        /// <summary>
        /// See <see cref="ObjectResult.Formatters"/>.
        /// </summary>
        public IEnumerable<OutputFormatterValues> Formatters { get; }

        /// <summary>
        /// See <see cref="ObjectResult.ContentTypes"/>.
        /// </summary>
        public IEnumerable<MediaTypeHeaderValueValues> ContentTypes { get; }

        /// <summary>
        /// See <see cref="ObjectResult.DeclaredType"/>.
        /// </summary>
        public Type DeclaredType { get; }

        /// <summary>
        /// The formatter that was selected as <see cref="OutputFormatterValues"/>.
        /// </summary>
        public OutputFormatterValues SelectedFormatter { get; }

        /// <summary>
        /// The accept header on the Http context.
        /// </summary>
        public string AcceptHeader { get; }

        /// <summary>
        /// The content-type header on the Http Context.
        /// </summary>
        public string ContentTypeHeader { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}