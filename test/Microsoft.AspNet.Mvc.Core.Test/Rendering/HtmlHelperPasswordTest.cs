// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Testing;
using Xunit;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class HtmlHelperPasswordTest
    {
        public static IEnumerable<object[]> PasswordWithViewDataAndAttributesData
        {
            get
            {
                var attributes1 = new Dictionary<string, object>
                {
                    { "baz", "BazValue" },
                    { "value", "attribute-value" }
                };

                var attributes2 = new { baz = "BazValue", value = "attribute-value" };

                var vdd = GetViewDataWithModelStateAndModelAndViewDataValues();
                vdd.Model.Prop1 = "does-not-get-used";
                yield return new object[] { vdd, attributes1 };
                yield return new object[] { vdd, attributes2 };

                yield return new object[] { GetViewDataWithNullModelAndNonNullViewData(), attributes1 };
                yield return new object[] { GetViewDataWithNullModelAndNonNullViewData(), attributes2 };
            }
        }

        [Theory]
        [MemberData(nameof(PasswordWithViewDataAndAttributesData))]
        public void Password_UsesArgumentValueWhenValueArgumentIsNull(ViewDataDictionary<PasswordModel> vdd,
                                                                      object attributes)
        {
            // Arrange
            var expected = @"<input baz=""BazValue"" id=""Prop1"" name=""Prop1"" type=""password"" " +
                            @"value=""attribute-value"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(vdd);

            // Act
            var result = helper.Password("Prop1", value: null, htmlAttributes: attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Theory]
        [MemberData(nameof(PasswordWithViewDataAndAttributesData))]
        public void Password_UsesExplicitValue_IfSpecified(ViewDataDictionary<PasswordModel> vdd,
                                                           object attributes)
        {
            // Arrange
            var expected = @"<input baz=""BazValue"" id=""Prop1"" name=""Prop1"" type=""password"" " +
                           @"value=""explicit-value"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(vdd);

            // Act
            var result = helper.Password("Prop1", "explicit-value", attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void PasswordWithPrefix_GeneratesExpectedValue()
        {
            // Arrange
            var expected = @"<input id=""MyPrefix_Prop1"" name=""MyPrefix.Prop1"" type=""password"" " +
                           @"value=""explicit-value"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            var result = helper.Password("Prop1", "explicit-value", htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void PasswordWithPrefix_UsesIdDotReplacementToken()
        {
            // Arrange
            var expected = @"<input id=""MyPrefix$Prop1"" name=""MyPrefix.Prop1"" type=""password"" " +
                           @"value=""explicit-value"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";
            helper.IdAttributeDotReplacement = "$";

            // Act
            var result = helper.Password("Prop1", "explicit-value", htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void PasswordWithPrefixAndEmptyName_GeneratesExpectedValue()
        {
            // Arrange
            var expected = @"<input id=""MyPrefix"" name=""MyPrefix"" type=""password"" value=""explicit-value"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            var result = helper.Password(string.Empty, "explicit-value", htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void PasswordWithEmptyNameAndPrefixThrows()
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            var attributes = new Dictionary<string, object>
            {
                { "class", "some-class"}
            };

            // Act and Assert
            ExceptionAssert.ThrowsArgumentNullOrEmpty(() => helper.Password(string.Empty, string.Empty, attributes),
                                                      "name");
        }

        [Fact]
        public void PasswordWithViewDataErrors_GeneratesExpectedValue()
        {
            // Arrange
            var expected = @"<input baz=""BazValue"" class=""input-validation-error some-class"" id=""Prop1""" +
                           @" name=""Prop1"" type=""password"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithErrors());
            var attributes = new Dictionary<string, object>
            {
                { "baz", "BazValue" },
                { "class", "some-class"}
            };

            // Act
            var result = helper.Password("Prop1", value: null, htmlAttributes: attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void PasswordGeneratesUnobtrusiveValidation()
        {
            // Arrange
            var expected = @"<input data-val=""true"" data-val-required=""The Prop2 field is required."" " +
                           @"id=""Prop2"" name=""Prop2"" type=""password"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());

            // Act
            var result = helper.Password("Prop2", value: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        public static IEnumerable<object[]> PasswordWithComplexExpressions_UsesIdDotSeparatorData
        {
            get
            {
                yield return new object[]
                {
                    "Prop4.Prop5",
                    @"<input data-test=""val"" id=""Prop4$$Prop5"" name=""Prop4.Prop5"" " +
                    @"type=""password"" />",
                };

                yield return new object[]
               {
                    "Prop4.Prop6[0]",
                    @"<input data-test=""val"" id=""Prop4$$Prop6$$0$$"" name=""Prop4.Prop6[0]"" " +
                    @"type=""password"" />",
               };
            }
        }

        [Theory]
        [MemberData(nameof(PasswordWithComplexExpressions_UsesIdDotSeparatorData))]
        public void PasswordWithComplexExpressions_UsesIdDotSeparator(string expression, string expected)
        {
            // Arrange
            var viewData = GetViewDataWithModelStateAndModelAndViewDataValues();
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(viewData);
            helper.IdAttributeDotReplacement = "$$";
            var attributes = new Dictionary<string, object> { { "data-test", "val" } };

            // Act
            var result = helper.Password(expression, value: null, htmlAttributes: attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Theory]
        [MemberData(nameof(PasswordWithViewDataAndAttributesData))]
        public void PasswordForWithAttributes_GeneratesExpectedValue(ViewDataDictionary<PasswordModel> vdd,
                                                                     object htmlAttributes)
        {
            // Arrange
            var expected = @"<input baz=""BazValue"" id=""Prop1"" name=""Prop1"" type=""password"" " +
                           @"value=""attribute-value"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            helper.ViewData.Model.Prop1 = "test";

            // Act
            var result = helper.PasswordFor(m => m.Prop1, htmlAttributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void PasswordForWithPrefix_GeneratesExpectedValue()
        {
            // Arrange
            var expected = @"<input id=""MyPrefix_Prop1"" name=""MyPrefix.Prop1"" type=""password"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            var result = helper.PasswordFor(m => m.Prop1);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void PasswordForWithViewDataErrors_GeneratesExpectedValue()
        {
            // Arrange
            var expected = @"<input baz=""BazValue"" class=""input-validation-error some-class"" id=""Prop1"" " +
                           @"name=""Prop1"" type=""password"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithErrors());
            var attributes = new Dictionary<string, object>
            {
                { "baz", "BazValue" },
                { "class", "some-class"}
            };

            // Act
            var result = helper.PasswordFor(m => m.Prop1, attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void PasswordFor_GeneratesUnobtrusiveValidationAttributes()
        {
            // Arrange
            var expected = @"<input data-val=""true"" data-val-required=""The Prop2 field is required."" " +
                           @"id=""Prop2"" name=""Prop2"" type=""password"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithErrors());

            // Act
            var result = helper.PasswordFor(m => m.Prop2, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        public static TheoryData PasswordFor_WithComplexExpressionsData
        {
            get
            {
                return new TheoryData<Expression<Func<PasswordModel, string>>, string>
                {
                    {
                        model => model.Prop3["key"],
                        @"<input data-val=""true"" id=""pre_Prop3_key_"" name=""pre.Prop3[key]"" " +
                        @"type=""password"" value=""attr-value"" />"
                    },
                    {
                        model => model.Prop4.Prop5,
                        @"<input data-val=""true"" id=""pre_Prop4_Prop5"" name=""pre.Prop4.Prop5"" " +
                        @"type=""password"" value=""attr-value"" />"
                    },
                    {
                        model => model.Prop4.Prop6[0],
                        @"<input data-val=""true"" id=""pre_Prop4_Prop6_0_"" " +
                        @"name=""pre.Prop4.Prop6[0]"" type=""password"" value=""attr-value"" />"
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(PasswordFor_WithComplexExpressionsData))]
        public void PasswordFor_WithComplexExpressionsAndFieldPrefix_UsesAttributeValueIfSpecified(
            Expression<Func<PasswordModel, string>> expression,
            string expected)
        {
            // Arrange
            var viewData = GetViewDataWithModelStateAndModelAndViewDataValues();
            viewData.ModelState.Add("pre.Prop3[key]", GetModelState("Prop3Val"));
            viewData.ModelState.Add("pre.Prop4.Prop5", GetModelState("Prop5Val"));
            viewData.ModelState.Add("pre.Prop4.Prop6[0]", GetModelState("Prop6Val"));
            viewData["pre.Prop3[key]"] = "vdd-value1";
            viewData["pre.Prop4.Prop5"] = "vdd-value2";
            viewData["pre.Prop4.Prop6[0]"] = "vdd-value3";
            viewData.Model.Prop3["key"] = "prop-value1";
            viewData.Model.Prop4.Prop5 = "prop-value2";
            viewData.Model.Prop4.Prop6.Add("prop-value3");

            var helper = DefaultTemplatesUtilities.GetHtmlHelper(viewData);
            viewData.TemplateInfo.HtmlFieldPrefix = "pre";
            var attributes = new { data_val = "true", value = "attr-value" };

            // Act
            var result = helper.PasswordFor(expression, attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        private static ViewDataDictionary<PasswordModel> GetViewDataWithNullModelAndNonNullViewData()
        {
            return new ViewDataDictionary<PasswordModel>(new EmptyModelMetadataProvider())
            {
                ["Prop1"] = "view-data-val",
            };
        }

        private static ViewDataDictionary<PasswordModel> GetViewDataWithModelStateAndModelAndViewDataValues()
        {
            var viewData = new ViewDataDictionary<PasswordModel>(new EmptyModelMetadataProvider())
            {
                Model = new PasswordModel(),
                ["Prop1"] = "view-data-val",
            };
            viewData.ModelState.Add("Prop1", GetModelState("ModelStateValue"));

            return viewData;
        }

        private static ViewDataDictionary<PasswordModel> GetViewDataWithErrors()
        {
            var viewData = GetViewDataWithModelStateAndModelAndViewDataValues();
            viewData.ModelState.AddModelError("Prop1", "error 1");
            viewData.ModelState.AddModelError("Prop1", "error 2");
            return viewData;
        }

        private static ModelState GetModelState(string value)
        {
            return new ModelState
            {
                Value = new ValueProviderResult(value, value, CultureInfo.InvariantCulture)
            };
        }

        public class PasswordModel
        {
            public string Prop1 { get; set; }

            [Required]
            public string Prop2 { get; set; }

            public Dictionary<string, string> Prop3 { get; } = new Dictionary<string, string>();

            public NestedClass Prop4 { get; } = new NestedClass();
        }

        public class NestedClass
        {
            public string Prop5 { get; set; }

            public List<string> Prop6 { get; } = new List<string>();
        }
    }
}