// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Test.DependencyInjection
{
    public class MvcRazorMvcBuilderExtensionsTest
    {
        [Fact]
        public void AddTagHelpersAsServices_ReplacesTagHelperActivatorAndTagHelperTypeResolver()
        {
            // Arrange
            var services = new ServiceCollection();
            var builder = services
                .AddMvc()
                .ConfigureApplicationPartManager(manager =>
                {
                    manager.ApplicationParts.Add(new TestApplicationPart());
                    manager.FeatureProviders.Add(new TagHelperFeatureProvider());
                });

            // Act
            builder.AddTagHelpersAsServices();

            // Assert
            var activatorDescriptor = Assert.Single(services.ToList(), d => d.ServiceType == typeof(ITagHelperActivator));
            Assert.Equal(typeof(ServiceBasedTagHelperActivator), activatorDescriptor.ImplementationType);

            var resolverDescriptor = Assert.Single(services.ToList(), d => d.ServiceType == typeof(ITagHelperTypeResolver));
            Assert.Equal(typeof(FeatureTagHelperTypeResolver), resolverDescriptor.ImplementationType);
        }

        [Fact]
        public void AddTagHelpersAsServices_RegistersDiscoveredTagHelpers()
        {
            // Arrange
            var services = new ServiceCollection();

            var manager = new ApplicationPartManager();
            manager.ApplicationParts.Add(new TestApplicationPart(
                typeof(TestTagHelperOne),
                typeof(TestTagHelperTwo)));

            manager.FeatureProviders.Add(new TestFeatureProvider());

            var builder = new MvcBuilder(services, manager);

            // Act
            builder.AddTagHelpersAsServices();

            // Assert
            var collection = services.ToList();
            Assert.Equal(4, collection.Count);

            var tagHelperOne = Assert.Single(collection,t => t.ServiceType == typeof(TestTagHelperOne));
            Assert.Equal(typeof(TestTagHelperOne), tagHelperOne.ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, tagHelperOne.Lifetime);

            var tagHelperTwo = Assert.Single(collection, t => t.ServiceType == typeof(TestTagHelperTwo));
            Assert.Equal(typeof(TestTagHelperTwo), tagHelperTwo.ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, tagHelperTwo.Lifetime);

            var activator = Assert.Single(collection, t => t.ServiceType == typeof(ITagHelperActivator));
            Assert.Equal(typeof(ServiceBasedTagHelperActivator), activator.ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, activator.Lifetime);

            var typeResolver = Assert.Single(collection, t => t.ServiceType == typeof(ITagHelperTypeResolver));
            Assert.Equal(typeof(FeatureTagHelperTypeResolver), typeResolver.ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, typeResolver.Lifetime);
        }

        private class TestTagHelperOne : TagHelper
        {
        }

        private class TestTagHelperTwo : TagHelper
        {
        }

        private class TestFeatureProvider : IApplicationFeatureProvider<TagHelperFeature>
        {
            public void PopulateFeature(IEnumerable<ApplicationPart> parts, TagHelperFeature feature)
            {
                foreach (var type in parts.OfType<IApplicationPartTypeProvider>().SelectMany(tp => tp.Types))
                {
                    feature.TagHelpers.Add(type);
                }
            }
        }
    }
}
