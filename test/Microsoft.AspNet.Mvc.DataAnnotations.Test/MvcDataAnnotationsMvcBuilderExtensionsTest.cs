// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.DataAnnotations.Internal;
using Microsoft.AspNet.Mvc.Localization;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Localization;
using Microsoft.Framework.OptionsModel;
using Xunit;

namespace Microsoft.AspNet.Mvc.DataAnnotations.Test
{
    public class DataAnnotationsLocalizationServiceTest
    {
        [Fact]
        public void AddDataAnnotationsLocalizationServices_AddsNeededServices()
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            DataAnnotationsLocalizationService.AddDataAnnotationsLocalizationServices(collection, setupAction: null);

            // Assert
            Assert.Collection(collection,
                service =>
                {
                    Assert.Equal(typeof(IStringLocalizerFactory), service.ServiceType);
                    Assert.Equal(typeof(ResourceManagerStringLocalizerFactory), service.ImplementationType);
                    Assert.Equal(ServiceLifetime.Singleton, service.Lifetime);
                },
                service =>
                {
                    Assert.Equal(typeof(IStringLocalizer<>), service.ServiceType);
                    Assert.Equal(typeof(StringLocalizer<>), service.ImplementationType);
                    Assert.Equal(ServiceLifetime.Transient, service.Lifetime);
                },
                service =>
                {
                    Assert.Equal(typeof(IOptions<>), service.ServiceType);
                    Assert.Equal(ServiceLifetime.Singleton, service.Lifetime);
                },
                service =>
                {
                    Assert.Equal(typeof(IConfigureOptions<MvcDataAnnotationsLocalizationOptions>), service.ServiceType);
                    Assert.Equal(ServiceLifetime.Transient, service.Lifetime);
                });
        }

        [Fact]
        public void AddDataAnnotationsLocalizationServicesWithOptios_AddsNeededServices()
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            DataAnnotationsLocalizationService.AddDataAnnotationsLocalizationServices(
                collection,
                options => options.DataAnnotationLocalizerProvider = (modelType, stringLocalizerFactory) =>
                    stringLocalizerFactory.Create("test", string.Empty));

            // Assert
            Assert.Collection(collection,
                service =>
                {
                    Assert.Equal(typeof(IStringLocalizerFactory), service.ServiceType);
                    Assert.Equal(typeof(ResourceManagerStringLocalizerFactory), service.ImplementationType);
                    Assert.Equal(ServiceLifetime.Singleton, service.Lifetime);
                },
                service =>
                {
                    Assert.Equal(typeof(IStringLocalizer<>), service.ServiceType);
                    Assert.Equal(typeof(StringLocalizer<>), service.ImplementationType);
                    Assert.Equal(ServiceLifetime.Transient, service.Lifetime);
                },
                service =>
                {
                    Assert.Equal(typeof(IOptions<>), service.ServiceType);
                    Assert.Equal(ServiceLifetime.Singleton, service.Lifetime);
                },
                service =>
                {
                    Assert.Equal(typeof(IConfigureOptions<MvcDataAnnotationsLocalizationOptions>), service.ServiceType);
                    Assert.Equal(ServiceLifetime.Singleton, service.Lifetime);
                });
        }
    }
}
