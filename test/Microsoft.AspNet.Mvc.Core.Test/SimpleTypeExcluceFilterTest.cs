// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class SimpleTypeExcluceFilterTest
    {
        [Theory]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(int?[]))]
        [InlineData(typeof(int?))]
        [InlineData(typeof(int))]
        [InlineData(typeof(SortedSet<int>))]
        [InlineData(typeof(SortedSet<int?>))]
        [InlineData(typeof(Dictionary<int, string>))]
        [InlineData(typeof(IReadOnlyDictionary<int?, char?>))]
        public void SimpleTypeExcluceFilter_ExcludedTypes(Type type)
        {
            // Arrange
            var filter = new SimpleTypesExcludeFilter();

            // Act & Assert
            Assert.True(filter.IsTypeExcluded(type));
        }

        [Theory]
        [InlineData(typeof(TestType))]
        [InlineData(typeof(TestType[]))]
        [InlineData(typeof(SortedSet<TestType>))]
        [InlineData(typeof(Dictionary<int, TestType>))]
        [InlineData(typeof(Dictionary<TestType, int>))]
        public void SimpleTypeExcluceFilter_IncludedTypes(Type type)
        {
            // Arrange
            var filter = new SimpleTypesExcludeFilter();

            // Act & Assert
            Assert.False(filter.IsTypeExcluded(type));
        }

        private class TestType
        {

        }
    }
}