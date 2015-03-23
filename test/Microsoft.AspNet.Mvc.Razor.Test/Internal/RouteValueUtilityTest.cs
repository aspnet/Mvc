// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Routing;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Testing;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor.Internal
{
    public class RouteValueUtilityTest
    {
        [Fact]
        public void GetNormalizedRouteValue_ReturnsValueFromRouteConstraints_IfKeyHandlingIsRequired()
        {
            // Arrange
            var key = "some-key";
            var actionDescriptor = new ActionDescriptor
            {
                RouteConstraints = new[]
                {
                    new RouteDataActionConstraint(key, "route-constraint-value")
                }
            };

            var actionContext = new ActionContext
            {
                ActionDescriptor = actionDescriptor,
                RouteData = new RouteData()
            };

            actionContext.RouteData.Values[key] = "route-data-value";

            // Act
            var result = RouteValueUtility.GetNormalizedRouteValue(actionContext, key);

            // Assert
            Assert.Equal("route-constraint-value", result);
        }

        [Fact]
        public void GetNormalizedRouteValue_ReturnsNull_IfRouteConstraintKeyHandlingIsDeny()
        {
            // Arrange
            var key = "some-key";
            var actionDescriptor = new ActionDescriptor
            {
                RouteConstraints = new[]
                {
                    new RouteDataActionConstraint(key, routeValue: string.Empty)
                }
            };

            var actionContext = new ActionContext
            {
                ActionDescriptor = actionDescriptor,
                RouteData = new RouteData()
            };

            actionContext.RouteData.Values[key] = "route-data-value";

            // Act
            var result = RouteValueUtility.GetNormalizedRouteValue(actionContext, key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetNormalizedRouteValue_ReturnsRouteDataValue_IfRouteConstraintKeyHandlingIsCatchAll()
        {
            // Arrange
            var key = "some-key";
            var actionDescriptor = new ActionDescriptor
            {
                RouteConstraints = new[]
                {
                    RouteDataActionConstraint.CreateCatchAll(key)
                }
            };

            var actionContext = new ActionContext
            {
                ActionDescriptor = actionDescriptor,
                RouteData = new RouteData()
            };

            actionContext.RouteData.Values[key] = "route-data-value";

            // Act
            var result = RouteValueUtility.GetNormalizedRouteValue(actionContext, key);

            // Assert
            Assert.Equal("route-data-value", result);
        }

        [Fact]
        public void GetNormalizedRouteValue_UsesRouteValueDefaults_IfAttributeRouted()
        {
            // Arrange
            var key = "some-key";
            var actionDescriptor = new ActionDescriptor
            {
                AttributeRouteInfo = new AttributeRouteInfo(),
            };
            actionDescriptor.RouteValueDefaults[key] = "route-default-value";

            var actionContext = new ActionContext
            {
                ActionDescriptor = actionDescriptor,
                RouteData = new RouteData()
            };

            actionContext.RouteData.Values[key] = "route-data-value";

            // Act
            var result = RouteValueUtility.GetNormalizedRouteValue(actionContext, key);

            // Assert
            Assert.Equal("route-default-value", result);
        }

        [Fact]
        public void GetNormalizedRouteValue_ConvertsRouteDefaultToStringValue_IfAttributeRouted()
        {
            using (new CultureReplacer())
            {
                // Arrange
                var key = "some-key";
                var actionDescriptor = new ActionDescriptor
                {
                    AttributeRouteInfo = new AttributeRouteInfo(),
                };
                actionDescriptor.RouteValueDefaults[key] = 32;

                var actionContext = new ActionContext
                {
                    ActionDescriptor = actionDescriptor,
                    RouteData = new RouteData()
                };

                actionContext.RouteData.Values[key] = "route-data-value";

                // Act
                var result = RouteValueUtility.GetNormalizedRouteValue(actionContext, key);

                // Assert
                Assert.Equal("32", result);
            }
        }

        [Fact]
        public void GetNormalizedRouteValue_UsesRouteDataValue_IfKeyDoesNotExistInRouteDefaultValues()
        {
            // Arrange
            var key = "some-key";
            var actionDescriptor = new ActionDescriptor
            {
                AttributeRouteInfo = new AttributeRouteInfo(),
            };

            var actionContext = new ActionContext
            {
                ActionDescriptor = actionDescriptor,
                RouteData = new RouteData()
            };

            actionContext.RouteData.Values[key] = "route-data-value";

            // Act
            var result = RouteValueUtility.GetNormalizedRouteValue(actionContext, key);

            // Assert
            Assert.Equal("route-data-value", result);
        }

        [Fact]
        public void GetNormalizedRouteValue_ConvertsRouteValueToString()
        {
            using (new CultureReplacer())
            {
                // Arrange
                var key = "some-key";
                var actionDescriptor = new ActionDescriptor
                {
                    AttributeRouteInfo = new AttributeRouteInfo(),
                };

                var actionContext = new ActionContext
                {
                    ActionDescriptor = actionDescriptor,
                    RouteData = new RouteData()
                };

                actionContext.RouteData.Values[key] = 43;

                // Act
                var result = RouteValueUtility.GetNormalizedRouteValue(actionContext, key);

                // Assert
                Assert.Equal("43", result);
            }
        }
    }
}