// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Razor.Hosting;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    public class CompiledViewDescriptorProviderTest
    {
        [Fact]
        public void GetCompiledViewDescriptors_ReturnsEmptySequenceIfAssemblyDoesNotHaveViewAssembly()
        {
            // Arrange
            var assembly = GetType().Assembly;
            var descriptorProvider = new AssemblyLoadFailureViewDescriptorProvider();

            // Act
            var result = descriptorProvider.GetCompiledViewDescriptors(assembly);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetCompiledViewDescriptors_ReturnsCompiledViewDescriptorsFromAssembly_WithoutViewAttribute()
        {
            // Arrange
            var assembly1 = typeof(object).Assembly;
            var assembly2 = GetType().Assembly;

            var items = new Dictionary<Assembly, IReadOnlyList<RazorCompiledItem>>
            {
                {
                    assembly1,
                    new[]
                    {
                        new TestRazorCompiledItem(typeof(object), "mvc.1.0.view", "/Views/test/Index.cshtml", new object[]{ }),

                        // This one doesn't have a RazorViewAttribute
                        new TestRazorCompiledItem(typeof(StringBuilder), "mvc.1.0.view", "/Views/test/About.cshtml", new object[]{ }),
                    }
                },
            };

            var attributes = new Dictionary<Assembly, IEnumerable<RazorViewAttribute>>
            {
                {
                    assembly1,
                    new[]
                    {
                        new RazorViewAttribute("/Views/test/Index.cshtml", typeof(object)),
                    }
                },
            };
            var descriptorProvider = new TestableCompiledViewDescriptorProvider(items, attributes);


            // Act
            var result = descriptorProvider.GetCompiledViewDescriptors(assembly1);

            // Assert
            Assert.Collection(result.OrderBy(f => f.RelativePath, StringComparer.Ordinal),
                view =>
                {
                    // This one doesn't have a RazorViewAttribute
                    Assert.Empty(view.ExpirationTokens);
                    Assert.True(view.IsPrecompiled);
                    Assert.Equal("/Views/test/About.cshtml", view.Item.Identifier);
                    Assert.Equal("mvc.1.0.view", view.Item.Kind);
                    Assert.Equal(typeof(StringBuilder), view.Item.Type);
                    Assert.Equal("/Views/test/About.cshtml", view.RelativePath);
                    Assert.Equal(typeof(StringBuilder), view.Type);
                    Assert.Null(view.ViewAttribute);
                },
                view =>
                {
                    Assert.Empty(view.ExpirationTokens);
                    Assert.True(view.IsPrecompiled);
                    Assert.Equal("/Views/test/Index.cshtml", view.Item.Identifier);
                    Assert.Equal("mvc.1.0.view", view.Item.Kind);
                    Assert.Equal(typeof(object), view.Item.Type);
                    Assert.Equal("/Views/test/Index.cshtml", view.RelativePath);
                    Assert.Equal(typeof(object), view.Type);
                    Assert.Equal("/Views/test/Index.cshtml", view.ViewAttribute.Path);
                    Assert.Equal(typeof(object), view.ViewAttribute.ViewType);
                });
        }

        [Fact]
        public void GetCompiledViewDescriptors_ReturnsCompiledViewDescriptorsFromAssembly_WithoutCompiledItem()
        {
            // Arrange
            var assembly1 = typeof(object).Assembly;
            var assembly2 = GetType().Assembly;

            var items = new Dictionary<Assembly, IReadOnlyList<RazorCompiledItem>>
            {
                {
                    assembly1, Array.Empty<RazorCompiledItem>()
                },
            };

            var attributes = new Dictionary<Assembly, IEnumerable<RazorViewAttribute>>
            {
                {
                    assembly1,
                    new[]
                    {
                        new RazorViewAttribute("/Views/test/Index.cshtml", typeof(object)),
                    }
                },
            };
            var descriptorProvider = new TestableCompiledViewDescriptorProvider(items, attributes);


            // Act
            var result = descriptorProvider.GetCompiledViewDescriptors(assembly1);

            // Assert
            Assert.Collection(result.OrderBy(f => f.RelativePath, StringComparer.Ordinal),
                view =>
                {
                    Assert.Empty(view.ExpirationTokens);
                    Assert.True(view.IsPrecompiled);
                    Assert.Null(view.Item);
                    Assert.Equal("/Views/test/Index.cshtml", view.RelativePath);
                    Assert.Equal(typeof(object), view.Type);
                    Assert.Equal("/Views/test/Index.cshtml", view.ViewAttribute.Path);
                    Assert.Equal(typeof(object), view.ViewAttribute.ViewType);
                });
        }

        [Fact]
        public void GetCompiledViewDescriptor_DoesNotFail_WhenAssemblyIsDDynamicallyGenerated()
        {
            // Arrange
            var name = new AssemblyName($"DynamicAssembly-{Guid.NewGuid()}");
            var assembly = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndCollect);
            var descriptorProvider = CompiledViewDescriptorProvider.Default;

            // Act
            var result = descriptorProvider.GetCompiledViewDescriptors(assembly);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetCompiledViewDescriptor_DoesNotFail_IfAssemblyHasEmptyLocation()
        {
            // Arrange
            var assembly = new AssemblyWithEmptyLocation();
            var descriptorProvider = CompiledViewDescriptorProvider.Default;

            // Act
            var result = descriptorProvider.GetCompiledViewDescriptors(assembly);

            // Assert
            Assert.Empty(result);
        }

        private class TestRazorCompiledItem : RazorCompiledItem
        {
            public TestRazorCompiledItem(Type type, string kind, string identifier, object[] metadata)
            {
                Type = type;
                Kind = kind;
                Identifier = identifier;
                Metadata = metadata;
            }

            public override string Identifier { get; }

            public override string Kind { get; }

            public override IReadOnlyList<object> Metadata { get; }

            public override Type Type { get; }
        }

        private class AssemblyWithEmptyLocation : Assembly
        {
            public override string Location => string.Empty;

            public override string FullName => typeof(ViewsFeatureProviderTest).Assembly.FullName;

            public override IEnumerable<TypeInfo> DefinedTypes => throw new NotImplementedException();

            public override IEnumerable<Module> Modules => throw new NotImplementedException();
        }

        private class AssemblyLoadFailureViewDescriptorProvider : CompiledViewDescriptorProvider.DefaultCompiledViewDescriptorProvider
        {
            public override Assembly GetViewAssembly(Assembly assembly) => null;
        }
    }
}
