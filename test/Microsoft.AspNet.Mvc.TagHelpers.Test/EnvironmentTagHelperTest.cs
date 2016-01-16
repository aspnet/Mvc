// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Razor.TagHelpers;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.TagHelpers.Test
{
    public class EnvironmentTagHelperTest
    {
        [Theory]
        [InlineData("Development", "Development")]
        [InlineData("development", "Development")]
        [InlineData("DEVELOPMENT", "Development")]
        [InlineData(" development", "Development")]
        [InlineData("development ", "Development")]
        [InlineData(" development ", "Development")]
        [InlineData("Development,Production", "Development")]
        [InlineData("Production,Development", "Development")]
        [InlineData("Development , Production", "Development")]
        [InlineData("   Development,Production   ", "Development")]
        [InlineData("Development ,  Production", "Development")]
        [InlineData("Development\t,Production", "Development")]
        [InlineData("Development,\tProduction", "Development")]
        [InlineData(" Development,Production ", "Development")]
        [InlineData("Development,Staging,Production", "Development")]
        [InlineData("Staging,Development,Production", "Development")]
        [InlineData("Staging,Production,Development", "Development")]
        [InlineData("Test", "Test")]
        [InlineData("Test,Staging", "Test")]
        public void ShowsContentWhenCurrentEnvironmentIsSpecified(string namesAttribute, string environmentName)
        {
            ShouldShowContent(namesAttribute, environmentName);
        }

        [Theory]
        [InlineData("", "Development")]
        [InlineData(null, "Development")]
        [InlineData("  ", "Development")]
        [InlineData(", ", "Development")]
        [InlineData("   , ", "Development")]
        [InlineData("\t,\t", "Development")]
        [InlineData(",", "Development")]
        [InlineData(",,", "Development")]
        [InlineData(",,,", "Development")]
        [InlineData(",,, ", "Development")]
        public void ShowsContentWhenNoEnvironmentIsSpecified(string namesAttribute, string environmentName)
        {
            ShouldShowContent(namesAttribute, environmentName);
        }

        [Theory]
        [InlineData("Development", null)]
        [InlineData("Development", "")]
        [InlineData("Development", " ")]
        [InlineData("Development", "  ")]
        [InlineData("Development", "\t")]
        [InlineData("Test", null)]
        public void ShowsContentWhenCurrentEnvironmentIsNotSet(string namesAttribute, string environmentName)
        {
            ShouldShowContent(namesAttribute, environmentName);
        }

        [Theory]
        [InlineData("NotDevelopment", "Development")]
        [InlineData("NOTDEVELOPMENT", "Development")]
        [InlineData("NotDevelopment,AlsoNotDevelopment", "Development")]
        [InlineData("Doesn'tMatchAtAll", "Development")]
        [InlineData("Development and a space", "Development")]
        [InlineData("Development and a space,SomethingElse", "Development")]
        public void DoesNotShowContentWhenCurrentEnvironmentIsNotSpecified(
            string namesAttribute,
            string environmentName)
        {
            // Arrange
            var content = "content";
            var context = MakeTagHelperContext(attributes: new TagHelperAttributeList { { "names", namesAttribute } });
            var output = MakeTagHelperOutput("environment", childContent: content);
            var hostingEnvironment = new Mock<IHostingEnvironment>();
            hostingEnvironment.SetupProperty(h => h.EnvironmentName);
            hostingEnvironment.Object.EnvironmentName = environmentName;

            // Act
            var helper = new EnvironmentTagHelper(hostingEnvironment.Object)
            {
                Names = namesAttribute
            };
            helper.Process(context, output);

            // Assert
            Assert.Null(output.TagName);
            Assert.Empty(output.PreContent.GetContent());
            Assert.True(output.Content.IsEmpty);
            Assert.Empty(output.PostContent.GetContent());
            Assert.True(output.IsContentModified);
        }

        private void ShouldShowContent(string namesAttribute, string environmentName)
        {
            // Arrange
            var content = "content";
            var context = MakeTagHelperContext(
                attributes: new TagHelperAttributeList { { "names", namesAttribute } });
            var output = MakeTagHelperOutput("environment", childContent: content);
            var hostingEnvironment = new Mock<IHostingEnvironment>();
            hostingEnvironment.SetupProperty(h => h.EnvironmentName);
            hostingEnvironment.Object.EnvironmentName = environmentName;

            // Act
            var helper = new EnvironmentTagHelper(hostingEnvironment.Object)
            {
                Names = namesAttribute
            };
            helper.Process(context, output);

            // Assert
            Assert.Null(output.TagName);
            Assert.False(output.IsContentModified);
        }

        private TagHelperContext MakeTagHelperContext(TagHelperAttributeList attributes = null)
        {
            attributes = attributes ?? new TagHelperAttributeList();

            return new TagHelperContext(
                attributes,
                items: new Dictionary<object, object>(),
                uniqueId: Guid.NewGuid().ToString("N"));
        }

        private TagHelperOutput MakeTagHelperOutput(
            string tagName,
            TagHelperAttributeList attributes = null,
            string childContent = null)
        {
            attributes = attributes ?? new TagHelperAttributeList();

            return new TagHelperOutput(
                tagName,
                attributes,
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent(childContent);
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
        }
    }
}