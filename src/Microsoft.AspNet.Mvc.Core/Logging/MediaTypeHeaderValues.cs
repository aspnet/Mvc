// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Logging representation of <see cref="MediaTypeHeaderValue"/>. 
    /// </summary>
    public class MediaTypeHeaderValueValues : LoggerStructureBase
    {
        public MediaTypeHeaderValueValues([NotNull] MediaTypeHeaderValue inner)
        {
            Charset = inner.Charset;
            MediaType = inner.MediaType;
            MediaSubType = inner.MediaSubType;
            MediaTypeRange = inner.MediaTypeRange;
            Parameters = new Dictionary<string, string>(inner.Parameters);
        }

        /// <summary>
        /// See <see cref="MediaTypeHeaderValue.Charset"/>.
        /// </summary>
        public string Charset { get; }

        /// <summary>
        /// See <see cref="MediaTypeHeaderValue.MediaType"/>.
        /// </summary>
        public string MediaType { get; }

        /// <summary>
        /// See <see cref="MediaTypeHeaderValue.MediaSubType"/>.
        /// </summary>
        public string MediaSubType { get; }

        /// <summary>
        /// See <see cref="MediaTypeHeaderValue.MediaTypeRange"/>.
        /// </summary>
        public MediaTypeHeaderValueRange MediaTypeRange { get; }

        /// <summary>
        /// See <see cref="MediaTypeHeaderValue.Parameters"/>.
        /// </summary>
        public IDictionary<string, string> Parameters { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}