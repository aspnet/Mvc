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
    public class HtmlHelperHiddenTest
    {

        public static IEnumerable<object[]> HiddenWithAttributesData
        {
            get
            {
                var expected1 = @"<input baz=""BazValue"" id=""Property1"" name=""Property1"" type=""hidden"" " +
                                @"value=""AttemptedValue"" />";
                yield return new object[] { new Dictionary<string, object> { { "baz", "BazValue" } }, expected1 };
                yield return new object[] { new { baz = "BazValue" }, expected1 };

                var expected2 = @"<input foo-baz=""BazValue"" id=""Property1"" name=""Property1"" type=""hidden"" " +
                                @"value=""AttemptedValue"" />";
                yield return new object[] { new Dictionary<string, object> { { "foo-baz", "BazValue" } }, expected2 };
            }
        }

        [Fact]
        public void HiddenWithByteArrayValue_GeneratesBase64EncodedValue()
        {
            // Arrange
            var expected = @"<input id=""ProductName"" name=""ProductName"" type=""hidden"" value=""Fys1"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();

            // Act
            var result = helper.Hidden("ProductName", new byte[] { 23, 43, 53 }, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Theory]
        [MemberData(nameof(HiddenWithAttributesData))]
        public void HiddenWithExplicitValueAndAttributes_GeneratesExpectedValue(object attributes,
                                                                                string expected)
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithModelAndViewDataValues());
            helper.ViewData.Model.Property1 = "should-not-be-used";

            // Act
            var result = helper.Hidden("Property1", "test", attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenOverridesValueFromAttributesWithExplicitValue()
        {
            // Arrange
            var expected = @"<input id=""Property1"" name=""Property1"" type=""hidden"" value=""explicit-value"" />";
            var attributes = new { value = "attribute-value" };
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithNullModel());
            helper.ViewData.Clear();

            // Act
            var result = helper.Hidden("Property1", "explicit-value", attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenWithExplicitValueAndNullModel_GeneratesExpectedValue()
        {
            // Arrange
            var expected = @"<input id=""Property1"" key=""value"" name=""Property1"" type=""hidden"" " +
                           @"value=""test"" />";
            var attributes = new { key = "value" };
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithNullModel());

            // Act
            var result = helper.Hidden("Property1", "test", attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenWithNullValueAndNullModel_GeneratesExpectedValue()
        {
            // Arrange
            var expected = @"<input data-key=""value"" id=""Property1"" name=""Property1"" type=""hidden"" " +
                           @"value=""test"" />";
            var attributes = new Dictionary<string, object> { { "data-key", "value" } };
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithNullModel());

            // Act
            var result = helper.Hidden("Property1", "test", attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenUsesValueFromViewDataDictionary_IfValueParameterIsNull()
        {
            // Arrange
            var expected = @"<input id=""Property1"" name=""Property1"" type=""hidden"" value=""AttemptedValue"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithModelAndViewDataValues());
            helper.ViewData.Model.Property1 = "test-value";

            // Act
            var result = helper.Hidden("Property1", value: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenUsesValueFromModel_IfValueParameterIsNullAndPropertyIsNotFoundInViewData()
        {
            // Arrange
            var expected = @"<input id=""Property1"" name=""Property1"" type=""hidden"" value=""AttemptedValue"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithModelAndViewDataValues());
            helper.ViewData.Clear();
            helper.ViewData.Model.Property1 = "test-value";

            // Act
            var result = helper.Hidden("Property1", value: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenUsesValueFromModelState_IfValueParameterIsNullAndPropertyIsNotSetInModelOrViewData()
        {
            // Arrange
            var expected = @"<input id=""Property1"" name=""Property1"" type=""hidden"" value=""AttemptedValue"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithModelAndViewDataValues());
            helper.ViewData.Clear();
            helper.ViewData.Model.Property1 = null;

            // Act
            var result = helper.Hidden("Property1", value: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenReturnsEmptyValue_IfPropertyIsNotFound()
        {
            // Arrange
            var expected = @"<input baz=""BazValue"" id=""keyNotFound"" name=""keyNotFound"" type=""hidden"" " +
                           @"value="""" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithModelAndViewDataValues());
            var attributes = new Dictionary<string, object> { { "baz", "BazValue" } };

            // Act
            var result = helper.Hidden("keyNotFound", value: null, htmlAttributes: attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenWithPrefix_GeneratesExpectedValue()
        {
            // Arrange
            var expected = @"<input id=""MyPrefix_Property1"" name=""MyPrefix.Property1"" type=""hidden"" " +
                           @"value=""PropValue"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithModelAndViewDataValues());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            var result = helper.Hidden("Property1", "PropValue", htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenWithPrefixAndEmptyName_GeneratesExpectedValue()
        {
            // Arrange
            var expected = @"<input id=""MyPrefix"" name=""MyPrefix"" type=""hidden"" value=""fooValue"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithModelAndViewDataValues());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            var result = helper.Hidden(string.Empty, "fooValue", htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenWithEmptyNameAndPrefixThrows()
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithModelAndViewDataValues());
            var attributes = new Dictionary<string, object>
            {
                { "class", "some-class"}
            };

            // Act and Assert
            ExceptionAssert.ThrowsArgumentNullOrEmpty(() => helper.Hidden(string.Empty, string.Empty, attributes),
                                                      "name");
        }

        [Fact]
        public void HiddenWithViewDataErrors_GeneratesExpectedValue()
        {
            // Arrange
            var expected = @"<input baz=""BazValue"" class=""input-validation-error some-class"" id=""Property1""" +
                           @" name=""Property1"" type=""hidden"" value=""AttemptedValue"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithErrors());
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

        [Fact]
        public void HiddenGeneratesUnobtrusiveValidation()
        {
            // Arrange
            var expected = @"<input data-val=""true"" data-val-required=""The Property2 field is required."" " +
                           @"id=""Property2"" name=""Property2"" type=""hidden"" value="""" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithModelAndViewDataValues());

            // Act
            var result = helper.Hidden("Property2", value: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        public static IEnumerable<object[]> HiddenWithComplexExpressions_UsesValueFromViewDataData
        {
            get
            {
                yield return new object[]
                {
                    "Property3[height]",
                    @"<input data-test=""val"" id=""Property3_height_"" name=""Property3[height]"" type=""hidden"" " +
                    @"value=""Prop3Value"" />",
                };

                yield return new object[]
                {
                    "Property4.Property5",
                    @"<input data-test=""val"" id=""Property4_Property5"" name=""Property4.Property5"" " +
                    @"type=""hidden"" value=""Prop4Value"" />",
                };

                yield return new object[]
               {
                    "Property4.Property6[0]",
                    @"<input data-test=""val"" id=""Property4_Property6_0_"" name=""Property4.Property6[0]"" " +
                    @"type=""hidden"" value=""Prop6Value"" />",
               };
            }
        }

        [Theory]
        [MemberData(nameof(HiddenWithComplexExpressions_UsesValueFromViewDataData))]
        public void HiddenWithComplexExpressions_UsesValueFromViewData(string expression, string expected)
        {
            // Arrange
            var viewData = GetViewDataWithModelAndViewDataValues();
            viewData["Property3[height]"] = "Prop3Value";
            viewData["Property4.Property5"] = "Prop4Value";
            viewData["Property4.Property6[0]"] = "Prop6Value";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(viewData);
            var attributes = new Dictionary<string, object> { { "data-test", "val" } };

            // Act
            var result = helper.Hidden(expression, value: null, htmlAttributes: attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        public static IEnumerable<object[]> HiddenWithComplexExpressions_UsesIdDotSeparatorData
        {
            get
            {
                yield return new object[]
                {
                    "Property4.Property5",
                    @"<input data-test=""val"" id=""Property4$$Property5"" name=""Property4.Property5"" " +
                    @"type=""hidden"" value=""Prop4Value"" />",
                };

                yield return new object[]
               {
                    "Property4.Property6[0]",
                    @"<input data-test=""val"" id=""Property4$$Property6$$0$$"" name=""Property4.Property6[0]"" " +
                    @"type=""hidden"" value=""Prop6Value"" />",
               };
            }
        }

        [Theory]
        [MemberData(nameof(HiddenWithComplexExpressions_UsesIdDotSeparatorData))]
        public void HiddenWithComplexExpressions_UsesIdDotSeparator(string expression, string expected)
        {
            // Arrange
            var viewData = GetViewDataWithModelAndViewDataValues();
            viewData["Property3[height]"] = "Prop3Value";
            viewData["Property4.Property5"] = "Prop4Value";
            viewData["Property4.Property6[0]"] = "Prop6Value";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(viewData);
            helper.IdAttributeDotReplacement = "$$";
            var attributes = new Dictionary<string, object> { { "data-test", "val" } };

            // Act
            var result = helper.Hidden(expression, value: null, htmlAttributes: attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenForWithByteArrayValue_GeneratesBase64EncodedValue()
        {
            // Arrange
            var expected = @"<input id=""Bytes"" name=""Bytes"" type=""hidden"" value=""Fys1"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithModelAndViewDataValues());
            helper.ViewData.Model.Bytes = new byte[] { 23, 43, 53 };

            // Act
            var result = helper.HiddenFor(m => m.Bytes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Theory]
        [MemberData(nameof(HiddenWithAttributesData))]
        public void HiddenForWithAttributes_GeneratesExpectedValue(object htmlAttributes, string expected)
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithModelAndViewDataValues());
            helper.ViewData.Model.Property1 = "test";

            // Act
            var result = helper.HiddenFor(m => m.Property1, htmlAttributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenFor_UsesPropertyValueOverValueInModelState()
        {
            // Arrange
            var expected = @"<input id=""Property1"" name=""Property1"" type=""hidden"" value=""AttemptedValue"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithModelAndViewDataValues());
            helper.ViewData.Model.Property1 = "DefaultValue";

            // Act
            var result = helper.HiddenFor(m => m.Property1, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenFor_UsesValuesFromModelStateIfModelIsNull()
        {
            // Arrange
            var expected = @"<input id=""Property1"" name=""Property1"" type=""hidden"" value=""AttemptedValue"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithModelAndViewDataValues());
            helper.ViewData.Model = null;

            // Act
            var result = helper.HiddenFor(m => m.Property1, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenFor_UsesValuesFromModelStateIfPropertyIsNull()
        {
            // Arrange
            var expected = @"<input id=""Property1"" name=""Property1"" type=""hidden"" value=""AttemptedValue"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithModelAndViewDataValues());
            helper.ViewData.Model.Property1 = null;

            // Act
            var result = helper.HiddenFor(m => m.Property1, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenForDoesNotUseValueFromViewDataDictionary_IfModelStateAndPropertyValueIsNull()
        {
            // Arrange
            var expected = @"<input id=""Property1"" name=""Property1"" type=""hidden"" value="""" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithModelAndViewDataValues());
            helper.ViewData.Model.Property1 = null;
            helper.ViewData.ModelState.Clear();

            // Act
            var result = helper.HiddenFor(m => m.Property1, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenForWithAttributesDictionaryAndNullModel_GeneratesExpectedValue()
        {
            // Arrange
            var expected = @"<input id=""Property1"" key=""value"" name=""Property1"" type=""hidden"" value="""" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithNullModel());
            var attributes = new Dictionary<string, object> { { "key", "value" } };

            // Act
            var result = helper.HiddenFor(m => m.Property1, attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenForWithPrefix_GeneratesExpectedValue()
        {
            // Arrange
            var expected = @"<input id=""MyPrefix_Property1"" name=""MyPrefix.Property1"" type=""hidden"" " +
                           @"value=""propValue"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithModelAndViewDataValues());
            helper.ViewData.Model.Property1 = "propValue";
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            var result = helper.HiddenFor(m => m.Property1);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenForWithViewDataErrors_GeneratesExpectedValue()
        {
            // Arrange
            var expected = @"<input baz=""BazValue"" class=""input-validation-error some-class"" id=""Property1"" " +
                           @"name=""Property1"" type=""hidden"" value=""AttemptedValue"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithErrors());
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

        [Fact]
        public void HiddenFor_GeneratesUnobtrusiveValidationAttributes()
        {
            // Arrange
            var expected = @"<input data-val=""true"" data-val-required=""The Property2 field is required."" " +
                           @"id=""Property2"" name=""Property2"" type=""hidden"" value="""" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithErrors());

            // Act
            var result = helper.HiddenFor(m => m.Property2, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        public static TheoryData HiddenFor_GeneratesExpectedValuesForComplexExpressionsData
        {
            get
            {
                return new TheoryData<Expression<Func<HiddenModel, string>>, string>
                {
                    {
                        model => model.Property3["key"],
                        @"<input data-val=""true"" id=""Property3_key_"" name=""Property3[key]"" " +
                        @"type=""hidden"" value=""ModelProp3Val"" />"
                    },
                    {
                        model => model.Property4.Property5,
                        @"<input data-val=""true"" id=""Property4_Property5"" name=""Property4.Property5"" " +
                        @"type=""hidden"" value=""ModelProp5Val"" />"
                    },
                    {
                        model => model.Property4.Property6[0],
                        @"<input data-val=""true"" id=""Property4_Property6_0_"" name=""Property4.Property6[0]"" " +
                        @"type=""hidden"" value=""ModelProp6Val"" />"
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(HiddenFor_GeneratesExpectedValuesForComplexExpressionsData))]
        public void HiddenFor_UsesModelValueForCompelxExpressions(
            Expression<Func<HiddenModel, string>> expression,
            string expected)
        {
            // Arrange
            var viewData = GetViewDataWithModelAndViewDataValues();
            viewData["Property3[key]"] = "Prop3Val";
            viewData["Property4.Property5"] = "Prop4Val";
            viewData["Property4.Property6[0]"] = "Prop6Val";
            viewData.Model.Property3["key"] = "ModelProp3Val";
            viewData.Model.Property4.Property5 = "ModelProp5Val";
            viewData.Model.Property4.Property6.Add("ModelProp6Val");

            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(viewData);
            var attributes = new Dictionary<string, object>
            {
                { "data-val", "true" },
                { "value", "attr-val" }
            };

            // Act
            var result = helper.HiddenFor(expression, attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        public static TheoryData HiddenFor_UsesViewDataValueForComplexExpressionsIfModelValueIsNullData
        {
            get
            {
                return new TheoryData<Expression<Func<HiddenModel, string>>, string>
                {
                    {
                        model => model.Property3["key"],
                        @"<input data-val=""true"" id=""pre_Property3_key_"" name=""pre.Property3[key]"" " +
                        @"type=""hidden"" value=""Prop3Val"" />"
                    },
                    {
                        model => model.Property4.Property5,
                        @"<input data-val=""true"" id=""pre_Property4_Property5"" name=""pre.Property4.Property5"" " +
                        @"type=""hidden"" value=""Prop5Val"" />"
                    },
                    {
                        model => model.Property4.Property6[0],
                        @"<input data-val=""true"" id=""pre_Property4_Property6_0_"" " +
                        @"name=""pre.Property4.Property6[0]"" type=""hidden"" value=""Prop6Val"" />"
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(HiddenFor_UsesViewDataValueForComplexExpressionsIfModelValueIsNullData))]
        public void HiddenFor_UsesViewDataValueForComplexExpressionsIfModelValueIsNull(
            Expression<Func<HiddenModel, string>> expression,
            string expected)
        {
            // Arrange
            var viewData = GetViewDataWithNullModel();
            viewData.ModelState.Add("pre.Property3[key]", GetModelState("Prop3Val"));
            viewData.ModelState.Add("pre.Property4.Property5", GetModelState("Prop5Val"));
            viewData.ModelState.Add("pre.Property4.Property6[0]", GetModelState("Prop6Val"));

            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(viewData);
            viewData.TemplateInfo.HtmlFieldPrefix = "pre";
            var attributes = new { data_val = "true", value = "attr-val" };

            // Act
            var result = helper.HiddenFor(expression, attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenFor_UsesAttributeValueIfValueIsNotSpecifiedByModelAndModelState()
        {
            // Arrange
            // var expected = @"<input id=""Property1"" name=""Property1"" type=""hidden"" value=""AttrValue"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelperForViewData(GetViewDataWithNullModel());
            var attributes = new Dictionary<string, object>
            {
                { "value", "AttrValue" }
            };

            // Act
            var result = helper.HiddenFor(m => m.Property1, attributes);

            // Assert
            // Requires resolution for https://github.com/aspnet/Mvc/issues/1462
            // Assert.Equal(expected, result.ToString());
        }

        private static ViewDataDictionary<HiddenModel> GetViewDataWithNullModel()
        {
            return new ViewDataDictionary<HiddenModel>(new EmptyModelMetadataProvider())
            {
                ["Property1"] = "VDDValue",
            };
        }

        private static ViewDataDictionary<HiddenModel> GetViewDataWithModelAndViewDataValues()
        {
            var viewData = new ViewDataDictionary<HiddenModel>(new EmptyModelMetadataProvider())
            {
                Model = new HiddenModel(),
                ["Property1"] = "VDDValue",
            };

            var modelState = new ModelState();
            modelState.Value = new ValueProviderResult("AttemptedValue",
                                                       "AttemptedValue",
                                                       CultureInfo.InvariantCulture);
            viewData.ModelState.Add("Property1", modelState);

            return viewData;
        }

        private static ViewDataDictionary<HiddenModel> GetViewDataWithErrors()
        {
            var viewData = GetViewDataWithModelAndViewDataValues();
            viewData.ModelState.AddModelError("Property1", "error 1");
            viewData.ModelState.AddModelError("Property1", "error 2");
            return viewData;
        }

        private static ModelState GetModelState(string value)
        {
            return new ModelState
            {
                Value = new ValueProviderResult(value, value, CultureInfo.InvariantCulture)
            };
        }

        public class HiddenModel
        {
            public string Property1 { get; set; }

            public byte[] Bytes { get; set; }

            [Required]
            public string Property2 { get; set; }

            public Dictionary<string, string> Property3 { get; } = new Dictionary<string, string>();

            public NestedClass Property4 { get; } = new NestedClass();
        }

        public class NestedClass
        {
            public string Property5 { get; set; }

            public List<string> Property6 { get; } = new List<string>();
        }
    }
}