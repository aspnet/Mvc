﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
        public void CheckBoxOverridesCalculatedValuesWithValuesFromHtmlAttributes()
        {
            // Arrange
            var expected = @"<input checked=""checked"" id=""Property3"" name=""Property3"" type=""checkbox"" " +
                           @"value=""false"" /><input name=""Property3"" type=""hidden"" value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetTestModelViewData());

            // Act
            var html = helper.CheckBox("Property3",
                                       isChecked: null,
                                       htmlAttributes: new { @checked = "checked", value = "false" });

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxExplicitParametersOverrideDictionary_ForValueInModel()
        {
            // Arrange
            var expected = @"<input checked=""checked"" id=""Property3"" name=""Property3"" type=""checkbox"" " +
                           @"value=""false"" /><input name=""Property3"" type=""hidden"" value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetTestModelViewData());

            // Act
            var html = helper.CheckBox("Property3",
                                       isChecked: true,
                                       htmlAttributes: new { @checked = "unchecked", value = "false" });

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxExplicitParametersOverrideDictionary_ForNullModel()
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
        public void CheckBoxWithInvalidBooleanThrows()
        {
            // Arrange
            var expected = "String was not recognized as a valid Boolean.";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetTestModelViewData());

            // Act & Assert
            var ex = Assert.Throws<FormatException>(
                        () => helper.CheckBox("Property2", isChecked: null, htmlAttributes: null));
            Assert.Equal(expected, ex.Message);
        }

        [Fact]
        public void CheckBoxCheckedWithOnlyName()
        {
            // Arrange
            var expected = @"<input checked=""checked"" id=""Property1"" name=""Property1"" type=""checkbox"" " +
                           @"value=""true"" /><input name=""Property1"" type=""hidden"" value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetTestModelViewData());

            // Act
            var html = helper.CheckBox("Property1", isChecked: true, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxUsesAttemptedValueFromModelState()
        {
            // Arrange
            var expected = @"<input id=""Property1"" name=""Property1"" type=""checkbox"" value=""true"" />" +
                           @"<input name=""Property1"" type=""hidden"" value=""false"" />";
            var valueProviderResult = new ValueProviderResult("false", "false", CultureInfo.InvariantCulture);
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetTestModelViewData());
            helper.ViewData.ModelState.SetModelValue("Property1", valueProviderResult);

            // Act
            var html = helper.CheckBox("Property1", isChecked: null, htmlAttributes: null);

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
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetModelWithValidationViewData());

            // Act
            var html = helper.CheckBox("Name", isChecked: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxReplacesUnderscoresInHtmlAttributesWithDashes()
        {
            // Arrange
            var expected = @"<input Property1-Property3=""Property3ObjValue"" checked=""checked"" id=""Property1"" " +
                           @"name=""Property1"" type=""checkbox"" value=""true"" />" +
                           @"<input name=""Property1"" type=""hidden"" value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetTestModelViewData());
            var htmlAttributes = new { Property1_Property3 = "Property3ObjValue" };

            // Act
            var html = helper.CheckBox("Property1", isChecked: true, htmlAttributes: htmlAttributes);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxWithPrefix_ReplaceDotsInIdByDefaultWithUnderscores()
        {
            // Arrange
            var expected = @"<input Property3=""Property3Value"" id=""MyPrefix_Property1"" " +
                           @"name=""MyPrefix.Property1"" type=""checkbox"" value=""true"" /><input " +
                           @"name=""MyPrefix.Property1"" type=""hidden"" value=""false"" />";
            var dictionary = new RouteValueDictionary(new { Property3 = "Property3Value" });
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            var html = helper.CheckBox("Property1", isChecked: false, htmlAttributes: dictionary);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxWithPrefix_ReplacesDotsInIdWithIdDotReplacement()
        {
            // Arrange
            var expected = @"<input Property3=""Property3Value"" id=""MyPrefix!!!Property1"" " +
                           @"name=""MyPrefix.Property1"" type=""checkbox"" value=""true"" /><input " +
                           @"name=""MyPrefix.Property1"" type=""hidden"" value=""false"" />";
            var dictionary = new Dictionary<string, object> { { "Property3", "Property3Value" } };
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();
            helper.IdAttributeDotReplacement = "!!!";
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            var html = helper.CheckBox("Property1", isChecked: false, htmlAttributes: dictionary);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxWithPrefixAndEmptyName()
        {
            // Arrange
            var expected = @"<input Property3=""Property3Value"" id=""MyPrefix"" name=""MyPrefix"" " +
                           @"type=""checkbox"" value=""true"" /><input name=""MyPrefix"" type=""hidden"" " +
                           @"value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(model: false);
            var attributes = new Dictionary<string, object> { { "Property3", "Property3Value" } };
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            var html = helper.CheckBox(string.Empty, isChecked: false, htmlAttributes: attributes);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxWithComplexExpressionsEvaluatesValuesInViewDataDictionary()
        {
            // Arrange
            var expected = @"<input checked=""checked"" id=""ComplexProperty_Property1"" name=""ComplexProperty."+
                           @"Property1"" type=""checkbox"" value=""true"" /><input name=""ComplexProperty.Property1""" +
                           @" type=""hidden"" value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetModelWithValidationViewData());

            // Act
            var html = helper.CheckBox("ComplexProperty.Property1", isChecked: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        //// CheckBoxFor

        [Fact]
        public void CheckBoxForWithInvalidBooleanThrows()
        {
            // Arrange
            var expected = "String was not recognized as a valid Boolean.";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetTestModelViewData());

            // Act & Assert
            // "Property2" in ViewData isn't a valid boolean
            var ex = Assert.Throws<FormatException>(() => helper.CheckBoxFor(m => m.Property2));
            Assert.Equal(expected, ex.Message);
        }

        [Fact]
        public void CheckBoxForOverridesCalculatedParametersWithValuesFromHtmlAttributes()
        {
            // Arrange
            var expected = @"<input checked=""checked"" id=""Property3"" name=""Property3"" type=""checkbox"" " +
                           @"value=""false"" /><input name=""Property3"" type=""hidden"" value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetTestModelViewData());

            // Act
            var html = helper.CheckBoxFor(m => m.Property3, new { @checked = "checked", value = "false" });

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

            // Act
            var html = helper.CheckBoxFor(m => m.Name, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxForUsesModelStateAttemptedValue()
        {
            // Arrange
            var expected = @"<input id=""Property1"" name=""Property1"" type=""checkbox"" value=""true"" />" +
                           @"<input name=""Property1"" type=""hidden"" value=""false"" />";
            var viewData = GetTestModelViewData();
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(viewData);
            var valueProviderResult = new ValueProviderResult("false", "false", CultureInfo.InvariantCulture);
            viewData.ModelState.SetModelValue("Property1", valueProviderResult);

            // Act
            var html = helper.CheckBoxFor(m => m.Property1);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxForWithObjectAttributeWithUnderscores()
        {
            // Arrange
            var expected = @"<input Property1-Property3=""Property3ObjValue"" checked=""checked"" id=""Property1"" " +
                           @"name=""Property1"" type=""checkbox"" value=""true"" />" +
                           @"<input name=""Property1"" type=""hidden"" value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetTestModelViewData());
            var htmlAttributes = new { Property1_Property3 = "Property3ObjValue" };

            // Act
            var html = helper.CheckBoxFor(m => m.Property1, htmlAttributes);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxForWithAttributeDictionary()
        {
            // Arrange
            var expected = @"<input Property3=""Property3Value"" checked=""checked"" id=""Property1"" " +
                           @"name=""Property1"" type=""checkbox"" value=""true"" /><input name=""Property1"" " +
                           @"type=""hidden"" value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetTestModelViewData());
            var attributes = new Dictionary<string, object> { { "Property3", "Property3Value" } };

            // Act
            var html = helper.CheckBoxFor(m => m.Property1, attributes);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckBoxForWithPrefix()
        {
            // Arrange
            var expected = @"<input Property3=""PropValue"" id=""MyPrefix_Property1"" name=""MyPrefix.Property1"" " +
                           @"type=""checkbox"" value=""true"" /><input name=""MyPrefix.Property1"" type=""hidden"" " +
                           @"value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetTestModelViewData());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";
            var attributes = new Dictionary<string, object> { { "Property3", "PropValue" } };

            // Act
            var html = helper.CheckBoxFor(m => m.Property1, attributes);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void CheckboxForWithComplexExpressionsUsesValuesFromViewDataDictionary()
        {
            // Arrange
            var expected = @"<input checked=""checked"" id=""ComplexProperty_Property1"" name=""ComplexProperty." +
                        @"Property1"" type=""checkbox"" value=""true"" /><input name=""ComplexProperty.Property1"" " +
                        @"type=""hidden"" value=""false"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetModelWithValidationViewData());

            // Act
            var html = helper.CheckBoxFor(m => m.ComplexProperty.Property1, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        // Hidden

        [Fact]
        public void HiddenWithBinaryArrayValueRendersBase64EncodedValue()
        {
            // Arrange
            var expected = @"<input id=""ProductName"" name=""ProductName"" type=""hidden"" value=""Fys1"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();

            // Act
            var result = helper.Hidden("ProductName", new byte[] { 23, 43, 53 }, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        public static IEnumerable<object[]> HiddenWithAttributesData
        {
            get
            {
                var expected1 = @"<input baz=""BazValue"" id=""Property1"" name=""Property1"" type=""hidden"" value=""test"" />";
                yield return new object[] { new Dictionary<string, object> { { "baz", "BazValue" } }, expected1 };
                yield return new object[] { new { baz = "BazValue" }, expected1 };

                var expected2 = @"<input foo-baz=""BazValue"" id=""Property1"" name=""Property1"" type=""hidden"" " +
                                @"value=""test"" />";
                yield return new object[] { new Dictionary<string, object> { { "foo-baz", "BazValue" } }, expected2 };
            }
        }

        [Theory]
        [MemberData(nameof(HiddenWithAttributesData))]
        public void HiddenWithExplicitValueAndAttributesDictionary(object attributes, string expected)
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetHiddenViewData());

            // Act
            var result = helper.Hidden("Property1", "test", attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenUsesValueFromViewDataDictionaryIfValuePropertyIsNull()
        {
            // Arrange
            var expected = @"<input id=""Property1"" name=""Property1"" type=""hidden"" value=""VDDValue"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetHiddenViewData());

            // Act
            var result = helper.Hidden("Property1", value: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenReturnsEmptyValueIfPropertyIsNotFound()
        {
            // Arrange
            var expected = @"<input baz=""BazValue"" id=""keyNotFound"" name=""keyNotFound"" type=""hidden"" " +
                           @"value="""" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetHiddenViewData());
            var attributes = new Dictionary<string, object> { { "baz", "BazValue" } };

            // Act
            var result = helper.Hidden("keyNotFound", value: null, htmlAttributes: attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenWithPrefix()
        {
            // Arrange
            var expected = @"<input id=""MyPrefix_Property1"" name=""MyPrefix.Property1"" type=""hidden"" " +
                           @"value=""PropValue"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetHiddenViewData());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            var result = helper.Hidden("Property1", "PropValue", htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenWithPrefixAndEmptyName()
        {
            // Arrange
            var expected = @"<input id=""MyPrefix"" name=""MyPrefix"" type=""hidden"" value=""fooValue"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetHiddenViewData());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            var result = helper.Hidden(string.Empty, "fooValue", htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenWithViewDataErrors()
        {
            // Arrange
            var expected = @"<input baz=""BazValue"" class=""input-validation-error some-class"" id=""Property1""" +
                           @" name=""Property1"" type=""hidden"" value=""AttemptedValueFoo"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetHiddenViewDataWithErrors());
            var attributes = new Dictionary<string, object>
            {
                { "baz", "BazValue" },
                { "class", "some-class"}
            };

            // Act
            var result = helper.Hidden("Property1", value: null, htmlAttributes: attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        // HiddenFor

        [Fact]
        public void HiddenForWithStringValue()
        {
            // Arrange
            var expected = @"<input id=""Property1"" name=""Property1"" type=""hidden"" value=""DefaultValue"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetHiddenViewData());
            helper.ViewData.Model.Property1 = "DefaultValue";

            // Act
            var result = helper.HiddenFor(m => m.Property1, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenForWithByteArrayValueRendersBase64EncodedValue()
        {
            // Arrange
            var expected = @"<input id=""Bytes"" name=""Bytes"" type=""hidden"" value=""Fys1"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetHiddenViewData());
            helper.ViewData.Model.Bytes = new byte[] { 23, 43, 53 };

            // Act
            var result = helper.HiddenFor(m => m.Bytes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Theory]
        [MemberData(nameof(HiddenWithAttributesData))]
        public void HiddenForWithAttributes(object htmlAttributes, string expected)
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetHiddenViewData());
            helper.ViewData.Model.Property1 = "test";

            // Act
            var result = helper.HiddenFor(m => m.Property1, htmlAttributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenForWithPrefix()
        {
            // Arrange
            var expected = @"<input id=""MyPrefix_Property1"" name=""MyPrefix.Property1"" type=""hidden"" " +
                           @"value=""propValue"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetHiddenViewData());
            helper.ViewData.Model.Property1 = "propValue";
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            var result = helper.HiddenFor(m => m.Property1);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenForWithViewDataErrors()
        {
            // Arrange
            var expected = @"<input baz=""BazValue"" class=""input-validation-error some-class"" id=""Property1"" " +
                           @"name=""Property1"" type=""hidden"" value=""AttemptedValueFoo"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetHiddenViewDataWithErrors());
            var attributes = new Dictionary<string, object>
            {
                { "baz", "BazValue" },
                { "class", "some-class"}
            };

            // Act
            var result = helper.HiddenFor(m => m.Property1, attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        private static ViewDataDictionary<TestModel> GetTestModelViewData()
        {
            return new ViewDataDictionary<TestModel>(new EmptyModelMetadataProvider())
            {
                { "Property1", true },
                { "Property2", "NotTrue" },
                { "Property3", false }
            };
        }

        private static ViewDataDictionary<ModelWithValidation> GetModelWithValidationViewData()
        {
            var provider = new DataAnnotationsModelMetadataProvider();
            var viewData = new ViewDataDictionary<ModelWithValidation>(provider)
            {
                { "ComplexProperty.Property1", true },
                { "ComplexProperty.Property2", "NotTrue" },
                { "ComplexProperty.Property3", false }
            };
            viewData.Model = new ModelWithValidation();

            return viewData;
        }

        private static ViewDataDictionary<HiddenModel> GetHiddenViewData()
        {
            return new ViewDataDictionary<HiddenModel>(new EmptyModelMetadataProvider())
            {
                Model = new HiddenModel(),
                ["Property1"] = "VDDValue",
            };
        }

        private static ViewDataDictionary<HiddenModel> GetHiddenViewDataWithErrors()
        {
            var viewData = GetHiddenViewData();
            var modelState = new ModelState();

            modelState.Errors.Add("error 1");
            modelState.Errors.Add("error 2");
            modelState.Value = new ValueProviderResult("AttemptedValueFoo", "AttemptedValueFoo", CultureInfo.InvariantCulture);

            viewData.ModelState.Add("Property1", modelState);
            return viewData;
        }

        private class HiddenModel
        {
            public string Property1 { get; set; }

            public byte[] Bytes { get; set; }
        }

        private class TestModel
        {
            public bool Property1 { get; set; }

            public bool Property2 { get; set; }

            public bool Property3 { get; set; }
        }

        private class ModelWithValidation
        {
            [Required]
            public bool Name { get; set; }

            public TestModel ComplexProperty { get; set; }
        }
    }
}