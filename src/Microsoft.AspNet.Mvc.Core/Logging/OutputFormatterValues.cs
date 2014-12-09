// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Logging representation of an <see cref="IOutputFormatter"/>. Logged during content negotiation.
    /// </summary>
    public class OutputFormatterValues : LoggerStructureBase
    {
        public OutputFormatterValues(IOutputFormatter inner)
        {
            var formatter = inner as OutputFormatter;
            if (formatter != null)
            {
                SupportedEncodings = new List<Encoding>(formatter.SupportedEncodings);
                SupportedMediaTypes = formatter.SupportedMediaTypes.Select(s => new MediaTypeHeaderValueValues(s));
            }

            OutputFormatterType = inner?.GetType();
        }

        /// <summary>
        /// The <see cref="Type"/> of the <see cref="IOutputFormatter"/>.
        /// </summary>
        public Type OutputFormatterType { get; }

        /// <summary>
        /// See <see cref="OutputFormatter.SupportedEncodings"/>.
        /// </summary>
        public IEnumerable<Encoding> SupportedEncodings { get; }

        /// <summary>
        /// See <see cref="OutputFormatter.SupportedMediaTypes"/>.
        /// </summary>
        public IEnumerable<MediaTypeHeaderValueValues> SupportedMediaTypes { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}