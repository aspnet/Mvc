using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Host.Test
{
    public class ViewComponentTagHelperDescriptorConventionsTest
    {
        public static TheoryData InvalidDescriptorData
        {
            get
            {
                var noProperties = CreateTagHelperDescriptor();

                var onlyViewComponentProperty = CreateTagHelperDescriptor();
                onlyViewComponentProperty.PropertyBag.Add(
                    ViewComponentTagHelperDescriptorConventions.ViewComponentProperty,
                    "view component property");

                var onlyViewComponentTagHelperProperty = CreateTagHelperDescriptor();
                onlyViewComponentTagHelperProperty.PropertyBag.Add(
                    ViewComponentTagHelperDescriptorConventions.ViewComponentTagHelperProperty,
                    "view component tag helper property");

                return new TheoryData<TagHelperDescriptor>
                {
                    noProperties,
                    onlyViewComponentProperty,
                    onlyViewComponentTagHelperProperty
                };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidDescriptorData))]
        public void IsViewComponentDescriptor_ReturnsFalseForInvalidDescriptor(TagHelperDescriptor descriptor)
        {
            // Arrange, Act
            var isViewComponentDescriptor = ViewComponentTagHelperDescriptorConventions
                .IsViewComponentDescriptor(descriptor);

            // Assert
            Assert.False(isViewComponentDescriptor);
        }

        [Fact]
        public void IsViewComponentDescriptor_ReturnsTrueForValidDescriptor()
        {
            // Arrange
            var descriptor = CreateViewComponentTagHelperDescriptor();

            // Act
            var isViewComponentDescriptor = ViewComponentTagHelperDescriptorConventions
                .IsViewComponentDescriptor(descriptor);

            // Assert
            Assert.True(isViewComponentDescriptor);
        }

        public static TheoryData NameData
        {
            get
            {
                var tagHelperDescriptor = CreateViewComponentTagHelperDescriptor();

                // GetViewComponentName()
                var viewComponentName = ViewComponentTagHelperDescriptorConventions
                    .GetViewComponentName(tagHelperDescriptor);
                var expectedViewComponentName = "ViewComponentName";

                // GetViewComponentTagHelperName()
                var viewComponentTagHelperName = ViewComponentTagHelperDescriptorConventions
                    .GetViewComponentTagHelperName(tagHelperDescriptor);
                var expectedViewComponentTagHelperName = "ViewComponentTagHelperName";

                var viewComponentDescriptor = new ViewComponentDescriptor
                {
                    FullName = "FullName",
                    ShortName = "ShortName"
                };

                // GetTagName()
                var tagName = ViewComponentTagHelperDescriptorConventions.GetTagName(viewComponentDescriptor);
                var expectedTagName = "vc:short-name";

                // GetTypeName()
                var typeName = ViewComponentTagHelperDescriptorConventions.GetTypeName(viewComponentDescriptor);
                var expectedTypeName = "__Generated__ShortNameViewComponentTagHelper";

                return new TheoryData<string, string>
                {
                    { viewComponentName, expectedViewComponentName },
                    { viewComponentTagHelperName, expectedViewComponentTagHelperName },
                    { tagName, expectedTagName },
                    { typeName, expectedTypeName }
                };
            }
        }

        [Theory]
        [MemberData(nameof(NameData))]
        public void GetNameMethods_ReturnCorrectNames(string name, string expectedName)
        {
            // Assert
            Assert.Equal(name, expectedName);
        }

        private static TagHelperDescriptor CreateTagHelperDescriptor()
        {
            var descriptor = new TagHelperDescriptor
            {
                TagName = "tag-name",
                TypeName = "TypeName",
                AssemblyName = "AssemblyName",
            };

            return descriptor;
        }

        private static TagHelperDescriptor CreateViewComponentTagHelperDescriptor()
        {
            var descriptor = CreateTagHelperDescriptor();
            descriptor.PropertyBag.Add(
                ViewComponentTagHelperDescriptorConventions.ViewComponentProperty,
                "ViewComponentName");
            descriptor.PropertyBag.Add(
                ViewComponentTagHelperDescriptorConventions.ViewComponentTagHelperProperty,
                "ViewComponentTagHelperName");

            return descriptor;
        }
    }
}