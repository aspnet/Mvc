// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public abstract class InputFormatter
    {
        /// <inheritdoc />
        public IList<MediaTypeHeaderValue> SupportedMediaTypes { get; private set; }

        /// <inheritdoc />
        public IList<Encoding> SupportedEncodings { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputFormatter"/> class.
        /// </summary>
        protected InputFormatter()
        {
            SupportedEncodings = new List<Encoding>();
            SupportedMediaTypes = new List<MediaTypeHeaderValue>();
        }

        /// <inheritdoc />
        public abstract Task ReadAsync(InputFormatterContext context);
    }
}
