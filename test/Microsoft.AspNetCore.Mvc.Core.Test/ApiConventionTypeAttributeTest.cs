﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class ApiConventionTypeAttributeTest
    {
        [Fact]
        public void Constructor_ThrowsIfConventionMethodIsAnnotatedWithProducesAttribute()
        {
            // Arrange
            var methodName = typeof(ConventionWithProducesAttribute).FullName + '.' + nameof(ConventionWithProducesAttribute.Get);
            var attribute = typeof(ProducesAttribute);

            var expected = GetErrorMessage(methodName, attribute);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => new ApiConventionTypeAttribute(typeof(ConventionWithProducesAttribute)),
                "conventionType",
                expected);
        }

        public static class ConventionWithProducesAttribute
        {
            [Produces(typeof(void))]
            public static void Get() { }
        }

        [Fact]
        public void Constructor_ThrowsIfConventionMethodHasRouteAttribute()
        {
            // Arrange
            var methodName = typeof(ConventionWithRouteAttribute).FullName + '.' + nameof(ConventionWithRouteAttribute.Get);
            var attribute = typeof(HttpGetAttribute);
            var expected = GetErrorMessage(methodName, attribute);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => new ApiConventionTypeAttribute(typeof(ConventionWithRouteAttribute)),
                "conventionType",
                expected);
        }

        public static class ConventionWithRouteAttribute
        {
            [HttpGet("url")]
            public static void Get() { }
        }

        [Fact]
        public void Constructor_ThrowsIfMultipleUnsupportedAttributesArePresentOnConvention()
        {
            // Arrange
            var methodName = typeof(ConventionWitUnsupportedAttributes).FullName + '.' + nameof(ConventionWitUnsupportedAttributes.Get);
            var attributes = new[] { typeof(ProducesAttribute), typeof(ServiceFilterAttribute), typeof(AuthorizeAttribute) };
            var expected = GetErrorMessage(methodName, attributes);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => new ApiConventionTypeAttribute(typeof(ConventionWitUnsupportedAttributes)),
                "conventionType",
                expected);
        }

        public static class ConventionWitUnsupportedAttributes
        {
            [ProducesResponseType(400)]
            [Produces(typeof(void))]
            [ServiceFilter(typeof(object))]
            [Authorize]
            public static void Get() { }
        }

        private static string GetErrorMessage(string methodName, params Type[] attributes)
        {
            return $"Method {methodName} is decorated with the following attributes that are not allowed on an API convention method:" +
                Environment.NewLine +
                string.Join(Environment.NewLine, attributes.Select(a => a.FullName)) +
                Environment.NewLine +
                $"The following attributes are allowed on API convention methods: {nameof(ProducesResponseTypeAttribute)}, {nameof(ProducesDefaultResponseTypeAttribute)}, {nameof(ApiConventionNameMatchAttribute)}";
        }
    }
}
