﻿#if NET45
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding.Test
{
    public class CompositeModelBinderTest
    {
        [Fact]
        public void BindModel_SuccessfulBind_RunsValidationAndReturnsModel()
        {
            // Arrange
            var validationCalled = false;

            var bindingContext = new ModelBindingContext
            {
                FallbackToEmptyPrefix = true,
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                ModelState = new ModelStateDictionary(),
                ValueProvider = new SimpleValueProvider
                {
                    { "someName", "dummyValue" }
                },
                ValidatorProviders = Enumerable.Empty<IModelValidatorProvider>()
            };

            var mockIntBinder = new Mock<IModelBinder>();
            mockIntBinder
                .Setup(o => o.BindModel(It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ModelBindingContext context)
                    {
                        Assert.Same(bindingContext.ModelMetadata, context.ModelMetadata);
                        Assert.Equal("someName", context.ModelName);
                        Assert.Same(bindingContext.ValueProvider, context.ValueProvider);

                        context.Model = 42;
                        bindingContext.ValidationNode.Validating += delegate { validationCalled = true; };
                        return true;
                    });

            var shimBinder = new CompositeModelBinder(mockIntBinder.Object);

            // Act
            var isBound = shimBinder.BindModel(bindingContext);

            // Assert
            Assert.True(isBound);
            Assert.Equal(42, bindingContext.Model);
            Assert.True(validationCalled);
            Assert.Equal(true, bindingContext.ModelState.IsValid);
        }

        [Fact]
        public void BindModel_SuccessfulBind_ComplexTypeFallback_RunsValidationAndReturnsModel()
        {
            // Arrange
            var validationCalled = false;
            var expectedModel = new List<int> { 1, 2, 3, 4, 5 };

            var bindingContext = new ModelBindingContext
            {
                FallbackToEmptyPrefix = true,
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(List<int>)),
                ModelName = "someName",
                ModelState = new ModelStateDictionary(),
                ValueProvider = new SimpleValueProvider
                {
                    { "someOtherName", "dummyValue" }
                },
                ValidatorProviders = Enumerable.Empty<IModelValidatorProvider>()
            };

            var mockIntBinder = new Mock<IModelBinder>();
            mockIntBinder
                .Setup(o => o.BindModel(It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ModelBindingContext mbc)
                    {
                        if (!String.IsNullOrEmpty(mbc.ModelName))
                        {
                            return false;
                        }

                        Assert.Same(bindingContext.ModelMetadata, mbc.ModelMetadata);
                        Assert.Equal("", mbc.ModelName);
                        Assert.Same(bindingContext.ValueProvider, mbc.ValueProvider);

                        mbc.Model = expectedModel;
                        mbc.ValidationNode.Validating += delegate { validationCalled = true; };
                        return true;
                    });

            IModelBinder shimBinder = new CompositeModelBinder(mockIntBinder.Object);

            // Act
            bool isBound = shimBinder.BindModel(bindingContext);

            // Assert
            Assert.True(isBound);
            Assert.Equal(expectedModel, bindingContext.Model);
            Assert.True(validationCalled);
            Assert.Equal(true, bindingContext.ModelState.IsValid);
        }

        [Fact]
        public void BindModel_UnsuccessfulBind_BinderFails_ReturnsNull()
        {
            // Arrange
            var mockListBinder = new Mock<IModelBinder>();
            mockListBinder.Setup(o => o.BindModel(It.IsAny<ModelBindingContext>()))
                          .Returns(false)
                          .Verifiable();

            var shimBinder = (IModelBinder)mockListBinder.Object;

            var bindingContext = new ModelBindingContext
            {
                FallbackToEmptyPrefix = false,
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(List<int>)),
            };

            // Act
            var isBound = shimBinder.BindModel(bindingContext);

            // Assert
            Assert.False(isBound);
            Assert.Null(bindingContext.Model);
            Assert.Equal(true, bindingContext.ModelState.IsValid);
            mockListBinder.Verify();
        }

        [Fact]
        public void BindModel_UnsuccessfulBind_SimpleTypeNoFallback_ReturnsNull()
        {
            // Arrange
            var innerBinder = Mock.Of<IModelBinder>();
            var shimBinder = new CompositeModelBinder(innerBinder);

            var bindingContext = new ModelBindingContext
            {
                FallbackToEmptyPrefix = true,
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelState = new ModelStateDictionary()
            };

            // Act
            var isBound = shimBinder.BindModel(bindingContext);

            // Assert
            Assert.False(isBound);
            Assert.Null(bindingContext.Model);
        }

        [Fact]
        public void BindModel_WithDefaultBinders_BindsSimpleType()
        {
            // Arrange
            var binder = CreateBinderWithDefaults();

            var valueProvider = new SimpleValueProvider
            {
                { "firstName", "firstName-value"},
                { "lastName", "lastName-value"}
            };
            var bindingContext = CreateBindingContext(binder, valueProvider, typeof(SimpleModel));

            // Act
            var isBound = binder.BindModel(bindingContext);

            // Assert
            Assert.True(isBound);
            var model = Assert.IsType<SimpleModel>(bindingContext.Model);
            Assert.Equal("firstName-value", model.FirstName);
            Assert.Equal("lastName-value", model.LastName);
        }

        [Fact]
        public void BindModel_WithDefaultBinders_BindsComplexType()
        {
            // Arrange
            var binder = CreateBinderWithDefaults();

            var valueProvider = new SimpleValueProvider
            {
                { "firstName", "firstName-value"},
                { "lastName", "lastName-value"},
                { "friends[0].firstName", "first-friend"},
                { "friends[0].age", "40"},
                { "friends[0].friends[0].firstname", "nested friend"},
                { "friends[1].firstName", "some other"},
                { "friends[1].lastName", "name"},
            };
            var bindingContext = CreateBindingContext(binder, valueProvider, typeof(Person));

            // Act
            var isBound = binder.BindModel(bindingContext);

            // Assert
            Assert.True(isBound);
            var model = Assert.IsType<Person>(bindingContext.Model);
            Assert.Equal("firstName-value", model.FirstName);
            Assert.Equal("lastName-value", model.LastName);
            Assert.Equal(2, model.Friends.Count);
            Assert.Equal("first-friend", model.Friends[0].FirstName);
            Assert.Equal(40, model.Friends[0].Age);
            var nestedFriend = Assert.Single(model.Friends[0].Friends);
            Assert.Equal("nested friend", nestedFriend.FirstName);
            Assert.Equal("some other", model.Friends[1].FirstName);
            Assert.Equal("name", model.Friends[1].LastName);
        }

        private static ModelBindingContext CreateBindingContext(IModelBinder binder,
                                                                IValueProvider valueProvider,
                                                                Type type)
        {
            var metadataProvider = new DataAnnotationsModelMetadataProvider();
            var bindingContext = new ModelBindingContext
            {
                ModelBinder = binder,
                FallbackToEmptyPrefix = true,
                MetadataProvider = metadataProvider,
                ModelMetadata = metadataProvider.GetMetadataForType(null, type),
                ModelState = new ModelStateDictionary(),
                ValueProvider = valueProvider,
                ValidatorProviders = Enumerable.Empty<IModelValidatorProvider>()
            };
            return bindingContext;
        }

        private static CompositeModelBinder CreateBinderWithDefaults()
        {
            var binders = new IModelBinder[] 
            { 
                new TypeMatchModelBinder(), 
                new GenericModelBinder(),
                new ComplexModelDtoModelBinder(),
                new TypeConverterModelBinder(),
                new MutableObjectModelBinder()
            };
            var binder = new CompositeModelBinder(binders);
            return binder;
        }

        private class SimpleModel
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }

        private sealed class Person
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public int Age { get; set; }

            public List<Person> Friends { get; set; }
        }

        private class SimpleValueProvider : Dictionary<string, object>, IValueProvider
        {
            private readonly CultureInfo _culture;

            public SimpleValueProvider()
                : this(null)
            {
            }

            public SimpleValueProvider(CultureInfo culture)
                : base(StringComparer.OrdinalIgnoreCase)
            {
                _culture = culture ?? CultureInfo.InvariantCulture;
            }

            // copied from ValueProviderUtil
            public bool ContainsPrefix(string prefix)
            {
                foreach (string key in Keys)
                {
                    if (key != null)
                    {
                        if (prefix.Length == 0)
                        {
                            return true; // shortcut - non-null key matches empty prefix
                        }

                        if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        {
                            if (key.Length == prefix.Length)
                            {
                                return true; // exact match
                            }
                            else
                            {
                                switch (key[prefix.Length])
                                {
                                    case '.': // known separator characters
                                    case '[':
                                        return true;
                                }
                            }
                        }
                    }
                }

                return false; // nothing found
            }

            public ValueProviderResult GetValue(string key)
            {
                object rawValue;
                if (TryGetValue(key, out rawValue))
                {
                    return new ValueProviderResult(rawValue, Convert.ToString(rawValue, _culture), _culture);
                }
                else
                {
                    // value not found
                    return null;
                }
            }
        }
    }
}
#endif
