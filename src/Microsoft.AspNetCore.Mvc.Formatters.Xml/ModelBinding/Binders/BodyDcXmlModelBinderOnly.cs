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
    /// An <see cref="IModelBinder"/> which binds models from the request body using an <see cref="XmlDataContractSerializerInputFormatter"/> only
    /// when a model has the binding source <see cref="BindingSource.Body"/>.
    /// </summary>
    public class BodyDcXmlModelBinderOnly : BodyModelBinder
    {
        /// <summary>
        /// Creates a new <see cref="BodyDcXmlModelBinderOnly"/>.
        /// </summary>
        /// <param name="readerFactory">
        /// The <see cref="IHttpRequestStreamReaderFactory"/>, used to create <see cref="System.IO.TextReader"/>
        /// instances for reading the request body.
        /// </param>
        public BodyDcXmlModelBinderOnly(IHttpRequestStreamReaderFactory readerFactory) : base(new List<IInputFormatter>() { new XmlDataContractSerializerInputFormatter() }, readerFactory)
        {
        }
    }
}
