using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Razor.ViewComponentTagHelpers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Test.ViewComponentTagHelpers
{
    public class ViewComponentTagHelperDescriptorFactoryTest
    {
        public static TheoryData<string, IEnumerable<TagHelperDescriptor>> AssemblyData
        {
            get
            {
                var provider = new TestViewComponentDescriptorProvider();

                var assemblyOne = "Microsoft.AspNetCore.Mvc.Razor";
                var assemblyTwo = "Microsoft.AspNetCore.Mvc.Razor.Test";
                var assemblyNone = "";

                return new TheoryData<string, IEnumerable<TagHelperDescriptor>>
                {
                    { assemblyOne, new List<TagHelperDescriptor> { provider.tagHelperDescriptorOne } },
                    { assemblyTwo, new List<TagHelperDescriptor> { provider.tagHelperDescriptorTwo } },
                    { assemblyNone, new List<TagHelperDescriptor>() }
                };
            }
        }

        [Theory]
        [MemberData(nameof(AssemblyData))]
        public void CreateDescriptors_ReturnsCorrectDescriptors_ForViewComponentsInAssembly(
            string assemblyName, 
            IEnumerable<TagHelperDescriptor> expectedDescriptors)
        {
            // Arrange
            var provider = new TestViewComponentDescriptorProvider();
            var factory = new ViewComponentTagHelperDescriptorFactory(provider);

            // Act
            var descriptors = factory.CreateDescriptors(assemblyName);

            // Assert
            Assert.Equal(descriptors.Count(), expectedDescriptors.Count());
            
            if (descriptors.Count() > 0) // Either empty or 1 descriptor.
            {
                Assert.Single(descriptors);

                var descriptor = descriptors.First();
                var expectedDescriptor = expectedDescriptors.First();

                Assert.Equal(descriptor.TagName, expectedDescriptor.TagName);
                Assert.Equal(descriptor.TypeName, expectedDescriptor.TypeName);
                Assert.Equal(descriptor.AssemblyName, expectedDescriptor.AssemblyName);
                Assert.Equal(descriptor.TagStructure, expectedDescriptor.TagStructure);

                // Check attributes.
                Assert.Equal(descriptor.Attributes.Count(), expectedDescriptor.Attributes.Count());
                for (var i = 0; i < descriptor.Attributes.Count(); i++)
                {
                    var attribute = descriptor.Attributes.ElementAt(i);
                    var expectedAttribute = expectedDescriptor.Attributes.ElementAt(i);

                    Assert.Equal(attribute.Name, expectedAttribute.Name);
                    Assert.Equal(attribute.PropertyName, expectedAttribute.PropertyName);
                    Assert.Equal(attribute.TypeName, expectedAttribute.TypeName);
                }
  
                // Check required attributes.
                Assert.Equal(descriptor.RequiredAttributes.Count(), expectedDescriptor.RequiredAttributes.Count());
                for (int i = 0; i < descriptor.RequiredAttributes.Count(); i++)
                {
                    var requiredAttribute = descriptor.RequiredAttributes.ElementAt(i);
                    var expectedRequiredAttribute = expectedDescriptor.RequiredAttributes.ElementAt(i);

                    Assert.Equal(requiredAttribute.Name, expectedRequiredAttribute.Name);
                }
            }
        }

        private class TestViewComponentDescriptorProvider : IViewComponentDescriptorProvider
        {
            private ViewComponentDescriptor _viewComponentDescriptorOne = new ViewComponentDescriptor
            {
                DisplayName = "OneDisplayName",
                FullName = "OneViewComponent",
                ShortName = "One",
                MethodInfo = typeof(ViewComponentTagHelperDescriptorFactoryTest)
                    .GetMethod("TestInvokeOne"),
                TypeInfo = typeof(ViewComponentTagHelperDescriptorFactory).GetTypeInfo()
            };

            private ViewComponentDescriptor _viewComponentDescriptorTwo = new ViewComponentDescriptor
            {
                DisplayName = "TwoDisplayName",
                FullName = "TwoViewComponent",
                ShortName = "Two",
                MethodInfo = typeof(ViewComponentTagHelperDescriptorFactoryTest)
                    .GetMethod("TestInvokeTwo"),
                TypeInfo = typeof(ViewComponentTagHelperDescriptorFactoryTest).GetTypeInfo()
            };

            public TagHelperDescriptor tagHelperDescriptorOne = new TagHelperDescriptor
            {
                TagName = "vc:one",
                TypeName = "__Generated__OneViewComponentTagHelper",
                AssemblyName = "Microsoft.AspNetCore.Mvc.Razor",
                Attributes = new List<TagHelperAttributeDescriptor>
                {
                    new TagHelperAttributeDescriptor
                    {
                        Name = "foo",
                        PropertyName = "foo",
                        TypeName = "System.String",
                        IsStringProperty = true
                    },
                    new TagHelperAttributeDescriptor
                    {
                        Name = "bar",
                        PropertyName = "bar",
                        TypeName = "System.String",
                        IsStringProperty = true
                    }
                },
                RequiredAttributes = new List<TagHelperRequiredAttributeDescriptor>
                {
                    new TagHelperRequiredAttributeDescriptor
                    {
                        Name = "foo"
                    },
                    new TagHelperRequiredAttributeDescriptor
                    {
                        Name = "bar"
                    }
                },
                TagStructure = TagStructure.NormalOrSelfClosing
            };

            public TagHelperDescriptor tagHelperDescriptorTwo = new TagHelperDescriptor
            {
                TagName = "vc:two",
                TypeName = "__Generated__TwoViewComponentTagHelper",
                AssemblyName = "Microsoft.AspNetCore.Mvc.Razor.Test",
                Attributes = new List<TagHelperAttributeDescriptor>
                {
                    new TagHelperAttributeDescriptor
                    {
                        Name = "baz",
                        PropertyName = "baz",
                        TypeName = "System.Int32"
                    }
                },
                RequiredAttributes = new List<TagHelperRequiredAttributeDescriptor>(),
                TagStructure = TagStructure.NormalOrSelfClosing
            };

            public IEnumerable<ViewComponentDescriptor> GetViewComponents()
            {
                return new List<ViewComponentDescriptor>
                {
                    _viewComponentDescriptorOne,
                    _viewComponentDescriptorTwo
                };
            }
        }

        public void TestInvokeOne(string foo, string bar)
        {
        }

        public void TestInvokeTwo(int baz = 5)
        {
        }
    }
}
