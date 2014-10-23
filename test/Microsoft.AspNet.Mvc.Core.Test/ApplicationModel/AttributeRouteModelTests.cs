﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Routing;
using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.AspNet.Mvc.ApplicationModels
{
    public class AttributeRouteModelTests
    {
        [Fact]
        public void CopyConstructor_CopiesAllProperties()
        {
            // Arrange
            var route = new AttributeRouteModel(new HttpGetAttribute("/api/Products"));

            route.Name = "products";
            route.Order = 5;

            // Act
            var route2 = new AttributeRouteModel(route);

            // Assert
            foreach (var property in typeof(AttributeRouteModel).GetProperties())
            {
                var value1 = property.GetValue(route);
                var value2 = property.GetValue(route2);

                if (typeof(IEnumerable<object>).IsAssignableFrom(property.PropertyType))
                {
                    Assert.Equal<object>((IEnumerable<object>)value1, (IEnumerable<object>)value2);

                    // Ensure non-default value
                    Assert.NotEmpty((IEnumerable<object>)value1);
                }
                else if (property.PropertyType.IsValueType ||
                    Nullable.GetUnderlyingType(property.PropertyType) != null)
                {
                    Assert.Equal(value1, value2);

                    // Ensure non-default value
                    Assert.NotEqual(value1, Activator.CreateInstance(property.PropertyType));
                }
                else
                {
                    Assert.Same(value1, value2);

                    // Ensure non-default value
                    Assert.NotNull(value1);
                }
            }
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("", null, "")]
        [InlineData(null, "", "")]
        [InlineData("/", null, "")]
        [InlineData(null, "/", "")]
        [InlineData("/", "", "")]
        [InlineData("", "/", "")]
        [InlineData("/", "/", "")]
        [InlineData("/", "/", "")]
        [InlineData("~/", null, "")]
        [InlineData("~/", "", "")]
        [InlineData("~/", "/", "")]
        [InlineData("~/", "~/", "")]
        [InlineData(null, "~/", "")]
        [InlineData("", "~/", "")]
        [InlineData("/", "~/", "")]
        public void Combine_EmptyTemplates(string left, string right, string expected)
        {
            // Arrange & Act
            var combined = AttributeRouteModel.CombineTemplates(left, right);

            // Assert
            Assert.Equal(expected, combined);
        }

        [Theory]
        [InlineData("home", null, "home")]
        [InlineData("home", "", "home")]
        [InlineData("/home/", "/", "")]
        [InlineData("/home/", "~/", "")]
        [InlineData(null, "GetEmployees", "GetEmployees")]
        [InlineData("/", "GetEmployees", "GetEmployees")]
        [InlineData("~/", "Blog/Index/", "Blog/Index")]
        [InlineData("", "/GetEmployees/{id}/", "GetEmployees/{id}")]
        [InlineData("~/home", null, "home")]
        [InlineData("~/home", "", "home")]
        [InlineData("~/home", "/", "")]
        [InlineData(null, "~/home", "home")]
        [InlineData("", "~/home", "home")]
        [InlineData("", "~/home/", "home")]
        [InlineData("/", "~/home", "home")]
        public void Combine_OneTemplateHasValue(string left, string right, string expected)
        {
            // Arrange & Act
            var combined = AttributeRouteModel.CombineTemplates(left, right);

            // Assert
            Assert.Equal(expected, combined);
        }

        [Theory]
        [InlineData("home", "About", "home/About")]
        [InlineData("home/", "/About", "About")]
        [InlineData("home/", "/About/", "About")]
        [InlineData("/home/{action}", "{id}", "home/{action}/{id}")]
        [InlineData("home", "~/index", "index")]
        [InlineData("home", "~/index/", "index")]
        public void Combine_BothTemplatesHasValue(string left, string right, string expected)
        {
            // Arrange & Act
            var combined = AttributeRouteModel.CombineTemplates(left, right);

            // Assert
            Assert.Equal(expected, combined);
        }

        [Theory]
        [InlineData("~~/", null, "~~")]
        [InlineData("~~/", "", "~~")]
        [InlineData("~~/", "//", "//")]
        [InlineData("~~/", "~~/", "~~/~~")]
        [InlineData("~~/", "home", "~~/home")]
        [InlineData("~~/", "home/", "~~/home")]
        [InlineData("//", null, "//")]
        [InlineData("//", "", "//")]
        [InlineData("//", "//", "//")]
        [InlineData("//", "~~/", "/~~")]
        [InlineData("//", "home", "/home")]
        [InlineData("//", "home/", "/home")]
        [InlineData("////", null, "//")]
        [InlineData("~~//", null, "~~/")]
        public void Combine_InvalidTemplates(string left, string right, string expected)
        {
            // Arrange & Act
            var combined = AttributeRouteModel.CombineTemplates(left, right);

            // Assert
            Assert.Equal(expected, combined);
        }

        [Theory]
        [MemberData(nameof(ReplaceTokens_ValueValuesData))]
        public void ReplaceTokens_ValidValues(string template, object values, string expected)
        {
            // Arrange
            var valuesDictionary = values as IDictionary<string, object>;
            if (valuesDictionary == null)
            {
                valuesDictionary = new RouteValueDictionary(values);
            }

            // Act
            var result = AttributeRouteModel.ReplaceTokens(template, valuesDictionary);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(ReplaceTokens_InvalidFormatValuesData))]
        public void ReplaceTokens_InvalidFormat(string template, object values, string reason)
        {
            // Arrange
            var valuesDictionary = values as IDictionary<string, object>;
            if (valuesDictionary == null)
            {
                valuesDictionary = new RouteValueDictionary(values);
            }

            var expected = string.Format(
                "The route template '{0}' has invalid syntax. {1}",
                template,
                reason);

            // Act
            var ex = Assert.Throws<InvalidOperationException>(
                () => { AttributeRouteModel.ReplaceTokens(template, valuesDictionary); });

            // Assert
            Assert.Equal(expected, ex.Message);
        }

        [Fact]
        public void ReplaceTokens_UnknownValue()
        {
            // Arrange
            var template = "[area]/[controller]/[action2]";
            var values = new RouteValueDictionary()
            {
                { "area", "Help" },
                { "controller", "Admin" },
                { "action", "SeeUsers" },
            };

            var expected =
                "While processing template '[area]/[controller]/[action2]', " +
                "a replacement value for the token 'action2' could not be found. " +
                "Available tokens: 'area, controller, action'.";

            // Act
            var ex = Assert.Throws<InvalidOperationException>(
                () => { AttributeRouteModel.ReplaceTokens(template, values); });

            // Assert
            Assert.Equal(expected, ex.Message);
        }

        [Theory]
        [MemberData(nameof(CombineOrdersTestData))]
        public void Combine_Orders(
            AttributeRouteModel left,
            AttributeRouteModel right,
            int? expected)
        {
            // Arrange & Act
            var combined = AttributeRouteModel.CombineAttributeRouteModel(left, right);

            // Assert
            Assert.NotNull(combined);
            Assert.Equal(expected, combined.Order);
        }

        [Theory]
        [MemberData(nameof(ValidReflectedAttributeRouteModelsTestData))]
        public void Combine_ValidReflectedAttributeRouteModels(
            AttributeRouteModel left,
            AttributeRouteModel right,
            AttributeRouteModel expectedResult)
        {
            // Arrange & Act
            var combined = AttributeRouteModel.CombineAttributeRouteModel(left, right);

            // Assert
            Assert.NotNull(combined);
            Assert.Equal(expectedResult.Template, combined.Template);
        }

        [Theory]
        [MemberData(nameof(NullOrNullTemplateReflectedAttributeRouteModelTestData))]
        public void Combine_NullOrNullTemplateReflectedAttributeRouteModels(
            AttributeRouteModel left,
            AttributeRouteModel right)
        {
            // Arrange & Act
            var combined = AttributeRouteModel.CombineAttributeRouteModel(left, right);

            // Assert
            Assert.Null(combined);
        }

        [Theory]
        [MemberData(nameof(RightOverridesReflectedAttributeRouteModelTestData))]
        public void Combine_RightOverridesReflectedAttributeRouteModel(
            AttributeRouteModel left,
            AttributeRouteModel right)
        {
            // Arrange
            var expectedTemplate = AttributeRouteModel.CombineTemplates(null, right.Template);

            // Act
            var combined = AttributeRouteModel.CombineAttributeRouteModel(left, right);

            // Assert
            Assert.NotNull(combined);
            Assert.Equal(expectedTemplate, combined.Template);
            Assert.Equal(combined.Order, right.Order);
        }

        [Theory]
        [MemberData(nameof(CombineNamesTestData))]
        public void Combine_Names(
            AttributeRouteModel left,
            AttributeRouteModel right,
            string expectedName)
        {
            // Arrange & Act
            var combined = AttributeRouteModel.CombineAttributeRouteModel(left, right);

            // Assert
            Assert.NotNull(combined);
            Assert.Equal(expectedName, combined.Name);
        }

        public static IEnumerable<object[]> CombineNamesTestData
        {
            get
            {
                // AttributeRoute on the controller, attribute route on the action, expected name.
                var data = new TheoryData<AttributeRouteModel, AttributeRouteModel, string>();

                // Combined name is null if no name is provided.
                data.Add(Create(template: "/", order: null, name: null), null, null);
                data.Add(Create(template: "~/", order: null, name: null), null, null);
                data.Add(Create(template: "", order: null, name: null), null, null);
                data.Add(Create(template: "home", order: null, name: null), null, null);
                data.Add(Create(template: "/", order: 1, name: null), null, null);
                data.Add(Create(template: "~/", order: 1, name: null), null, null);
                data.Add(Create(template: "", order: 1, name: null), null, null);
                data.Add(Create(template: "home", order: 1, name: null), null, null);

                // Combined name is inherited if no right name is provided and the template is empty.
                data.Add(Create(template: "/", order: null, name: "Named"), null, "Named");
                data.Add(Create(template: "~/", order: null, name: "Named"), null, "Named");
                data.Add(Create(template: "", order: null, name: "Named"), null, "Named");
                data.Add(Create(template: "home", order: null, name: "Named"), null, "Named");
                data.Add(Create(template: "home", order: null, name: "Named"), Create(null, null, null), "Named");
                data.Add(Create(template: "", order: null, name: "Named"), Create("", null, null), "Named");

                // Order doesn't matter for combining the name.
                data.Add(Create(template: "", order: null, name: "Named"), Create("", 1, null), "Named");
                data.Add(Create(template: "", order: 1, name: "Named"), Create("", 1, null), "Named");
                data.Add(Create(template: "", order: 2, name: "Named"), Create("", 1, null), "Named");
                data.Add(Create(template: "", order: null, name: "Named"), Create("index", 1, null), null);
                data.Add(Create(template: "", order: 1, name: "Named"), Create("index", 1, null), null);
                data.Add(Create(template: "", order: 2, name: "Named"), Create("index", 1, null), null);
                data.Add(Create(template: "", order: null, name: "Named"), Create("", 1, "right"), "right");
                data.Add(Create(template: "", order: 1, name: "Named"), Create("", 1, "right"), "right");
                data.Add(Create(template: "", order: 2, name: "Named"), Create("", 1, "right"), "right");

                // Combined name is not inherited if right name is provided or the template is not empty.
                data.Add(Create(template: "/", order: null, name: "Named"), Create(null, null, "right"), "right");
                data.Add(Create(template: "~/", order: null, name: "Named"), Create(null, null, "right"), "right");
                data.Add(Create(template: "", order: null, name: "Named"), Create(null, null, "right"), "right");
                data.Add(Create(template: "home", order: null, name: "Named"), Create(null, null, "right"), "right");
                data.Add(Create(template: "home", order: null, name: "Named"), Create("index", null, null), null);
                data.Add(Create(template: "home", order: null, name: "Named"), Create("/", null, null), null);
                data.Add(Create(template: "home", order: null, name: "Named"), Create("~/", null, null), null);
                data.Add(Create(template: "home", order: null, name: "Named"), Create("index", null, "right"), "right");
                data.Add(Create(template: "home", order: null, name: "Named"), Create("/", null, "right"), "right");
                data.Add(Create(template: "home", order: null, name: "Named"), Create("~/", null, "right"), "right");
                data.Add(Create(template: "home", order: null, name: "Named"), Create("index", null, ""), "");

                return data;
            }
        }

        public static IEnumerable<object[]> CombineOrdersTestData
        {
            get
            {
                // AttributeRoute on the controller, attribute route on the action, expected order.
                var data = new TheoryData<AttributeRouteModel, AttributeRouteModel, int?>();

                data.Add(Create("", order: 1), Create("", order: 2), 2);
                data.Add(Create("", order: 1), Create("", order: null), 1);
                data.Add(Create("", order: 1), null, 1);
                data.Add(Create("", order: 1), Create("/", order: 2), 2);
                data.Add(Create("", order: 1), Create("/", order: null), null);
                data.Add(Create("", order: null), Create("", order: 2), 2);
                data.Add(Create("", order: null), Create("", order: null), null);
                data.Add(Create("", order: null), null, null);
                data.Add(Create("", order: null), Create("/", order: 2), 2);
                data.Add(Create("", order: null), Create("/", order: null), null);
                data.Add(null, Create("", order: 2), 2);
                data.Add(null, Create("", order: null), null);
                data.Add(null, Create("/", order: 2), 2);
                data.Add(null, Create("/", order: null), null);

                // We don't a test case for (left = null, right = null) as it is already tested in another test 
                // and will produce a null ReflectedAttributeRouteModel, which complicates the test case.

                return data;
            }
        }

        public static IEnumerable<object[]> RightOverridesReflectedAttributeRouteModelTestData
        {
            get
            {
                // AttributeRoute on the controller, attribute route on the action.
                var data = new TheoryData<AttributeRouteModel, AttributeRouteModel>();
                var leftModel = Create("Home", order: 3);

                data.Add(leftModel, Create("/"));
                data.Add(leftModel, Create("~/"));
                data.Add(null, Create("/"));
                data.Add(null, Create("~/"));
                data.Add(Create(null), Create("/"));
                data.Add(Create(null), Create("~/"));

                return data;
            }
        }

        public static IEnumerable<object[]> NullOrNullTemplateReflectedAttributeRouteModelTestData
        {
            get
            {
                // AttributeRoute on the controller, attribute route on the action.
                var data = new TheoryData<AttributeRouteModel, AttributeRouteModel>();

                data.Add(null, null);
                data.Add(null, Create(null));
                data.Add(Create(null), null);
                data.Add(Create(null), Create(null));

                return data;
            }
        }

        public static IEnumerable<object[]> ValidReflectedAttributeRouteModelsTestData
        {
            get
            {
                // AttributeRoute on the controller, attribute route on the action, expected combined attribute route.
                var data = new TheoryData<AttributeRouteModel, AttributeRouteModel, AttributeRouteModel>();
                data.Add(null, Create("Index"), Create("Index"));
                data.Add(Create(null), Create("Index"), Create("Index"));
                data.Add(Create("Home"), null, Create("Home"));
                data.Add(Create("Home"), Create(null), Create("Home"));
                data.Add(Create("Home"), Create("Index"), Create("Home/Index"));
                data.Add(Create("Blog"), Create("/Index"), Create("Index"));

                return data;
            }
        }

        public static IEnumerable<object[]> ReplaceTokens_ValueValuesData
        {
            get
            {
                yield return new object[]
                {
                    "[controller]/[action]",
                    new { controller = "Home", action = "Index" },
                    "Home/Index"
                };

                yield return new object[]
                {
                    "[controller]",
                    new { controller = "Home", action = "Index" },
                    "Home"
                };

                yield return new object[]
                {
                    "[controller][[",
                    new { controller = "Home", action = "Index" },
                    "Home["
                };

                yield return new object[]
                {
                    "[coNTroller]",
                    new { contrOLler = "Home", action = "Index" },
                    "Home"
                };

                yield return new object[]
                {
                    "thisisSomeText[action]",
                    new { controller = "Home", action = "Index" },
                    "thisisSomeTextIndex"
                };

                yield return new object[]
                {
                    "[[-]][[/[[controller]]",
                    new { controller = "Home", action = "Index" },
                    "[-][/[controller]"
                };

                yield return new object[]
                {
                    "[contr[[oller]/[act]]ion]",
                    new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "contr[oller", "Home" },
                        { "act]ion", "Index" }
                    },
                    "Home/Index"
                };

                yield return new object[]
                {
                    "[controller][action]",
                    new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "controller", "Home" },
                        { "action", "Index" }
                    },
                    "HomeIndex"
                };

                yield return new object[]
                {
                    "[contr}oller]/[act{ion]/{id}",
                    new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "contr}oller", "Home" },
                        { "act{ion", "Index" }
                    },
                    "Home/Index/{id}"
                };
            }
        }

        public static IEnumerable<object[]> ReplaceTokens_InvalidFormatValuesData
        {
            get
            {
                yield return new object[]
                {
                    "[",
                    new { },
                    "A replacement token is not closed."
                };

                yield return new object[]
                {
                    "text]",
                    new { },
                    "Token delimiters ('[', ']') are imbalanced.",
                };

                yield return new object[]
                {
                    "text]morecooltext",
                    new { },
                    "Token delimiters ('[', ']') are imbalanced.",
                };

                yield return new object[]
                {
                    "[action",
                    new { },
                    "A replacement token is not closed.",
                };

                yield return new object[]
                {
                    "[action]]][",
                    new RouteValueDictionary()
                    {
                        { "action]", "Index" }
                    },
                    "A replacement token is not closed.",
                };

                yield return new object[]
                {
                    "[action]]",
                    new { },
                    "A replacement token is not closed."
                };

                yield return new object[]
                {
                    "[ac[tion]",
                    new { },
                    "An unescaped '[' token is not allowed inside of a replacement token. Use '[[' to escape."
                };

                yield return new object[]
                {
                    "[]",
                    new { },
                    "An empty replacement token ('[]') is not allowed.",
                };
            }
        }

        private static AttributeRouteModel Create(string template, int? order = null, string name = null)
        {
            return new AttributeRouteModel
            {
                Template = template,
                Order = order,
                Name = name
            };
        }
    }
}