﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Cors;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters.Json;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class MvcServiceCollectionExtensionsTest
    {
        // Some MVC services can be registered multiple times, for example, 'IConfigureOptions<MvcOptions>' can
        // be registered by calling 'ConfigureMvc(...)' before the call to 'AddMvc()' in which case the options
        // configuration is run in the order they were registered.
        //
        // For these kind of multi registration service types, we want to make sure that MVC will still add its
        // services if the implementation type is different.
        [Fact]
        public void MultiRegistrationServiceTypes_AreRegistered_MultipleTimes()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IHostingEnvironment>(GetHostingEnvironment());

            // Register a mock implementation of each service, AddMvcServices should add another implementation.
            foreach (var serviceType in MultiRegistrationServiceTypes)
            {
                var mockType = typeof(Mock<>).MakeGenericType(serviceType.Key);
                services.Add(ServiceDescriptor.Transient(serviceType.Key, mockType));
            }

            // Act
            services.AddMvc();

            // Assert
            foreach (var serviceType in MultiRegistrationServiceTypes)
            {
                AssertServiceCountEquals(services, serviceType.Key, serviceType.Value.Length + 1);

                foreach (var implementationType in serviceType.Value)
                {
                    AssertContainsSingle(services, serviceType.Key, implementationType);
                }
            }
        }

        [Fact]
        public void SingleRegistrationServiceTypes_AreNotRegistered_MultipleTimes()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IHostingEnvironment>(GetHostingEnvironment());

            // Register a mock implementation of each service, AddMvcServices should not replace it.
            foreach (var serviceType in SingleRegistrationServiceTypes)
            {
                var mockType = typeof(Mock<>).MakeGenericType(serviceType);
                services.Add(ServiceDescriptor.Transient(serviceType, mockType));
            }

            // Act
            services.AddMvc();

            // Assert
            foreach (var singleRegistrationType in SingleRegistrationServiceTypes)
            {
                AssertServiceCountEquals(services, singleRegistrationType, 1);
            }
        }

        [Fact]
        public void AddMvcServicesTwice_DoesNotAddDuplicates()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IHostingEnvironment>(GetHostingEnvironment());

            // Act
            services.AddMvc();
            services.AddMvc();

            // Assert
            var singleRegistrationServiceTypes = SingleRegistrationServiceTypes;
            foreach (var service in services)
            {
                if (singleRegistrationServiceTypes.Contains(service.ServiceType))
                {
                    // 'single-registration' services should only have one implementation registered.
                    AssertServiceCountEquals(services, service.ServiceType, 1);
                }
                else if (service.ImplementationType != null && !service.ImplementationType.GetTypeInfo().Assembly.FullName.Contains("Mvc"))
                {
                    // Ignore types that don't come from MVC
                }
                else
                {
                    // 'multi-registration' services should only have one *instance* of each implementation registered.
                    AssertContainsSingle(services, service.ServiceType, service.ImplementationType);
                }
            }
        }

        [Fact]
        public void AddMvc_AddsAssemblyPartsForFrameworkTagHelpers()
        {
            // Arrange
            var mvcRazorAssembly = typeof(UrlResolutionTagHelper).GetTypeInfo().Assembly;
            var mvcTagHelpersAssembly = typeof(InputTagHelper).GetTypeInfo().Assembly;
            var services = new ServiceCollection();
            var providers = new IApplicationFeatureProvider[]
            {
                new ControllerFeatureProvider(),
                new ViewComponentFeatureProvider()
            };

            // Act
            services.AddMvc();

            // Assert
            var descriptor = Assert.Single(services, d => d.ServiceType == typeof(ApplicationPartManager));
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
            Assert.NotNull(descriptor.ImplementationInstance);
            var manager = Assert.IsType<ApplicationPartManager>(descriptor.ImplementationInstance);

            Assert.Equal(2, manager.ApplicationParts.Count);
            Assert.Single(manager.ApplicationParts.OfType<AssemblyPart>(), p => p.Assembly == mvcRazorAssembly);
            Assert.Single(manager.ApplicationParts.OfType<AssemblyPart>(), p => p.Assembly == mvcTagHelpersAssembly);
        }

        [Fact]
        public void AddMvcTwice_DoesNotAddDuplicateFrameworkParts()
        {
            // Arrange
            var mvcRazorAssembly = typeof(UrlResolutionTagHelper).GetTypeInfo().Assembly;
            var mvcTagHelpersAssembly = typeof(InputTagHelper).GetTypeInfo().Assembly;
            var services = new ServiceCollection();
            var providers = new IApplicationFeatureProvider[]
            {
                new ControllerFeatureProvider(),
                new ViewComponentFeatureProvider()
            };

            // Act
            services.AddMvc();
            services.AddMvc();

            // Assert
            var descriptor = Assert.Single(services, d => d.ServiceType == typeof(ApplicationPartManager));
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
            Assert.NotNull(descriptor.ImplementationInstance);
            var manager = Assert.IsType<ApplicationPartManager>(descriptor.ImplementationInstance);

            Assert.Equal(2, manager.ApplicationParts.Count);
            Assert.Single(manager.ApplicationParts.OfType<AssemblyPart>(), p => p.Assembly == mvcRazorAssembly);
            Assert.Single(manager.ApplicationParts.OfType<AssemblyPart>(), p => p.Assembly == mvcTagHelpersAssembly);
        }

        [Fact]
        public void AddMvcTwice_DoesNotAddApplicationFeatureProvidersTwice()
        {
            // Arrange
            var services = new ServiceCollection();
            var providers = new IApplicationFeatureProvider[]
            {
                new ControllerFeatureProvider(),
                new ViewComponentFeatureProvider()
            };

            // Act
            services.AddMvc();
            services.AddMvc();

            // Assert
            var descriptor = Assert.Single(services, d => d.ServiceType == typeof(ApplicationPartManager));
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
            Assert.NotNull(descriptor.ImplementationInstance);
            var manager = Assert.IsType<ApplicationPartManager>(descriptor.ImplementationInstance);

            Assert.Collection(manager.FeatureProviders,
                feature => Assert.IsType<ControllerFeatureProvider>(feature),
                feature => Assert.IsType<ViewComponentFeatureProvider>(feature),
                feature => Assert.IsType<TagHelperFeatureProvider>(feature),
                feature => Assert.IsType<RazorCompiledItemFeatureProvider>(feature),
#pragma warning disable CS0618 // Type or member is obsolete
                feature => Assert.IsType<ViewsFeatureProvider>(feature));
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [Fact]
        public void AddMvcCore_ReusesExistingApplicationPartManagerInstance_IfFoundOnServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var manager = new ApplicationPartManager();
            services.AddSingleton(manager);

            // Act
            services.AddMvc();

            // Assert
            var descriptor = Assert.Single(services, d => d.ServiceType == typeof(ApplicationPartManager));
            Assert.Same(manager, descriptor.ImplementationInstance);
        }

        [Fact]
        public void AddMvcCore_AddsMvcJsonOption()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddMvcCore()
                .AddJsonOptions((options) =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            // Assert
            Assert.Single(services, d => d.ServiceType == typeof(IConfigureOptions<MvcJsonOptions>));
        }

        [Fact]
        public void AddMvc_NoScopedServiceIsReferredToByASingleton()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddSingleton<IHostingEnvironment>(GetHostingEnvironment());
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            var diagnosticListener = new DiagnosticListener("Microsoft.AspNet");
            services.AddSingleton<DiagnosticSource>(diagnosticListener);
            services.AddSingleton<DiagnosticListener>(diagnosticListener);
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            services.AddLogging();
            services.AddOptions();
            services.AddMvc();

            var root = services.BuildServiceProvider(validateScopes: true);

            var scopeFactory = root.GetRequiredService<IServiceScopeFactory>();

            // Act & Assert
            using (var scope = scopeFactory.CreateScope())
            {
                foreach (var serviceType in services.Select(d => d.ServiceType).Where(t => !t.GetTypeInfo().IsGenericTypeDefinition).Distinct())
                {
                    // This will throw if something is invalid.
                    scope.ServiceProvider.GetService(typeof(IEnumerable<>).MakeGenericType(serviceType));
                }
            }
        }

        [Fact]
        public void AddMvc_RegistersExpectedTempDataProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddMvc();

            // Assert
            var descriptor = Assert.Single(services, item => item.ServiceType == typeof(ITempDataProvider));
            Assert.Equal(typeof(CookieTempDataProvider), descriptor.ImplementationType);
        }

        [Fact]
        public void AddMvc_DoesNotRegisterCookieTempDataOptionsConfiguration()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var builder = services.AddMvc();

            // Assert
            Assert.DoesNotContain(
                services,
                item => item.ServiceType == typeof(IConfigureOptions<CookieTempDataProviderOptions>));
        }

        private IEnumerable<Type> SingleRegistrationServiceTypes
        {
            get
            {
                var services = new ServiceCollection();
                services.AddSingleton<IHostingEnvironment>(GetHostingEnvironment());
                services.AddMvc();

                var multiRegistrationServiceTypes = MultiRegistrationServiceTypes;
                return services
                    .Where(sd => !multiRegistrationServiceTypes.Keys.Contains(sd.ServiceType))
                    .Where(sd => sd.ServiceType.GetTypeInfo().Assembly.FullName.Contains("Mvc"))
                    .Select(sd => sd.ServiceType);
            }
        }

        private Dictionary<Type, Type[]> MultiRegistrationServiceTypes
        {
            get
            {
                return new Dictionary<Type, Type[]>()
                {
                    {
                        typeof(IConfigureOptions<MvcOptions>),
                        new Type[]
                        {
                            typeof(MvcCoreMvcOptionsSetup),
                            typeof(MvcDataAnnotationsMvcOptionsSetup),
                            typeof(MvcJsonMvcOptionsSetup),
                            typeof(TempDataMvcOptionsSetup),
                        }
                    },
                    {
                        typeof(IConfigureOptions<RouteOptions>),
                        new Type[]
                        {
                            typeof(MvcCoreRouteOptionsSetup),
                            typeof(MvcCoreRouteOptionsSetup),
                        }
                    },
                    {
                        typeof(IConfigureOptions<ApiBehaviorOptions>),
                        new Type[]
                        {
                            typeof(ApiBehaviorOptionsSetup),
                        }
                    },
                    {
                        typeof(IConfigureOptions<MvcViewOptions>),
                        new Type[]
                        {
                            typeof(MvcViewOptionsSetup),
                            typeof(MvcRazorMvcViewOptionsSetup),
                        }
                    },
                    {
                        typeof(IConfigureOptions<RazorViewEngineOptions>),
                        new[]
                        {
                            typeof(RazorViewEngineOptionsSetup),
                            typeof(RazorPagesRazorViewEngineOptionsSetup),
                        }
                    },
                    {
                        typeof(IPostConfigureOptions<MvcOptions>),
                        new[]
                        {
                            typeof(MvcOptionsConfigureCompatibilityOptions),
                            typeof(MvcCoreMvcOptionsSetup),
                        }
                    },
                    {
                        typeof(IPostConfigureOptions<RazorPagesOptions>),
                        new[]
                        {
                            typeof(RazorPagesOptionsConfigureCompatibilityOptions),
                        }
                    },
                    {
                        typeof(IPostConfigureOptions<MvcJsonOptions>),
                        new[]
                        {
                            typeof(MvcJsonOptionsConfigureCompatibilityOptions),
                        }
                    },
                    {
                        typeof(IActionConstraintProvider),
                        new Type[]
                        {
                            typeof(DefaultActionConstraintProvider),
                        }
                    },
                    {
                        typeof(IActionDescriptorProvider),
                        new Type[]
                        {
                            typeof(ControllerActionDescriptorProvider),
                            typeof(PageActionDescriptorProvider),
                        }
                    },
                    {
                        typeof(IActionInvokerProvider),
                        new Type[]
                        {
                            typeof(ControllerActionInvokerProvider),
                            typeof(PageActionInvokerProvider),
                        }
                    },
                    {
                        typeof(IFilterProvider),
                        new Type[]
                        {
                            typeof(DefaultFilterProvider),
                        }
                    },
                    {
                        typeof(IControllerPropertyActivator),
                        new Type[]
                        {
                            typeof(DefaultControllerPropertyActivator),
                            typeof(ViewDataDictionaryControllerPropertyActivator),
                        }
                    },
                    {
                        typeof(IApplicationModelProvider),
                        new Type[]
                        {
                            typeof(DefaultApplicationModelProvider),
                            typeof(CorsApplicationModelProvider),
                            typeof(AuthorizationApplicationModelProvider),
                            typeof(TempDataApplicationModelProvider),
                            typeof(ViewDataAttributeApplicationModelProvider),
                            typeof(ApiBehaviorApplicationModelProvider),
                        }
                    },
                    {
                        typeof(IApiDescriptionProvider),
                        new Type[]
                        {
                            typeof(DefaultApiDescriptionProvider),
                            typeof(JsonPatchOperationsArrayProvider),
                        }
                    },
                    {
                        typeof(IPageRouteModelProvider),
                        new[]
                        {
                            typeof(CompiledPageRouteModelProvider),
                        }
                    },
                    {
                        typeof(IPageApplicationModelProvider),
                        new[]
                        {
                            typeof(AuthorizationPageApplicationModelProvider),
                            typeof(AuthorizationPageApplicationModelProvider),
                            typeof(DefaultPageApplicationModelProvider),
                            typeof(TempDataFilterPageApplicationModelProvider),
                            typeof(ViewDataAttributePageApplicationModelProvider),
                            typeof(ResponseCacheFilterApplicationModelProvider),
                        }
                    },
                };
            }
        }

        private void AssertServiceCountEquals(
            IServiceCollection services,
            Type serviceType,
            int expectedServiceRegistrationCount)
        {
            var serviceDescriptors = services.Where(serviceDescriptor => serviceDescriptor.ServiceType == serviceType);
            var actual = serviceDescriptors.Count();

            Assert.True(
                (expectedServiceRegistrationCount == actual),
                $"Expected service type '{serviceType}' to be registered {expectedServiceRegistrationCount}" +
                $" time(s) but was actually registered {actual} time(s).");
        }

        private void AssertContainsSingle(
            IServiceCollection services,
            Type serviceType,
            Type implementationType)
        {
            var matches = services
                .Where(sd =>
                    sd.ServiceType == serviceType &&
                    sd.ImplementationType == implementationType)
                .ToArray();

            if (matches.Length == 0)
            {
                Assert.True(
                    false,
                    $"Could not find an instance of {implementationType} registered as {serviceType}");
            }
            else if (matches.Length > 1)
            {
                Assert.True(
                    false,
                    $"Found multiple instances of {implementationType} registered as {serviceType}");
            }
        }

        private IHostingEnvironment GetHostingEnvironment()
        {
            var environment = new Mock<IHostingEnvironment>();
            environment
                .Setup(e => e.ApplicationName)
                .Returns(typeof(MvcServiceCollectionExtensionsTest).GetTypeInfo().Assembly.GetName().Name);

            return environment.Object;
        }
    }
}
