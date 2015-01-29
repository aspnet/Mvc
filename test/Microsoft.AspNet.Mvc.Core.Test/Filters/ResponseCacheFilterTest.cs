// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Http.Core;
using Microsoft.AspNet.Routing;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class ResponseCacheFilterTest
    {
        [Fact]
        public void OnActionExecuting_DoesNotThrow_WhenNoStoreIsTrue()
        {
            // Arrange
            var cache = new ResponseCacheFilter(0, ResponseCacheLocation.Any, true, null);
            var context = GetActionExecutingContext(new List<IFilter> { cache });

            // Act
            cache.OnActionExecuting(context);

            // Assert
            Assert.Equal("no-store", context.HttpContext.Response.Headers.Get("Cache-control"));
        }

        public static IEnumerable<object[]> CacheControlData
        {
            get
            {
                yield return new object[] {
                    new ResponseCacheFilter(
                        duration: 0, location: ResponseCacheLocation.Any, noStore: true, varyByHeader: null),
                    "no-store"
                };
                // If no-store is set, then location is ignored.
                yield return new object[] {
                    new ResponseCacheFilter(
                        duration: 0, location: ResponseCacheLocation.Client, noStore: true, varyByHeader: null),
                    "no-store"
                };
                yield return new object[] {
                    new ResponseCacheFilter(
                        duration: 0, location: ResponseCacheLocation.Any, noStore: true, varyByHeader: null),
                    "no-store"
                };
                // If no-store is set, then duration is ignored.
                yield return new object[] {
                    new ResponseCacheFilter(
                        duration: 100, location: ResponseCacheLocation.Any, noStore: true, varyByHeader: null),
                    "no-store"
                };

                yield return new object[] {
                    new ResponseCacheFilter(
                        duration: 10, location: ResponseCacheLocation.Client, noStore: false, varyByHeader: null),
                    "private,max-age=10"
                };
                yield return new object[] {
                    new ResponseCacheFilter(
                        duration: 10, location: ResponseCacheLocation.Any, noStore: false, varyByHeader: null),
                    "public,max-age=10"
                };
                yield return new object[] {
                    new ResponseCacheFilter(
                        duration: 10, location: ResponseCacheLocation.None, noStore: false, varyByHeader: null),
                    "no-cache,max-age=10"
                };
                yield return new object[] {
                    new ResponseCacheFilter(
                        duration: 31536000, location: ResponseCacheLocation.Any, noStore: false, varyByHeader: null),
                    "public,max-age=31536000"
                };
                yield return new object[] {
                    new ResponseCacheFilter(
                        duration: 20, location: ResponseCacheLocation.Any, noStore: false, varyByHeader: null),
                    "public,max-age=20"
                };
            }
        }

        [Theory]
        [MemberData(nameof(CacheControlData))]
        public void OnActionExecuting_CanSetCacheControlHeaders(ResponseCacheFilter cache, string output)
        {
            // Arrange
            var context = GetActionExecutingContext(new List<IFilter> { cache });

            // Act
            cache.OnActionExecuting(context);

            // Assert
            Assert.Equal(output, context.HttpContext.Response.Headers.Get("Cache-control"));
        }

        public static IEnumerable<object[]> NoStoreData
        {
            get
            {
                // If no-store is set, then location is ignored.
                yield return new object[] {
                    new ResponseCacheFilter(
                        duration: 0, location: ResponseCacheLocation.Client, noStore: true, varyByHeader: null),
                    "no-store"
                };
                yield return new object[] {
                    new ResponseCacheFilter(
                        duration: 0, location: ResponseCacheLocation.Any, noStore: true, varyByHeader: null),
                    "no-store"
                };
                // If no-store is set, then duration is ignored.
                yield return new object[] {
                    new ResponseCacheFilter(
                        duration: 100, location: ResponseCacheLocation.Any, noStore: true, varyByHeader: null),
                    "no-store"
                };
            }
        }

        [Theory]
        [MemberData(nameof(NoStoreData))]
        public void OnActionExecuting_DoesNotSetLocationOrDuration_IfNoStoreIsSet(
            ResponseCacheFilter cache, string output)
        {
            // Arrange
            var context = GetActionExecutingContext(new List<IFilter> { cache });

            // Act
            cache.OnActionExecuting(context);

            // Assert
            Assert.Equal(output, context.HttpContext.Response.Headers.Get("Cache-control"));
        }

        public static IEnumerable<object[]> VaryData
        {
            get
            {
                yield return new object[] {
                    new ResponseCacheFilter(
                        duration: 10, location: ResponseCacheLocation.Any, noStore: false, varyByHeader: "Accept"),
                    "Accept",
                    "public,max-age=10" };
                yield return new object[] {
                    new ResponseCacheFilter(
                        duration: 0, location: ResponseCacheLocation.Any, noStore: true, varyByHeader: "Accept"),
                    "Accept",
                    "no-store"
                };
                yield return new object[] {
                    new ResponseCacheFilter(
                        duration: 10, location: ResponseCacheLocation.Client, noStore: false, varyByHeader: "Accept"),
                    "Accept",
                    "private,max-age=10"
                };
                yield return new object[] {
                    new ResponseCacheFilter(
                        duration: 10, location: ResponseCacheLocation.Client, noStore: false, varyByHeader: "Test"),
                    "Test",
                    "private,max-age=10"
                };
                yield return new object[] {
                    new ResponseCacheFilter(
                        duration: 31536000, location: ResponseCacheLocation.Any, noStore: false, varyByHeader: "Test"),
                    "Test",
                    "public,max-age=31536000"
                };
            }
        }

        [Theory]
        [MemberData(nameof(VaryData))]
        public void ResponseCacheCanSetVary(ResponseCacheFilter cache, string varyOutput, string cacheControlOutput)
        {
            // Arrange
            var context = GetActionExecutingContext(new List<IFilter> { cache });

            // Act
            cache.OnActionExecuting(context);

            // Assert
            Assert.Equal(varyOutput, context.HttpContext.Response.Headers.Get("Vary"));
            Assert.Equal(cacheControlOutput, context.HttpContext.Response.Headers.Get("Cache-control"));
        }

        [Fact]
        public void SetsPragmaOnNoCache()
        {
            // Arrange
            var cache = new ResponseCacheFilter(
                duration: 0, location: ResponseCacheLocation.None, noStore: true, varyByHeader: null);
            var context = GetActionExecutingContext(new List<IFilter> { cache });

            // Act
            cache.OnActionExecuting(context);

            // Assert
            Assert.Equal("no-store,no-cache", context.HttpContext.Response.Headers.Get("Cache-control"));
            Assert.Equal("no-cache", context.HttpContext.Response.Headers.Get("Pragma"));
        }

        [Fact]
        public void IsOverridden_ReturnsTrueForAllButLastFilter()
        {
            // Arrange
            var caches = new List<IFilter>();
            caches.Add(new ResponseCacheFilter(
                duration: 0, location: ResponseCacheLocation.Any, noStore: false, varyByHeader: null));
            caches.Add(new ResponseCacheFilter(
                duration: 0, location: ResponseCacheLocation.Any, noStore: false, varyByHeader: null));

            var context = GetActionExecutingContext(caches);

            // Act & Assert
            Assert.True((caches[0] as ResponseCacheFilter).IsOverridden(context));
            Assert.False((caches[1] as ResponseCacheFilter).IsOverridden(context));
        }

        private ActionExecutingContext GetActionExecutingContext(List<IFilter> filters = null)
        {
            return new ActionExecutingContext(
                new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
                filters ?? new List<IFilter>(),
                new Dictionary<string, object>(),
                new object());
        }
    }
}