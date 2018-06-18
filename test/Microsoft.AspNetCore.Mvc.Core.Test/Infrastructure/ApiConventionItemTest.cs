// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Infrastructure
{
    public class ApiConventionItemTest
    {
        [Fact]
        public void GetApiConventionItem_ReturnsNull_IfNoConventionMatches()
        {
            // Arrange
            var method = typeof(GetApiConventionItem_ReturnsNull_IfNoConventionMatchesController).GetMethod(nameof(GetApiConventionItem_ReturnsNull_IfNoConventionMatchesController.NoMatch));
            var convention = new ApiConventionAttribute(typeof(DefaultApiConventions));

            // Act
            var result = ApiConventionItem.GetApiConventionItem(method, new[] { convention });

            // Assert
            Assert.Null(result);
        }

        public class GetApiConventionItem_ReturnsNull_IfNoConventionMatchesController
        {
            public IActionResult NoMatch(int id) => null;
        }

        [Fact]
        public void GetApiConventionItem_ReturnsResultFromConvention()
        {
            // Arrange
            var method = typeof(GetApiConventionItem_ReturnsResultFromConventionController)
                .GetMethod(nameof(GetApiConventionItem_ReturnsResultFromConventionController.Match));
            var convention = new ApiConventionAttribute(typeof(GetApiConventionItem_ReturnsResultFromConventionType));

            // Act
            var result = ApiConventionItem.GetApiConventionItem(method, new[] { convention });

            // Assert
            Assert.NotNull(result);
            Assert.Collection(
                result.ResponseMetadataProviders.OrderBy(o => o.StatusCode),
                r => Assert.Equal(201, r.StatusCode),
                r => Assert.Equal(403, r.StatusCode));
        }

        public class GetApiConventionItem_ReturnsResultFromConventionController
        {
            public IActionResult Match(int id) => null;
        }

        public static class GetApiConventionItem_ReturnsResultFromConventionType
        {
            [ProducesResponseType(200)]
            [ProducesResponseType(202)]
            [ProducesResponseType(404)]
            public static void Get(int id) { }

            [ProducesResponseType(201)]
            [ProducesResponseType(403)]
            public static void Match(int id) { }
        }

        [Fact]
        public void GetApiConventionItem_ReturnsResultFromFirstMatchingConvention()
        {
            // Arrange
            var method = typeof(GetApiConventionItem_ReturnsResultFromFirstMatchingConventionController)
                .GetMethod(nameof(GetApiConventionItem_ReturnsResultFromFirstMatchingConventionController.Get));
            var conventions = new[]
            {
                new ApiConventionAttribute(typeof(GetApiConventionItem_ReturnsResultFromConventionType)),
                new ApiConventionAttribute(typeof(DefaultApiConventions)),
            };

            // Act
            var result = ApiConventionItem.GetApiConventionItem(method, conventions);

            // Assert
            Assert.NotNull(result);
            Assert.Collection(
                result.ResponseMetadataProviders.OrderBy(o => o.StatusCode),
                r => Assert.Equal(200, r.StatusCode),
                r => Assert.Equal(202, r.StatusCode),
                r => Assert.Equal(404, r.StatusCode));
        }

        public class GetApiConventionItem_ReturnsResultFromFirstMatchingConventionController
        {
            public IActionResult Get(int id) => null;
        }

        [Fact]
        public void GetApiConventionItem_GetAction_MatchesDefaultConvention()
        {
            // Arrange
            var method = typeof(DefaultConventionController)
                .GetMethod(nameof(DefaultConventionController.GetUser));
            var conventions = new[]
            {
                new ApiConventionAttribute(typeof(DefaultApiConventions)),
            };

            // Act
            var result = ApiConventionItem.GetApiConventionItem(method, conventions);

            // Assert
            Assert.NotNull(result);
            Assert.Collection(
                result.ResponseMetadataProviders.OrderBy(o => o.StatusCode),
                r => Assert.Equal(200, r.StatusCode),
                r => Assert.Equal(404, r.StatusCode));
        }

        [Fact]
        public void GetApiConventionItem_PostAction_MatchesDefaultConvention()
        {
            // Arrange
            var method = typeof(DefaultConventionController)
                .GetMethod(nameof(DefaultConventionController.PostUser));
            var conventions = new[]
            {
                new ApiConventionAttribute(typeof(DefaultApiConventions)),
            };

            // Act
            var result = ApiConventionItem.GetApiConventionItem(method, conventions);

            // Assert
            Assert.NotNull(result);
            Assert.Collection(
                result.ResponseMetadataProviders.OrderBy(o => o.StatusCode),
                r => Assert.Equal(201, r.StatusCode),
                r => Assert.Equal(400, r.StatusCode));
        }

        [Fact]
        public void GetApiConventionItem_PutAction_MatchesDefaultConvention()
        {
            // Arrange
            var method = typeof(DefaultConventionController)
                .GetMethod(nameof(DefaultConventionController.PutUser));
            var conventions = new[]
            {
                new ApiConventionAttribute(typeof(DefaultApiConventions)),
            };

            // Act
            var result = ApiConventionItem.GetApiConventionItem(method, conventions);

            // Assert
            Assert.NotNull(result);
            Assert.Collection(
                result.ResponseMetadataProviders.OrderBy(o => o.StatusCode),
                r => Assert.Equal(204, r.StatusCode),
                r => Assert.Equal(400, r.StatusCode),
                r => Assert.Equal(404, r.StatusCode));
        }

        [Fact]
        public void GetApiConventionItem_DeleteAction_MatchesDefaultConvention()
        {
            // Arrange
            var method = typeof(DefaultConventionController)
                .GetMethod(nameof(DefaultConventionController.Delete));
            var conventions = new[]
            {
                new ApiConventionAttribute(typeof(DefaultApiConventions)),
            };

            // Act
            var result = ApiConventionItem.GetApiConventionItem(method, conventions);

            // Assert
            Assert.NotNull(result);
            Assert.Collection(
                result.ResponseMetadataProviders.OrderBy(o => o.StatusCode),
                r => Assert.Equal(200, r.StatusCode),
                r => Assert.Equal(400, r.StatusCode),
                r => Assert.Equal(404, r.StatusCode));
        }

        public class DefaultConventionController
        {
            public IActionResult GetUser(Guid id) => null;

            public IActionResult PostUser(User user) => null;

            public IActionResult PutUser(Guid userId, User user) => null;

            public IActionResult Delete(Guid userId) => null;
        }

        public class User { }

        [Theory]
        [InlineData("Method", "method")]
        [InlineData("Method", "ConventionMethod")]
        [InlineData("p", "model")]
        [InlineData("person", "model")]
        public void IsNameMatch_WithAny_AlwaysReturnsTrue(string name, string conventionName)
        {
            // Act
            var result = ApiConventionItem.IsNameMatch(name, conventionName, ApiConventionNameMatchBehavior.Any);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNameMatch_WithExact_ReturnsFalse_IfNamesDifferInCase()
        {
            // Arrange
            var name = "Name";
            var conventionName = "name";

            // Act
            var result = ApiConventionItem.IsNameMatch(name, conventionName, ApiConventionNameMatchBehavior.Exact);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsNameMatch_WithExact_ReturnsFalse_IfNamesAreDifferent()
        {
            // Arrange
            var name = "Name";
            var conventionName = "Different";

            // Act
            var result = ApiConventionItem.IsNameMatch(name, conventionName, ApiConventionNameMatchBehavior.Exact);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsNameMatch_WithExact_ReturnsFalse_IfConventionNameIsSubString()
        {
            // Arrange
            var name = "RegularName";
            var conventionName = "Regular";

            // Act
            var result = ApiConventionItem.IsNameMatch(name, conventionName, ApiConventionNameMatchBehavior.Exact);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsNameMatch_WithExact_ReturnsFalse_IfConventionNameIsSuperString()
        {
            // Arrange
            var name = "Regular";
            var conventionName = "RegularName";

            // Act
            var result = ApiConventionItem.IsNameMatch(name, conventionName, ApiConventionNameMatchBehavior.Exact);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsNameMatch_WithExact_ReturnsTrue_IfExactMatch()
        {
            // Arrange
            var name = "parameterName";
            var conventionName = "parameterName";

            // Act
            var result = ApiConventionItem.IsNameMatch(name, conventionName, ApiConventionNameMatchBehavior.Exact);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNameMatch_WithPrefix_ReturnsTrue_IfNamesAreExact()
        {
            // Arrange
            var name = "PostPerson";
            var conventionName = "PostPerson";

            // Act
            var result = ApiConventionItem.IsNameMatch(name, conventionName, ApiConventionNameMatchBehavior.Prefix);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNameMatch_WithPrefix_ReturnsTrue_IfNameIsProperPrefix()
        {
            // Arrange
            var name = "PostPerson";
            var conventionName = "Post";

            // Act
            var result = ApiConventionItem.IsNameMatch(name, conventionName, ApiConventionNameMatchBehavior.Prefix);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNameMatch_WithPrefix_ReturnsFalse_IfNamesAreDifferent()
        {
            // Arrange
            var name = "GetPerson";
            var conventionName = "Post";

            // Act
            var result = ApiConventionItem.IsNameMatch(name, conventionName, ApiConventionNameMatchBehavior.Prefix);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsNameMatch_WithPrefix_ReturnsFalse_IfNamesDifferInCase()
        {
            // Arrange
            var name = "GetPerson";
            var conventionName = "post";

            // Act
            var result = ApiConventionItem.IsNameMatch(name, conventionName, ApiConventionNameMatchBehavior.Prefix);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsNameMatch_WithPrefix_ReturnsFalse_IfNameIsNotProperPrfix()
        {
            // Arrange
            var name = "Postman";
            var conventionName = "Post";

            // Act
            var result = ApiConventionItem.IsNameMatch(name, conventionName, ApiConventionNameMatchBehavior.Prefix);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsNameMatch_WithPrefix_ReturnsFalse_IfNameIsSuffix()
        {
            // Arrange
            var name = "GoPost";
            var conventionName = "Post";

            // Act
            var result = ApiConventionItem.IsNameMatch(name, conventionName, ApiConventionNameMatchBehavior.Prefix);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsNameMatch_WithSuffix_ReturnsFalse_IfNamesAreDifferent()
        {
            // Arrange
            var name = "name";
            var conventionName = "diff";

            // Act
            var result = ApiConventionItem.IsNameMatch(name, conventionName, ApiConventionNameMatchBehavior.Suffix);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsNameMatch_WithSuffix_ReturnsFalse_IfNameIsNotSuffix()
        {
            // Arrange
            var name = "personId";
            var conventionName = "idx";

            // Act
            var result = ApiConventionItem.IsNameMatch(name, conventionName, ApiConventionNameMatchBehavior.Suffix);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsNameMatch_WithSuffix_ReturnTrue_IfNameIsExact()
        {
            // Arrange
            var name = "test";
            var conventionName = "test";

            // Act
            var result = ApiConventionItem.IsNameMatch(name, conventionName, ApiConventionNameMatchBehavior.Suffix);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNameMatch_WithSuffix_ReturnFalse_IfNameDiffersInCase()
        {
            // Arrange
            var name = "test";
            var conventionName = "Test";

            // Act
            var result = ApiConventionItem.IsNameMatch(name, conventionName, ApiConventionNameMatchBehavior.Suffix);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsNameMatch_WithSuffix_ReturnTrue_IfNameIsProperSuffix()
        {
            // Arrange
            var name = "personId";
            var conventionName = "id";

            // Act
            var result = ApiConventionItem.IsNameMatch(name, conventionName, ApiConventionNameMatchBehavior.Suffix);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("candid", "id")]
        [InlineData("canDid", "id")]
        public void IsNameMatch_WithSuffix_ReturnFalse_IfNameIsNotProperSuffix(string name, string conventionName)
        {
            // Act
            var result = ApiConventionItem.IsNameMatch(name, conventionName, ApiConventionNameMatchBehavior.Suffix);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(typeof(object), typeof(object))]
        [InlineData(typeof(int), typeof(void))]
        [InlineData(typeof(string), typeof(DateTime))]
        public void IsTypeMatch_WithAny_ReturnsTrue(Type type, Type conventionType)
        {
            // Act
            var result = ApiConventionItem.IsTypeMatch(type, conventionType, ApiConventionTypeMatchBehavior.Any);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsTypeMatch_WithExact_ReturnsTrueForExactType()
        {
            // Arrange
            var type = typeof(int);
            var conventionType = typeof(int);

            // Act
            var result = ApiConventionItem.IsTypeMatch(type, conventionType, ApiConventionTypeMatchBehavior.Exact);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsTypeMatch_WithExact_ReturnsFalseForDifferentTypes()
        {
            // Arrange
            var type = typeof(int);
            var conventionType = typeof(string);

            // Act
            var result = ApiConventionItem.IsTypeMatch(type, conventionType, ApiConventionTypeMatchBehavior.Exact);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsTypeMatch_WithExact_ReturnsFalseForDerivedTypes()
        {
            // Arrange
            var type = typeof(Base);
            var conventionType = typeof(Derived);

            // Act
            var result = ApiConventionItem.IsTypeMatch(type, conventionType, ApiConventionTypeMatchBehavior.Exact);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsTypeMatch_WithAssinableFrom_ReturnsTrueForExact()
        {
            // Arrange
            var type = typeof(Base);
            var conventionType = typeof(Base);

            // Act
            var result = ApiConventionItem.IsTypeMatch(type, conventionType, ApiConventionTypeMatchBehavior.AssignableFrom);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsTypeMatch_WithAssinableFrom_ReturnsTrueForDerived()
        {
            // Arrange
            var type = typeof(Derived);
            var conventionType = typeof(Base);

            // Act
            var result = ApiConventionItem.IsTypeMatch(type, conventionType, ApiConventionTypeMatchBehavior.AssignableFrom);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsTypeMatch_WithAssinableFrom_ReturnsFalseForBaseTypes()
        {
            // Arrange
            var type = typeof(Base);
            var conventionType = typeof(Derived);

            // Act
            var result = ApiConventionItem.IsTypeMatch(type, conventionType, ApiConventionTypeMatchBehavior.AssignableFrom);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsTypeMatch_WithAssinableFrom_ReturnsFalseForUnrelated()
        {
            // Arrange
            var type = typeof(string);
            var conventionType = typeof(Derived);

            // Act
            var result = ApiConventionItem.IsTypeMatch(type, conventionType, ApiConventionTypeMatchBehavior.AssignableFrom);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsMatch_ReturnsFalse_IfMethodNamesDoNotMatch()
        {
            // Arrange
            var method = typeof(TestController).GetMethod(nameof(TestController.Get));
            var conventionMethod = typeof(TestConvention).GetMethod(nameof(TestConvention.Post));

            // Act
            var result = ApiConventionItem.IsMatch(method, conventionMethod);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsMatch_ReturnsFalse_IMethodHasMoreParametersThanConvention()
        {
            // Arrange
            var method = typeof(TestController).GetMethod(nameof(TestController.Get));
            var conventionMethod = typeof(TestConvention).GetMethod(nameof(TestConvention.GetNoArgs));

            // Act
            var result = ApiConventionItem.IsMatch(method, conventionMethod);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsMatch_ReturnsFalse_IfMethodHasFewerParametersThanConvention()
        {
            // Arrange
            var method = typeof(TestController).GetMethod(nameof(TestController.Get));
            var conventionMethod = typeof(TestConvention).GetMethod(nameof(TestConvention.GetTwoArgs));

            // Act
            var result = ApiConventionItem.IsMatch(method, conventionMethod);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsMatch_ReturnsFalse_IfParametersDoNotMatch()
        {
            // Arrange
            var method = typeof(TestController).GetMethod(nameof(TestController.Get));
            var conventionMethod = typeof(TestConvention).GetMethod(nameof(TestConvention.GetParameterNotMatching));

            // Act
            var result = ApiConventionItem.IsMatch(method, conventionMethod);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsMatch_ReturnsTrue_IfMethodNameAndParametersMatchs()
        {
            // Arrange
            var method = typeof(TestController).GetMethod(nameof(TestController.Get));
            var conventionMethod = typeof(TestConvention).GetMethod(nameof(TestConvention.Get));

            // Act
            var result = ApiConventionItem.IsMatch(method, conventionMethod);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsMatch_ReturnsTrue_IfParamsArrayMatchesRemainingArguments()
        {
            // Arrange
            var method = typeof(TestController).GetMethod(nameof(TestController.Search));
            var conventionMethod = typeof(TestConvention).GetMethod(nameof(TestConvention.Search));

            // Act
            var result = ApiConventionItem.IsMatch(method, conventionMethod);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsMatch_WithEmpty_MatchesMethodWithNoParameters()
        {
            // Arrange
            var method = typeof(TestController).GetMethod(nameof(TestController.SearchEmpty));
            var conventionMethod = typeof(TestConvention).GetMethod(nameof(TestConvention.SearchWithParams));

            // Act
            var result = ApiConventionItem.IsMatch(method, conventionMethod);

            // Assert
            Assert.True(result);
        }

        public class Base { }

        public class Derived : Base { }

        public class TestController
        {
            public IActionResult Get(int id) => null;

            public IActionResult Search(string searchTerm, bool sortDescending, int page) => null;

            public IActionResult SearchEmpty() => null;
        }

        public static class TestConvention
        {
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
            public static void Get(int id) { }

            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
            public static void GetNoArgs() { }

            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
            public static void GetTwoArgs(int id, string name) { }

            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
            public static void Post(Derived model) { }

            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
            public static void GetParameterNotMatching([ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Exact)] Derived model) { }

            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
            public static void Search(
                [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Exact)]
                string searchTerm,
                params object[] others)
            { }

            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
            public static void SearchWithParams(params object[] others) { }
        }
    }
}
