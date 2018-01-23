// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.WebEncoders.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Localization.Internal
{
    public class MvcLocalizationServicesTest
    {
        private readonly static HtmlTestEncoder HtmlEncoderInstance = new HtmlTestEncoder(); 

        private readonly IDictionary<Type, Tuple<object, ServiceLifetime>> ExpectedServices = new Dictionary<Type, Tuple<object, ServiceLifetime>> {
            { typeof(IHtmlLocalizerFactory), new Tuple<object, ServiceLifetime>(typeof(HtmlLocalizerFactory), ServiceLifetime.Singleton)},
            { typeof(IHtmlLocalizer<>),  new Tuple<object, ServiceLifetime>(typeof(HtmlLocalizer<>), ServiceLifetime.Transient)},
            { typeof(IViewLocalizer), new Tuple<object, ServiceLifetime>(typeof(ViewLocalizer), ServiceLifetime.Transient) },
            { typeof(IConfigureOptions<RazorViewEngineOptions>), new Tuple<object, ServiceLifetime>(null, ServiceLifetime.Singleton) }
        };

        [Fact]
        public void AddLocalizationServices_AddsNeededServices()
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            MvcLocalizationServices.AddMvcViewLocalizationServices(
                collection,
                LanguageViewLocationExpanderFormat.Suffix,
                setupAction: null);

            // Assert
            AssertContainsExpected(collection, ExpectedServices);
        }

        [Fact]
        public void AddCustomLocalizers_BeforeAddLocalizationServices_AddsNeededServices()
        {
            // Arrange
            var collection = new ServiceCollection();
            var testEncoder = HtmlEncoderInstance;

            // Act
            collection.Add(ServiceDescriptor.Singleton(typeof(IHtmlLocalizerFactory), typeof(TestHtmlLocalizerFactory)));
            collection.Add(ServiceDescriptor.Transient(typeof(IHtmlLocalizer<>), typeof(TestHtmlLocalizer<>)));
            collection.Add(ServiceDescriptor.Transient(typeof(IViewLocalizer), typeof(TestViewLocalizer)));
            collection.Add(ServiceDescriptor.Singleton(typeof(HtmlEncoder), testEncoder));

            MvcLocalizationServices.AddMvcViewLocalizationServices(
                collection,
                LanguageViewLocationExpanderFormat.Suffix,
                setupAction: null);

            var expected = ExpectedServices;
            expected[typeof(IHtmlLocalizerFactory)] = new Tuple<object, ServiceLifetime>(typeof(TestHtmlLocalizerFactory), ServiceLifetime.Singleton);
            expected[typeof(IHtmlLocalizer<>)] = new Tuple<object, ServiceLifetime>(typeof(TestHtmlLocalizer<>), ServiceLifetime.Transient);
            expected[typeof(IViewLocalizer)] = new Tuple<object, ServiceLifetime>(typeof(TestViewLocalizer), ServiceLifetime.Transient);
            expected.Add(typeof(HtmlEncoder), new Tuple<object, ServiceLifetime>(HtmlEncoderInstance, ServiceLifetime.Singleton));

            // Assert
            AssertContainsExpected(collection, expected);
        }

        [Fact]
        public void AddCustomLocalizers_AfterAddLocalizationServices_AddsNeededServices()
        {
            // Arrange
            var collection = new ServiceCollection();
            var htmlEncoder = HtmlEncoderInstance;

            collection.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new CustomPartialDirectoryViewLocationExpander());
            });

            // Act
            MvcLocalizationServices.AddMvcViewLocalizationServices(
                collection,
                LanguageViewLocationExpanderFormat.Suffix,
                setupAction: null);


            collection.Add(ServiceDescriptor.Transient(typeof(IHtmlLocalizer<>), typeof(TestHtmlLocalizer<>)));
            collection.Add(ServiceDescriptor.Transient(typeof(IHtmlLocalizer), typeof(TestViewLocalizer)));
            collection.Add(ServiceDescriptor.Singleton(typeof(HtmlEncoder), htmlEncoder));

            var expected = ExpectedServices;

            expected.Add(typeof(HtmlEncoder), new Tuple<object, ServiceLifetime>(HtmlEncoderInstance, ServiceLifetime.Singleton));
            expected[typeof(IConfigureOptions<RazorViewEngineOptions>)] = new Tuple<object, ServiceLifetime>(2, ServiceLifetime.Singleton);

            // Assert
            AssertContainsExpected(collection, ExpectedServices);
        }

        [Fact]
        public void AddLocalizationServicesWithLocalizationOptions_AddsNeededServices()
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            MvcLocalizationServices.AddMvcViewLocalizationServices(
                collection,
                LanguageViewLocationExpanderFormat.Suffix,
                options => options.ResourcesPath = "Resources");

            // Assert
            AssertContainsExpected(collection, ExpectedServices);
        }

        private static void AssertContainsExpected(IServiceCollection services, IDictionary<Type, Tuple<object, ServiceLifetime>> expectedServices)
        {
            foreach(var expectedService in expectedServices)
            {
                var implementationType = expectedService.Value.Item1 as Type;
                var expectedCount = expectedService.Value.Item1 as int?;
                if(implementationType != null)
                {
                    AssertContainsSingleType(services, expectedService.Key, implementationType, expectedService.Value.Item2);
                }
                else if(expectedCount != null)
                {
                    AssertContainsMultiple(services, expectedService.Key, expectedCount.Value, expectedService.Value.Item2);
                }
                else
                {
                    AssertContainsSingleInstance(services, expectedService.Key, expectedService.Value.Item1, expectedService.Value.Item2);
                }
            }
        }

        private static void AssertContainsMultiple(
            IServiceCollection services,
            Type serviceType,
            int expectedCount,
            ServiceLifetime lifetime)
        {
            var matches = services
                .Where(sd =>
                    sd.ServiceType == serviceType &&
                    sd.Lifetime == lifetime)
                .ToArray();

            if (matches.Length != expectedCount)
            {
                Assert.True(
                    false,
                    $"Found {matches.Length} instances registered as {serviceType} with {lifetime} lifetime but expected {expectedCount}.");
            }
        }

        private static void AssertContainsSingleInstance(
            IServiceCollection services,
            Type serviceType,
            object implementationInstance,
            ServiceLifetime lifetime)
        {
            var matches = services
                .Where(sd =>
                    sd.ServiceType == serviceType &&
                    (sd.ImplementationInstance == implementationInstance || implementationInstance == null) &&
                    sd.Lifetime == lifetime)
                .ToArray();

            if (matches.Length == 0)
            {
                Assert.True(
                    false,
                    $"Could not find the instance of {implementationInstance?.GetType().ToString() ?? "the implementation"} registered as {serviceType} with {lifetime} lifetime.");
            }
            else if (matches.Length > 1)
            {
                Assert.True(
                    false,
                    $"Found multiple instances of {implementationInstance?.GetType().ToString() ?? "the implementation"} registered as {serviceType} with {lifetime} lifetime.");
            }
        }

        private static void AssertContainsSingleType(
            IServiceCollection services,
            Type serviceType,
            Type implementationType,
            ServiceLifetime lifetime)
        {
            var matches = services
                .Where(sd =>
                    sd.ServiceType == serviceType &&
                    sd.ImplementationType == implementationType &&
                    sd.Lifetime == lifetime)
                .ToArray();

            if (matches.Length == 0)
            {
                Assert.True(
                    false,
                    $"Could not find an instance of {implementationType} registered as {serviceType} with {lifetime} lifetime.");
            }
            else if (matches.Length > 1)
            {
                Assert.True(
                    false,
                    $"Found multiple instances of {implementationType} registered as {serviceType} with {lifetime} lifetime.");
            }
        }
    }

    public class TestViewLocalizer : IViewLocalizer
    {
        public LocalizedHtmlString this[string name]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public LocalizedHtmlString this[string name, params object[] arguments]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public LocalizedString GetString(string name)
        {
            throw new NotImplementedException();
        }

        public LocalizedString GetString(string name, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            throw new NotImplementedException();
        }

        public IHtmlLocalizer WithCulture(CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TestHtmlLocalizer<HomeController> : IHtmlLocalizer<HomeController>
    {
        public LocalizedHtmlString this[string name]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public LocalizedHtmlString this[string name, params object[] arguments]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public LocalizedString GetString(string name)
        {
            throw new NotImplementedException();
        }

        public LocalizedString GetString(string name, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            throw new NotImplementedException();
        }

        public IHtmlLocalizer WithCulture(CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TestHtmlLocalizerFactory : IHtmlLocalizerFactory
    {
        public IHtmlLocalizer Create(Type resourceSource)
        {
            throw new NotImplementedException();
        }

        public IHtmlLocalizer Create(string baseName, string location)
        {
            throw new NotImplementedException();
        }
    }

    public class CustomPartialDirectoryViewLocationExpander : IViewLocationExpander
    {
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            throw new NotImplementedException();
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }
    }
}
