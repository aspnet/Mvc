// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    public class ApplicationPartTests
    {
        [Fact]
        public void Constructor_InitializesApplicationPart()
        {
            // Arrange
            var assembly = typeof(Feature).GetTypeInfo().Assembly;

            // Act
            var part = new ApplicationPart(assembly);

            // Assert
            Assert.Equal(assembly, part.Assembly);
        }

        [Fact]
        public void GetFeature_ReturnsNull_IfFeatureDoesNotExist()
        {
            // Arrange
            var part = new ApplicationPart(typeof(Feature).GetTypeInfo().Assembly);

            // Act
            var feature = part.GetFeature<Feature>();

            // Assert
            Assert.Null(feature);
        }

        [Fact]
        public void GetFeature_ReturnsDefaultValue_IfValueTypeFeatureDoesNotExist()
        {
            // Arrange
            var part = new ApplicationPart(typeof(Feature).GetTypeInfo().Assembly);

            // Act
            var feature = part.GetFeature<ValueFeature>();

            // Assert
            Assert.Equal(default(ValueFeature), feature);
        }

        [Fact]
        public void GetSetFeature_RoundTrips()
        {
            // Arrange
            var part = new ApplicationPart(typeof(Feature).GetTypeInfo().Assembly);
            var expectedFeature = new Feature();
            part.SetFeature(expectedFeature);

            // Act
            var feature = part.GetFeature<Feature>();

            // Assert
            Assert.Equal(expectedFeature, feature);
        }

        [Fact]
        public void GetFeature_UsesDeclaredType_ToFindTheFeature()
        {
            // Arrange
            var part = new ApplicationPart(typeof(Feature).GetTypeInfo().Assembly);
            var expectedFeature = new DerivedFeature();
            part.SetFeature<DerivedFeature>(expectedFeature);

            // Act
            var feature = part.GetFeature<Feature>();

            // Assert
            Assert.Null(feature);
        }

        [Fact]
        public void GetFeature_DoesNotSupportInheritanceOnKeys()
        {
            // Arrange
            var part = new ApplicationPart(typeof(Feature).GetTypeInfo().Assembly);
            var expectedFeature = new DerivedFeature();
            part.SetFeature<Feature>(expectedFeature);

            // Act
            var feature = part.GetFeature<DerivedFeature>();

            // Assert
            Assert.Null(feature);
        }

        [Fact]
        public void SetFeature_ValuesCanBeOfDerivedTypes()
        {
            // Arrange
            var part = new ApplicationPart(typeof(Feature).GetTypeInfo().Assembly);
            var expectedFeature = new DerivedFeature();
            part.SetFeature<Feature>(expectedFeature);

            // Act
            var feature = part.GetFeature<Feature>();

            // Assert
            Assert.Equal(expectedFeature, feature);
        }

        [Fact]
        public void SetFeature_AcceptsDefaultValueForValueTypeFeatures()
        {
            // Arrange
            var part = new ApplicationPart(typeof(Feature).GetTypeInfo().Assembly);
            part.SetFeature(new ValueFeature { Value = 3 });
            part.SetFeature(default(ValueFeature));

            // Act
            var feature = part.GetFeature<ValueFeature>();

            // Assert
            Assert.Equal(default(ValueFeature), feature);
        }

        [Fact]
        public void SetFeature_AcceptsNullForReferenceTypeFeatures()
        {
            // Arrange
            var part = new ApplicationPart(typeof(Feature).GetTypeInfo().Assembly);
            part.SetFeature(new Feature());
            part.SetFeature<Feature>(null);

            // Act
            var feature = part.GetFeature<Feature>();

            // Assert
            Assert.Null(feature);
        }

        private class Feature
        {
        }

        private class DerivedFeature : Feature
        {
        }

        private struct ValueFeature
        {
            public int Value { get; set; }
        }
    }
}
