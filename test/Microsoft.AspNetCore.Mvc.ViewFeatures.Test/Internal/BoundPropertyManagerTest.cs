// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public class BoundPropertyManagerTest
    {
        [Fact]
        public void Create_ProducesEmptyListIfNoPropertyHasAttributes()
        {
            // Arrange
            var viewOptions = new MvcViewOptions();
            var type = typeof(TestController_WithoutAttributedProperties);

            // Act
            var manager = BoundPropertyManager.Create(viewOptions, type);

            // Assert
            Assert.Empty(manager.PropertyItems);
        }

        [Fact]
        public void Create_ThrowsIfTempDataPropertyDoesNotHavePublicSetter()
        {
            // Arrange
            var viewOptions = new MvcViewOptions();
            var type = typeof(TestController_PrivateSet);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => BoundPropertyManager.Create(viewOptions, type));

            Assert.Equal(
                $"The '{typeof(TestController_PrivateSet).FullName}.{nameof(TestController_NonPrimitiveType.Test)}'" +
                $" property with {nameof(TempDataAttribute)} is invalid. A property using {nameof(TempDataAttribute)}" +
                " must have a public setter.",
                exception.Message);
        }

        [Fact]
        public void Create_ThrowsInvalidOperationException_NonPrimitiveType()
        {
            // Arrange
            var viewOptions = new MvcViewOptions();
            var type = typeof(TestController_NonPrimitiveType);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => BoundPropertyManager.Create(viewOptions, type));

            Assert.Equal(
                $"The '{typeof(TestController_NonPrimitiveType).FullName}.{nameof(TestController_NonPrimitiveType.Test)}'" +
                $" property with {nameof(TempDataAttribute)} is invalid. The '{typeof(TempDataSerializer).FullName}'" +
                $" cannot serialize an object of type '{typeof(Object).FullName}'.",
                exception.Message);
        }

        [Fact]
        public void Create_ThrowsInvalidOperationException_NonStringDictionaryKey()
        {
            // Arrange
            var viewOptions = new MvcViewOptions();
            var type = typeof(TestController_NonStringDictionaryKey);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => BoundPropertyManager.Create(viewOptions, type));

            Assert.Equal(
                $"The '{typeof(TestController_NonStringDictionaryKey).FullName}.{nameof(TestController_NonStringDictionaryKey.Test)}'" +
                $" property with {nameof(TempDataAttribute)} is invalid. The '{typeof(TempDataSerializer).FullName}'" +
                $" cannot serialize a dictionary with a key of type '{typeof(object)}'. The key must be of type" +
                $" '{typeof(string).FullName}'.",
                exception.Message);
        }

        [Fact]
        public void Create_FindsTempDataProperties()
        {
            // Arrange
            var viewOptions = new MvcViewOptions();
            var type = typeof(TestController_OneTempDataProperty);

            // Act
            var manager = BoundPropertyManager.Create(viewOptions, type);

            // Assert
            Assert.Collection(
                manager.PropertyItems.OrderBy(p => p.PropertyHelper.Name),
                item =>
                {
                    Assert.Equal(nameof(TestController_OneTempDataProperty.Test2), item.PropertyHelper.Name);
                    Assert.Equal(BoundPropertySource.TempData, item.Source);
                });
        }

        [Fact]
        public void Create_FindsTempDataAndViewDataProperties()
        {
            // Arrange
            var viewOptions = new MvcViewOptions
            {
                // Equivalent to 2.1 compat switch
                SuppressTempDataPropertyPrefix = true,
            };
            var type = typeof(TestController_WithViewDataAndTempDataProperties);

            // Act
            var manager = BoundPropertyManager.Create(viewOptions, type);

            // Assert
            Assert.Collection(
                manager.PropertyItems.OrderBy(p => p.PropertyHelper.Name),
                item =>
                {
                    Assert.Equal(nameof(TestController_WithViewDataAndTempDataProperties.TempData), item.PropertyHelper.Name);
                    Assert.Equal(nameof(TestController_WithViewDataAndTempDataProperties.TempData), item.SourceKey);
                    Assert.Equal(BoundPropertySource.TempData, item.Source);
                },
                item =>
                {
                    Assert.Equal(nameof(TestController_WithViewDataAndTempDataProperties.ViewDataPrivateSetter), item.PropertyHelper.Name);
                    Assert.Equal(nameof(TestController_WithViewDataAndTempDataProperties.ViewDataPrivateSetter), item.SourceKey);
                    Assert.Equal(BoundPropertySource.ViewData, item.Source);
                });
        }

        [Fact]
        public void Create_AppliesTempDataPrefix_WithCompatSwitchDisabled()
        {
            // Arrange
            var viewOptions = new MvcViewOptions
            {
                // Equivalent to 2.0 compat switch
                SuppressTempDataPropertyPrefix = false,
            };
            var type = typeof(TestController_WithViewDataAndTempDataProperties);

            // Act
            var manager = BoundPropertyManager.Create(viewOptions, type);

            // Assert
            Assert.Collection(
                manager.PropertyItems.OrderBy(p => p.PropertyHelper.Name),
                item =>
                {
                    Assert.Equal(nameof(TestController_WithViewDataAndTempDataProperties.TempData), item.PropertyHelper.Name);
                    Assert.Equal("TempDataProperty-TempData", item.SourceKey);
                    Assert.Equal(BoundPropertySource.TempData, item.Source);
                },
                item =>
                {
                    Assert.Equal(nameof(TestController_WithViewDataAndTempDataProperties.ViewDataPrivateSetter), item.PropertyHelper.Name);
                    Assert.Equal(nameof(TestController_WithViewDataAndTempDataProperties.ViewDataPrivateSetter), item.SourceKey);
                    Assert.Equal(BoundPropertySource.ViewData, item.Source);
                });
        }

        [Fact]
        public void Create_UsesKeysOnAttributes()
        {
            // Arrange
            var viewOptions = new MvcViewOptions();
            var type = typeof(TypeWithViewDataAndTempDataPropertiesWithKeys);

            // Act
            var manager = BoundPropertyManager.Create(viewOptions, type);

            // Assert
            Assert.Collection(
                manager.PropertyItems.OrderBy(p => p.PropertyHelper.Name),
                item =>
                {
                    Assert.Equal(nameof(TypeWithViewDataAndTempDataPropertiesWithKeys.TempData), item.PropertyHelper.Name);
                    Assert.Equal("MyTempDataKey", item.SourceKey);
                    Assert.Equal(BoundPropertySource.TempData, item.Source);
                },
                item =>
                {
                    Assert.Equal(nameof(TypeWithViewDataAndTempDataPropertiesWithKeys.ViewData), item.PropertyHelper.Name);
                    Assert.Equal("MyViewDataKey", item.SourceKey);
                    Assert.Equal(BoundPropertySource.ViewData, item.Source);
                });
        }

        [Fact]
        public void Populate_SetsValuesFromContext()
        {
            // Arrange
            var viewOptions = new MvcViewOptions
            {
                SuppressTempDataPropertyPrefix = true,
            };
            var instance = new TestController_WithViewDataAndTempDataProperties();
            var manager = BoundPropertyManager.Create(viewOptions, instance.GetType());
            var tempDataValues = new Dictionary<string, object>
            {
                { nameof(TestController_WithViewDataAndTempDataProperties.TempData), "TempData-Value" },
            };
            var context = CreateContext(tempDataValues);

            // Arrange
            manager.Populate(instance, context);

            // Assert
            Assert.Equal("TempData-Value", instance.TempData);
            Assert.Null(instance.ViewDataPrivateSetter);
        }

        [Fact]
        public void Save_WritesValuesToContextCollections()
        {
            // Arrange
            var viewOptions = new MvcViewOptions
            {
                SuppressTempDataPropertyPrefix = true,
            };
            var instance = new TypeWithViewDataAndTempDataProperties
            {
                TempData = "new-tempdata-value",
                ViewData = "new-vdd-value",
            };
            var manager = BoundPropertyManager.Create(viewOptions, instance.GetType());
            var tempDataValues = new Dictionary<string, object>
            {
                { nameof(TypeWithViewDataAndTempDataProperties.TempData), "old-value" },
            };
            var context = CreateContext(tempDataValues);

            // Arrange
            manager.Save(instance, context);

            // Assert
            Assert.Equal("new-tempdata-value", context.TempData[nameof(TypeWithViewDataAndTempDataProperties.TempData)]);
            Assert.Equal("new-vdd-value", context.ViewData[nameof(TypeWithViewDataAndTempDataProperties.ViewData)]);

            // Ensure the key is retained
            var tempData = (TempDataDictionary)context.TempData;
            Assert.Equal(new[] { nameof(TypeWithViewDataAndTempDataProperties.TempData) }, tempData.RetainedKeys);
        }

        [Fact]
        public void Save_DoesNotKeepPropertiesIfValueIsUnchanged()
        {
            // Arrange
            var viewOptions = new MvcViewOptions
            {
                SuppressTempDataPropertyPrefix = true,
            };
            var instance = new TypeWithViewDataAndTempDataProperties
            {
                TempData = "old-value",
            };
            var manager = BoundPropertyManager.Create(viewOptions, instance.GetType());
            var tempDataValues = new Dictionary<string, object>
            {
                { nameof(TypeWithViewDataAndTempDataProperties.TempData), "old-value" },
            };
            var context = CreateContext(tempDataValues);

            // Arrange
            manager.Save(instance, context);

            // Assert
            Assert.Equal("old-value", context.TempData[nameof(TypeWithViewDataAndTempDataProperties.TempData)]);
            Assert.Null(context.ViewData[nameof(TypeWithViewDataAndTempDataProperties.ViewData)]);

            // Ensure the key is not retained
            var tempData = (TempDataDictionary)context.TempData;
            Assert.Empty(tempData.RetainedKeys);
        }

        private static BoundPropertyContext CreateContext(IDictionary<string, object> tempDataValues)
        {
            var viewDataDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider());
            var tempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider(tempDataValues));
            var context = new BoundPropertyContext(new ActionContext(), tempData, viewDataDictionary);
            return context;
        }

        public class TestController_WithoutAttributedProperties : Controller
        {
            [FromBody]
            public object Body { get; set; }
        }

        public class TestController_NullableNonPrimitiveTempDataProperty
        {
            [TempData]
            public DateTime? DateTime { get; set; }
        }

        public class TestController_OneTempDataProperty
        {
            public string Test { get; set; }

            [TempData]
            public string Test2 { get; set; }
        }

        public class TestController_TwoTempDataProperties
        {
            [TempData]
            public string Test { get; set; }

            [TempData]
            public int Test2 { get; set; }
        }

        public class TestController_OneNullableTempDataProperty
        {
            public string Test { get; set; }

            [TempData]
            public int? Test2 { get; set; }
        }

        public class TestController_ListOfString
        {
            [TempData]
            public IList<string> Test { get; set; }
        }

        public class TestController_PrivateSet
        {
            [TempData]
            public string Test { get; private set; }
        }

        public class TestController_NonPrimitiveType
        {
            [TempData]
            public object Test { get; set; }
        }

        public class TestController_NonStringDictionaryKey
        {
            [TempData]
            public IDictionary<object, object> Test { get; set; }
        }

        public class TestController_WithViewDataAndTempDataProperties
        {
            [ViewData]
            public object ViewDataPrivateSetter { get; private set; }

            [TempData]
            public string TempData { get; set; }

            public string NonAttributed { get; set; }
        }

        public class TypeWithViewDataAndTempDataProperties
        {
            [ViewData]
            public object ViewData { get; set; }

            [TempData]
            public string TempData { get; set; }
        }

        public class TypeWithViewDataAndTempDataPropertiesWithKeys
        {
            [ViewData(Key = "MyViewDataKey")]
            public object ViewData { get; set; }

            [TempData(Key = "MyTempDataKey")]
            public string TempData { get; set; }
        }
    }
}
