// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Framework.OptionsModel;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class ResponseCacheAttributeTest
    {
        [Theory]
        [InlineData("Blah")]
        // To verify case-insensitive lookup.
        [InlineData("blah")]
        public void CreateInstance_SelectsTheAppropriateCacheProfile(string profileName)
        {
            // Arrange
            var responseCache = new ResponseCacheAttribute() {
                CacheProfileName = profileName
            };
            var cacheProfiles = new List<CacheProfile>();
            cacheProfiles.Add(new CacheProfile { Name = "Blah", NoStore = true });
            cacheProfiles.Add(new CacheProfile { Name = "Test", Duration = 20 });

            // Act
            var createdFilter = responseCache.CreateInstance(GetServiceProvider(cacheProfiles));

            // Assert
            var responseCacheFilter = Assert.IsType<ResponseCacheFilter>(createdFilter);
            Assert.True(responseCacheFilter.NoStore);
        }

        public static IEnumerable<object[]> OverrideData
        {
            get
            {
                // When there are no cache profiles then the passed in data is returned unchanged
                yield return new object[] {
                    new ResponseCacheAttribute()
                    { Duration = 20, Location = ResponseCacheLocation.Any, NoStore = false, VaryByHeader = "Accept" },
                    null,
                    new CacheProfile()
                    { Duration = 20, Location = ResponseCacheLocation.Any, NoStore = false, VaryByHeader = "Accept" }
                };

                // Everything gets overriden if attribute parameters are present,
                // when a particular cache profile is chosen.
                yield return new object[] {
                    new ResponseCacheAttribute()
                    {
                        Duration = 20,
                        Location = ResponseCacheLocation.Any,
                        NoStore = false,
                        VaryByHeader = "Accept",
                        CacheProfileName = "TestCacheProfile"
                    },
                    new CacheProfile()
                    {
                        Name = "TestCacheProfile",
                        Duration = 10,
                        Location = ResponseCacheLocation.Client,
                        NoStore = true,
                        VaryByHeader = "Test"
                    },
                    new CacheProfile()
                    { Duration = 20, Location = ResponseCacheLocation.Any, NoStore = false, VaryByHeader = "Accept" }
                };

                // Select parameters override the selected profile.
                yield return new object[] {
                    new ResponseCacheAttribute()
                    {
                        Duration = 534,
                        CacheProfileName = "TestCacheProfile"
                    },
                    new CacheProfile()
                    {
                        Name = "TestCacheProfile",
                        Duration = 10,
                        Location = ResponseCacheLocation.Client,
                        NoStore = false,
                        VaryByHeader = "Test"
                    },
                    new CacheProfile()
                    { Duration = 534, Location = ResponseCacheLocation.Client, NoStore = false, VaryByHeader = "Test" }
                };

                // Duration parameter gets added to the selected profile.
                yield return new object[] {
                    new ResponseCacheAttribute()
                    {
                        Duration = 534,
                        CacheProfileName = "TestCacheProfile"
                    },
                    new CacheProfile()
                    {
                        Name = "TestCacheProfile",
                        Location = ResponseCacheLocation.Client,
                        NoStore = false,
                        VaryByHeader = "Test"
                    },
                    new CacheProfile()
                    { Duration = 534, Location = ResponseCacheLocation.Client, NoStore = false, VaryByHeader = "Test" }
                };

                // Default values gets added for parameters which are absent
                yield return new object[] {
                    new ResponseCacheAttribute()
                    {
                        Duration = 5234,
                        CacheProfileName = "TestCacheProfile"
                    },
                    new CacheProfile()
                    {
                        Name = "TestCacheProfile",
                    },
                    new CacheProfile()
                    { Duration = 5234, Location = ResponseCacheLocation.Any, NoStore = false, VaryByHeader = null }
                };
            }
        }

        [Theory]
        [MemberData(nameof(OverrideData))]
        public void CreateInstance_HonorsOverrides(
            ResponseCacheAttribute responseCache, CacheProfile profile, CacheProfile expectedProfile)
        {
            // Arrange
            var cacheProfiles = new List<CacheProfile>();
            if (profile != null)
            {
                cacheProfiles.Add(profile);
            }

            // Act
            var createdFilter = responseCache.CreateInstance(GetServiceProvider(cacheProfiles));

            // Assert
            var responseCacheFilter = Assert.IsType<ResponseCacheFilter>(createdFilter);
            Assert.Equal(expectedProfile.Duration, responseCacheFilter.Duration);
            Assert.Equal(expectedProfile.Location, responseCacheFilter.Location);
            Assert.Equal(expectedProfile.NoStore, responseCacheFilter.NoStore);
            Assert.Equal(expectedProfile.VaryByHeader, responseCacheFilter.VaryByHeader);
        }

        [Fact]
        public void CreateInstance_ThrowsWhenTheDurationIsNotSet_WithNoStoreFalse()
        {
            // Arrange
            var responseCache = new ResponseCacheAttribute()
            {
                CacheProfileName = "Test"
            };
            var cacheProfiles = new List<CacheProfile>();
            cacheProfiles.Add(new CacheProfile { Name = "Test", NoStore = false });

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => responseCache.CreateInstance(GetServiceProvider(cacheProfiles)));
        }

        private IServiceProvider GetServiceProvider(List<CacheProfile> cacheProfiles)
        {
            var serviceProvider = new Mock<IServiceProvider>();
            var optionsAccessor = new Mock<IOptions<MvcOptions>>();
            var options = new MvcOptions();
            if (cacheProfiles != null)
            {
                foreach (CacheProfile p in cacheProfiles)
                {
                    options.CacheProfiles.Add(p);
                }
            }

            optionsAccessor.SetupGet(o => o.Options).Returns(options);
            serviceProvider
                .Setup(s => s.GetService(typeof(IOptions<MvcOptions>)))
                .Returns(optionsAccessor.Object);

            return serviceProvider.Object;
        }
    }
}