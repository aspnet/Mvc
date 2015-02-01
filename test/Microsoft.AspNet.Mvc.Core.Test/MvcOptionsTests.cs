// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class MvcOptionsTests
    {
        [Fact]
        public void AntiForgeryOptions_SettingNullValue_Throws()
        {
            // Arrange
            var options = new MvcOptions();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => options.AntiForgeryOptions = null);
            Assert.Equal("The 'AntiForgeryOptions' property of 'Microsoft.AspNet.Mvc.MvcOptions' must not be null." +
                         Environment.NewLine + "Parameter name: value", ex.Message);
        }

        [Fact]
        public void MaxValidationError_ThrowsIfValueIsOutOfRange()
        {
            // Arrange
            var options = new MvcOptions();

            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => options.MaxModelValidationErrors = -1);
            Assert.Equal("value", ex.ParamName);
        }

        [Fact]
        public void AddCacheProfile_AddsCacheProfiles()
        {
            // Arrange
            var options = new MvcOptions();
            var cacheProfile = new CacheProfile("Sample")
            {
                Duration = 20
            };

            // Act
            options.AddCacheProfile(cacheProfile);

            // Assert
            Assert.Equal(cacheProfile, options.GetCacheProfile("Sample"));
        }

        [Fact]
        public void AddCacheProfile_AddsCacheProfiles_WithDifferentNames()
        {
            // Arrange
            var options = new MvcOptions();
            var cacheProfile1 = new CacheProfile("Sample1")
            {
                Duration = 20
            };
            var cacheProfile2 = new CacheProfile("Sample2")
            {
                Duration = 0,
                NoStore = true,
                Location = ResponseCacheLocation.None
            };

            // Act
            options.AddCacheProfile(cacheProfile1);
            options.AddCacheProfile(cacheProfile2);

            // Assert
            Assert.Equal(cacheProfile1, options.GetCacheProfile("Sample1"));
            Assert.Equal(cacheProfile2, options.GetCacheProfile("Sample2"));
        }

        [Fact]
        public void AddCacheProfile_ThrowsWhenMultipleCacheProfilesWithSameNameAreAdded()
        {
            // Arrange
            var options = new MvcOptions();
            var cacheProfile1 = new CacheProfile("Sample")
            {
                Duration = 20
            };
            var cacheProfile2 = new CacheProfile("Sample")
            {
                Location = ResponseCacheLocation.Any
            };

            // Act & Assert
            options.AddCacheProfile(cacheProfile1);
            Assert.Throws<InvalidOperationException>(() => options.AddCacheProfile(cacheProfile2));
        }
    }
}