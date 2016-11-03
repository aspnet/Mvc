// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc.Formatters.Xml
{
    public class FromBodyXmlAttributeTest
    {
        [Theory]
        [InlineData(XmlSerializerType.XmlSeriralizer, false,typeof(BodyXmlModelBinder))]
        [InlineData(XmlSerializerType.XmlSeriralizer, true,typeof(BodyXmlModelBinderOnly))]
        [InlineData(XmlSerializerType.DataContractSerializer, false, typeof(BodyDcXmlModelBinder))]
        [InlineData(XmlSerializerType.DataContractSerializer, true, typeof(BodyDcXmlModelBinderOnly))]
        public void Create_FromBodyXmlAttribute(XmlSerializerType xmlSerializerType, bool useXmlBinderOnly,Type expectedType)
        {
            // Act
            var att = new FromBodyXmlAttribute()
            {
                XmlSerializerType = xmlSerializerType,
                UseXmlBinderOnly = useXmlBinderOnly
            };
            //Assert

            Assert.Equal(expectedType, att.BinderType);
            Assert.Equal(BindingSource.Body, att.BindingSource);

        }
    }
}