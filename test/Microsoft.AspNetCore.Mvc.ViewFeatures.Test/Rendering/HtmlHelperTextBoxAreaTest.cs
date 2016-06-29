// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.TestCommon;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    public class HtmlHelperTextBoxAreaTest
    {
        [Fact]
        public void TextAreaFor_GeneratesPlaceholderAttribute_WhenDisplayAttributePromptIsSetAndTypeIsValid()
        {
            // Arrange            
            var model = new TextAreaModelWithAPlaceholder();
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(model);

            // Act
            var result = HtmlContentUtilities.HtmlContentToString(helper.TextAreaFor(m => m.Property1));

            // Assert 
            Assert.True(result.Contains(@"placeholder=""HtmlEncode[[placeholder]]"""));
        }

        [Fact]
        public void TextAreaFor_DoesNotGeneratePlaceholderAttribute_WhenNoPlaceholderPresentInModel()
        {
            // Arrange            
            var model = new TextAreaModelWithoutAPlaceholder();
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(model);

            // Act
            var result = HtmlContentUtilities.HtmlContentToString(helper.TextAreaFor(m => m.Property1));
            
            Assert.False(result.Contains(@"placeholder=""HtmlEncode[[placeholder]]"""));
        }

        private class TextAreaModelWithAPlaceholder
        {
            [Display(Prompt = "placeholder")]
            public string Property1 { get; set; }
        }

        private class TextAreaModelWithoutAPlaceholder
        {
            public string Property1 { get; set; }
        }
    }
}
