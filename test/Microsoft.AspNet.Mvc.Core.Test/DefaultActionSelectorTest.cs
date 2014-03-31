
using System.Collections.Generic;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class DefaultActionSelectorTest
    {
        public static IEnumerable<ActionDescriptor> Actions
        {
            get
            {
                // Like a typical RPC controller
                yield return CreateAction(area: null, controller: "Home", action: "Index");
                yield return CreateAction(area: null, controller: "Home", action: "Edit");

                // Like a typical REST controller
                yield return CreateAction(area: null, controller: "Product", action: null);
                yield return CreateAction(area: null, controller: "Product", action: null);

                // RPC controller in an area with the same name as home
                yield return CreateAction(area: "Admin", controller: "Home", action: "Index");
                yield return CreateAction(area: "Admin", controller: "Home", action: "Diagnostics");
            }
        }

        [Fact]
        public void IsValidAction_Explicit_ExactMatch()
        {
            // Arrange
            var selector = CreateSelector();
            var context = CreateContext(new {controller = "Home", action = "Index"});

            context.ProvidedValues = new RouteValueDictionary(new {controller = "Home", action = "Index"});

            // Act
            var isValid = selector.IsValidAction(context);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValidAction_RouteWithArea_NoMatch()
        {
            // Arrange
            var selector = CreateSelector();
            var context = CreateContext(new { controller = "Home", action = "Index" });

            // Even though there is a valid action in the area, it's not valid because the user didn't specify the area
            // and area is not part of the ambient context.
            context.ProvidedValues = new RouteValueDictionary(new { area = "Admin", controller = "Home", action = "Index" });

            // Act
            var isValid = selector.IsValidAction(context);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValidAction_InArea_RouteWithoutArea_NoMatch()
        {
            // Arrange
            var selector = CreateSelector();
            var context = CreateContext(new { controller = "Home", action = "Index", area = "Admin" });

            // We can't leave the area since the Area token was explicitly provided.
            context.ProvidedValues = new RouteValueDictionary(new { controller = "Home", action = "Index" });

            // Act
            var isValid = selector.IsValidAction(context);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValidAction_InArea_RouteWithArea_ExactMatch()
        {
            // Arrange
            var selector = CreateSelector();
            var context = CreateContext(new { controller = "Home", action = "Index", area = "Admin" });

            context.ProvidedValues = new RouteValueDictionary(new { area = "Admin", controller = "Home", action = "Index" });

            // Act
            var isValid = selector.IsValidAction(context);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValidAction_InArea_ImplicitLeaveArea()
        {
            // Arrange
            var selector = CreateSelector();
            var context = CreateContext(new { controller = "Home", action = "Edit" }, new { area = "Admin" });

            context.ProvidedValues = new RouteValueDictionary(new { controller = "Home", action = "Edit" });

            // Act
            var isValid = selector.IsValidAction(context);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValidAction_InArea_ImplicitLeaveArea_AreaRoute_NoMatch()
        {
            // Arrange
            var selector = CreateSelector();
            var context = CreateContext(new { controller = "Home", action = "Edit" }, new { area = "Admin" });

            // This won't match because the action doesn't exist
            context.ProvidedValues = new RouteValueDictionary(new { area = "Admin", controller = "Home", action = "Edit" });

            // Act
            var isValid = selector.IsValidAction(context);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValidAction_InArea_ExplicitLeaveArea()
        {
            // Arrange
            var selector = CreateSelector();
            var context = CreateContext(new { area = (string) null, controller = "Home", action = "Index" }, new { area = "Admin" });

            context.ProvidedValues = new RouteValueDictionary(new { controller = "Home", action = "Index" });

            // Act
            var isValid = selector.IsValidAction(context);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValidAction_InArea_StayInArea()
        {
            // Arrange
            var selector = CreateSelector();
            var context = CreateContext(new { action = "Diagnostics" }, new { area = "Admin", controller = "Home", action = "Index" });

            context.ProvidedValues = new RouteValueDictionary(new { area = "Admin", controller = "Home", action = "Diagnostics" });

            // Act
            var isValid = selector.IsValidAction(context);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValidAction_InRPCController_ImplictREST()
        {
            // Arrange
            var selector = CreateSelector();
            var context = CreateContext(new { controller = "Product" }, new { controller = "Home", action = "Index" });

            context.ProvidedValues = new RouteValueDictionary(new { controller = "Product" });

            // Act
            var isValid = selector.IsValidAction(context);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValidAction_InRPCController_ExplicitREST()
        {
            // Arrange
            var selector = CreateSelector();
            var context = CreateContext(new { controller = "Product", action = (string)null }, new { controller = "Home", action = "Index" });

            context.ProvidedValues = new RouteValueDictionary(new { controller = "Product" });

            // Act
            var isValid = selector.IsValidAction(context);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValidAction_InRESTController_StayInREST()
        {
            // Arrange
            var selector = CreateSelector();
            var context = CreateContext(new { action = (string)null }, new { controller = "Product" });

            context.ProvidedValues = new RouteValueDictionary(new { controller = "Product" });

            // Act
            var isValid = selector.IsValidAction(context);

            // Assert
            Assert.True(isValid);
        }

        private static DefaultActionSelector CreateSelector()
        {
            var actionProvider = new Mock<INestedProviderManager<ActionDescriptorProviderContext>>(MockBehavior.Strict);
            actionProvider
                .Setup(p => p.Invoke(It.IsAny<ActionDescriptorProviderContext>()))
                .Callback<ActionDescriptorProviderContext>(c => c.Results.AddRange(Actions));

            var bindingProvider = new Mock<IActionBindingContextProvider>(MockBehavior.Strict);

            return new DefaultActionSelector(actionProvider.Object, bindingProvider.Object);
        }

        private static VirtualPathContext CreateContext(object values)
        {
            return CreateContext(values, ambientValues: null);
        }

        private static VirtualPathContext CreateContext(object values, object ambientValues)
        {
            var httpContext = new Mock<HttpContext>(MockBehavior.Strict);

            return new VirtualPathContext(
                httpContext.Object,
                new RouteValueDictionary(ambientValues),
                new RouteValueDictionary(values));
        }

        private static ActionDescriptor CreateAction(string area, string controller, string action)
        {
            var actionDescriptor = new ActionDescriptor()
            {
                Name = string.Format("Area: {0}, Controller: {1}, Action: {2}", area, controller, action),
                RouteConstraints = new List<RouteDataActionConstraint>(),
            };

            actionDescriptor.RouteConstraints.Add(
                area == null ? 
                new RouteDataActionConstraint("area", RouteKeyHandling.DenyKey) : 
                new RouteDataActionConstraint("area", area));

            actionDescriptor.RouteConstraints.Add(
                controller == null ? 
                new RouteDataActionConstraint("controller", RouteKeyHandling.DenyKey) : 
                new RouteDataActionConstraint("controller", controller));

            actionDescriptor.RouteConstraints.Add(
                action == null ? 
                new RouteDataActionConstraint("action", RouteKeyHandling.DenyKey) : 
                new RouteDataActionConstraint("action", action));

            return actionDescriptor;
        }
    }
}
