// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc.Formatters.Xml
{
    public class FromXmlBodyAttributeTest
    {
        [Theory]
        [InlineData(XmlSerializerType.XmlSeriralizer, false,typeof(XmlBodyModelBinder))]
        [InlineData(XmlSerializerType.XmlSeriralizer, true,typeof(XmlBodyModelBinderOnly))]
        [InlineData(XmlSerializerType.DataContractSerializer, false, typeof(DcXmlBodyModelBinder))]
        [InlineData(XmlSerializerType.DataContractSerializer, true, typeof(DcXmlBodyModelBinderOnly))]
        public void Create_FromXmlBodyAttribute(XmlSerializerType xmlSerializerType, bool useXmlBinderOnly,Type expectedType)
        {
            // Act
            var att = new FromXmlBodyAttribute()
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