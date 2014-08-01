﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Routing;
using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.AspNet.Mvc.ReflectedModelBuilder
{
    public class ReflectedAttributeRouteModelTests
    {
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
            var combined = ReflectedAttributeRouteModel.CombineTemplates(left, right);

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
            var combined = ReflectedAttributeRouteModel.CombineTemplates(left, right);

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
            var combined = ReflectedAttributeRouteModel.CombineTemplates(left, right);

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
            var combined = ReflectedAttributeRouteModel.CombineTemplates(left, right);

            // Assert
            Assert.Equal(expected, combined);
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

        [Theory]
        [MemberData("ReplaceTokens_ValueValuesData")]
        public void ReplaceTokens_ValidValues(string template, object values, string expected)
        {
            // Arrange
            var valuesDictionary = values as IDictionary<string, object>;
            if (valuesDictionary == null)
            {
                valuesDictionary = new RouteValueDictionary(values);
            }

            // Act
            var result = ReflectedAttributeRouteModel.ReplaceTokens(template, valuesDictionary);

            // Assert
            Assert.Equal(expected, result);
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

        [Theory]
        [MemberData("ReplaceTokens_InvalidFormatValuesData")]
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
                () => { ReflectedAttributeRouteModel.ReplaceTokens(template, valuesDictionary); });

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
                () => { ReflectedAttributeRouteModel.ReplaceTokens(template, values); });

            // Assert
            Assert.Equal(expected, ex.Message);
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData(null, 1, 1)]
        [InlineData(2, 1, 1)]
        [InlineData(1, 2, 2)]
        [InlineData(1, null, 1)]
        public void Combine_Orders(int? left, int? right, int? expected)
        {
            // Arrange & Act
            var combined = ReflectedAttributeRouteModel.CombineOrders(left, right);

            // Assert
            Assert.Equal(expected, combined);
        }

        [Theory]
        [MemberData("ValidReflectedAttributeRouteModelsTestData")]
        public void Combine_ValidReflectedAttributeRouteModels(
            ReflectedAttributeRouteModel left,
            ReflectedAttributeRouteModel right,
            ReflectedAttributeRouteModel expectedResult)
        {
            // Arrange & Act
            var combined = ReflectedAttributeRouteModel.CombineReflectedAttributeRouteModel(left, right);

            // Assert
            Assert.NotNull(combined);
            Assert.Equal(expectedResult.Template, combined.Template);
        }

        [Theory]
        [MemberData("NullOrNullTemplateReflectedAttributeRouteModelTestData")]
        public void Combine_NullOrNullTemplateReflectedAttributeRouteModels(
            ReflectedAttributeRouteModel left,
            ReflectedAttributeRouteModel right)
        {
            // Arrange & Act
            var combined = ReflectedAttributeRouteModel.CombineReflectedAttributeRouteModel(left, right);

            // Assert
            Assert.Null(combined);
        }

        [Theory]
        [MemberData("RightOverridesReflectedAttributeRouteModel")]
        public void Combine_RightOverridesReflectedAttributeRouteModel(
            ReflectedAttributeRouteModel left,
            ReflectedAttributeRouteModel right)
        {
            // Arrange
            var expectedTemplate = ReflectedAttributeRouteModel.CombineTemplates(null, right.Template);

            // Act
            var combined = ReflectedAttributeRouteModel.CombineReflectedAttributeRouteModel(left, right);

            // Assert
            Assert.NotNull(combined);
            Assert.Equal(expectedTemplate, combined.Template);
            Assert.Equal(combined.Order, right.Order);
        }

        public static IEnumerable<object[]> RightOverridesReflectedAttributeRouteModel
        {
            get
            {
                var data = new TheoryData<ReflectedAttributeRouteModel, ReflectedAttributeRouteModel>();
                var leftModel = Create("Home");
                leftModel.Order = 3;

                data.Add(leftModel, Create("/"));
                data.Add(leftModel, Create("~/"));

                return data;
            }
        }

        public static IEnumerable<object[]> NullOrNullTemplateReflectedAttributeRouteModelTestData
        {
            get
            {
                var data = new TheoryData<ReflectedAttributeRouteModel, ReflectedAttributeRouteModel>();
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
                var data = new TheoryData<ReflectedAttributeRouteModel, ReflectedAttributeRouteModel, ReflectedAttributeRouteModel>();
                data.Add(null, Create("Index"), Create("Index"));
                data.Add(Create("Home"), null, Create("Home"));
                data.Add(Create("Home"), Create("Index"), Create("Home/Index"));
                data.Add(Create("Blog"), Create("/Index"), Create("Index"));

                return data;
            }
        }

        private static ReflectedAttributeRouteModel Create(string template)
        {
            return new ReflectedAttributeRouteModel
            {
                Template = template
            };
        }
    }
}