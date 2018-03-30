// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Cors.Internal
{
    public class CorsApplicationModelProviderTest
    {
        [Fact]
        public void CreateControllerModel_EnableCorsAttributeAddsCorsAuthorizationFilterFactory()
        {
            // Arrange
            var corsProvider = new CorsApplicationModelProvider();
            var context = GetProviderContext(typeof(CorsController));

            // Act
            corsProvider.OnProvidersExecuting(context);

            // Assert
            var model = Assert.Single(context.Result.Controllers);
            Assert.Single(model.Filters, f => f is CorsAuthorizationFilterFactory);
            var action = Assert.Single(model.Actions);
            var selector = Assert.Single(action.Selectors);
            var constraint = Assert.Single(selector.ActionConstraints, c => c is HttpMethodActionConstraint);
            Assert.IsType<CorsHttpMethodActionConstraint>(constraint);
        }

        [Fact]
        public void CreateControllerModel_DisableCorsAttributeAddsDisableCorsAuthorizationFilter()
        {
            // Arrange
            var corsProvider = new CorsApplicationModelProvider();
            var context = GetProviderContext(typeof(DisableCorsController));

            // Act
            corsProvider.OnProvidersExecuting(context);

            // Assert
            var model = Assert.Single(context.Result.Controllers);
            Assert.Single(model.Filters, f => f is DisableCorsAuthorizationFilter);
            var action = Assert.Single(model.Actions);
            var selector = Assert.Single(action.Selectors);
            var constraint = Assert.Single(selector.ActionConstraints, c => c is HttpMethodActionConstraint);
            Assert.IsType<CorsHttpMethodActionConstraint>(constraint);
        }

        [Fact]
        public void CreateControllerModel_CustomCorsFilter_ReplacesHttpConstraints()
        {
            // Arrange
            var corsProvider = new CorsApplicationModelProvider();
            var context = GetProviderContext(typeof(CustomCorsFilterController));

            // Act
            corsProvider.OnProvidersExecuting(context);

            // Assert
            var controller = Assert.Single(context.Result.Controllers);
            var action = Assert.Single(controller.Actions);
            var selector = Assert.Single(action.Selectors);
            var constraint = Assert.Single(selector.ActionConstraints, c => c is HttpMethodActionConstraint);
            Assert.IsType<CorsHttpMethodActionConstraint>(constraint);
        }

        [Fact]
        public void BuildActionModel_EnableCorsAttributeAddsCorsAuthorizationFilterFactory()
        {
            // Arrange
            var corsProvider = new CorsApplicationModelProvider();
            var context = GetProviderContext(typeof(EnableCorsController));

            // Act
            corsProvider.OnProvidersExecuting(context);

            // Assert
            var controller = Assert.Single(context.Result.Controllers);
            var action = Assert.Single(controller.Actions);
            Assert.Single(action.Filters, f => f is CorsAuthorizationFilterFactory);
            var selector = Assert.Single(action.Selectors);
            var constraint = Assert.Single(selector.ActionConstraints, c => c is HttpMethodActionConstraint);
            Assert.IsType<CorsHttpMethodActionConstraint>(constraint);
        }

        [Fact]
        public void BuildActionModel_DisableCorsAttributeAddsDisableCorsAuthorizationFilter()
        {
            // Arrange
            var corsProvider = new CorsApplicationModelProvider();
            var context = GetProviderContext(typeof(DisableCorsActionController));

            // Act
            corsProvider.OnProvidersExecuting(context);

            // Assert
            var controller = Assert.Single(context.Result.Controllers);
            var action = Assert.Single(controller.Actions);
            Assert.Contains(action.Filters, f => f is DisableCorsAuthorizationFilter);
            var selector = Assert.Single(action.Selectors);
            var constraint = Assert.Single(selector.ActionConstraints, c => c is HttpMethodActionConstraint);
            Assert.IsType<CorsHttpMethodActionConstraint>(constraint);
        }

        [Fact]
        public void BuildActionModel_CustomCorsAuthorizationFilterOnAction_ReplacesHttpConstraints()
        {
            // Arrange
            var corsProvider = new CorsApplicationModelProvider();
            var context = GetProviderContext(typeof(CustomCorsFilterOnActionController));

            // Act
            corsProvider.OnProvidersExecuting(context);

            // Assert
            var controller = Assert.Single(context.Result.Controllers);
            var action = Assert.Single(controller.Actions);
            var selector = Assert.Single(action.Selectors);
            var constraint = Assert.Single(selector.ActionConstraints, c => c is HttpMethodActionConstraint);
            Assert.IsType<CorsHttpMethodActionConstraint>(constraint);
        }

        [Fact]
        public void CreateControllerModel_EnableCorsGloballyReplacesHttpMethodConstraints()
        {
            // Arrange
            var corsProvider = new CorsApplicationModelProvider();
            var context = GetProviderContext(typeof(RegularController));

            context.Result.Filters.Add(
                new CorsAuthorizationFilter(Mock.Of<ICorsService>(), Mock.Of<ICorsPolicyProvider>(), Mock.Of<ILoggerFactory>()));

            // Act
            corsProvider.OnProvidersExecuting(context);

            // Assert
            var model = Assert.Single(context.Result.Controllers);
            var action = Assert.Single(model.Actions);
            var selector = Assert.Single(action.Selectors);
            var constraint = Assert.Single(selector.ActionConstraints, c => c is HttpMethodActionConstraint);
            Assert.IsType<CorsHttpMethodActionConstraint>(constraint);
        }

        [Fact]
        public void CreateControllerModel_DisableCorsGloballyReplacesHttpMethodConstraints()
        {
            // Arrange
            var corsProvider = new CorsApplicationModelProvider();
            var context = GetProviderContext(typeof(RegularController));
            context.Result.Filters.Add(new DisableCorsAuthorizationFilter());

            // Act
            corsProvider.OnProvidersExecuting(context);

            // Assert
            var model = Assert.Single(context.Result.Controllers);
            var action = Assert.Single(model.Actions);
            var selector = Assert.Single(action.Selectors);
            var constraint = Assert.Single(selector.ActionConstraints, c => c is HttpMethodActionConstraint);
            Assert.IsType<CorsHttpMethodActionConstraint>(constraint);
        }

        [Fact]
        public void CreateControllerModel_CustomCorsFilterGloballyReplacesHttpMethodConstraints()
        {
            // Arrange
            var corsProvider = new CorsApplicationModelProvider();
            var context = GetProviderContext(typeof(RegularController));
            context.Result.Filters.Add(new CustomCorsFilterAttribute());

            // Act
            corsProvider.OnProvidersExecuting(context);

            // Assert
            var model = Assert.Single(context.Result.Controllers);
            var action = Assert.Single(model.Actions);
            var selector = Assert.Single(action.Selectors);
            var constraint = Assert.Single(selector.ActionConstraints, c => c is HttpMethodActionConstraint);
            Assert.IsType<CorsHttpMethodActionConstraint>(constraint);
        }

        [Fact]
        public void CreateControllerModel_CorsNotInUseDoesNotOverrideHttpConstraints()
        {
            // Arrange
            var corsProvider = new CorsApplicationModelProvider();
            var context = GetProviderContext(typeof(RegularController));

            // Act
            corsProvider.OnProvidersExecuting(context);

            // Assert
            var model = Assert.Single(context.Result.Controllers);
            var action = Assert.Single(model.Actions);
            var selector = Assert.Single(action.Selectors);
            var constraint = Assert.Single(selector.ActionConstraints, c => c is HttpMethodActionConstraint);
            Assert.IsNotType<CorsHttpMethodActionConstraint>(constraint);
        }

        private static ApplicationModelProviderContext GetProviderContext(Type controllerType)
        {
            var context = new ApplicationModelProviderContext(new[] { controllerType.GetTypeInfo() });
            var provider = new DefaultApplicationModelProvider(
                Options.Create(new MvcOptions()),
                TestModelMetadataProvider.CreateDefaultProvider());
            provider.OnProvidersExecuting(context);

            return context;
        }

        private class EnableCorsController
        {
            [EnableCors("policy")]
            [HttpGet]
            public IActionResult Action()
            {
                return null;
            }
        }

        private class DisableCorsActionController
        {
            [DisableCors]
            [HttpGet]
            public void Action()
            {
            }
        }

        [EnableCors("policy")]
        public class CorsController
        {
            [HttpGet]
            public IActionResult Action()
            {
                return null;
            }
        }

        [DisableCors]
        public class DisableCorsController
        {
            [HttpOptions]
            public IActionResult Action()
            {
                return null;
            }
        }

        public class RegularController
        {
            [HttpPost]
            public IActionResult Action()
            {
                return null;
            }
        }

        [CustomCorsFilter]
        public class CustomCorsFilterController
        {
            [HttpPost]
            public IActionResult Action()
            {
                return null;
            }
        }

        public class CustomCorsFilterOnActionController
        {
            [HttpPost]
            [CustomCorsFilter]
            public IActionResult Action()
            {
                return null;
            }
        }

        public class CustomCorsFilterAttribute : Attribute, ICorsAuthorizationFilter
        {
            public int Order { get; } = 1000;

            public Task OnAuthorizationAsync(AuthorizationFilterContext context)
            {
                return Task.FromResult(0);
            }
        }
    }
}