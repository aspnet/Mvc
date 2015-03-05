// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ASPNET50
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Core;
using Microsoft.AspNet.Testing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class MutableObjectModelBinderTest
    {
        [Theory]
        [InlineData(typeof(Person), true)]
        [InlineData(typeof(Person), false)]
        [InlineData(typeof(EmptyModel), true)]
        [InlineData(typeof(EmptyModel), false)]
        public async Task CanCreateModel_CreatesModel_ForTopLevelObjectIfThereIsExplicitPrefix(
            Type modelType,
            bool isPrefixProvided)
        {
            var mockValueProvider = new Mock<IValueProvider>();
            mockValueProvider.Setup(o => o.ContainsPrefixAsync(It.IsAny<string>()))
                             .Returns(Task.FromResult(false));

            var metadataProvider = new TestModelMetadataProvider();
            if (isPrefixProvided)
            {
                metadataProvider.ForType<Person>().BindingDetails(bd => bd.BinderModelName = "prefix");
            }

            var bindingContext = new MutableObjectBinderContext
            {
                ModelBindingContext = new ModelBindingContext
                {
                    // Random type.
                    ModelMetadata = metadataProvider.GetMetadataForType(typeof(Person)),
                    ValueProvider = mockValueProvider.Object,
                    OperationBindingContext = new OperationBindingContext
                    {
                        ValueProvider = mockValueProvider.Object,
                        MetadataProvider = metadataProvider,
                        ValidatorProvider = Mock.Of<IModelValidatorProvider>(),
                    },

                    // Setting it to empty ensures that model does not get created becasue of no model name.
                    ModelName = "dummyModelName",
                }
            };

            var mutableBinder = new TestableMutableObjectModelBinder();
            bindingContext.PropertyMetadata = mutableBinder.GetMetadataForProperties(
                                                                bindingContext.ModelBindingContext);

            // Act
            var retModel = await mutableBinder.CanCreateModel(bindingContext);

            // Assert
            Assert.Equal(isPrefixProvided, retModel);
        }

        [Theory]
        [InlineData(typeof(Person), true)]
        [InlineData(typeof(Person), false)]
        [InlineData(typeof(EmptyModel), true)]
        [InlineData(typeof(EmptyModel), false)]
        public async Task
            CanCreateModel_CreatesModel_ForTopLevelObjectIfThereIsEmptyModelName(Type modelType, bool emptyModelName)
        {
            var mockValueProvider = new Mock<IValueProvider>();
            mockValueProvider.Setup(o => o.ContainsPrefixAsync(It.IsAny<string>()))
                             .Returns(Task.FromResult(false));

            var bindingContext = new MutableObjectBinderContext
            {
                ModelBindingContext = new ModelBindingContext
                {
                    // Random type.
                    ModelMetadata = GetMetadataForType(typeof(Person)),
                    ValueProvider = mockValueProvider.Object,
                    OperationBindingContext = new OperationBindingContext
                    {
                        ValidatorProvider = Mock.Of<IModelValidatorProvider>(),
                        ValueProvider = mockValueProvider.Object,
                        MetadataProvider = TestModelMetadataProvider.CreateDefaultProvider()
                    }
                }
            };

            bindingContext.ModelBindingContext.ModelName = emptyModelName ? string.Empty : "dummyModelName";
            var mutableBinder = new TestableMutableObjectModelBinder();
            bindingContext.PropertyMetadata = mutableBinder.GetMetadataForProperties(
                                                                bindingContext.ModelBindingContext);

            // Act
            var retModel = await mutableBinder.CanCreateModel(bindingContext);

            // Assert
            Assert.Equal(emptyModelName, retModel);
        }

        [Fact]
        public async Task CanCreateModel_ReturnsFalse_ForNonTopLevelModel_IfModelIsMarkedWithBinderMetadata()
        {
            var bindingContext = new MutableObjectBinderContext
            {
                ModelBindingContext = new ModelBindingContext
                {
                    // Get the property metadata so that it is not a top level object.
                    ModelMetadata = GetMetadataForType(typeof(Document))
                                        .Properties
                                        .First(metadata => metadata.PropertyName == nameof(Document.SubDocument)),
                    OperationBindingContext = new OperationBindingContext
                    {
                        ValidatorProvider = Mock.Of<IModelValidatorProvider>(),
                    }
                }
            };

            var mutableBinder = new MutableObjectModelBinder();

            // Act
            var canCreate = await mutableBinder.CanCreateModel(bindingContext);

            // Assert
            Assert.False(canCreate);
        }

        [Fact]
        public async Task CanCreateModel_ReturnsTrue_ForTopLevelModel_IfModelIsMarkedWithBinderMetadata()
        {
            var bindingContext = new MutableObjectBinderContext
            {
                ModelBindingContext = new ModelBindingContext
                {
                    // Here the metadata represents a top level object.
                    ModelMetadata = GetMetadataForType(typeof(Document)),
                    OperationBindingContext = new OperationBindingContext
                    {
                        ValidatorProvider = Mock.Of<IModelValidatorProvider>(),
                    }
                }
            };

            var mutableBinder = new MutableObjectModelBinder();

            // Act
            var canCreate = await mutableBinder.CanCreateModel(bindingContext);

            // Assert
            Assert.True(canCreate);
        }

        [Fact]
        public async Task CanCreateModel_CreatesModel_IfTheModelIsBinderPoco()
        {
            var mockValueProvider = new Mock<IValueProvider>();
            mockValueProvider.Setup(o => o.ContainsPrefixAsync(It.IsAny<string>()))
                             .Returns(Task.FromResult(false));

            var bindingContext = new MutableObjectBinderContext
            {
                ModelBindingContext = new ModelBindingContext
                {
                    ModelMetadata = GetMetadataForType(typeof(BinderMetadataPocoType)),
                    ValueProvider = mockValueProvider.Object,
                    OperationBindingContext = new OperationBindingContext
                    {
                        ValidatorProvider = Mock.Of<IModelValidatorProvider>(),
                        ValueProvider = mockValueProvider.Object,
                        MetadataProvider = TestModelMetadataProvider.CreateDefaultProvider(),
                    },

                    // Setting it to empty ensures that model does not get created becasue of no model name.
                    ModelName = "dummyModelName",
                },
            };

            var mutableBinder = new TestableMutableObjectModelBinder();
            bindingContext.PropertyMetadata = mutableBinder.GetMetadataForProperties(
                                                                bindingContext.ModelBindingContext);

            // Act
            var retModel = await mutableBinder.CanCreateModel(bindingContext);

            // Assert
            Assert.True(retModel);
        }

        [Theory]
        [InlineData(typeof(TypeWithNoBinderMetadata), false)]
        [InlineData(typeof(TypeWithNoBinderMetadata), true)]
        [InlineData(typeof(TypeWithAtLeastOnePropertyMarkedUsingValueBinderMetadata), false)]
        [InlineData(typeof(TypeWithAtLeastOnePropertyMarkedUsingValueBinderMetadata), true)]
        [InlineData(typeof(TypeWithUnmarkedAndBinderMetadataMarkedProperties), false)]
        [InlineData(typeof(TypeWithUnmarkedAndBinderMetadataMarkedProperties), true)]
        public async Task
            CanCreateModel_CreatesModelForValueProviderBasedBinderMetadatas_IfAValueProviderProvidesValue
                (Type modelType, bool valueProviderProvidesValue)
        {
            var mockValueProvider = new Mock<IValueProvider>();
            mockValueProvider.Setup(o => o.ContainsPrefixAsync(It.IsAny<string>()))
                             .Returns(Task.FromResult(valueProviderProvidesValue));

            var bindingContext = new MutableObjectBinderContext
            {
                ModelBindingContext = new ModelBindingContext
                {
                    ModelMetadata = GetMetadataForType(modelType),
                    ValueProvider = mockValueProvider.Object,
                    OperationBindingContext = new OperationBindingContext
                    {
                        ValidatorProvider = Mock.Of<IModelValidatorProvider>(),
                        ValueProvider = mockValueProvider.Object,
                        MetadataProvider = TestModelMetadataProvider.CreateDefaultProvider(),
                    },
                    // Setting it to empty ensures that model does not get created becasue of no model name.
                    ModelName = "dummyName"
                }
            };

            var mutableBinder = new TestableMutableObjectModelBinder();
            bindingContext.PropertyMetadata = mutableBinder.GetMetadataForProperties(
                                                                bindingContext.ModelBindingContext);

            // Act
            var retModel = await mutableBinder.CanCreateModel(bindingContext);

            // Assert
            Assert.Equal(valueProviderProvidesValue, retModel);
        }

        [Theory]
        [InlineData(typeof(TypeWithAtLeastOnePropertyMarkedUsingValueBinderMetadata), false)]
        [InlineData(typeof(TypeWithAtLeastOnePropertyMarkedUsingValueBinderMetadata), true)]
        public async Task CanCreateModel_ForExplicitValueProviderMetadata_UsesOriginalValueProvider(
            Type modelType,
            bool originalValueProviderProvidesValue)
        {
            var mockValueProvider = new Mock<IValueProvider>();
            mockValueProvider.Setup(o => o.ContainsPrefixAsync(It.IsAny<string>()))
                             .Returns(Task.FromResult(false));

            var mockOriginalValueProvider = new Mock<IBindingSourceValueProvider>();
            mockOriginalValueProvider
                .Setup(o => o.ContainsPrefixAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(originalValueProviderProvidesValue));

            mockOriginalValueProvider
                .Setup(o => o.Filter(It.IsAny<BindingSource>()))
                .Returns<BindingSource>(source =>
                {
                    if (source == BindingSource.Query)
                    {
                        return mockOriginalValueProvider.Object;
                    }

                    return null;
                });

            var bindingContext = new MutableObjectBinderContext
            {
                ModelBindingContext = new ModelBindingContext
                {
                    ModelMetadata = GetMetadataForType(modelType),
                    ValueProvider = mockValueProvider.Object,
                    OperationBindingContext = new OperationBindingContext
                    {
                        ValueProvider = mockOriginalValueProvider.Object,
                        MetadataProvider = TestModelMetadataProvider.CreateDefaultProvider(),
                        ValidatorProvider = Mock.Of<IModelValidatorProvider>(),
                    },

                    // Setting it to empty ensures that model does not get created becasue of no model name.
                    ModelName = "dummyName"
                }
            };

            var mutableBinder = new TestableMutableObjectModelBinder();
            bindingContext.PropertyMetadata = mutableBinder.GetMetadataForProperties(
                                                                bindingContext.ModelBindingContext);

            // Act
            var retModel = await mutableBinder.CanCreateModel(bindingContext);

            // Assert
            Assert.Equal(originalValueProviderProvidesValue, retModel);
        }

        [Theory]
        [InlineData(typeof(TypeWithUnmarkedAndBinderMetadataMarkedProperties), false)]
        [InlineData(typeof(TypeWithUnmarkedAndBinderMetadataMarkedProperties), true)]
        [InlineData(typeof(TypeWithNoBinderMetadata), false)]
        [InlineData(typeof(TypeWithNoBinderMetadata), true)]
        public async Task CanCreateModel_UnmarkedProperties_UsesCurrentValueProvider(Type modelType, bool valueProviderProvidesValue)
        {
            var mockValueProvider = new Mock<IValueProvider>();
            mockValueProvider.Setup(o => o.ContainsPrefixAsync(It.IsAny<string>()))
                             .Returns(Task.FromResult(valueProviderProvidesValue));

            var mockOriginalValueProvider = new Mock<IValueProvider>();
            mockOriginalValueProvider.Setup(o => o.ContainsPrefixAsync(It.IsAny<string>()))
                                     .Returns(Task.FromResult(false));

            var bindingContext = new MutableObjectBinderContext
            {
                ModelBindingContext = new ModelBindingContext
                {
                    ModelMetadata = GetMetadataForType(modelType),
                    ValueProvider = mockValueProvider.Object,
                    OperationBindingContext = new OperationBindingContext
                    {
                        ValidatorProvider = Mock.Of<IModelValidatorProvider>(),
                        ValueProvider = mockOriginalValueProvider.Object,
                        MetadataProvider = TestModelMetadataProvider.CreateDefaultProvider(),
                    },
                    // Setting it to empty ensures that model does not get created becasue of no model name.
                    ModelName = "dummyName"
                }
            };

            var mutableBinder = new TestableMutableObjectModelBinder();
            bindingContext.PropertyMetadata = mutableBinder.GetMetadataForProperties(
                                                                bindingContext.ModelBindingContext);

            // Act
            var retModel = await mutableBinder.CanCreateModel(bindingContext);

            // Assert
            Assert.Equal(valueProviderProvidesValue, retModel);
        }

        [Fact]
        public async Task BindModel_InitsInstance()
        {
            // Arrange
            var mockValueProvider = new Mock<IValueProvider>();
            mockValueProvider.Setup(o => o.ContainsPrefixAsync(It.IsAny<string>()))
                             .Returns(Task.FromResult(true));

            var mockDtoBinder = new Mock<IModelBinder>();
            var bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForType(typeof(Person)),
                ModelName = "someName",
                ValueProvider = mockValueProvider.Object,
                OperationBindingContext = new OperationBindingContext
                {
                    ModelBinder = mockDtoBinder.Object,
                    MetadataProvider = TestModelMetadataProvider.CreateDefaultProvider(),
                    ValidatorProvider = Mock.Of<IModelValidatorProvider>()
                }
            };

            mockDtoBinder
                .Setup(o => o.BindModelAsync(It.IsAny<ModelBindingContext>()))
                .Returns((ModelBindingContext mbc) =>
                {
                    // just return the DTO unchanged
                    return Task.FromResult(new ModelBindingResult(mbc.Model, mbc.ModelName, true));
                });

            var model = new Person();

            var testableBinder = new Mock<TestableMutableObjectModelBinder> { CallBase = true };
            testableBinder
                .Setup(o => o.EnsureModelPublic(bindingContext))
                .Callback<ModelBindingContext>(c => c.Model = model)
                .Verifiable();
            testableBinder
                .Setup(o => o.GetMetadataForProperties(bindingContext))
                              .Returns(new ModelMetadata[0]);

            // Act
            var retValue = await testableBinder.Object.BindModelAsync(bindingContext);

            // Assert
            Assert.NotNull(retValue);
            Assert.True(retValue.IsModelSet);
            Assert.Same(model, retValue.Model);
            Assert.IsType<Person>(retValue.Model);
            testableBinder.Verify();
        }

        [Fact]
        public async Task BindModel_InitsInstance_ForEmptyModelName()
        {
            // Arrange
            var mockValueProvider = new Mock<IValueProvider>();
            mockValueProvider.Setup(o => o.ContainsPrefixAsync(It.IsAny<string>()))
                             .Returns(Task.FromResult(false));

            var mockDtoBinder = new Mock<IModelBinder>();
            var bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForType(typeof(Person)),
                ModelName = "",
                ValueProvider = mockValueProvider.Object,
                OperationBindingContext = new OperationBindingContext
                {
                    ModelBinder = mockDtoBinder.Object,
                    MetadataProvider = TestModelMetadataProvider.CreateDefaultProvider(),
                    ValidatorProvider = Mock.Of<IModelValidatorProvider>()
                }
            };

            mockDtoBinder
                .Setup(o => o.BindModelAsync(It.IsAny<ModelBindingContext>()))
                .Returns((ModelBindingContext mbc) =>
                {
                    // just return the DTO unchanged
                    return Task.FromResult(new ModelBindingResult(mbc.Model, mbc.ModelName, true));
                });

            var model = new Person();

            var testableBinder = new Mock<TestableMutableObjectModelBinder> { CallBase = true };
            testableBinder
                .Setup(o => o.EnsureModelPublic(bindingContext))
                .Callback<ModelBindingContext>(c => c.Model = model)
                .Verifiable();

            testableBinder
                .Setup(o => o.GetMetadataForProperties(bindingContext))
                .Returns(new ModelMetadata[0]);

            // Act
            var retValue = await testableBinder.Object.BindModelAsync(bindingContext);

            // Assert
            Assert.NotNull(retValue);
            Assert.True(retValue.IsModelSet);
            Assert.Same(model, retValue.Model);
            Assert.IsType<Person>(retValue.Model);
            testableBinder.Verify();
        }

        [Fact]
        public void CanUpdateProperty_HasPublicSetter_ReturnsTrue()
        {
            // Arrange
            var propertyMetadata = GetMetadataForCanUpdateProperty("ReadWriteString");

            // Act
            var canUpdate = MutableObjectModelBinder.CanUpdatePropertyInternal(propertyMetadata);

            // Assert
            Assert.True(canUpdate);
        }

        [Fact]
        public void CanUpdateProperty_ReadOnlyArray_ReturnsFalse()
        {
            // Arrange
            var propertyMetadata = GetMetadataForCanUpdateProperty("ReadOnlyArray");

            // Act
            var canUpdate = MutableObjectModelBinder.CanUpdatePropertyInternal(propertyMetadata);

            // Assert
            Assert.False(canUpdate);
        }

        [Fact]
        public void CanUpdateProperty_ReadOnlyReferenceTypeNotBlacklisted_ReturnsTrue()
        {
            // Arrange
            var propertyMetadata = GetMetadataForCanUpdateProperty("ReadOnlyObject");

            // Act
            var canUpdate = MutableObjectModelBinder.CanUpdatePropertyInternal(propertyMetadata);

            // Assert
            Assert.True(canUpdate);
        }

        [Fact]
        public void CanUpdateProperty_ReadOnlyString_ReturnsFalse()
        {
            // Arrange
            var propertyMetadata = GetMetadataForCanUpdateProperty("ReadOnlyString");

            // Act
            var canUpdate = MutableObjectModelBinder.CanUpdatePropertyInternal(propertyMetadata);

            // Assert
            Assert.False(canUpdate);
        }

        [Fact]
        public void CanUpdateProperty_ReadOnlyValueType_ReturnsFalse()
        {
            // Arrange
            var propertyMetadata = GetMetadataForCanUpdateProperty("ReadOnlyInt");

            // Act
            var canUpdate = MutableObjectModelBinder.CanUpdatePropertyInternal(propertyMetadata);

            // Assert
            Assert.False(canUpdate);
        }

        [Fact]
        public void CreateModel_InstantiatesInstanceOfMetadataType()
        {
            // Arrange
            var bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForType(typeof(Person))
            };

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            var retModel = testableBinder.CreateModelPublic(bindingContext);

            // Assert
            Assert.IsType<Person>(retModel);
        }

        [Fact]
        public void EnsureModel_ModelIsNotNull_DoesNothing()
        {
            // Arrange
            var bindingContext = new ModelBindingContext
            {
                Model = new Person(),
                ModelMetadata = GetMetadataForType(typeof(Person))
            };

            var testableBinder = new Mock<TestableMutableObjectModelBinder> { CallBase = true };

            // Act
            var originalModel = bindingContext.Model;
            testableBinder.Object.EnsureModelPublic(bindingContext);

            // Assert
            Assert.Same(originalModel, bindingContext.Model);
            testableBinder.Verify(o => o.CreateModelPublic(bindingContext), Times.Never());
        }

        [Fact]
        public void EnsureModel_ModelIsNull_CallsCreateModel()
        {
            // Arrange
            var bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForType(typeof(Person))
            };

            var testableBinder = new Mock<TestableMutableObjectModelBinder> { CallBase = true };
            testableBinder.Setup(o => o.CreateModelPublic(bindingContext))
                          .Returns(new Person()).Verifiable();

            // Act
            var originalModel = bindingContext.Model;
            testableBinder.Object.EnsureModelPublic(bindingContext);
            var newModel = bindingContext.Model;

            // Assert
            Assert.Null(originalModel);
            Assert.IsType<Person>(newModel);
            testableBinder.Verify();
        }

        [Fact]
        public void GetMetadataForProperties_WithBindAttribute()
        {
            // Arrange
            var expectedPropertyNames = new[] { "FirstName", "LastName" };
            var bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForType(typeof(PersonWithBindExclusion)),
                OperationBindingContext = new OperationBindingContext
                {
                    ValidatorProvider = Mock.Of<IModelValidatorProvider>(),
                    MetadataProvider = TestModelMetadataProvider.CreateDefaultProvider()
                }
            };

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            var propertyMetadatas = testableBinder.GetMetadataForProperties(bindingContext);
            var returnedPropertyNames = propertyMetadatas.Select(o => o.PropertyName).ToArray();

            // Assert
            Assert.Equal(expectedPropertyNames, returnedPropertyNames);
        }

        [Fact]
        public void GetMetadataForProperties_WithoutBindAttribute()
        {
            // Arrange
            var expectedPropertyNames = new[]
            {
                "DateOfBirth",
                "DateOfDeath",
                "ValueTypeRequired",
                "FirstName",
                "LastName",
                "PropertyWithDefaultValue",
                "PropertyWithInitializedValue",
                "PropertyWithInitializedValueAndDefault",
            };
            var bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForType(typeof(Person)),
                OperationBindingContext = new OperationBindingContext
                {
                    ValidatorProvider = Mock.Of<IModelValidatorProvider>(),
                    MetadataProvider = TestModelMetadataProvider.CreateDefaultProvider()
                },
            };

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            var propertyMetadatas = testableBinder.GetMetadataForProperties(bindingContext);
            var returnedPropertyNames = propertyMetadatas.Select(o => o.PropertyName).ToArray();

            // Assert
            Assert.Equal(expectedPropertyNames, returnedPropertyNames);
        }

        [Fact]
        public void GetMetadataForProperties_DoesNotReturn_ExcludedProperties()
        {
            // Arrange
            var expectedPropertyNames = new[] { "IncludedByDefault1", "IncludedByDefault2" };
            var bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForType(typeof(TypeWithExcludedPropertiesUsingBindAttribute)),
                OperationBindingContext = new OperationBindingContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        RequestServices = CreateServices()
                    },
                    ValidatorProvider = Mock.Of<IModelValidatorProvider>(),
                    MetadataProvider = TestModelMetadataProvider.CreateDefaultProvider(),
                }
            };

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            var propertyMetadatas = testableBinder.GetMetadataForProperties(bindingContext);
            var returnedPropertyNames = propertyMetadatas.Select(o => o.PropertyName).ToArray();

            // Assert
            Assert.Equal(expectedPropertyNames, returnedPropertyNames);
        }

        [Fact]
        public void GetMetadataForProperties_ReturnsOnlyIncludedProperties_UsingBindAttributeInclude()
        {
            // Arrange
            var expectedPropertyNames = new[] { "IncludedExplicitly1", "IncludedExplicitly2" };
            var bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForType(typeof(TypeWithIncludedPropertiesUsingBindAttribute)),
                OperationBindingContext = new OperationBindingContext
                {
                    ValidatorProvider = Mock.Of<IModelValidatorProvider>(),
                    MetadataProvider = TestModelMetadataProvider.CreateDefaultProvider(),
                }
            };

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            var propertyMetadatas = testableBinder.GetMetadataForProperties(bindingContext);
            var returnedPropertyNames = propertyMetadatas.Select(o => o.PropertyName).ToArray();

            // Assert
            Assert.Equal(expectedPropertyNames, returnedPropertyNames);
        }

        [Fact]
        public void GetRequiredPropertiesCollection_MixedAttributes()
        {
            // Arrange
            var bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForType(typeof(ModelWithMixedBindingBehaviors)),
                OperationBindingContext = new OperationBindingContext
                {
                    ValidatorProvider = Mock.Of<IModelValidatorProvider>()
                }
            };

            // Act
            var validationInfo = MutableObjectModelBinder.GetPropertyValidationInfo(bindingContext);

            // Assert
            Assert.Equal(new[] { "Required" }, validationInfo.RequiredProperties);
            Assert.Equal(new[] { "Never" }, validationInfo.SkipProperties);
        }

        [Fact]
        [ReplaceCulture]
        public void ProcessDto_BindRequiredFieldMissing_RaisesModelError()
        {
            // Arrange
            var model = new ModelWithBindRequired
            {
                Name = "original value",
                Age = -20
            };

            var containerMetadata = GetMetadataForType(model.GetType());
            var bindingContext = new ModelBindingContext
            {
                Model = model,
                ModelMetadata = containerMetadata,
                ModelName = "theModel",
                OperationBindingContext = new OperationBindingContext
                {
                    MetadataProvider = TestModelMetadataProvider.CreateDefaultProvider(),
                    ValidatorProvider = Mock.Of<IModelValidatorProvider>()
                }
            };
            var dto = new ComplexModelDto(containerMetadata, containerMetadata.Properties);

            var nameProperty = dto.PropertyMetadata.Single(o => o.PropertyName == "Name");
            dto.Results[nameProperty] = new ModelBindingResult(
                "John Doe",
                isModelSet: true,
                key: "");

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.ProcessDto(bindingContext, dto);

            // Assert
            var modelStateDictionary = bindingContext.ModelState;
            Assert.False(modelStateDictionary.IsValid);
            Assert.Single(modelStateDictionary);

            // Check Age error.
            ModelState modelState;
            Assert.True(modelStateDictionary.TryGetValue("theModel.Age", out modelState));
            var modelError = Assert.Single(modelState.Errors);
            Assert.Null(modelError.Exception);
            Assert.NotNull(modelError.ErrorMessage);
            Assert.Equal("The 'Age' property is required.", modelError.ErrorMessage);
        }

        [Fact]
        [ReplaceCulture]
        public void ProcessDto_BindRequiredFieldNull_RaisesModelError()
        {
            // Arrange
            var model = new ModelWithBindRequired
            {
                Name = "original value",
                Age = -20
            };

            var containerMetadata = GetMetadataForType(model.GetType());
            var bindingContext = new ModelBindingContext()
            {
                Model = model,
                ModelMetadata = containerMetadata,
                ModelName = "theModel",
                ModelState = new ModelStateDictionary(),
                OperationBindingContext = new OperationBindingContext
                {
                    MetadataProvider = TestModelMetadataProvider.CreateDefaultProvider(),
                    ValidatorProvider = Mock.Of<IModelValidatorProvider>()
                }
            };

            var dto = new ComplexModelDto(containerMetadata, containerMetadata.Properties);
            var testableBinder = new TestableMutableObjectModelBinder();

            var propertyMetadata = dto.PropertyMetadata.Single(o => o.PropertyName == "Name");
            dto.Results[propertyMetadata] = new ModelBindingResult(
                "John Doe",
                isModelSet: true,
                key: "theModel.Name");

            // Attempt to set non-Nullable property to null. BindRequiredAttribute should not be relevant in this
            // case because the binding exists.
            propertyMetadata = dto.PropertyMetadata.Single(o => o.PropertyName == "Age");
            dto.Results[propertyMetadata] = new ModelBindingResult(
                null,
                isModelSet: true,
                key: "theModel.Age");

            // Act
            testableBinder.ProcessDto(bindingContext, dto);

            // Assert
            var modelStateDictionary = bindingContext.ModelState;
            Assert.False(modelStateDictionary.IsValid);
            Assert.Equal(1, modelStateDictionary.Count);

            // Check Age error.
            ModelState modelState;
            Assert.True(modelStateDictionary.TryGetValue("theModel.Age", out modelState));
            Assert.Equal(ModelValidationState.Invalid, modelState.ValidationState);

            var modelError = Assert.Single(modelState.Errors);
            Assert.Null(modelError.Exception);
            Assert.NotNull(modelError.ErrorMessage);
            Assert.Equal("A value is required.", modelError.ErrorMessage);
        }

        [Fact]
        [ReplaceCulture]
        public void ProcessDto_RequiredFieldMissing_RaisesModelError()
        {
            // Arrange
            var model = new ModelWithRequired();
            var containerMetadata = GetMetadataForType(model.GetType());

            var bindingContext = CreateContext(containerMetadata, model);

            // Set no properties though Age (a non-Nullable struct) and City (a class) properties are required.
            var dto = new ComplexModelDto(containerMetadata, containerMetadata.Properties);
            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.ProcessDto(bindingContext, dto);

            // Assert
            var modelStateDictionary = bindingContext.ModelState;
            Assert.False(modelStateDictionary.IsValid);
            Assert.Equal(2, modelStateDictionary.Count);

            // Check Age error.
            ModelState modelState;
            Assert.True(modelStateDictionary.TryGetValue("theModel.Age", out modelState));

            var modelError = Assert.Single(modelState.Errors);
            Assert.Null(modelError.Exception);
            Assert.NotNull(modelError.ErrorMessage);
            var expected = ValidationAttributeUtil.GetRequiredErrorMessage(nameof(ModelWithRequired.Age));
            Assert.Equal(expected, modelError.ErrorMessage);

            // Check City error.
            Assert.True(modelStateDictionary.TryGetValue("theModel.City", out modelState));

            modelError = Assert.Single(modelState.Errors);
            Assert.Null(modelError.Exception);
            Assert.NotNull(modelError.ErrorMessage);
            expected = ValidationAttributeUtil.GetRequiredErrorMessage(nameof(ModelWithRequired.City));
            Assert.Equal(expected, modelError.ErrorMessage);
        }

        [Fact]
        [ReplaceCulture]
        public void ProcessDto_RequiredFieldNull_RaisesModelError()
        {
            // Arrange
            var model = new ModelWithRequired();
            var containerMetadata = GetMetadataForType(model.GetType());
            var bindingContext = CreateContext(containerMetadata, model);

            var dto = new ComplexModelDto(containerMetadata, containerMetadata.Properties);
            var testableBinder = new TestableMutableObjectModelBinder();

            // Make Age valid and City invalid.
            var propertyMetadata = dto.PropertyMetadata.Single(p => p.PropertyName == "Age");
            dto.Results[propertyMetadata] = new ModelBindingResult(
                23,
                isModelSet: true,
                key: "theModel.Age");

            propertyMetadata = dto.PropertyMetadata.Single(p => p.PropertyName == "City");
            dto.Results[propertyMetadata] = new ModelBindingResult(
                null,
                isModelSet: true,
                key: "theModel.City");

            // Act
            testableBinder.ProcessDto(bindingContext, dto);

            // Assert
            var modelStateDictionary = bindingContext.ModelState;
            Assert.False(modelStateDictionary.IsValid);
            Assert.Single(modelStateDictionary);

            // Check City error.
            ModelState modelState;
            Assert.True(modelStateDictionary.TryGetValue("theModel.City", out modelState));

            var modelError = Assert.Single(modelState.Errors);
            Assert.Null(modelError.Exception);
            Assert.NotNull(modelError.ErrorMessage);
            var expected = ValidationAttributeUtil.GetRequiredErrorMessage(nameof(ModelWithRequired.City));
            Assert.Equal(expected, modelError.ErrorMessage);
        }

        [Fact]
        public void ProcessDto_RequiredFieldMissing_RaisesModelErrorWithMessage()
        {
            // Arrange
            var model = new Person();
            var containerMetadata = GetMetadataForType(model.GetType());
            var bindingContext = CreateContext(containerMetadata, model);

            // Set no properties though ValueTypeRequired (a non-Nullable struct) property is required.
            var dto = new ComplexModelDto(containerMetadata, containerMetadata.Properties);
            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.ProcessDto(bindingContext, dto);

            // Assert
            var modelStateDictionary = bindingContext.ModelState;
            Assert.False(modelStateDictionary.IsValid);
            Assert.Single(modelStateDictionary);

            // Check ValueTypeRequired error.
            ModelState modelState;
            Assert.True(modelStateDictionary.TryGetValue("theModel.ValueTypeRequired", out modelState));

            var modelError = Assert.Single(modelState.Errors);
            Assert.Null(modelError.Exception);
            Assert.NotNull(modelError.ErrorMessage);
            Assert.Equal("Sample message", modelError.ErrorMessage);
        }

        [Fact]
        public void ProcessDto_RequiredFieldNull_RaisesModelErrorWithMessage()
        {
            // Arrange
            var model = new Person();
            var containerMetadata = GetMetadataForType(model.GetType());

            var bindingContext = CreateContext(containerMetadata, model);

            var dto = new ComplexModelDto(containerMetadata, containerMetadata.Properties);
            var testableBinder = new TestableMutableObjectModelBinder();

            // Make ValueTypeRequired invalid.
            var propertyMetadata = dto.PropertyMetadata.Single(p => p.PropertyName == "ValueTypeRequired");
            dto.Results[propertyMetadata] = new ModelBindingResult(
                null,
                isModelSet: true,
                key: "theModel.ValueTypeRequired");

            // Act
            testableBinder.ProcessDto(bindingContext, dto);

            // Assert
            var modelStateDictionary = bindingContext.ModelState;
            Assert.False(modelStateDictionary.IsValid);
            Assert.Single(modelStateDictionary);

            // Check ValueTypeRequired error.
            ModelState modelState;
            Assert.True(modelStateDictionary.TryGetValue("theModel.ValueTypeRequired", out modelState));

            var modelError = Assert.Single(modelState.Errors);
            Assert.Null(modelError.Exception);
            Assert.NotNull(modelError.ErrorMessage);
            Assert.Equal("Sample message", modelError.ErrorMessage);
        }

        [Fact]
        public void ProcessDto_Success()
        {
            // Arrange
            var dob = new DateTime(2001, 1, 1);
            var model = new PersonWithBindExclusion
            {
                DateOfBirth = dob
            };
            var containerMetadata = GetMetadataForType(model.GetType());

            var bindingContext = CreateContext(containerMetadata, model);
            var dto = new ComplexModelDto(containerMetadata, containerMetadata.Properties);

            var firstNameProperty = dto.PropertyMetadata.Single(o => o.PropertyName == "FirstName");
            dto.Results[firstNameProperty] = new ModelBindingResult(
                "John",
                isModelSet: true,
                key: "");

            var lastNameProperty = dto.PropertyMetadata.Single(o => o.PropertyName == "LastName");
            dto.Results[lastNameProperty] = new ModelBindingResult(
                "Doe",
                isModelSet: true,
                key: "");

            var dobProperty = dto.PropertyMetadata.Single(o => o.PropertyName == "DateOfBirth");
            dto.Results[dobProperty] = null;

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.ProcessDto(bindingContext, dto);

            // Assert
            Assert.Equal("John", model.FirstName);
            Assert.Equal("Doe", model.LastName);
            Assert.Equal(dob, model.DateOfBirth);
            Assert.True(bindingContext.ModelState.IsValid);
        }

        [Fact]
        public void SetProperty_PropertyHasDefaultValue_SetsDefaultValue()
        {
            // Arrange
            var model = new Person();
            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);

            var metadataProvider = bindingContext.OperationBindingContext.MetadataProvider;
            var modelExplorer = metadataProvider.GetModelExplorerForType(typeof(Person), model);
            var propertyMetadata = bindingContext.ModelMetadata.Properties["PropertyWithDefaultValue"];

            var dtoResult = new ModelBindingResult(
                model: null,
                isModelSet: false,
                key: "foo");

            var requiredValidator = bindingContext.OperationBindingContext
                                                  .ValidatorProvider
                                                  .GetValidators(propertyMetadata)
                                                  .FirstOrDefault(v => v.IsRequired);

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetProperty(
                bindingContext,
                modelExplorer,
                propertyMetadata,
                dtoResult,
                requiredValidator);

            // Assert
            var person = Assert.IsType<Person>(bindingContext.Model);
            Assert.Equal(123.456m, person.PropertyWithDefaultValue);
            Assert.True(bindingContext.ModelState.IsValid);
        }

        [Fact]
        public void SetProperty_PropertyIsPreinitialized_NoValue_DoesNothing()
        {
            // Arrange
            var model = new Person();
            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);

            var metadataProvider = bindingContext.OperationBindingContext.MetadataProvider;
            var modelExplorer = metadataProvider.GetModelExplorerForType(typeof(Person), model);
            var propertyMetadata = bindingContext.ModelMetadata.Properties["PropertyWithInitializedValue"];

            // This value won't be used because IsModelBound = false.
            var dtoResult = new ModelBindingResult(
                model: "bad-value",
                isModelSet: false,
                key: "foo");

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetProperty(
                bindingContext,
                modelExplorer,
                propertyMetadata,
                dtoResult,
                requiredValidator: null);

            // Assert
            var person = Assert.IsType<Person>(bindingContext.Model);
            Assert.Equal("preinitialized", person.PropertyWithInitializedValue);
            Assert.True(bindingContext.ModelState.IsValid);
        }

        [Fact]
        public void SetProperty_PropertyIsPreinitialized_WithDefaultValue_NoValue_CallsSetter()
        {
            // Arrange
            var model = new Person();
            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);

            var metadataProvider = bindingContext.OperationBindingContext.MetadataProvider;
            var modelExplorer = metadataProvider.GetModelExplorerForType(typeof(Person), model);
            var propertyMetadata = bindingContext.ModelMetadata.Properties["PropertyWithInitializedValueAndDefault"];

            // This value won't be used because IsModelBound = false.
            var dtoResult = new ModelBindingResult(
                model: "bad-value",
                isModelSet: false,
                key: "foo");

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetProperty(
                bindingContext,
                modelExplorer,
                propertyMetadata,
                dtoResult,
                requiredValidator: null);

            // Assert
            var person = Assert.IsType<Person>(bindingContext.Model);
            Assert.Equal("default", person.PropertyWithInitializedValueAndDefault);
            Assert.True(bindingContext.ModelState.IsValid);
        }

        [Fact]
        public void SetProperty_PropertyIsReadOnly_DoesNothing()
        {
            // Arrange
            var model = new Person();
            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);

            var metadataProvider = bindingContext.OperationBindingContext.MetadataProvider;
            var modelExplorer = metadataProvider.GetModelExplorerForType(typeof(Person), model);
            var propertyMetadata = bindingContext.ModelMetadata.Properties["NonUpdateableProperty"];

            var dtoResult = new ModelBindingResult(
                model: null,
                isModelSet: false,
                key: "foo");

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetProperty(
                bindingContext,
                modelExplorer,
                propertyMetadata,
                dtoResult,
                requiredValidator: null);

            // Assert
            // If didn't throw, success!
        }

        [Fact]
        public void SetProperty_PropertyIsSettable_CallsSetter()
        {
            // Arrange
            var model = new Person();
            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);

            var metadataProvider = bindingContext.OperationBindingContext.MetadataProvider;
            var modelExplorer = metadataProvider.GetModelExplorerForType(typeof(Person), model);
            var propertyMetadata = bindingContext.ModelMetadata.Properties["DateOfBirth"];

            var dtoResult = new ModelBindingResult(
                new DateTime(2001, 1, 1),
                key: "foo",
                isModelSet: true);


            var requiredValidator = bindingContext.OperationBindingContext
                                                  .ValidatorProvider
                                                  .GetValidators(propertyMetadata)
                                                  .FirstOrDefault(v => v.IsRequired);

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetProperty(
                bindingContext,
                modelExplorer,
                propertyMetadata,
                dtoResult,
                requiredValidator);

            // Assert
            Assert.True(bindingContext.ModelState.IsValid);
            Assert.Equal(new DateTime(2001, 1, 1), model.DateOfBirth);
        }

        [Fact]
        [ReplaceCulture]
        public void SetProperty_PropertyIsSettable_SetterThrows_RecordsError()
        {
            // Arrange
            var model = new Person
            {
                DateOfBirth = new DateTime(1900, 1, 1)
            };

            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);

            var metadataProvider = bindingContext.OperationBindingContext.MetadataProvider;
            var modelExplorer = metadataProvider.GetModelExplorerForType(typeof(Person), model);
            var propertyMetadata = bindingContext.ModelMetadata.Properties["DateOfDeath"];

            var dtoResult = new ModelBindingResult(
                new DateTime(1800, 1, 1),
                isModelSet: true,
                key: "foo");

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetProperty(
                bindingContext,
                modelExplorer,
                propertyMetadata,
                dtoResult,
                requiredValidator: null);

            // Assert
            Assert.Equal("Date of death can't be before date of birth." + Environment.NewLine
                       + "Parameter name: value",
                         bindingContext.ModelState["foo"].Errors[0].Exception.Message);
        }

        [Fact]
        public void SetProperty_SettingNonNullableValueTypeToNull_RequiredValidatorNotPresent_RegistersValidationCallback()
        {
            // Arrange
            var model = new Person();
            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);

            var metadataProvider = bindingContext.OperationBindingContext.MetadataProvider;
            var modelExplorer = metadataProvider.GetModelExplorerForType(typeof(Person), model);
            var propertyMetadata = bindingContext.ModelMetadata.Properties["DateOfBirth"];

            var dtoResult = new ModelBindingResult(
                model: null,
                isModelSet: true,
                key: "foo");

            var requiredValidator = GetRequiredValidator(bindingContext, propertyMetadata);

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetProperty(
                bindingContext,
                modelExplorer,
                propertyMetadata,
                dtoResult,
                requiredValidator);

            // Assert
            Assert.False(bindingContext.ModelState.IsValid);
        }

        [Fact]
        public void SetProperty_SettingNonNullableValueTypeToNull_RequiredValidatorPresent_AddsModelError()
        {
            // Arrange
            var model = new Person();
            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);
            bindingContext.ModelName = " foo";

            var metadataProvider = bindingContext.OperationBindingContext.MetadataProvider;
            var modelExplorer = metadataProvider.GetModelExplorerForType(typeof(Person), model);
            var propertyMetadata = bindingContext.ModelMetadata.Properties["ValueTypeRequired"];

            var dtoResult = new ModelBindingResult(
                model: null,
                isModelSet: true,
                key: "foo.ValueTypeRequired");

            var requiredValidator = GetRequiredValidator(bindingContext, propertyMetadata);

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetProperty(
                bindingContext,
                modelExplorer,
                propertyMetadata,
                dtoResult,
                requiredValidator);

            // Assert
            Assert.False(bindingContext.ModelState.IsValid);
            Assert.Equal("Sample message", bindingContext.ModelState["foo.ValueTypeRequired"].Errors[0].ErrorMessage);
        }

        [Fact]
        [ReplaceCulture]
        public void SetProperty_SettingNullableTypeToNull_RequiredValidatorNotPresent_PropertySetterThrows_AddsRequiredMessageString()
        {
            // Arrange
            var model = new ModelWhosePropertySetterThrows();
            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);
            bindingContext.ModelName = "foo";

            var metadataProvider = bindingContext.OperationBindingContext.MetadataProvider;
            var modelExplorer = metadataProvider.GetModelExplorerForType(typeof(Person), model);
            var propertyMetadata = bindingContext.ModelMetadata.Properties["NameNoAttribute"];

            var dtoResult = new ModelBindingResult(
                model: null,
                isModelSet: true,
                key: "foo.NameNoAttribute");

            var requiredValidator = GetRequiredValidator(bindingContext, propertyMetadata);

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetProperty(
                bindingContext,
                modelExplorer,
                propertyMetadata,
                dtoResult,
                requiredValidator);

            // Assert
            Assert.False(bindingContext.ModelState.IsValid);
            Assert.Equal(1, bindingContext.ModelState["foo.NameNoAttribute"].Errors.Count);
            Assert.Equal("This is a different exception." + Environment.NewLine
                       + "Parameter name: value",
                         bindingContext.ModelState["foo.NameNoAttribute"].Errors[0].Exception.Message);
        }

        [Fact]
        public void SetProperty_SettingNullableTypeToNull_RequiredValidatorPresent_PropertySetterThrows_AddsRequiredMessageString()
        {
            // Arrange
            var model = new ModelWhosePropertySetterThrows();
            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);
            bindingContext.ModelName = "foo";

            var metadataProvider = bindingContext.OperationBindingContext.MetadataProvider;
            var modelExplorer = metadataProvider.GetModelExplorerForType(typeof(Person), model);
            var propertyMetadata = bindingContext.ModelMetadata.Properties["Name"];

            var dtoResult = new ModelBindingResult(
                model: null,
                isModelSet: true,
                key: "foo.Name");

            var requiredValidator = GetRequiredValidator(bindingContext, propertyMetadata);

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetProperty(
                bindingContext,
                modelExplorer,
                propertyMetadata,
                dtoResult,
                requiredValidator);

            // Assert
            Assert.False(bindingContext.ModelState.IsValid);
            var error = Assert.Single(bindingContext.ModelState["foo.Name"].Errors);
            Assert.Equal("This message comes from the [Required] attribute.", error.ErrorMessage);
        }

        private static ModelBindingContext CreateContext(ModelMetadata metadata, object model)
        {
            var providers = new IModelValidatorProvider[]
            {
                new DataAnnotationsModelValidatorProvider(),
                new DataMemberModelValidatorProvider()
            };

            return new ModelBindingContext
            {
                Model = model,
                ModelState = new ModelStateDictionary(),
                ModelMetadata = metadata,
                ModelName = "theModel",
                OperationBindingContext = new OperationBindingContext
                {
                    MetadataProvider = TestModelMetadataProvider.CreateDefaultProvider(),
                    ValidatorProvider = new CompositeModelValidatorProvider(providers)
                }
            };
        }

        private static IModelValidator GetRequiredValidator(ModelBindingContext bindingContext, ModelMetadata propertyMetadata)
        {
            return bindingContext.OperationBindingContext
                                 .ValidatorProvider
                                 .GetValidators(propertyMetadata)
                                 .FirstOrDefault(v => v.IsRequired);
        }

        private static ModelMetadata GetMetadataForCanUpdateProperty(string propertyName)
        {
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            return metadataProvider.GetMetadataForProperty(typeof(MyModelTestingCanUpdateProperty), propertyName);
        }

        private static ModelMetadata GetMetadataForType(Type t)
        {
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            return metadataProvider.GetMetadataForType(t);
        }

        private class EmptyModel
        {
        }

        private class Person
        {
            private DateTime? _dateOfDeath;

            public DateTime DateOfBirth { get; set; }

            public DateTime? DateOfDeath
            {
                get { return _dateOfDeath; }
                set
                {
                    if (value < DateOfBirth)
                    {
                        throw new ArgumentOutOfRangeException("value", "Date of death can't be before date of birth.");
                    }
                    _dateOfDeath = value;
                }
            }

            [Required(ErrorMessage = "Sample message")]
            public int ValueTypeRequired { get; set; }

            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string NonUpdateableProperty { get; private set; }

            [DefaultValue(typeof(decimal), "123.456")]
            public decimal PropertyWithDefaultValue { get; set; }

            public string PropertyWithInitializedValue { get; set; } = "preinitialized";

            [DefaultValue("default")]
            public string PropertyWithInitializedValueAndDefault { get; set; } = "preinitialized";
        }

        private class PersonWithBindExclusion
        {
            [BindNever]
            public DateTime DateOfBirth { get; set; }

            [BindNever]
            public DateTime? DateOfDeath { get; set; }

            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string NonUpdateableProperty { get; private set; }
        }

        private class ModelWithRequired
        {
            public string Name { get; set; }

            [Required]
            public int Age { get; set; }

            [Required]
            public string City { get; set; }
        }

        private class ModelWithBindRequired
        {
            public string Name { get; set; }

            [BindRequired]
            public int Age { get; set; }
        }

        [BindRequired]
        private class ModelWithMixedBindingBehaviors
        {
            public string Required { get; set; }

            [BindNever]
            public string Never { get; set; }

            [BindingBehavior(BindingBehavior.Optional)]
            public string Optional { get; set; }
        }

        private sealed class MyModelTestingCanUpdateProperty
        {
            public int ReadOnlyInt { get; private set; }
            public string ReadOnlyString { get; private set; }
            public string[] ReadOnlyArray { get; private set; }
            public object ReadOnlyObject { get; private set; }
            public string ReadWriteString { get; set; }
        }

        private sealed class ModelWhosePropertySetterThrows
        {
            [Required(ErrorMessage = "This message comes from the [Required] attribute.")]
            public string Name
            {
                get { return null; }
                set { throw new ArgumentException("This is an exception.", "value"); }
            }

            public string NameNoAttribute
            {
                get { return null; }
                set { throw new ArgumentException("This is a different exception.", "value"); }
            }
        }

        private class TypeWithNoBinderMetadata
        {
            public int UnMarkedProperty { get; set; }
        }

        private class BinderMetadataPocoType
        {
            [NonValueBinderMetadata]
            public string MarkedWithABinderMetadata { get; set; }
        }

        // Not a Metadata poco because there is a property with value binder Metadata.
        private class TypeWithAtLeastOnePropertyMarkedUsingValueBinderMetadata
        {
            [NonValueBinderMetadata]
            public string MarkedWithABinderMetadata { get; set; }

            [ValueBinderMetadata]
            public string MarkedWithAValueBinderMetadata { get; set; }
        }

        // not a Metadata poco because there is an unmarked property.
        private class TypeWithUnmarkedAndBinderMetadataMarkedProperties
        {
            public int UnmarkedProperty { get; set; }

            [NonValueBinderMetadata]
            public string MarkedWithABinderMetadata { get; set; }
        }

        [Bind(new[] { nameof(IncludedExplicitly1), nameof(IncludedExplicitly2) })]
        private class TypeWithIncludedPropertiesUsingBindAttribute
        {
            public int ExcludedByDefault1 { get; set; }

            public int ExcludedByDefault2 { get; set; }

            public int IncludedExplicitly1 { get; set; }

            public int IncludedExplicitly2 { get; set; }
        }

        [Bind(typeof(ExcludedProvider))]
        private class TypeWithExcludedPropertiesUsingBindAttribute
        {
            public int Excluded1 { get; set; }

            public int Excluded2 { get; set; }

            public int IncludedByDefault1 { get; set; }
            public int IncludedByDefault2 { get; set; }
        }

        public class Document
        {
            [NonValueBinderMetadata]
            public string Version { get; set; }

            [NonValueBinderMetadata]
            public Document SubDocument { get; set; }
        }

        private class NonValueBinderMetadataAttribute : Attribute, IBindingSourceMetadata
        {
            public BindingSource BindingSource { get { return BindingSource.Body; } }
        }

        private class ValueBinderMetadataAttribute : Attribute, IBindingSourceMetadata
        {
            public BindingSource BindingSource { get { return BindingSource.Query; } }
        }

        public class ExcludedProvider : IPropertyBindingPredicateProvider
        {
            public Func<ModelBindingContext, string, bool> PropertyFilter
            {
                get
                {
                    return (context, propertyName) =>
                       !string.Equals("Excluded1", propertyName, StringComparison.OrdinalIgnoreCase) &&
                       !string.Equals("Excluded2", propertyName, StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        private IServiceProvider CreateServices()
        {
            var services = new Mock<IServiceProvider>(MockBehavior.Strict);
            return services.Object;
        }

        public class TestableMutableObjectModelBinder : MutableObjectModelBinder
        {
            public virtual bool CanUpdatePropertyPublic(ModelMetadata propertyMetadata)
            {
                return base.CanUpdateProperty(propertyMetadata);
            }

            protected override bool CanUpdateProperty(ModelMetadata propertyMetadata)
            {
                return CanUpdatePropertyPublic(propertyMetadata);
            }

            public virtual object CreateModelPublic(ModelBindingContext bindingContext)
            {
                return base.CreateModel(bindingContext);
            }

            protected override object CreateModel(ModelBindingContext bindingContext)
            {
                return CreateModelPublic(bindingContext);
            }

            public virtual void EnsureModelPublic(ModelBindingContext bindingContext)
            {
                base.EnsureModel(bindingContext);
            }

            protected override void EnsureModel(ModelBindingContext bindingContext)
            {
                EnsureModelPublic(bindingContext);
            }

            public virtual new IEnumerable<ModelMetadata> GetMetadataForProperties(ModelBindingContext bindingContext)
            {
                return base.GetMetadataForProperties(bindingContext);
            }

            public new void SetProperty(
                ModelBindingContext bindingContext,
                ModelExplorer modelExplorer,
                ModelMetadata propertyMetadata,
                ModelBindingResult dtoResult,
                IModelValidator requiredValidator)
            {
                base.SetProperty(
                    bindingContext,
                    modelExplorer,
                    propertyMetadata,
                    dtoResult,
                    requiredValidator);
            }
        }
    }
}
#endif
