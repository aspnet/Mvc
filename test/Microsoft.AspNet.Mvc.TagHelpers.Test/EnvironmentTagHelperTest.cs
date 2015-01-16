// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.TagHelpers.Test
{
    public class EnvironmentTagHelperTest
    {
        [Theory]
        [InlineData("Development")]
        [InlineData("development")]
        [InlineData("DEVELOPMENT")]
        [InlineData(" development")]
        [InlineData("development ")]
        [InlineData(" development ")]
        [InlineData("Development,Production")]
        [InlineData("Production,Development")]
        [InlineData("Development , Production")]
        [InlineData(" Development,Production ")]
        [InlineData("Development,Staging,Production")]
        [InlineData("Staging,Development,Production")]
        [InlineData("Staging,Production,Development")]
        public void ShowsContentWhenCurrentEnvironmentIsSpecified(string namesAttribute)
        {
            ShouldShowContent(namesAttribute);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("  ")]
        [InlineData(", ")]
        [InlineData("   , ")]
        [InlineData(",")]
        [InlineData(",,")]
        [InlineData(",,,")]
        [InlineData(",,, ")]
        public void ShowsContentWhenNoEnvironmentIsSpecified(string namesAttribute)
        {
            ShouldShowContent(namesAttribute);
        }

        [Theory]
        [InlineData("NotDevelopment")]
        [InlineData("NOTDEVELOPMENT")]
        [InlineData("NotDevelopment,AlsoNotDevelopment")]
        [InlineData("Doesn'tMatchAtAll")]
        [InlineData("Development and a space")]
        [InlineData("Development and a space,SomethingElse")]
        public void DoesNotShowContentWhenCurrentEnvironmentIsNotSpecified(string namesAttribute)
        {
            var content = "content";
            var context = MakeTagHelperContext(
                attributes: new Dictionary<string, object> { { "names", namesAttribute } },
                content: content);
            var output = MakeTagHelperOutput("environment");
            var hostingEnvironment = new Mock<IHostingEnvironment>();
            hostingEnvironment.SetupProperty(h => h.EnvironmentName);
            hostingEnvironment.Object.EnvironmentName = "Development";

            var helper = new EnvironmentTagHelper
            {
                HostingEnvironment = hostingEnvironment.Object,
                Names = namesAttribute
            };
            helper.Process(context, output);

            Assert.Null(output.TagName);
            Assert.Null(output.PreContent);
            Assert.Null(output.Content);
            Assert.Null(output.PostContent);
            Assert.True(output.ContentSet);
        }

        private void ShouldShowContent(string namesAttribute)
        {
            var content = "content";
            var context = MakeTagHelperContext(
                attributes: new Dictionary<string, object> { { "names", namesAttribute } },
                content: content);
            var output = MakeTagHelperOutput("environment");
            var hostingEnvironment = new Mock<IHostingEnvironment>();
            hostingEnvironment.SetupProperty(h => h.EnvironmentName);
            hostingEnvironment.Object.EnvironmentName = "Development";

            var helper = new EnvironmentTagHelper
            {
                HostingEnvironment = hostingEnvironment.Object,
                Names = namesAttribute
            };
            helper.Process(context, output);

            Assert.Null(output.TagName);
            Assert.False(output.ContentSet);
        }

        private TagHelperContext MakeTagHelperContext(IDictionary<string, object> attributes = null, string content = null)
        {
            if (attributes == null)
            {
                attributes = new Dictionary<string, object>();
            }

            return new TagHelperContext(attributes, Guid.NewGuid().ToString("N"), () => Task.FromResult(content));
        }

        private TagHelperOutput MakeTagHelperOutput(string tagName, IDictionary<string, string> attributes = null)
        {
            if (attributes == null)
            {
                attributes = new Dictionary<string, string>();
            }

            return new TagHelperOutput(tagName, attributes);
        }
    }
}