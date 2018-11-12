// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.DataAnnotations
{
    public class DataMemberRequiredBindingMetadataProviderTest
    {
        [Fact]
        public void IsBindingRequired_SetToTrue_WithDataMemberIsRequiredTrue()
        {
            // Arrange
            var provider = new DataMemberRequiredBindingMetadataProvider();

            var attributes = new object[]
            {
                new DataMemberAttribute() { IsRequired = true, }
            };

            var key = ModelMetadataIdentity.ForProperty(
                typeof(string),
                nameof(ClassWithDataMemberIsRequiredTrue.StringProperty),
                typeof(ClassWithDataMemberIsRequiredTrue));
            var context = new BindingMetadataProviderContext(key, GetModelAttributes(new object[0], attributes));

            // Act
            provider.CreateBindingMetadata(context);

            // Assert
            Assert.True(context.BindingMetadata.IsBindingRequired);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsBindingRequired_LeftAlone_DataMemberIsRequiredFalse(bool initialValue)
        {
            // Arrange
            var provider = new DataMemberRequiredBindingMetadataProvider();

            var attributes = new object[]
            {
                new DataMemberAttribute() { IsRequired = false, }
            };

            var key = ModelMetadataIdentity.ForProperty(
                typeof(string),
                nameof(ClassWithDataMemberIsRequiredFalse.StringProperty),
                typeof(ClassWithDataMemberIsRequiredFalse));
            var context = new BindingMetadataProviderContext(key, GetModelAttributes(new object[0], attributes));

            context.BindingMetadata.IsBindingRequired = initialValue;

            // Act
            provider.CreateBindingMetadata(context);

            // Assert
            Assert.Equal(initialValue, context.BindingMetadata.IsBindingRequired);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsBindingRequired_LeftAlone_ForNonPropertyMetadata(bool initialValue)
        {
            // Arrange
            var provider = new DataMemberRequiredBindingMetadataProvider();

            var attributes = new object[]
            {
                new DataMemberAttribute() { IsRequired = true, }
            };

            var key = ModelMetadataIdentity.ForType(typeof(ClassWithDataMemberIsRequiredTrue));
            var context = new BindingMetadataProviderContext(key, GetModelAttributes(new object[0], attributes));

            context.BindingMetadata.IsBindingRequired = initialValue;

            // Act
            provider.CreateBindingMetadata(context);

            // Assert
            Assert.Equal(initialValue, context.BindingMetadata.IsBindingRequired);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsBindingRequired_LeftAlone_WithoutDataMemberAttribute(bool initialValue)
        {
            // Arrange
            var provider = new DataMemberRequiredBindingMetadataProvider();

            var key = ModelMetadataIdentity.ForProperty(
                typeof(string),
                nameof(ClassWithoutAttributes.StringProperty),
                typeof(ClassWithoutAttributes));
            var context = new BindingMetadataProviderContext(key, GetModelAttributes(new object[0], new object[0]));

            context.BindingMetadata.IsBindingRequired = initialValue;

            // Act
            provider.CreateBindingMetadata(context);

            // Assert
            Assert.Equal(initialValue, context.BindingMetadata.IsBindingRequired);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsBindingRequired_LeftAlone_WithoutDataContractAttribute(bool initialValue)
        {
            // Arrange
            var provider = new DataMemberRequiredBindingMetadataProvider();

            var attributes = new object[]
            {
                new DataMemberAttribute() { IsRequired = true, }
            };

            var key = ModelMetadataIdentity.ForProperty(
                typeof(string),
                nameof(ClassWithDataMemberIsRequiredTrueWithoutDataContract.StringProperty),
                typeof(ClassWithDataMemberIsRequiredTrueWithoutDataContract));
            var context = new BindingMetadataProviderContext(key, GetModelAttributes(new object[0], attributes));

            context.BindingMetadata.IsBindingRequired = initialValue;

            // Act
            provider.CreateBindingMetadata(context);

            // Assert
            Assert.Equal(initialValue, context.BindingMetadata.IsBindingRequired);
        }

        private ModelAttributes GetModelAttributes(
            IEnumerable<object> typeAttributes,
            IEnumerable<object> propertyAttributes)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var modelAttributes = new ModelAttributes(propertyAttributes, typeAttributes);
#pragma warning restore CS0618 // Type or member is obsolete

            return modelAttributes;
        }

        [DataContract]
        private class ClassWithDataMemberIsRequiredTrue
        {
            [DataMember(IsRequired = true)]
            public string StringProperty { get; set; }
        }

        [DataContract]
        private class ClassWithDataMemberIsRequiredFalse
        {
            [DataMember(IsRequired = false)]
            public string StringProperty { get; set; }
        }

        private class ClassWithDataMemberIsRequiredTrueWithoutDataContract
        {
            [DataMember(IsRequired = true)]
            public string StringProperty { get; set; }
        }

        private class ClassWithoutAttributes
        {
            public string StringProperty { get; set; }
        }
    }
}
