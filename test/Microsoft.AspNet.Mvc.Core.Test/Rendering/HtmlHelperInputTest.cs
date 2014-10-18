// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Routing;
using Xunit;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class HtmlHelperInputTest
    {
        // CheckBox

        [Fact]
        public void CheckBoxDictionaryOverridesImplicitParameters()
        {
            // Arrange
            var expected = @"<input checked=""checked"" id=""baz"" name=""baz"" type=""checkbox"" value=""false"" />" +
                           @"<input name=""baz"" type=""hidden"" value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetCheckBoxViewData());

            // Act
            var html = helper.CheckBox("baz",
                                       isChecked: null,
                                       htmlAttributes: new { @checked = "checked", value = "false" });

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxExplicitParametersOverrideDictionary()
        {
            // Arrange
            var expected = @"<input checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" value=""false"" />" +
                           @"<input name=""foo"" type=""hidden"" value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();

            // Act
            var html = helper.CheckBox("foo",
                                       isChecked: true,
                                       htmlAttributes: new { @checked = "unchecked", value = "false" });

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxDoesNotCopyAttributesForHidden()
        {
            // Arrange
            var expected = @"<input checked=""checked"" id=""myID"" name=""foo"" type=""checkbox"" value=""true"" />" +
                           @"<input name=""foo"" type=""hidden"" value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();

            // Act
            var html = helper.CheckBox("foo",
                                       isChecked: true,
                                       htmlAttributes: new { id = "myID" });

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxWithInvalidBooleanThrows()
        {
            // Arrange
            var expected = "String was not recognized as a valid Boolean.";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetCheckBoxViewData());

            // Act & Assert
            var ex = Assert.Throws<FormatException>(
                        () => helper.CheckBox("bar", isChecked: null, htmlAttributes: null));
            Assert.Equal(expected, ex.Message);
        }

        [Fact]
        public void CheckBoxCheckedWithOnlyName()
        {
            // Arrange
            var expected = @"<input checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" value=""true"" />" +
                           @"<input name=""foo"" type=""hidden"" value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();

            // Act
            var html = helper.CheckBox("foo", isChecked: true, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxShouldRespectModelStateAttemptedValue()
        {
            // Arrange
            var expected = @"<input id=""foo"" name=""foo"" type=""checkbox"" value=""true"" />" +
                           @"<input name=""foo"" type=""hidden"" value=""false"" />";
            var valueProviderResult = new ValueProviderResult("false", "false", CultureInfo.InvariantCulture);
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetCheckBoxViewData());
            helper.ViewData.ModelState.SetModelValue("foo", valueProviderResult);

            // Act
            var html = helper.CheckBox("foo", isChecked: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxGeneratesUnobtrusiveValidationAttributes()
        {
            // Arrange
            var expected = @"<input data-val=""true"" data-val-required=""The Name field is required."" id=""Name""" +
                           @" name=""Name"" type=""checkbox"" value=""true"" />" +
                           @"<input name=""Name"" type=""hidden"" value=""false"" />";
            var metadataProvider = new DataAnnotationsModelMetadataProvider();
            var viewDataDictionary = new ViewDataDictionary<ModelWithValidation>(metadataProvider)
            {
                Model = new ModelWithValidation()
            };
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(viewDataDictionary);
            helper.ViewContext.ClientValidationEnabled = true;
            helper.ViewContext.FormContext = new FormContext();

            // Act
            var html = helper.CheckBox("Name", isChecked: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxReplacesUnderscoresWithDashes()
        {
            // Arrange
            var expected = @"<input checked=""checked"" foo-baz=""BazObjValue"" id=""foo"" name=""foo"" " +
                           @"type=""checkbox"" value=""true"" />" +
                           @"<input name=""foo"" type=""hidden"" value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetCheckBoxViewData());

            // Act
            var html = helper.CheckBox("foo", isChecked: true, htmlAttributes: new { foo_baz = "BazObjValue" });

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxWithPrefixReplaceDotsInIdByDefaultWithUnderscores()
        {
            // Arrange
            var expected = @"<input baz=""BazValue"" id=""MyPrefix_foo"" name=""MyPrefix.foo"" type=""checkbox"" " +
                           @"value=""true"" /><input name=""MyPrefix.foo"" type=""hidden"" value=""false"" />";
            var dictionary = new RouteValueDictionary(new { baz = "BazValue" });
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            var html = helper.CheckBox("foo", isChecked: false, htmlAttributes: dictionary);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxWithPrefix_ReplacesDotsInIdWithIdDotReplacement()
        {
            // Arrange
            var expected = @"<input baz=""BazValue"" id=""MyPrefix!!!foo"" name=""MyPrefix.foo"" type=""checkbox"" " +
                           @"value=""true"" /><input name=""MyPrefix.foo"" type=""hidden"" value=""false"" />";
            var dictionary = new RouteValueDictionary(new { baz = "BazValue" });
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();
            helper.IdAttributeDotReplacement = "!!!";
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            var html = helper.CheckBox("foo", isChecked: false, htmlAttributes: dictionary);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxWithPrefixAndEmptyName()
        {
            // Arrange
            var expected = @"<input baz=""BazValue"" id=""MyPrefix"" name=""MyPrefix"" type=""checkbox"" " +
                           @"value=""true"" /><input name=""MyPrefix"" type=""hidden"" value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();
            var attributes = new RouteValueDictionary(new { baz = "BazValue" });
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            var html = helper.CheckBox(string.Empty, isChecked: false, htmlAttributes: attributes);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        //// CheckBoxFor

        [Fact]
        public void CheckBoxForWithInvalidBooleanThrows()
        {
            // Arrange
            var expected = "String was not recognized as a valid Boolean.";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetCheckBoxViewData());

            // Act & Assert
            // "bar" in ViewData isn't a valid boolean
            var ex = Assert.Throws<FormatException>(() => helper.CheckBoxFor(m => m.bar));
            Assert.Equal(expected, ex.Message);
        }

        [Fact]
        public void CheckBoxForDictionaryOverridesImplicitParameters()
        {
            // Arrange
            var expected = @"<input checked=""checked"" id=""baz"" name=""baz"" type=""checkbox"" value=""false"" />" +
                           @"<input name=""baz"" type=""hidden"" value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetCheckBoxViewData());

            // Act
            var html = helper.CheckBoxFor(m => m.baz, new { @checked = "checked", value = "false" });

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxForShouldNotCopyAttributesForHidden()
        {
            // Arrange
            var expected = @"<input checked=""checked"" id=""myID"" name=""foo"" type=""checkbox"" value=""true"" />" +
                           @"<input name=""foo"" type=""hidden"" value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetCheckBoxViewData());

            // Act
            var html = helper.CheckBoxFor(m => m.foo, new { id = "myID" });

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxForGeneratesUnobtrusiveValidationAttributes()
        {
            // Arrange
            var expected = @"<input data-val=""true"" data-val-required=""The Name field is required."" id=""Name""" +
                           @" name=""Name"" type=""checkbox"" value=""true"" />" +
                           @"<input name=""Name"" type=""hidden"" value=""false"" />";
            var metadataProvider = new DataAnnotationsModelMetadataProvider();
            var viewDataDictionary = new ViewDataDictionary<ModelWithValidation>(metadataProvider)
            {
                Model = new ModelWithValidation()
            };
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(viewDataDictionary);
            helper.ViewContext.ClientValidationEnabled = true;
            helper.ViewContext.FormContext = new FormContext();

            // Act
            var html = helper.CheckBoxFor(m => m.Name, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxForRespectsModelStateAttemptedValue()
        {
            // Arrange
            var expected = @"<input id=""foo"" name=""foo"" type=""checkbox"" value=""true"" />" +
                           @"<input name=""foo"" type=""hidden"" value=""false"" />";
            var viewData = GetCheckBoxViewData();
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(viewData);
            var valueProviderResult = new ValueProviderResult("false", "false", CultureInfo.InvariantCulture);
            viewData.ModelState.SetModelValue("foo", valueProviderResult);

            // Act
            var html = helper.CheckBoxFor(m => m.foo);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxForWithObjectAttributeWithUnderscores()
        {
            // Arrange
            var expected = @"<input checked=""checked"" foo-baz=""BazObjValue"" id=""foo"" name=""foo"" " +
                           @"type=""checkbox"" value=""true"" />" +
                           @"<input name=""foo"" type=""hidden"" value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetCheckBoxViewData());

            // Act
            var html = helper.CheckBoxFor(m => m.foo, htmlAttributes: new { foo_baz = "BazObjValue" });

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxForWithAttributeDictionary()
        {
            // Arrange
            var expected = @"<input baz=""BazValue"" checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" " +
                           @"value=""true"" /><input name=""foo"" type=""hidden"" value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetCheckBoxViewData());
            var attributes = new RouteValueDictionary(new { baz = "BazValue" });

            // Act
            var html = helper.CheckBoxFor(m => m.foo, attributes);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxForWithPrefix()
        {
            // Arrange
            var expected = @"<input baz=""BazValue"" id=""MyPrefix_foo"" name=""MyPrefix.foo"" type=""checkbox"" " +
                           @"value=""true"" /><input name=""MyPrefix.foo"" type=""hidden"" value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetCheckBoxViewData());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";
            var attributes = new RouteValueDictionary(new { baz = "BazValue" });

            // Act
            var html = helper.CheckBoxFor(m => m.foo, attributes);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        private static ViewDataDictionary<FooBarBazModel> GetCheckBoxViewData()
        {
            return new ViewDataDictionary<FooBarBazModel>(new EmptyModelMetadataProvider())
            {
                { "foo", true },
                { "bar", "NotTrue" },
                { "baz", false }
            };
        }

        private class FooBarBazModel
        {
            public bool foo { get; set; }

            public bool bar { get; set; }

            public bool baz { get; set; }
        }

        private class ModelWithValidation
        {
            [Required]
            public bool Name { get; set; }
        }
    }
}