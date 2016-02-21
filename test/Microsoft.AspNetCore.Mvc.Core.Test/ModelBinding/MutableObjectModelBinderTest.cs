// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding.Test;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    public class MutableObjectModelBinderTest
    {
        private static readonly IModelMetadataProvider _metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void CanCreateModel_ReturnsTrue_IfIsTopLevelObject(
            bool isTopLevelObject,
            bool expectedCanCreate)
        {
            var bindingContext = CreateContext(GetMetadataForType(typeof(Person)));
            bindingContext.IsTopLevelObject = isTopLevelObject;

            var binder = new TestableMutableObjectModelBinder();

            // Act
            var canCreate = binder.CanCreateModel(bindingContext);

            // Assert
            Assert.Equal(expectedCanCreate, canCreate);
        }

        [Fact]
        public void CanCreateModel_ReturnsFalse_IfNotIsTopLevelObjectAndModelIsMarkedWithBinderMetadata()
        {
            var modelMetadata = GetMetadataForProperty(typeof(Document), nameof(Document.SubDocument));

            var bindingContext = CreateContext(modelMetadata);
            bindingContext.IsTopLevelObject = false;

            var binder = new MutableObjectModelBinder();

            // Act
            var canCreate = binder.CanCreateModel(bindingContext);

            // Assert
            Assert.False(canCreate);
        }

        [Fact]
        public void CanCreateModel_ReturnsTrue_IfIsTopLevelObjectAndModelIsMarkedWithBinderMetadata()
        {
            var bindingContext = CreateContext(GetMetadataForType(typeof(Document)));
            bindingContext.IsTopLevelObject = true;

            var binder = new MutableObjectModelBinder();

            // Act
            var canCreate = binder.CanCreateModel(bindingContext);

            // Assert
            Assert.True(canCreate);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanCreateModel_CreatesModel_WithAllGreedyProperties(bool isTopLevelObject)
        {
            var bindingContext = CreateContext(GetMetadataForType(typeof(HasAllGreedyProperties)));
            bindingContext.IsTopLevelObject = isTopLevelObject;

            var binder = new TestableMutableObjectModelBinder();

            // Act
            var canCreate = binder.CanCreateModel(bindingContext);

            // Assert
            Assert.True(canCreate);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanCreateModel_ReturnsTrue_IfNotIsTopLevelObject_BasedOnValueAvailability(
            bool valueAvailable)
        {
            // Arrange
            var valueProvider = new Mock<IValueProvider>(MockBehavior.Strict);
            valueProvider
                .Setup(provider => provider.ContainsPrefix("SimpleContainer.Simple.Name"))
                .Returns(valueAvailable);

            var modelMetadata = GetMetadataForProperty(typeof(SimpleContainer), nameof(SimpleContainer.Simple));
            var bindingContext = CreateContext(modelMetadata);
            bindingContext.IsTopLevelObject = false;
            bindingContext.ModelName = "SimpleContainer.Simple";
            bindingContext.ValueProvider = valueProvider.Object;
            bindingContext.OperationBindingContext.ValueProvider = valueProvider.Object;

            var binder = new MutableObjectModelBinder();

            // Act
            var canCreate = binder.CanCreateModel(bindingContext);

            // Assert
            // Result matches whether first Simple property can bind.
            Assert.Equal(valueAvailable, canCreate);
        }

        [Fact]
        public void CanCreateModel_ReturnsFalse_IfNotIsTopLevelObjectAndModelHasNoProperties()
        {
            // Arrange
            var bindingContext = CreateContext(GetMetadataForType(typeof(PersonWithNoProperties)));
            bindingContext.IsTopLevelObject = false;

            var binder = new TestableMutableObjectModelBinder();

            // Act
            var canCreate = binder.CanCreateModel(bindingContext);

            // Assert
            Assert.False(canCreate);
        }

        [Fact]
        public void CanCreateModel_ReturnsTrue_IfIsTopLevelObjectAndModelHasNoProperties()
        {
            // Arrange
            var bindingContext = CreateContext(GetMetadataForType(typeof(PersonWithNoProperties)));
            bindingContext.IsTopLevelObject = true;

            var binder = new TestableMutableObjectModelBinder();

            // Act
            var canCreate = binder.CanCreateModel(bindingContext);

            // Assert
            Assert.True(canCreate);
        }

        [Theory]
        [InlineData(typeof(TypeWithNoBinderMetadata), false)]
        [InlineData(typeof(TypeWithNoBinderMetadata), true)]
        [InlineData(typeof(TypeWithAtLeastOnePropertyMarkedUsingValueBinderMetadata), false)]
        [InlineData(typeof(TypeWithAtLeastOnePropertyMarkedUsingValueBinderMetadata), true)]
        [InlineData(typeof(TypeWithUnmarkedAndBinderMetadataMarkedProperties), false)]
        [InlineData(typeof(TypeWithUnmarkedAndBinderMetadataMarkedProperties), true)]
        public void CanCreateModel_CreatesModelForValueProviderBasedBinderMetadatas_IfAValueProviderProvidesValue(
            Type modelType,
            bool valueProviderProvidesValue)
        {
            var valueProvider = new Mock<IValueProvider>();
            valueProvider
                .Setup(o => o.ContainsPrefix(It.IsAny<string>()))
                .Returns(valueProviderProvidesValue);

            var bindingContext = CreateContext(GetMetadataForType(modelType));
            bindingContext.IsTopLevelObject = false;
            bindingContext.ValueProvider = valueProvider.Object;
            bindingContext.OperationBindingContext.ValueProvider = valueProvider.Object;

            var binder = new TestableMutableObjectModelBinder();

            // Act
            var canCreate = binder.CanCreateModel(bindingContext);

            // Assert
            Assert.Equal(valueProviderProvidesValue, canCreate);
        }

        [Theory]
        [InlineData(typeof(TypeWithAtLeastOnePropertyMarkedUsingValueBinderMetadata), false)]
        [InlineData(typeof(TypeWithAtLeastOnePropertyMarkedUsingValueBinderMetadata), true)]
        public void CanCreateModel_ForExplicitValueProviderMetadata_UsesOriginalValueProvider(
            Type modelType,
            bool originalValueProviderProvidesValue)
        {
            var valueProvider = new Mock<IValueProvider>();
            valueProvider
                .Setup(o => o.ContainsPrefix(It.IsAny<string>()))
                .Returns(false);

            var originalValueProvider = new Mock<IBindingSourceValueProvider>();
            originalValueProvider
                .Setup(o => o.ContainsPrefix(It.IsAny<string>()))
                .Returns(originalValueProviderProvidesValue);

            originalValueProvider
                .Setup(o => o.Filter(It.IsAny<BindingSource>()))
                .Returns<BindingSource>(source => source == BindingSource.Query ? originalValueProvider.Object : null);

            var bindingContext = CreateContext(GetMetadataForType(modelType));
            bindingContext.IsTopLevelObject = false;
            bindingContext.ValueProvider = valueProvider.Object;
            bindingContext.OperationBindingContext.ValueProvider = originalValueProvider.Object;

            var binder = new TestableMutableObjectModelBinder();

            // Act
            var canCreate = binder.CanCreateModel(bindingContext);

            // Assert
            Assert.Equal(originalValueProviderProvidesValue, canCreate);
        }

        [Theory]
        [InlineData(typeof(TypeWithUnmarkedAndBinderMetadataMarkedProperties), false)]
        [InlineData(typeof(TypeWithUnmarkedAndBinderMetadataMarkedProperties), true)]
        [InlineData(typeof(TypeWithNoBinderMetadata), false)]
        [InlineData(typeof(TypeWithNoBinderMetadata), true)]
        public void CanCreateModel_UnmarkedProperties_UsesCurrentValueProvider(
            Type modelType,
            bool valueProviderProvidesValue)
        {
            var valueProvider = new Mock<IValueProvider>();
            valueProvider
                .Setup(o => o.ContainsPrefix(It.IsAny<string>()))
                .Returns(valueProviderProvidesValue);

            var originalValueProvider = new Mock<IValueProvider>();
            originalValueProvider
                .Setup(o => o.ContainsPrefix(It.IsAny<string>()))
                .Returns(false);

            var bindingContext = CreateContext(GetMetadataForType(modelType));
            bindingContext.IsTopLevelObject = false;
            bindingContext.ValueProvider = valueProvider.Object;
            bindingContext.OperationBindingContext.ValueProvider = originalValueProvider.Object;

            var binder = new TestableMutableObjectModelBinder();

            // Act
            var canCreate = binder.CanCreateModel(bindingContext);

            // Assert
            Assert.Equal(valueProviderProvidesValue, canCreate);
        }

        [Fact]
        public async Task BindModelAsync_CreatesModel_IfIsTopLevelObject()
        {
            // Arrange
            var mockValueProvider = new Mock<IValueProvider>();
            mockValueProvider
                .Setup(o => o.ContainsPrefix(It.IsAny<string>()))
                .Returns(false);

            // Mock binder fails to bind all properties.
            var mockBinder = new StubModelBinder();

            var bindingContext = new DefaultModelBindingContext
            {
                IsTopLevelObject = true,
                ModelMetadata = GetMetadataForType(typeof(Person)),
                ModelName = string.Empty,
                ValueProvider = mockValueProvider.Object,
                OperationBindingContext = new OperationBindingContext
                {
                    ModelBinder = mockBinder,
                    MetadataProvider = _metadataProvider,
                    ValidatorProvider = Mock.Of<IModelValidatorProvider>()
                },
                ModelState = new ModelStateDictionary(),
            };

            var model = new Person();

            var testableBinder = new Mock<TestableMutableObjectModelBinder> { CallBase = true };
            testableBinder
                .Setup(o => o.CreateModelPublic(bindingContext))
                .Returns(model)
                .Verifiable();
            testableBinder
                .Setup(o => o.CanBindPropertyPublic(bindingContext, It.IsAny<ModelMetadata>()))
                .Returns(false);

            // Act
            var result = await testableBinder.Object.BindModelResultAsync(bindingContext);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsModelSet);
            var returnedPerson = Assert.IsType<Person>(result.Model);
            Assert.Same(model, returnedPerson);
            testableBinder.Verify();
        }

        [Theory]
        [InlineData(nameof(MyModelTestingCanUpdateProperty.ReadOnlyInt), false)]    // read-only value type
        [InlineData(nameof(MyModelTestingCanUpdateProperty.ReadOnlyObject), true)]
        [InlineData(nameof(MyModelTestingCanUpdateProperty.ReadOnlySimple), true)]
        [InlineData(nameof(MyModelTestingCanUpdateProperty.ReadOnlyString), false)]
        [InlineData(nameof(MyModelTestingCanUpdateProperty.ReadWriteString), true)]
        public void CanUpdateProperty_ReturnsExpectedValue(string propertyName, bool expected)
        {
            // Arrange

            var propertyMetadata = GetMetadataForProperty(typeof(MyModelTestingCanUpdateProperty), propertyName);

            // Act
            var canUpdate = MutableObjectModelBinder.CanUpdatePropertyInternal(propertyMetadata);

            // Assert
            Assert.Equal(expected, canUpdate);
        }

        [Theory]
        [InlineData(nameof(CollectionContainer.ReadOnlyArray), false)]
        [InlineData(nameof(CollectionContainer.ReadOnlyDictionary), true)]
        [InlineData(nameof(CollectionContainer.ReadOnlyList), true)]
        [InlineData(nameof(CollectionContainer.SettableArray), true)]
        [InlineData(nameof(CollectionContainer.SettableDictionary), true)]
        [InlineData(nameof(CollectionContainer.SettableList), true)]
        public void CanUpdateProperty_CollectionProperty_FalseOnlyForArray(string propertyName, bool expected)
        {
            // Arrange
            var metadataProvider = _metadataProvider;
            var metadata = metadataProvider.GetMetadataForProperty(typeof(CollectionContainer), propertyName);

            // Act
            var canUpdate = MutableObjectModelBinder.CanUpdatePropertyInternal(metadata);

            // Assert
            Assert.Equal(expected, canUpdate);
        }

        [Fact]
        public void CreateModel_InstantiatesInstanceOfMetadataType()
        {
            // Arrange
            var bindingContext = new DefaultModelBindingContext
            {
                ModelMetadata = GetMetadataForType(typeof(Person))
            };

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            var model = testableBinder.CreateModelPublic(bindingContext);

            // Assert
            Assert.IsType<Person>(model);
        }

        [Fact]
        public async Task BindModelAsync_ModelIsNotNull_DoesNotCallCreateModel()
        {
            // Arrange
            var bindingContext = CreateContext(GetMetadataForType(typeof(Person)), new Person());
            var originalModel = bindingContext.Model;

            var binder = new Mock<TestableMutableObjectModelBinder> { CallBase = true };
            binder
                .Setup(b => b.CreateModelPublic(It.IsAny<ModelBindingContext>()))
                .Verifiable();

            // Act
            await binder.Object.BindModelAsync(bindingContext);

            // Assert
            Assert.Same(originalModel, bindingContext.Model);
            binder.Verify(o => o.CreateModelPublic(bindingContext), Times.Never());
        }

        [Fact]
        public async Task BindModelAsync_ModelIsNull_CallsCreateModel()
        {
            // Arrange
            var bindingContext = CreateContext(GetMetadataForType(typeof(Person)), model: null);

            var testableBinder = new Mock<TestableMutableObjectModelBinder> { CallBase = true };
            testableBinder
                .Setup(o => o.CreateModelPublic(bindingContext))
                .Returns(new Person())
                .Verifiable();

            // Act
            await testableBinder.Object.BindModelAsync(bindingContext);

            // Assert
            Assert.NotNull(bindingContext.Model);
            Assert.IsType<Person>(bindingContext.Model);
            testableBinder.Verify();
        }

        [Theory]
        [InlineData(nameof(PersonWithBindExclusion.FirstName))]
        [InlineData(nameof(PersonWithBindExclusion.LastName))]
        public void CanBindProperty_GetSetProperty(string property)
        {
            // Arrange
            var binder = new TestableMutableObjectModelBinder();

            var metadata = GetMetadataForProperty(typeof(PersonWithBindExclusion), property);
            var context = new DefaultModelBindingContext()
            {
                ModelMetadata = GetMetadataForType(typeof(PersonWithBindExclusion)),
                OperationBindingContext = new OperationBindingContext()
                {
                    ActionContext = new ActionContext()
                    {
                        HttpContext = new DefaultHttpContext()
                        {
                            RequestServices = new ServiceCollection().BuildServiceProvider(),
                        },
                    },
                },
            };

            // Act
            var result = binder.CanBindPropertyPublic(context, metadata);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(nameof(PersonWithBindExclusion.NonUpdateableProperty))]
        public void CanBindProperty_GetOnlyProperty_WithBindNever(string property)
        {
            // Arrange
            var binder = new TestableMutableObjectModelBinder();

            var metadata = GetMetadataForProperty(typeof(PersonWithBindExclusion), property);
            var context = new DefaultModelBindingContext()
            {
                ModelMetadata = GetMetadataForType(typeof(PersonWithBindExclusion)),
                OperationBindingContext = new OperationBindingContext()
                {
                    ActionContext = new ActionContext()
                    {
                        HttpContext = new DefaultHttpContext()
                        {
                            RequestServices = new ServiceCollection().BuildServiceProvider(),
                        },
                    },
                },
            };

            // Act
            var result = binder.CanBindPropertyPublic(context, metadata);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(nameof(PersonWithBindExclusion.DateOfBirth))]
        [InlineData(nameof(PersonWithBindExclusion.DateOfDeath))]
        public void CanBindProperty_GetSetProperty_WithBindNever(string property)
        {
            // Arrange
            var binder = new TestableMutableObjectModelBinder();

            var metadata = GetMetadataForProperty(typeof(PersonWithBindExclusion), property);
            var context = new DefaultModelBindingContext()
            {
                ModelMetadata = GetMetadataForType(typeof(PersonWithBindExclusion)),
                OperationBindingContext = new OperationBindingContext()
                {
                    ActionContext = new ActionContext()
                    {
                        HttpContext = new DefaultHttpContext()
                        {
                            RequestServices = new ServiceCollection().BuildServiceProvider(),
                        },
                    },
                },
            };

            // Act
            var result = binder.CanBindPropertyPublic(context, metadata);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(nameof(TypeWithExcludedPropertiesUsingBindAttribute.IncludedByDefault1), true)]
        [InlineData(nameof(TypeWithExcludedPropertiesUsingBindAttribute.IncludedByDefault2), true)]
        [InlineData(nameof(TypeWithExcludedPropertiesUsingBindAttribute.Excluded1), false)]
        [InlineData(nameof(TypeWithExcludedPropertiesUsingBindAttribute.Excluded2), false)]
        public void CanBindProperty_WithPredicate(string property, bool expected)
        {
            // Arrange
            var binder = new TestableMutableObjectModelBinder();

            var metadata = GetMetadataForProperty(typeof(TypeWithExcludedPropertiesUsingBindAttribute), property);
            var context = new DefaultModelBindingContext()
            {
                ModelMetadata = GetMetadataForType(typeof(TypeWithExcludedPropertiesUsingBindAttribute)),
                OperationBindingContext = new OperationBindingContext()
                {
                    ActionContext = new ActionContext()
                    {
                        HttpContext = new DefaultHttpContext()
                        {
                            RequestServices = new ServiceCollection().BuildServiceProvider(),
                        },
                    },
                },
            };

            // Act
            var result = binder.CanBindPropertyPublic(context, metadata);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(nameof(TypeWithIncludedPropertiesUsingBindAttribute.IncludedExplicitly1), true)]
        [InlineData(nameof(TypeWithIncludedPropertiesUsingBindAttribute.IncludedExplicitly2), true)]
        [InlineData(nameof(TypeWithIncludedPropertiesUsingBindAttribute.ExcludedByDefault1), false)]
        [InlineData(nameof(TypeWithIncludedPropertiesUsingBindAttribute.ExcludedByDefault2), false)]
        public void CanBindProperty_WithBindInclude(string property, bool expected)
        {
            // Arrange
            var binder = new TestableMutableObjectModelBinder();

            var metadata = GetMetadataForProperty(typeof(TypeWithIncludedPropertiesUsingBindAttribute), property);
            var context = new DefaultModelBindingContext()
            {
                ModelMetadata = GetMetadataForType(typeof(TypeWithIncludedPropertiesUsingBindAttribute)),
                OperationBindingContext = new OperationBindingContext()
                {
                    ActionContext = new ActionContext()
                    {
                        HttpContext = new DefaultHttpContext()
                        {
                            RequestServices = new ServiceCollection().BuildServiceProvider(),
                        },
                    },
                },
            };

            // Act
            var result = binder.CanBindPropertyPublic(context, metadata);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(nameof(ModelWithMixedBindingBehaviors.Required), true)]
        [InlineData(nameof(ModelWithMixedBindingBehaviors.Optional), true)]
        [InlineData(nameof(ModelWithMixedBindingBehaviors.Never), false)]
        public void CanBindProperty_BindingAttributes_OverridingBehavior(string property, bool expected)
        {
            // Arrange
            var binder = new TestableMutableObjectModelBinder();

            var metadata = GetMetadataForProperty(typeof(ModelWithMixedBindingBehaviors), property);
            var context = new DefaultModelBindingContext()
            {
                ModelMetadata = GetMetadataForType(typeof(ModelWithMixedBindingBehaviors)),
                OperationBindingContext = new OperationBindingContext()
                {
                    ActionContext = new ActionContext()
                    {
                        HttpContext = new DefaultHttpContext()
                        {
                            RequestServices = new ServiceCollection().BuildServiceProvider(),
                        },
                    },
                },
            };

            // Act
            var result = binder.CanBindPropertyPublic(context, metadata);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        [ReplaceCulture]
        public async Task BindModelAsync_BindRequiredFieldMissing_RaisesModelError()
        {
            // Arrange
            var model = new ModelWithBindRequired
            {
                Name = "original value",
                Age = -20
            };

            var binder = new TestableMutableObjectModelBinder();

            var property = GetMetadataForProperty(model.GetType(), nameof(ModelWithBindRequired.Age));
            binder.Results[property] = ModelBindingResult.Failed("theModel.Age");

            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);

            // Act
            await binder.BindModelAsync(bindingContext);

            // Assert
            var modelStateDictionary = bindingContext.ModelState;
            Assert.False(modelStateDictionary.IsValid);
            Assert.Single(modelStateDictionary);

            // Check Age error.
            ModelStateEntry entry;
            Assert.True(modelStateDictionary.TryGetValue("theModel.Age", out entry));
            var modelError = Assert.Single(entry.Errors);
            Assert.Null(modelError.Exception);
            Assert.NotNull(modelError.ErrorMessage);
            Assert.Equal("A value for the 'Age' property was not provided.", modelError.ErrorMessage);
        }

        [Fact]
        [ReplaceCulture]
        public async Task BindModelAsync_DataMemberIsRequiredFieldMissing_RaisesModelError()
        {
            // Arrange
            var model = new ModelWithDataMemberIsRequired
            {
                Name = "original value",
                Age = -20
            };

            var binder = new TestableMutableObjectModelBinder();

            var property = GetMetadataForProperty(model.GetType(), nameof(ModelWithDataMemberIsRequired.Age));
            binder.Results[property] = ModelBindingResult.Failed("theModel.Age");

            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);

            // Act
            await binder.BindModelAsync(bindingContext);

            // Assert
            var modelStateDictionary = bindingContext.ModelState;
            Assert.False(modelStateDictionary.IsValid);
            Assert.Single(modelStateDictionary);

            // Check Age error.
            ModelStateEntry entry;
            Assert.True(modelStateDictionary.TryGetValue("theModel.Age", out entry));
            var modelError = Assert.Single(entry.Errors);
            Assert.Null(modelError.Exception);
            Assert.NotNull(modelError.ErrorMessage);
            Assert.Equal("A value for the 'Age' property was not provided.", modelError.ErrorMessage);
        }

        [Fact]
        [ReplaceCulture]
        public async Task BindModelAsync_ValueTypePropertyWithBindRequired_SetToNull_CapturesException()
        {
            // Arrange
            var model = new ModelWithBindRequired
            {
                Name = "original value",
                Age = -20
            };

            var binder = new TestableMutableObjectModelBinder();

            // Attempt to set non-Nullable property to null. BindRequiredAttribute should not be relevant in this
            // case because the property did have a result.
            var property = GetMetadataForProperty(model.GetType(), nameof(ModelWithBindRequired.Age));
            binder.Results[property] = ModelBindingResult.Success("theModel.Age", model: null);

            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);

            // Act
            await binder.BindModelAsync(bindingContext);

            // Assert
            var modelStateDictionary = bindingContext.ModelState;
            Assert.False(modelStateDictionary.IsValid);
            Assert.Equal(1, modelStateDictionary.Count);

            // Check Age error.
            ModelStateEntry entry;
            Assert.True(modelStateDictionary.TryGetValue("theModel.Age", out entry));
            Assert.Equal(ModelValidationState.Invalid, entry.ValidationState);

            var modelError = Assert.Single(entry.Errors);
            Assert.Equal(string.Empty, modelError.ErrorMessage);
            Assert.IsType<NullReferenceException>(modelError.Exception);
        }

        [Fact]
        public async Task BindModelAsync_ValueTypeProperty_WithBindingOptional_NoValueSet_NoError()
        {
            // Arrange
            var model = new BindingOptionalProperty();
            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);

            var binder = new TestableMutableObjectModelBinder();

            var property = GetMetadataForProperty(model.GetType(), nameof(BindingOptionalProperty.ValueTypeRequired));
            binder.Results[property] = ModelBindingResult.Failed("theModel.ValueTypeRequired");

            // Act
            await binder.BindModelAsync(bindingContext);

            // Assert
            var modelStateDictionary = bindingContext.ModelState;
            Assert.True(modelStateDictionary.IsValid);
        }

        [Fact]
        public async Task BindModelAsync_NullableValueTypeProperty_NoValueSet_NoError()
        {
            // Arrange
            var model = new NullableValueTypeProperty();
            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);

            var binder = new TestableMutableObjectModelBinder();

            var property = GetMetadataForProperty(model.GetType(), nameof(NullableValueTypeProperty.NullableValueType));
            binder.Results[property] = ModelBindingResult.Failed("theModel.NullableValueType");

            // Act
            await binder.BindModelAsync(bindingContext);

            // Assert
            var modelStateDictionary = bindingContext.ModelState;
            Assert.True(modelStateDictionary.IsValid);
        }

        [Fact]
        public async Task BindModelAsync_ValueTypeProperty_NoValue_NoError()
        {
            // Arrange
            var model = new Person();
            var containerMetadata = GetMetadataForType(model.GetType());

            var bindingContext = CreateContext(containerMetadata, model);

            var binder = new TestableMutableObjectModelBinder();

            var property = GetMetadataForProperty(model.GetType(), nameof(Person.ValueTypeRequired));
            binder.Results[property] = ModelBindingResult.Failed("theModel." + nameof(Person.ValueTypeRequired));

            // Act
            await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.True(bindingContext.ModelState.IsValid);
            Assert.Equal(0, model.ValueTypeRequired);
        }

        [Fact]
        public async Task BindModelAsync_ProvideRequiredField_Success()
        {
            // Arrange
            var model = new Person();
            var containerMetadata = GetMetadataForType(model.GetType());

            var bindingContext = CreateContext(containerMetadata, model);

            var binder = new TestableMutableObjectModelBinder();

            var property = GetMetadataForProperty(model.GetType(), nameof(Person.ValueTypeRequired));
            binder.Results[property] = ModelBindingResult.Success(
                key: "theModel." + nameof(Person.ValueTypeRequired),
                model: 57);

            // Act
            await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.True(bindingContext.ModelState.IsValid);
            Assert.Equal(57, model.ValueTypeRequired);
        }

        [Fact]
        public async Task BindModelAsync_Success()
        {
            // Arrange
            var dob = new DateTime(2001, 1, 1);
            var model = new PersonWithBindExclusion
            {
                DateOfBirth = dob
            };

            var containerMetadata = GetMetadataForType(model.GetType());

            var bindingContext = CreateContext(containerMetadata, model);

            var binder = new TestableMutableObjectModelBinder();

            foreach (var property in containerMetadata.Properties)
            {
                binder.Results[property] = ModelBindingResult.Failed(property.PropertyName);
            }

            var firstNameProperty = containerMetadata.Properties[nameof(model.FirstName)];
            binder.Results[firstNameProperty] = ModelBindingResult.Success(
                nameof(model.FirstName),
                "John");

            var lastNameProperty = containerMetadata.Properties[nameof(model.LastName)];
            binder.Results[lastNameProperty] = ModelBindingResult.Success(
                nameof(model.LastName),
                "Doe");

            // Act
            await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.Equal("John", model.FirstName);
            Assert.Equal("Doe", model.LastName);
            Assert.Equal(dob, model.DateOfBirth);
            Assert.True(bindingContext.ModelState.IsValid);
        }

        [Fact]
        public void SetProperty_PropertyHasDefaultValue_DefaultValueAttributeDoesNothing()
        {
            // Arrange
            var model = new Person();
            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);

            var metadataProvider = bindingContext.OperationBindingContext.MetadataProvider;
            var metadata = metadataProvider.GetMetadataForType(typeof(Person));
            var propertyMetadata = metadata.Properties[nameof(model.PropertyWithDefaultValue)];

            var result = ModelBindingResult.Failed("foo");
            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(bindingContext, propertyMetadata, result);

            // Assert
            var person = Assert.IsType<Person>(bindingContext.Model);
            Assert.Equal(0m, person.PropertyWithDefaultValue);
            Assert.True(bindingContext.ModelState.IsValid);
        }

        [Fact]
        public void SetProperty_PropertyIsPreinitialized_NoValue_DoesNothing()
        {
            // Arrange
            var model = new Person();
            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);

            var metadataProvider = bindingContext.OperationBindingContext.MetadataProvider;
            var metadata = metadataProvider.GetMetadataForType(typeof(Person));
            var propertyMetadata = metadata.Properties[nameof(model.PropertyWithInitializedValue)];

            // The null model value won't be used because IsModelBound = false.
            var result = ModelBindingResult.Failed("foo");

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(bindingContext, propertyMetadata, result);

            // Assert
            var person = Assert.IsType<Person>(bindingContext.Model);
            Assert.Equal("preinitialized", person.PropertyWithInitializedValue);
            Assert.True(bindingContext.ModelState.IsValid);
        }

        [Fact]
        public void SetProperty_PropertyIsPreinitialized_DefaultValueAttributeDoesNothing()
        {
            // Arrange
            var model = new Person();
            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);

            var metadataProvider = bindingContext.OperationBindingContext.MetadataProvider;
            var metadata = metadataProvider.GetMetadataForType(typeof(Person));
            var propertyMetadata = metadata.Properties[nameof(model.PropertyWithInitializedValueAndDefault)];

            // The null model value won't be used because IsModelBound = false.
            var result = ModelBindingResult.Failed("foo");

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(bindingContext, propertyMetadata, result);

            // Assert
            var person = Assert.IsType<Person>(bindingContext.Model);
            Assert.Equal("preinitialized", person.PropertyWithInitializedValueAndDefault);
            Assert.True(bindingContext.ModelState.IsValid);
        }

        [Fact]
        public void SetProperty_PropertyIsReadOnly_DoesNothing()
        {
            // Arrange
            var model = new Person();
            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);

            var metadataProvider = bindingContext.OperationBindingContext.MetadataProvider;
            var metadata = metadataProvider.GetMetadataForType(typeof(Person));
            var propertyMetadata = metadata.Properties[nameof(model.NonUpdateableProperty)];

            var result = ModelBindingResult.Failed("foo");
            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(bindingContext, propertyMetadata, result);

            // Assert
            // If didn't throw, success!
        }

        // Property name, property accessor
        public static TheoryData<string, Func<object, object>> MyCanUpdateButCannotSetPropertyData
        {
            get
            {
                return new TheoryData<string, Func<object, object>>
                {
                    {
                        nameof(MyModelTestingCanUpdateProperty.ReadOnlyObject),
                        model => ((Simple)((MyModelTestingCanUpdateProperty)model).ReadOnlyObject).Name
                    },
                    {
                        nameof(MyModelTestingCanUpdateProperty.ReadOnlySimple),
                        model => ((MyModelTestingCanUpdateProperty)model).ReadOnlySimple.Name
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(MyCanUpdateButCannotSetPropertyData))]
        public void SetProperty_ValueProvidedAndCanUpdatePropertyTrue_DoesNothing(
            string propertyName,
            Func<object, object> propertyAccessor)
        {
            // Arrange
            var model = new MyModelTestingCanUpdateProperty();
            var type = model.GetType();
            var bindingContext = CreateContext(GetMetadataForType(type), model);
            var modelState = bindingContext.ModelState;
            var metadataProvider = bindingContext.OperationBindingContext.MetadataProvider;
            var metadata = metadataProvider.GetMetadataForType(type);

            var propertyMetadata = bindingContext.ModelMetadata.Properties[propertyName];
            var result = ModelBindingResult.Success(
                propertyName,
                new Simple { Name = "Hanna" });

            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(bindingContext, propertyMetadata, result);

            // Assert
            Assert.Equal("Joe", propertyAccessor(model));
            Assert.True(modelState.IsValid);
            Assert.Empty(modelState);
        }

        [Fact]
        public void SetProperty_ReadOnlyProperty_IsNoOp()
        {
            // Arrange
            var model = new CollectionContainer();
            var originalCollection = model.ReadOnlyList;

            var modelMetadata = GetMetadataForType(model.GetType());
            var propertyMetadata = GetMetadataForProperty(model.GetType(), nameof(CollectionContainer.ReadOnlyList));

            var bindingContext = CreateContext(modelMetadata, model);
            var result = ModelBindingResult.Success(propertyMetadata.PropertyName, new List<string>() { "hi" });

            var binder = new TestableMutableObjectModelBinder();

            // Act
            binder.SetPropertyPublic(bindingContext, propertyMetadata, result);

            // Assert
            Assert.Same(originalCollection, model.ReadOnlyList);
            Assert.Empty(model.ReadOnlyList);
        }

        [Fact]
        public void SetProperty_PropertyIsSettable_CallsSetter()
        {
            // Arrange
            var model = new Person();
            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);

            var metadataProvider = bindingContext.OperationBindingContext.MetadataProvider;
            var metadata = metadataProvider.GetMetadataForType(typeof(Person));
            var propertyMetadata = bindingContext.ModelMetadata.Properties[nameof(model.DateOfBirth)];

            var result = ModelBindingResult.Success("foo", new DateTime(2001, 1, 1));
            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(bindingContext, propertyMetadata, result);

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
            var metadata = metadataProvider.GetMetadataForType(typeof(Person));
            var propertyMetadata = bindingContext.ModelMetadata.Properties[nameof(model.DateOfDeath)];

            var result = ModelBindingResult.Success("foo", new DateTime(1800, 1, 1));
            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(bindingContext, propertyMetadata, result);

            // Assert
            Assert.Equal("Date of death can't be before date of birth." + Environment.NewLine
                       + "Parameter name: value",
                         bindingContext.ModelState["foo"].Errors[0].Exception.Message);
        }

        [Fact]
        [ReplaceCulture]
        public void SetProperty_PropertySetterThrows_CapturesException()
        {
            // Arrange
            var model = new ModelWhosePropertySetterThrows();
            var bindingContext = CreateContext(GetMetadataForType(model.GetType()), model);
            bindingContext.ModelName = "foo";

            var metadataProvider = bindingContext.OperationBindingContext.MetadataProvider;
            var metadata = metadataProvider.GetMetadataForType(typeof(ModelWhosePropertySetterThrows));
            var propertyMetadata = bindingContext.ModelMetadata.Properties[nameof(model.NameNoAttribute)];

            var result = ModelBindingResult.Success("foo.NameNoAttribute", model: null);
            var testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(bindingContext, propertyMetadata, result);

            // Assert
            Assert.False(bindingContext.ModelState.IsValid);
            Assert.Equal(1, bindingContext.ModelState["foo.NameNoAttribute"].Errors.Count);
            Assert.Equal("This is a different exception." + Environment.NewLine
                       + "Parameter name: value",
                         bindingContext.ModelState["foo.NameNoAttribute"].Errors[0].Exception.Message);
        }

        private static DefaultModelBindingContext CreateContext(ModelMetadata metadata, object model = null)
        {
            var valueProvider = new TestValueProvider(new Dictionary<string, object>());
            return new DefaultModelBindingContext()
            {
                BinderModelName = metadata.BinderModelName,
                BindingSource = metadata.BindingSource,
                IsTopLevelObject = true,
                Model = model,
                ModelMetadata = metadata,
                ModelName = "theModel",
                ModelState = new ModelStateDictionary(),
                OperationBindingContext = new OperationBindingContext
                {
                    MetadataProvider = _metadataProvider,
                    ValidatorProvider = TestModelValidatorProvider.CreateDefaultProvider(),
                    ValueProvider = valueProvider,
                },
                ValueProvider = valueProvider,
            };
        }

        private static ModelMetadata GetMetadataForType(Type type)
        {
            return _metadataProvider.GetMetadataForType(type);
        }

        private static ModelMetadata GetMetadataForProperty(Type type, string propertyName)
        {
            return _metadataProvider.GetMetadataForProperty(type, propertyName);
        }

        private class BindingOptionalProperty
        {
            [BindingBehavior(BindingBehavior.Optional)]
            public int ValueTypeRequired { get; set; }
        }

        private class NullableValueTypeProperty
        {
            [BindingBehavior(BindingBehavior.Optional)]
            public int? NullableValueType { get; set; }
        }

        private class Person
        {
            private DateTime? _dateOfDeath;

            [BindingBehavior(BindingBehavior.Optional)]
            public DateTime DateOfBirth { get; set; }

            public DateTime? DateOfDeath
            {
                get { return _dateOfDeath; }
                set
                {
                    if (value < DateOfBirth)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value), "Date of death can't be before date of birth.");
                    }
                    _dateOfDeath = value;
                }
            }

            [Required(ErrorMessage = "Sample message")]
            public int ValueTypeRequired { get; set; }

            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string NonUpdateableProperty { get; private set; }

            [BindingBehavior(BindingBehavior.Optional)]
            [DefaultValue(typeof(decimal), "123.456")]
            public decimal PropertyWithDefaultValue { get; set; }

            public string PropertyWithInitializedValue { get; set; } = "preinitialized";

            [DefaultValue("default")]
            public string PropertyWithInitializedValueAndDefault { get; set; } = "preinitialized";
        }

        private class PersonWithNoProperties
        {
            public string name = null;
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

        private class ModelWithBindRequired
        {
            public string Name { get; set; }

            [BindRequired]
            public int Age { get; set; }
        }

        [DataContract]
        private class ModelWithDataMemberIsRequired
        {
            public string Name { get; set; }

            [DataMember(IsRequired = true)]
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
            public object ReadOnlyObject { get; } = new Simple { Name = "Joe" };
            public string ReadWriteString { get; set; }
            public Simple ReadOnlySimple { get; } = new Simple { Name = "Joe" };
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

        private class HasAllGreedyProperties
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

        private class Document
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

        private class ExcludedProvider : IPropertyBindingPredicateProvider
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

        private class SimpleContainer
        {
            public Simple Simple { get; set; }
        }

        private class Simple
        {
            public string Name { get; set; }
        }

        private class CollectionContainer
        {
            public int[] ReadOnlyArray { get; } = new int[4];

            // Read-only collections get added values.
            public IDictionary<int, string> ReadOnlyDictionary { get; } = new Dictionary<int, string>();

            public IList<int> ReadOnlyList { get; } = new List<int>();

            // Settable values are overwritten.
            public int[] SettableArray { get; set; } = new int[] { 0, 1 };

            public IDictionary<int, string> SettableDictionary { get; set; } = new Dictionary<int, string>
            {
                { 0, "zero" },
                { 25, "twenty-five" },
            };

            public IList<int> SettableList { get; set; } = new List<int> { 3, 9, 0 };
        }

        // Provides the ability to easily mock + call each of these APIs
        public class TestableMutableObjectModelBinder : MutableObjectModelBinder
        {
            public TestableMutableObjectModelBinder()
            {
                Results = new Dictionary<ModelMetadata, ModelBindingResult>();
            }

            public Dictionary<ModelMetadata, ModelBindingResult> Results { get; }

            public virtual Task BindPropertyPublic(ModelBindingContext bindingContext)
            {
                if (Results.Count == 0)
                {
                    return base.BindModelAsync(bindingContext);
                }

                ModelBindingResult result;
                if (Results.TryGetValue(bindingContext.ModelMetadata, out result))
                {
                    bindingContext.Result = result;
                }

                return TaskCache.CompletedTask;
            }

            protected override Task BindProperty(ModelBindingContext bindingContext)
            {
                return BindPropertyPublic(bindingContext);
            }

            public virtual bool CanBindPropertyPublic(
                ModelBindingContext bindingContext,
                ModelMetadata propertyMetadata)
            {
                if (Results.Count == 0)
                {
                    return base.CanBindProperty(bindingContext, propertyMetadata);
                }

                // If this is being used to test binding, then only attempt to bind properties
                // we have results for.
                return Results.ContainsKey(propertyMetadata);
            }

            protected override bool CanBindProperty(
                ModelBindingContext bindingContext,
                ModelMetadata propertyMetadata)
            {
                return CanBindPropertyPublic(bindingContext, propertyMetadata);
            }

            public virtual object CreateModelPublic(ModelBindingContext bindingContext)
            {
                return base.CreateModel(bindingContext);
            }

            protected override object CreateModel(ModelBindingContext bindingContext)
            {
                return CreateModelPublic(bindingContext);
            }

            public virtual void SetPropertyPublic(
                ModelBindingContext bindingContext,
                ModelMetadata propertyMetadata,
                ModelBindingResult result)
            {
                base.SetProperty(bindingContext, propertyMetadata, result);
            }

            protected override void SetProperty(
                ModelBindingContext bindingContext,
                ModelMetadata propertyMetadata,
                ModelBindingResult result)
            {
                SetPropertyPublic(bindingContext, propertyMetadata, result);
            }
        }
    }
}
