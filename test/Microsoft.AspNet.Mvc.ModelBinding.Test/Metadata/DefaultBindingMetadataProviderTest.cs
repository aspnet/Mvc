// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
{
    public class DefaultModelMetadataBindingDetailsProviderTest
    {
        [Fact]
        public void GetBindingDetails_FindsBinderTypeProvider()
        {
            // Arrange
            var attributes = new object[]
            {
                new ModelBinderAttribute() { BinderType = typeof(HeaderModelBinder) },
                new ModelBinderAttribute() { BinderType = typeof(ArrayModelBinder<string>) },
            };

            var context = new BindingMetadataProviderContext(
                ModelMetadataIdentity.ForType(typeof(string)), 
                attributes);

            var provider = new DefaultBindingMetadataProvider();

            // Act
            provider.GetBindingMetadata(context);

            // Assert
            Assert.Equal(typeof(HeaderModelBinder), context.BindingMetadata.BinderType);
        }

        [Fact]
        public void GetBindingDetails_FindsBinderTypeProvider_IfNullFallsBack()
        {
            // Arrange
            var attributes = new object[]
            {
                new ModelBinderAttribute(),
                new ModelBinderAttribute() { BinderType = typeof(HeaderModelBinder) },
                new ModelBinderAttribute() { BinderType = typeof(ArrayModelBinder<string>) },
            };

            var context = new BindingMetadataProviderContext(
                ModelMetadataIdentity.ForType(typeof(string)),
                attributes);

            var provider = new DefaultBindingMetadataProvider();

            // Act
            provider.GetBindingMetadata(context);

            // Assert
            Assert.Equal(typeof(HeaderModelBinder), context.BindingMetadata.BinderType);
        }

        [Fact]
        public void GetBindingDetails_FindsModelName()
        {
            // Arrange
            var attributes = new object[]
            {
                new ModelBinderAttribute() { Name = "Product" },
                new ModelBinderAttribute() { Name = "Order" },
            };

            var context = new BindingMetadataProviderContext(
                ModelMetadataIdentity.ForType(typeof(string)),
                attributes);

            var provider = new DefaultBindingMetadataProvider();

            // Act
            provider.GetBindingMetadata(context);

            // Assert
            Assert.Equal("Product", context.BindingMetadata.BinderModelName);
        }

        [Fact]
        public void GetBindingDetails_FindsModelName_IfNullFallsBack()
        {
            // Arrange
            var attributes = new object[]
            {
                new ModelBinderAttribute(),
                new ModelBinderAttribute() { Name = "Product" },
                new ModelBinderAttribute() { Name = "Order" },
            };

            var context = new BindingMetadataProviderContext(
                ModelMetadataIdentity.ForType(typeof(string)),
                attributes);

            var provider = new DefaultBindingMetadataProvider();

            // Act
            provider.GetBindingMetadata(context);

            // Assert
            Assert.Equal("Product", context.BindingMetadata.BinderModelName);
        }

        [Fact]
        public void GetBindingDetails_FindsBindingSource()
        {
            // Arrange
            var attributes = new object[]
            {
                new ModelBinderAttribute() { BindingSource = BindingSource.Body },
                new ModelBinderAttribute() { BindingSource = BindingSource.Query },
            };

            var context = new BindingMetadataProviderContext(
                ModelMetadataIdentity.ForType(typeof(string)),
                attributes);

            var provider = new DefaultBindingMetadataProvider();

            // Act
            provider.GetBindingMetadata(context);

            // Assert
            Assert.Equal(BindingSource.Body, context.BindingMetadata.BindingSource);
        }

        [Fact]
        public void GetBindingDetails_FindsBindingSource_IfNullFallsBack()
        {
            // Arrange
            var attributes = new object[]
            {
                new ModelBinderAttribute(),
                new ModelBinderAttribute() { BindingSource = BindingSource.Body },
                new ModelBinderAttribute() { BindingSource = BindingSource.Query },
            };

            var context = new BindingMetadataProviderContext(
                ModelMetadataIdentity.ForType(typeof(string)),
                attributes);

            var provider = new DefaultBindingMetadataProvider();

            // Act
            provider.GetBindingMetadata(context);

            // Assert
            Assert.Equal(BindingSource.Body, context.BindingMetadata.BindingSource);
        }

        [Fact]
        public void GetBindingMetadata_BindingBehavior_Defaults()
        {
            // Arrange
            var key = ModelMetadataIdentity.ForProperty(
                typeof(string),
                nameof(BindingBehaviorOnProperties.Defaults),
                typeof(BindingBehaviorOnProperties));

            var context = new BindingMetadataProviderContext(
                key,
                attributes: new object[0]);

            var provider = new DefaultBindingMetadataProvider();

            // Act
            provider.GetBindingMetadata(context);

            // Assert
            Assert.True(context.BindingMetadata.CanBeBound);
            Assert.Null(context.BindingMetadata.IsRequired);
        }

        [Fact]
        public void GetBindingMetadata_BindingBehaviorRequired()
        {
            // Arrange
            var key = ModelMetadataIdentity.ForProperty(
                typeof(string),
                nameof(BindingBehaviorOnProperties.BindingBehaviorRequired),
                typeof(BindingBehaviorOnProperties));

            var context = new BindingMetadataProviderContext(
                key,
                attributes: new object[0]);

            var provider = new DefaultBindingMetadataProvider();

            // Act
            provider.GetBindingMetadata(context);

            // Assert
            Assert.True(context.BindingMetadata.CanBeBound);
            Assert.True(context.BindingMetadata.IsRequired);
        }

        [Fact]
        public void GetBindingMetadata_BindingBehaviorNever()
        {
            // Arrange
            var key = ModelMetadataIdentity.ForProperty(
                typeof(string),
                nameof(BindingBehaviorOnProperties.BindingBehaviorNever),
                typeof(BindingBehaviorOnProperties));

            var context = new BindingMetadataProviderContext(
                key,
                attributes: new object[0]);

            var provider = new DefaultBindingMetadataProvider();

            // Act
            provider.GetBindingMetadata(context);

            // Assert
            Assert.False(context.BindingMetadata.CanBeBound);
            Assert.Null(context.BindingMetadata.IsRequired);
        }

        // BindingBehavior.Optional leaves both settings with their defaults.
        [Fact]
        public void GetBindingMetadata_FindsBindingBehavior_Optional()
        {
            // Arrange
            var key = ModelMetadataIdentity.ForProperty(
                typeof(string),
                nameof(BindingBehaviorOnProperties.BindingBehaviorOptional),
                typeof(BindingBehaviorOnProperties));

            var context = new BindingMetadataProviderContext(
                key,
                attributes: new object[0]);

            var provider = new DefaultBindingMetadataProvider();

            // Act
            provider.GetBindingMetadata(context);

            // Assert
            Assert.True(context.BindingMetadata.CanBeBound);
            Assert.Null(context.BindingMetadata.IsRequired);
        }

        [Fact]
        public void GetBindingMetadata_BindRequired()
        {
            // Arrange
            var key = ModelMetadataIdentity.ForProperty(
                typeof(string),
                nameof(BindingBehaviorOnProperties.BindRequired),
                typeof(BindingBehaviorOnProperties));

            var context = new BindingMetadataProviderContext(
                key,
                attributes: new object[0]);

            var provider = new DefaultBindingMetadataProvider();

            // Act
            provider.GetBindingMetadata(context);

            // Assert
            Assert.True(context.BindingMetadata.CanBeBound);
            Assert.True(context.BindingMetadata.IsRequired);
        }

        [Fact]
        public void GetBindingMetadata_BindNever()
        {
            // Arrange
            var key = ModelMetadataIdentity.ForProperty(
                typeof(string),
                nameof(BindingBehaviorOnProperties.BindNever),
                typeof(BindingBehaviorOnProperties));

            var context = new BindingMetadataProviderContext(
                key,
                attributes: new object[0]);

            var provider = new DefaultBindingMetadataProvider();

            // Act
            provider.GetBindingMetadata(context);

            // Assert
            Assert.False(context.BindingMetadata.CanBeBound);
            Assert.Null(context.BindingMetadata.IsRequired);
        }

        // We only look at BindingBehaviorAttribute for a model property
        [Fact]
        public void GetBindingMetadata_IgnoresBindingBehavior_ForType()
        {
            // Arrange
            var context = new BindingMetadataProviderContext(
                ModelMetadataIdentity.ForType(typeof(BindingBehaviorFallbackToContainer)),
                attributes: new object[0]);

            var provider = new DefaultBindingMetadataProvider();

            // Act
            provider.GetBindingMetadata(context);

            // Assert
            Assert.True(context.BindingMetadata.CanBeBound);
            Assert.Null(context.BindingMetadata.IsRequired);
        }

        [Fact]
        public void GetBindingMetadata_IgnoresAttributesOnModelType()
        {
            // Arrange
            var key = ModelMetadataIdentity.ForProperty(
                typeof(BindingBehaviorFallbackToContainer),
                nameof(BindingBehaviorOnProperties.IgnoresModelType),
                typeof(BindingBehaviorOnProperties));

            var context = new BindingMetadataProviderContext(
                key,
                attributes: new object[0]);

            var provider = new DefaultBindingMetadataProvider();

            // Act
            provider.GetBindingMetadata(context);

            // Assert
            Assert.True(context.BindingMetadata.CanBeBound);
            Assert.Null(context.BindingMetadata.IsRequired);
        }

        // BindingBehaviorAttribute is peculiar in that we fallback to the container
        // type not the model type.
        [Fact]
        public void GetBindingMetadata_BindingBehavior_FallbackToContainer()
        {
            // Arrange
            var key = ModelMetadataIdentity.ForProperty(
                typeof(string),
                nameof(BindingBehaviorFallbackToContainer.FallbackToContainer),
                typeof(BindingBehaviorFallbackToContainer));

            var context = new BindingMetadataProviderContext(
                key,
                attributes: new object[0]);

            var provider = new DefaultBindingMetadataProvider();

            // Act
            provider.GetBindingMetadata(context);

            // Assert
            Assert.True(context.BindingMetadata.CanBeBound);
            Assert.True(context.BindingMetadata.IsRequired);
        }

        // BindingBehaviorAttribute is peculiar in that we fallback to the container
        // type not the model type.
        [Fact]
        public void GetBindingMetadata_BindingBehavior_Overridden()
        {
            // Arrange
            var key = ModelMetadataIdentity.ForProperty(
                typeof(string),
                nameof(BindingBehaviorFallbackToContainer.Overridden),
                typeof(BindingBehaviorFallbackToContainer));

            var context = new BindingMetadataProviderContext(
                key,
                attributes: new object[0]);

            var provider = new DefaultBindingMetadataProvider();

            // Act
            provider.GetBindingMetadata(context);

            // Assert
            Assert.False(context.BindingMetadata.CanBeBound);
            Assert.Null(context.BindingMetadata.IsRequired);
        }

        [BindingBehavior(BindingBehavior.Required)]
        private class BindingBehaviorFallbackToContainer
        {
            public string FallbackToContainer { get; set; }

            [BindingBehavior(BindingBehavior.Never)]
            public string Overridden { get; set; }
        }

        private class BindingBehaviorOnProperties
        {
            public string Defaults { get; set; }

            [BindingBehavior(BindingBehavior.Never)]
            public string BindingBehaviorNever { get; set; }

            [BindingBehavior(BindingBehavior.Optional)]
            public string BindingBehaviorOptional { get; set; }

            [BindingBehavior(BindingBehavior.Required)]
            public string BindingBehaviorRequired { get; set; }

            [BindNever]
            public string BindNever { get; set; }

            [BindRequired]
            public string BindRequired { get; set;}

            public BindingBehaviorFallbackToContainer IgnoresModelType { get; set; }
        }
    }
}