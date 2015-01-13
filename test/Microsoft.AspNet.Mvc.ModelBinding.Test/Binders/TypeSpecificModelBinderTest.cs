using System;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding.Test
{
    public class TypeSpecificModelBinderTest
    {
        [Fact]
        public async Task BindModel_SameType_Succeeds()
        {
            // Arrange
            var binder = new TypeSpecificModelBinder(typeof(Person), CreatePersonBinder());
            var valueProvider = new SimpleHttpValueProvider
            {
                { "FirstName", "Foo" },
                { "LastName", "Bar" }
            };
            var bindingContext = GetBindingContext(valueProvider, typeof(Person));

            // Act
            var retVal = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.True(retVal);

            var boundModel = bindingContext.Model as Person;
            Assert.Equal("Foo Bar", boundModel.FullName);
        }

        [Fact]
        public async Task BindModel_DifferentType_Fails()
        {
            // Arrange
            var binder = new TypeSpecificModelBinder(typeof(Person), CreatePersonBinder());
            var valueProvider = new SimpleHttpValueProvider
            {
                { "FirstName", "Foo" },
                { "LastName", "Bar" }
            };
            var bindingContext = GetBindingContext(valueProvider, typeof(string));

            // Act
            var retVal = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.False(retVal);
        }

        private static ModelBindingContext GetBindingContext(IValueProvider valueProvider, Type type)
        {
            var metadataProvider = new EmptyModelMetadataProvider();
            var bindingContext = new ModelBindingContext
            {
                ModelMetadata = metadataProvider.GetMetadataForType(null, type),
                ValueProvider = valueProvider
            };
            return bindingContext;
        }

        private static IModelBinder CreatePersonBinder()
        {
            var mockPersonBinder = new Mock<IModelBinder>();
            mockPersonBinder
                .Setup(o => o.BindModelAsync(It.IsAny<ModelBindingContext>()))
                .Returns(async (ModelBindingContext mbc) =>
                {
                    var firstNameResult = await mbc.ValueProvider.GetValueAsync("FirstName");
                    var lastNameResult = await mbc.ValueProvider.GetValueAsync("LastName");

                    var person = new Person
                    {
                        FirstName = firstNameResult.AttemptedValue,
                        LastName = lastNameResult.AttemptedValue
                    };
                    person.FullName = string.Format("{0} {1}", person.FirstName, person.LastName);
                    mbc.Model = person;

                    return true;
                });
            return mockPersonBinder.Object;
        }

        private class Person
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string FullName { get; set; }
        }
    }
}