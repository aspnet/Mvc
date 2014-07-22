// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;

namespace Microsoft.AspNet.Mvc
{
    public class OutputFormatterContext
    {
        public object Object { get; set; }

        public Type DeclaredType { get; set; }

        public ActionContext ActionContext { get; set; }

        public Encoding SelectedEncoding { get; set; }

        public MediaTypeHeaderValue SelectedContentType { get; set; }
    }
}
