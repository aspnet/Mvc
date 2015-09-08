// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Mvc.ActionConstraints;
using Microsoft.AspNet.Mvc.ActionResults;
using Microsoft.AspNet.Mvc.Actions;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class ConsumesAttributeTests
    {
        [Theory]
        [InlineData("application")]
        [InlineData("")]
        [InlineData(null)]
        public void Constructor_ForInvalidContentType_Throws(string contentType)
        {
            // Arrange
            var expectedMessage = string.Format("Invalid value '{0}'.", contentType ?? "<null>");

            // Act & Assert
            var exception = Assert.Throws<FormatException>(() => new ConsumesAttribute(contentType));
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("application/xml,, application/json", "")]
        [InlineData(", application/json", "")]
        [InlineData("invalid", "invalid")]
        [InlineData("application/xml,invalid, application/json", "invalid")]
        [InlineData("invalid, application/json", "invalid")]
        public void Constructor_UnparsableContentType_Throws(string content, string invalidContentType)
        {
            // Act
            var contentTypes = content.Split(',').Select(contentType => contentType.Trim()).ToArray();

            // Assert
            var ex = Assert.Throws<FormatException>(
                       () => new ConsumesAttribute(contentTypes[0], contentTypes.Skip(1).ToArray()));
            Assert.Equal("Invalid value '" + (invalidContentType ?? "<null>") + "'.",
                         ex.Message);
        }

        [Theory]
        [InlineData("application/*", "application/*")]
        [InlineData("application/xml, application/*, application/json", "application/*")]
        [InlineData("application/*, application/json", "application/*")]

        [InlineData("*/*", "*/*")]
        [InlineData("application/xml, */*, application/json", "*/*")]
        [InlineData("*/*, application/json", "*/*")]
        public void Constructor_InvalidContentType_Throws(string content, string invalidContentType)
        {
            // Act
            var contentTypes = content.Split(',').Select(contentType => contentType.Trim()).ToArray();

            // Assert
            var ex = Assert.Throws<InvalidOperationException>(
                       () => new ConsumesAttribute(contentTypes[0], contentTypes.Skip(1).ToArray()));

            Assert.Equal(
                string.Format("The argument '{0}' is invalid. "+
                              "Media types which match all types or match all subtypes are not supported.",
                              invalidContentType),
                ex.Message);
        }

        [Theory]
        [InlineData("application/json")]
        [InlineData("application/json;Parameter1=12")]
        [InlineData("text/xml")]
        public void Accept_MatchesForMachingRequestContentType(string contentType)
        {
            // Arrange
            var constraint = new ConsumesAttribute("application/json", "text/xml");
            var action = new ActionDescriptor()
            {
                FilterDescriptors =
                    new List<FilterDescriptor>() { new FilterDescriptor(constraint, FilterScope.Action) }
            };

            var context = new ActionConstraintContext();
            context.Candidates = new List<ActionSelectorCandidate>()
            {
                new ActionSelectorCandidate(action, new [] { constraint }),
            };

            context.CurrentCandidate = context.Candidates[0];
            context.RouteContext = CreateRouteContext(contentType: contentType);

            // Act & Assert
            Assert.True(constraint.Accept(context));
        }

        [Fact]
        public void Accept_TheFirstCandidateReturnsFalse_IfALaterOneMatches()
        {
            // Arrange
            var constraint1 = new ConsumesAttribute("application/json", "text/xml");
            var action1 = new ActionDescriptor()
            {
                FilterDescriptors =
                    new List<FilterDescriptor>() { new FilterDescriptor(constraint1, FilterScope.Action) }
            };

            var constraint2 = new Mock<ITestConsumeConstraint>();
            var action2 = new ActionDescriptor()
            {
                FilterDescriptors =
                    new List<FilterDescriptor>() { new FilterDescriptor(constraint2.Object, FilterScope.Action) }
            };

            constraint2.Setup(o => o.Accept(It.IsAny<ActionConstraintContext>()))
                       .Returns(true);

            var context = new ActionConstraintContext();
            context.Candidates = new List<ActionSelectorCandidate>()
            {
                new ActionSelectorCandidate(action1, new [] { constraint1 }),
                new ActionSelectorCandidate(action2, new [] { constraint2.Object }),
            };

            context.CurrentCandidate = context.Candidates[0];
            context.RouteContext = CreateRouteContext(contentType: "application/custom");

            // Act & Assert
            Assert.False(constraint1.Accept(context));
        }

        [Theory]
        [InlineData("application/custom")]
        [InlineData("")]
        [InlineData(null)]
        public void Accept_ForNoMatchingCandidates_SelectsTheFirstCandidate(string contentType)
        {
            // Arrange
            var constraint1 = new ConsumesAttribute("application/json", "text/xml");
            var action1 = new ActionDescriptor()
            {
                FilterDescriptors =
                    new List<FilterDescriptor>() { new FilterDescriptor(constraint1, FilterScope.Action) }
            };

            var constraint2 = new Mock<ITestConsumeConstraint>();
            var action2 = new ActionDescriptor()
            {
                FilterDescriptors =
                    new List<FilterDescriptor>() { new FilterDescriptor(constraint2.Object, FilterScope.Action) }
            };

            constraint2.Setup(o => o.Accept(It.IsAny<ActionConstraintContext>()))
                       .Returns(false);

            var context = new ActionConstraintContext();
            context.Candidates = new List<ActionSelectorCandidate>()
            {
                new ActionSelectorCandidate(action1, new [] { constraint1 }),
                new ActionSelectorCandidate(action2, new [] { constraint2.Object }),
            };

            context.CurrentCandidate = context.Candidates[0];
            context.RouteContext = CreateRouteContext(contentType: contentType);

            // Act & Assert
            Assert.True(constraint1.Accept(context));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Accept_ForNoRequestType_SelectsTheCandidateWithoutConstraintIfPresent(string contentType)
        {
            // Arrange
            var constraint1 = new ConsumesAttribute("application/json");
            var actionWithConstraint = new ActionDescriptor()
            {
                FilterDescriptors =
                    new List<FilterDescriptor>() { new FilterDescriptor(constraint1, FilterScope.Action) }
            };

            var constraint2 = new ConsumesAttribute("text/xml");
            var actionWithConstraint2 = new ActionDescriptor()
            {
                FilterDescriptors =
                    new List<FilterDescriptor>() { new FilterDescriptor(constraint2, FilterScope.Action) }
            };

            var actionWithoutConstraint = new ActionDescriptor();

            var context = new ActionConstraintContext();
            context.Candidates = new List<ActionSelectorCandidate>()
            {
                new ActionSelectorCandidate(actionWithConstraint, new [] { constraint1 }),
                new ActionSelectorCandidate(actionWithConstraint2, new [] { constraint2 }),
                new ActionSelectorCandidate(actionWithoutConstraint, new List<IActionConstraint>()),
            };

            context.RouteContext = CreateRouteContext(contentType: contentType);

            // Act & Assert
            context.CurrentCandidate = context.Candidates[0];
            Assert.False(constraint1.Accept(context));
            context.CurrentCandidate = context.Candidates[1];
            Assert.False(constraint2.Accept(context));
        }

        [Theory]
        [InlineData("application/xml")]
        [InlineData("application/custom")]
        [InlineData("invalid/invalid")]
        public void Accept_UnrecognizedMediaType_SelectsTheCandidateWithoutConstraintIfPresent(string contentType)
        {
            // Arrange
            var actionWithoutConstraint = new ActionDescriptor();
            var constraint1 = new ConsumesAttribute("application/json");
            var actionWithConstraint = new ActionDescriptor()
            {
                FilterDescriptors =
                    new List<FilterDescriptor>() { new FilterDescriptor(constraint1, FilterScope.Action) }
            };

            var constraint2 = new ConsumesAttribute("text/xml");
            var actionWithConstraint2 = new ActionDescriptor()
            {
                FilterDescriptors =
                    new List<FilterDescriptor>() { new FilterDescriptor(constraint2, FilterScope.Action) }
            };

            var context = new ActionConstraintContext();
            context.Candidates = new List<ActionSelectorCandidate>()
            {
                new ActionSelectorCandidate(actionWithConstraint, new [] { constraint1 }),
                new ActionSelectorCandidate(actionWithConstraint2, new [] { constraint2 }),
                new ActionSelectorCandidate(actionWithoutConstraint, new List<IActionConstraint>()),
            };

            context.RouteContext = CreateRouteContext(contentType: contentType);

            // Act & Assert
            context.CurrentCandidate = context.Candidates[0];
            Assert.False(constraint1.Accept(context));

            context.CurrentCandidate = context.Candidates[1];
            Assert.False(constraint2.Accept(context));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Accept_ForNoRequestType_ReturnsTrueForAllConstraints(string contentType)
        {
            // Arrange
            var constraint1 = new ConsumesAttribute("application/json");
            var actionWithConstraint = new ActionDescriptor()
            {
                FilterDescriptors =
                    new List<FilterDescriptor>() { new FilterDescriptor(constraint1, FilterScope.Action) }
            };

            var constraint2 = new ConsumesAttribute("text/xml");
            var actionWithConstraint2 = new ActionDescriptor()
            {
                FilterDescriptors =
                    new List<FilterDescriptor>() { new FilterDescriptor(constraint2, FilterScope.Action) }
            };

            var actionWithoutConstraint = new ActionDescriptor();

            var context = new ActionConstraintContext();
            context.Candidates = new List<ActionSelectorCandidate>()
            {
                new ActionSelectorCandidate(actionWithConstraint, new [] { constraint1 }),
                new ActionSelectorCandidate(actionWithConstraint2, new [] { constraint2 }),
            };

            context.RouteContext = CreateRouteContext(contentType: contentType);

            // Act & Assert
            context.CurrentCandidate = context.Candidates[0];
            Assert.True(constraint1.Accept(context));
            context.CurrentCandidate = context.Candidates[1];
            Assert.True(constraint2.Accept(context));
        }

        [Theory]
        [InlineData("application/xml")]
        [InlineData("application/custom")]
        public void OnResourceExecuting_ForNoContentTypeMatch_SetsUnsupportedMediaTypeResult(string contentType)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.ContentType = contentType;
            var consumesFilter = new ConsumesAttribute("application/json");
            var actionWithConstraint = new ActionDescriptor()
            {
                ActionConstraints = new List<IActionConstraintMetadata>() { consumesFilter },
                FilterDescriptors =
                    new List<FilterDescriptor>() { new FilterDescriptor(consumesFilter, FilterScope.Action) }
            };
            var actionContext = new ActionContext(httpContext, new RouteData(), actionWithConstraint);

            var resourceExecutingContext = new ResourceExecutingContext(actionContext, new[] { consumesFilter });

            // Act
            consumesFilter.OnResourceExecuting(resourceExecutingContext);

            // Assert
            Assert.NotNull(resourceExecutingContext.Result);
            Assert.IsType<UnsupportedMediaTypeResult>(resourceExecutingContext.Result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void OnResourceExecuting_NullOrEmptyRequestContentType_IsNoOp(string contentType)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.ContentType = contentType;
            var consumesFilter = new ConsumesAttribute("application/json");
            var actionWithConstraint = new ActionDescriptor()
            {
                ActionConstraints = new List<IActionConstraintMetadata>() { consumesFilter },
                FilterDescriptors =
                    new List<FilterDescriptor>() { new FilterDescriptor(consumesFilter, FilterScope.Action) }
            };
            var actionContext = new ActionContext(httpContext, new RouteData(), actionWithConstraint);

            var resourceExecutingContext = new ResourceExecutingContext(actionContext, new[] { consumesFilter });

            // Act
            consumesFilter.OnResourceExecuting(resourceExecutingContext);

            // Assert
            Assert.Null(resourceExecutingContext.Result);
        }

        [Theory]
        [InlineData("application/xml")]
        [InlineData("application/json")]
        public void OnResourceExecuting_ForAContentTypeMatch_IsNoOp(string contentType)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.ContentType = contentType;
            var consumesFilter = new ConsumesAttribute("application/json", "application/xml");
            var actionWithConstraint = new ActionDescriptor()
            {
                ActionConstraints = new List<IActionConstraintMetadata>() { consumesFilter },
                FilterDescriptors =
                    new List<FilterDescriptor>() { new FilterDescriptor(consumesFilter, FilterScope.Action) }
            };
            var actionContext = new ActionContext(httpContext, new RouteData(), actionWithConstraint);
            var resourceExecutingContext = new ResourceExecutingContext(actionContext, new[] { consumesFilter });

            // Act
            consumesFilter.OnResourceExecuting(resourceExecutingContext);

            // Assert
            Assert.Null(resourceExecutingContext.Result);
        }

        private static RouteContext CreateRouteContext(string contentType = null, object routeValues = null)
        {
            var httpContext = new DefaultHttpContext();
            if (contentType != null)
            {
                httpContext.Request.ContentType = contentType;
            }

            var routeContext = new RouteContext(httpContext);
            routeContext.RouteData = new RouteData();

            foreach (var kvp in new RouteValueDictionary(routeValues))
            {
                routeContext.RouteData.Values.Add(kvp.Key, kvp.Value);
            }

            return routeContext;
        }

        public interface ITestConsumeConstraint : IConsumesActionConstraint, IResourceFilter
        {
        }
    }
}