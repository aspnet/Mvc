// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Microsoft.AspNetCore.Mvc.Formatters.Xml
{
    /// <summary>
    /// Specifies that a parameter or property should be bound using the request body XML.
    /// Requires the XML DataContractSerializer formatters or/and the XML Serializer formatters to be add to MVC.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FromBodyXmlAttribute : Attribute, IBinderTypeProviderMetadata
    {
        /// <inheritdoc />
        public BindingSource BindingSource => BindingSource.Body;

        /// Gets the proper type of the XML binder provider
        /// <inheritdoc />
        ///<remarks> Requires the XML DataContractSerializer formatters or/and the XML Serializer formatters to be add to MVC.</remarks>
        public Type BinderType => UseXmlBinderOnly ?
                                    (XmlSerializerType == XmlSerializerType.DataContractSerializer ? typeof(BodyDcXmlModelBinderOnly) : typeof(BodyXmlModelBinderOnly)) :
                                    (XmlSerializerType == XmlSerializerType.DataContractSerializer ? typeof(BodyDcXmlModelBinder) : typeof(BodyXmlModelBinder));

        /// <summary>
        /// Gets or sets the flag that selects a Data Contract XML input formatter.
        /// </summary>
        public XmlSerializerType XmlSerializerType { get; set; }

        /// <summary>
        /// Gets or sets the flag that limits an input formatter to  XML  or Data Contract XML <see cref="XmlSerializerType"/>.
        /// </summary>
        public bool UseXmlBinderOnly { get; set; }

    }
}
