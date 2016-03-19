// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    public class ApplicationPartCollectionTests
    {
        [Fact]
        public void Register_AddsApplicationPart_ForTheRegisteredAssembly()
        {
            // Arrange
            var collection = new ApplicationPartCollection();

            var assembly = typeof(Feature).GetTypeInfo().Assembly;

            // Act
            collection.Register(assembly);

            // Assert
            Assert.Equal(1, collection.Count);
            Assert.Equal(assembly, collection[0].Assembly);
        }

        [Fact]
        public void RegisterAssemblyMultipleTimes_AddsApplicationPart_OnlyOnce()
        {
            // Arrange
            var collection = new ApplicationPartCollection();

            var assembly = typeof(Feature).GetTypeInfo().Assembly;
            collection.Register(assembly);

            // Act
            collection.Register(assembly);

            // Assert
            Assert.Equal(1, collection.Count);
            Assert.Equal(assembly, collection[0].Assembly);
        }

        [Fact]
        public void Add_AddsPartToCollection()
        {
            // Arrange
            var collection = new ApplicationPartCollection();

            var assembly = typeof(Feature).GetTypeInfo().Assembly;
            var applicationPart = new ApplicationPart(assembly);

            // Act
            collection.Add(applicationPart);

            // Assert
            Assert.Equal(1, collection.Count);
            Assert.Same(applicationPart, collection[0]);
        }

        [Fact]
        public void Add_AddsPartToCollectionOnlyOnce()
        {
            // Arrange
            var collection = new ApplicationPartCollection();

            var assembly = typeof(Feature).GetTypeInfo().Assembly;
            var applicationPart = new ApplicationPart(assembly);

            collection.Add(applicationPart);

            // Act
            collection.Add(applicationPart);

            // Assert
            Assert.Equal(1, collection.Count);
            Assert.Same(applicationPart, collection[0]);
        }

        [Fact]
        public void AddFeature_AddsFeatureToAllRegisteredAssemblies()
        {
            // Arrange
            var collection = new ApplicationPartCollection();

            var firstAssembly = typeof(Feature).GetTypeInfo().Assembly;
            var secondAssembly = typeof(Controller).GetTypeInfo().Assembly;

            collection.Register(firstAssembly);
            collection.Register(secondAssembly);

            var feature = new Feature();

            // Act
            collection.AddFeature(new TestFeatureProvider(feature));

            // Assert
            Assert.Equal(2, collection.Count);
            Assert.All(collection, part => Assert.Equal(feature, part.GetFeature<Feature>()));
        }

        [Fact]
        public void RegisterAssembly_AddsAllRegisteredFeaturesToTheAssembly()
        {
            // Arrange
            var collection = new ApplicationPartCollection();

            var assembly = typeof(Feature).GetTypeInfo().Assembly;

            var feature = new Feature();
            var otherFeature = new OtherFeature();

            collection.AddFeature(new TestFeatureProvider(feature));
            collection.AddFeature(new TestOtherFeatureProvider(otherFeature));

            // Act
            collection.Register(assembly);

            // Assert
            Assert.Equal(1, collection.Count);
            var part = collection[0];

            Assert.Equal(feature, part.GetFeature<Feature>());
            Assert.Equal(otherFeature, part.GetFeature<OtherFeature>());
        }

        [Fact]
        public void AddFeature_DoesNotAddFeature_ToExplicitlyIncludedParts()
        {
            // Arrange
            var collection = new ApplicationPartCollection();

            var feature = new Feature();
            var otherFeature = new OtherFeature();

            var part = new ApplicationPart(typeof(Controller).GetTypeInfo().Assembly);
            part.SetFeature(new ExplicitFeature());

            // Act
            collection.AddFeature(new TestFeatureProvider(feature));
            collection.Add(part);
            collection.AddFeature(new TestOtherFeatureProvider(otherFeature));

            // Assert
            Assert.Equal(1, collection.Count);

            var configuredPart = collection[0];
            Assert.Null(configuredPart.GetFeature<Feature>());
            Assert.Null(configuredPart.GetFeature<OtherFeature>());
            Assert.NotNull(configuredPart.GetFeature<ExplicitFeature>());
        }

        private class TestOtherFeatureProvider : IApplicationFeatureProvider<OtherFeature>
        {
            private readonly OtherFeature _feature;

            public TestOtherFeatureProvider(OtherFeature feature)
            {
                _feature = feature;
            }

            public OtherFeature GetFeature(Assembly assembly)
            {
                return _feature;
            }
        }

        private class TestFeatureProvider : IApplicationFeatureProvider<Feature>
        {
            private readonly Feature _feature;

            public TestFeatureProvider(Feature feature)
            {
                _feature = feature;
            }

            public Feature GetFeature(Assembly assembly)
            {
                return _feature;
            }
        }

        private class Feature
        {
        }

        private class OtherFeature
        {
        }

        private class ExplicitFeature
        {
        }
    }
}
