﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Xunit;
using System.Reflection.Emit;

namespace Microsoft.AspNetCore.Mvc.Razor.Compilation
{
    public class ViewsFeatureProviderTest
    {
        [Fact]
        public void PopulateFeature_ReturnsEmptySequenceIfNoAssemblyPartHasViewAssembly()
        {
            // Arrange
            var applicationPartManager = new ApplicationPartManager();
            applicationPartManager.ApplicationParts.Add(
                new AssemblyPart(typeof(ViewsFeatureProviderTest).GetTypeInfo().Assembly));
            applicationPartManager.FeatureProviders.Add(new ViewsFeatureProvider());
            var feature = new ViewsFeature();

            // Act
            applicationPartManager.PopulateFeature(feature);

            // Assert
            Assert.Empty(feature.Views);
        }

        [Fact]
        public void PopulateFeature_ReturnsViewsFromAllAvailableApplicationParts()
        {
            // Arrange
            var part1 = new AssemblyPart(typeof(object).GetTypeInfo().Assembly);
            var part2 = new AssemblyPart(GetType().GetTypeInfo().Assembly);
            var featureProvider = new TestableViewsFeatureProvider(new Dictionary<AssemblyPart, Type>
            {
                { part1, typeof(ViewInfoContainer1) },
                { part2, typeof(ViewInfoContainer2) },
            });

            var applicationPartManager = new ApplicationPartManager();
            applicationPartManager.ApplicationParts.Add(part1);
            applicationPartManager.ApplicationParts.Add(part2);
            applicationPartManager.FeatureProviders.Add(featureProvider);
            var feature = new ViewsFeature();

            // Act
            applicationPartManager.PopulateFeature(feature);

            // Assert
            Assert.Collection(feature.Views.OrderBy(f => f.Key, StringComparer.Ordinal),
                view =>
                {
                    Assert.Equal("/Areas/Admin/Views/About.cshtml", view.Key);
                    Assert.Equal(typeof(int), view.Value);
                },
                view =>
                {
                    Assert.Equal("/Areas/Admin/Views/Index.cshtml", view.Key);
                    Assert.Equal(typeof(string), view.Value);
                },
                view =>
                {
                    Assert.Equal("/Views/test/Index.cshtml", view.Key);
                    Assert.Equal(typeof(object), view.Value);
                });
        }

        [Fact]
        public void GetViewInfoContainerType_ReturnsNullForEmptyDynamicAssembly()
        {
            // Arrange
            var name = new AssemblyName($"DynamicAssembly-{Guid.NewGuid()}");
            var builder = AssemblyBuilder.DefineDynamicAssembly(name,
                AssemblyBuilderAccess.RunAndCollect);
            var module = builder.DefineDynamicModule("Main");

            var provider = new TestableViewsFeatureProvider2();

            // Act
            var type = provider.TestGetViewInfoContainerType(new AssemblyPart(builder));

            // Assert
            Assert.Null(type);
        }

        private class TestableViewsFeatureProvider : ViewsFeatureProvider
        {
            private readonly Dictionary<AssemblyPart, Type> _containerLookup;

            public TestableViewsFeatureProvider(Dictionary<AssemblyPart, Type> containerLookup)
            {
                _containerLookup = containerLookup;
            }

            protected override Type GetViewInfoContainerType(AssemblyPart assemblyPart) =>
                _containerLookup[assemblyPart];
        }

        private class TestableViewsFeatureProvider2 : ViewsFeatureProvider
        {

            public Type TestGetViewInfoContainerType(AssemblyPart assemblyPart) =>
                GetViewInfoContainerType(assemblyPart);

        }

        private class ViewInfoContainer1 : ViewInfoContainer
        {
            public ViewInfoContainer1()
                : base(new[]
                {
                    new ViewInfo("/Views/test/Index.cshtml", typeof(object))
                })
            {
            }
        }

        private class ViewInfoContainer2 : ViewInfoContainer
        {
            public ViewInfoContainer2()
                : base(new[]
                {
                    new ViewInfo("/Areas/Admin/Views/Index.cshtml", typeof(string)),
                    new ViewInfo("/Areas/Admin/Views/About.cshtml", typeof(int))
                })
            {
            }
        }
    }
}
