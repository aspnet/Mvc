// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;

namespace Microsoft.AspNet.Mvc
{
    public class OutputFormatterContext
    {
        public ObjectResult ObjectResult { get; set; }

        public Type DeclaredType { get; set; }

        public HttpContext HttpContext { get; set; }

        /// <summary>
        /// The selected content type is only available after a formatter has been selected.
        /// </summary>
        public MediaTypeHeaderValue SelectedContentType { get; set; }
    }
}
