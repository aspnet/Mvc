// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelBinderAttributeTest
    {
        [Fact]
        public void NoBinderType_NoBindingSource()
        {
            // Arrange
            var attribute = new ModelBinderAttribute();

            // Act
            var source = attribute.BindingSource;

            // Assert
            Assert.Null(source);
        }

        [Fact]
        public void BinderType_DefaultCustomBindingSource()
        {
            // Arrange
            var attribute = new ModelBinderAttribute();
            attribute.BinderType = typeof(ByteArrayModelBinder);

            // Act
            var source = attribute.BindingSource;

            // Assert
            Assert.Equal(BindingSource.Custom, source);
        }

        [Fact]
        public void BinderType_SettingBindingSource_OverridesDefaultCustomBindingSource()
        {
            // Arrange
            var attribute = new FromQueryModelBinderAttribute();

            // Act
            var source = attribute.BindingSource;

            // Assert
            Assert.Equal(BindingSource.Query, source);
        }

        private class FromQueryModelBinderAttribute : ModelBinderAttribute
        {
            // Not the perfect way to override this property since setting its value (possible when not using the class
            // as an attribute) has no effect. base.BindingSource && BindingSource.Query may be right for some cases.
            public override BindingSource BindingSource => BindingSource.Query;
        }
    }
}