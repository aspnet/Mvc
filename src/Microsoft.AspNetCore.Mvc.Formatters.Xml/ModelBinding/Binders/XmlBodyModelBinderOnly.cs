// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Internal;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    /// <summary>
    /// An <see cref="IModelBinder"/> which binds models from the request body using an <see cref="XmlSerializerInputFormatter"/> only
    /// when a model has the binding source <see cref="BindingSource.Body"/>.
    /// </summary>
    public class XmlBodyModelBinderOnly : BodyModelBinder
    {
        /// <summary>
        /// Creates a new <see cref="XmlBodyModelBinderOnly"/>.
        /// </summary>
        /// <param name="readerFactory">
        /// The <see cref="IHttpRequestStreamReaderFactory"/>, used to create <see cref="System.IO.TextReader"/>
        /// instances for reading the request body.
        /// </param>
        public XmlBodyModelBinderOnly(IHttpRequestStreamReaderFactory readerFactory) : base(new List<IInputFormatter>() { new XmlSerializerInputFormatter() }, readerFactory)
        {
        }
    }
}
