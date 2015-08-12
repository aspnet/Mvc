// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc.Razor.Compilation;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Dnx.Compilation;
using Microsoft.Dnx.Runtime.Infrastructure;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor.Precompilation
{
    public class PrecompilationTagHelperTypeResolverTest
    {
        private static readonly Assembly ExecutingAssembly = typeof(PrecompilationTagHelperTypeResolverTest).GetTypeInfo().Assembly;
        private static readonly Assembly RuntimeFilesAssembly = typeof(TypeDerivingFromTagHelper).GetTypeInfo().Assembly;
        private static readonly TypeInfo TagHelperTypeInfo = typeof(ITagHelper).GetTypeInfo();
        private static readonly string GeneratedFilesAssemblyName = Path.GetRandomFileName() + "." + Path.GetRandomFileName();

        [Theory]
        [InlineData(typeof(TypeDerivingFromITagHelper))]
        [InlineData(typeof(TypeDerivingFromTagHelper))]
        [InlineData(typeof(TypeInGlobalNamespace))]
        public void TypesReturnedFromGetTopLevelExportedTypes_ReturnTrueForImplmentsInterfaceTest(Type expected)
        {
            // Arrange
            var compilation = GetCompilation(expected.Name);
            var tagHelperResolver = new PrecompilationTagHelperTypeResolver(compilation);

            // Act
            var exportedTypes = tagHelperResolver.GetExportedTypes(compilation.AssemblyName);

            // Assert
            var actual = Assert.Single(exportedTypes);
            AssertEqual(expected, actual);
        }

        [Fact]
        public void GetTopLevelExportedTypes_DoesNotReturnNonPublicOrNestedTypes()
        {
            // Arrange
            var compilation = GetCompilation("AssemblyWithNonPublicTypes");
            var tagHelperResolver = new PrecompilationTagHelperTypeResolver(compilation);

            // Act
            var exportedTypes = tagHelperResolver.GetExportedTypes(compilation.AssemblyName);

            // Assert
            Assert.Collection(exportedTypes,
                typeInfo =>
                {
                    AssertEqual(typeof(PublicType), typeInfo);
                },
                typeInfo =>
                {
                    AssertEqual(typeof(ContainerType), typeInfo);
                },
                typeInfo =>
                {
                    AssertEqual(typeof(Sum<>), typeInfo);
                });
        }

        [Fact]
        public void GetTopLevelExportedTypes_DoesNotReturnValueTypesOrInterfaces()
        {
            // Arrange
            var compilation = GetCompilation("AssemblyWithNonTypes");
            var tagHelperResolver = new PrecompilationTagHelperTypeResolver(compilation);

            // Act
            var exportedTypes = tagHelperResolver.GetExportedTypes(compilation.AssemblyName);

            // Assert
            Assert.Collection(exportedTypes,
                typeInfo =>
                {
                    AssertEqual(typeof(MyAbstractClass), typeInfo);
                },
                typeInfo =>
                {
                    AssertEqual(typeof(MyPartialClass), typeInfo);
                },
                typeInfo =>
                {
                    AssertEqual(typeof(MySealedClass), typeInfo);
                });
        }

        [Fact]
        public void GetExportedTypes_PopulatesAttributes()
        {
            // Arrange
            var expected = typeof(TypeWithAttributes).GetTypeInfo();
            var compilation = GetCompilation(expected.Name);
            var tagHelperResolver = new PrecompilationTagHelperTypeResolver(compilation);

            // Act
            var exportedTypes = tagHelperResolver.GetExportedTypes(compilation.AssemblyName);

            // Assert
            var actual = Assert.Single(exportedTypes);
            AssertEqual(expected.AsType(), actual);

            AssertAttributes<TargetElementAttribute>(
                expected,
                actual,
                (expectedAttribute, actualAttribute) =>
                {
                    Assert.Equal(expectedAttribute.Tag, actualAttribute.Tag);
                    Assert.Equal(expectedAttribute.Attributes, actualAttribute.Attributes);
                });

            // Verify if enum in attribute constructors works.
            AssertAttributes<EditorBrowsableAttribute>(
                expected,
                actual,
                (expectedAttribute, actualAttribute) =>
                {
                    Assert.Equal(expectedAttribute.State, actualAttribute.State);
                });

            // Verify if enum in attribute property works.
            AssertAttributes<ResponseCacheAttribute>(
                expected,
                actual,
                (expectedAttribute, actualAttribute) =>
                {
                    Assert.Equal(expectedAttribute.Location, actualAttribute.Location);
                });

            // Verify if Type in attribute constructor and property works.
            AssertAttributes<CustomValidationAttribute>(
                expected,
                actual,
                (expectedAttribute, actualAttribute) =>
                {
                    Assert.Same(expectedAttribute.ValidatorType, actualAttribute.ValidatorType);
                    Assert.Equal(expectedAttribute.Method, actualAttribute.Method);
                    Assert.Same(
                        expectedAttribute.ErrorMessageResourceType,
                        actualAttribute.ErrorMessageResourceType);
                });

            // Verify if array arguments work in constructor.
            AssertAttributes<BindAttribute>(
                expected,
                actual,
                (expectedAttribute, actualAttribute) =>
                {
                    Assert.Equal(expectedAttribute.Include, actualAttribute.Include);
                    Assert.Equal(expectedAttribute.Prefix, actualAttribute.Prefix);
                    Assert.Equal(expectedAttribute.PredicateProviderType, actualAttribute.PredicateProviderType);
                });

            // Verify if array arguments work in property.
            AssertAttributes<RestrictChildrenAttribute>(
                expected,
                actual,
                (expectedAttribute, actualAttribute) =>
                {
                    Assert.Equal(
                        expectedAttribute.ChildTagNames,
                        actualAttribute.ChildTagNames);
                });

            // Complex array bindings
            AssertAttributes<AttributeWithArrayPropertiesAttribute>(
                expected,
                actual,
                (expectedAttribute, actualAttribute) =>
                {
                    Assert.Equal(expectedAttribute.ArrayOfTypes, actualAttribute.ArrayOfTypes);
                    Assert.Equal(expectedAttribute.ArrayOfInts, actualAttribute.ArrayOfInts);
                    Assert.Equal(expectedAttribute.Days, actualAttribute.Days);
                });

            var expectedProperties = expected.DeclaredProperties;
            Assert.Collection(actual.Properties,
                property =>
                {
                    Assert.Equal(nameof(TypeWithAttributes.Src), property.Name);
                    var expectedProperty = Assert.Single(expectedProperties, p => p.Name == property.Name);
                    AssertAttributes<HtmlAttributeNameAttribute>(
                        expectedProperty,
                        property,
                        (expectedAttribute, actualAttribute) =>
                        {
                            Assert.Equal(expectedAttribute.Name, actualAttribute.Name);
                            Assert.Equal(expectedAttribute.DictionaryAttributePrefix, actualAttribute.DictionaryAttributePrefix);
                        });

                    // Verify boolean values bind.
                    AssertAttributes<RequiredAttribute>(
                       expectedProperty,
                       property,
                       (expectedAttribute, actualAttribute) =>
                       {
                           Assert.Equal(expectedAttribute.AllowEmptyStrings, actualAttribute.AllowEmptyStrings);
                       });
                },
                property =>
                {
                    Assert.Equal(nameof(TypeWithAttributes.AppendVersion), property.Name);
                    var expectedProperty = Assert.Single(expectedProperties, p => p.Name == property.Name);
                    AssertAttributes<HtmlAttributeNameAttribute>(
                        expectedProperty,
                        property,
                        (expectedAttribute, actualAttribute) =>
                        {
                            Assert.Equal(expectedAttribute.Name, actualAttribute.Name);
                            Assert.Equal(expectedAttribute.DictionaryAttributePrefix, actualAttribute.DictionaryAttributePrefix);
                        });

                    // Attribute without constructor arguments or properties.
                    Assert.Single(expectedProperty.GetCustomAttributes<HtmlAttributeNotBoundAttribute>());
                    Assert.Single(property.GetCustomAttributes<HtmlAttributeNotBoundAttribute>());
                },
                property =>
                {
                    Assert.Equal(nameof(TypeWithAttributes.ViewContext), property.Name);
                    var expectedProperty = Assert.Single(expectedProperties, p => p.Name == property.Name);

                    AssertAttributes<EditorBrowsableAttribute>(
                        expectedProperty,
                        property,
                        (expectedAttribute, actualAttribute) =>
                        {
                            Assert.Equal(expectedAttribute.State, actualAttribute.State);
                        });

                    // Complex array bindings in properties
                    AssertAttributes<AttributeWithArrayPropertiesAttribute>(
                        expected,
                        actual,
                        (expectedAttribute, actualAttribute) =>
                        {
                            Assert.Equal(expectedAttribute.ArrayOfTypes, actualAttribute.ArrayOfTypes);
                            Assert.Equal(expectedAttribute.ArrayOfInts, actualAttribute.ArrayOfInts);
                            Assert.Equal(expectedAttribute.Days, actualAttribute.Days);
                        });
                },
                property =>
                {
                    Assert.Equal("HostingEnvironment", property.Name);
                    Assert.Single(expectedProperties, p => p.Name == property.Name);

                    // Complex array bindings in constructor arguments
                    AssertAttributes<AttributesWithArrayConstructorArgumentsAttribute>(
                        expected,
                        actual,
                        (expectedAttribute, actualAttribute) =>
                        {
                            Assert.Equal(expectedAttribute.StringArgs, actualAttribute.StringArgs);
                            Assert.Equal(expectedAttribute.IntArgs, actualAttribute.IntArgs);
                            Assert.Equal(expectedAttribute.TypeArgs, actualAttribute.TypeArgs);
                        });
                },
                property =>
                {
                    Assert.Equal(nameof(TypeWithAttributes.FormId), property.Name);
                    var expectedProperty = Assert.Single(expectedProperties, p => p.Name == property.Name);

                    AssertAttributes<DerivedAttribute>(
                        expectedProperty,
                        property,
                        (expectedAttribute, actualAttribute) =>
                        {
                            Assert.Equal(expectedAttribute.BaseProperty, actualAttribute.BaseProperty);
                            Assert.Equal(expectedAttribute.DerivedProperty, actualAttribute.DerivedProperty);
                        });
                });
        }

        [Fact]
        public void GetExportedTypes_WithDerivedAttributes()
        {
            // Arrange
            var expected = typeof(DerivedTagHelper);
            var compilation = GetCompilation(expected.Name);
            var tagHelperResolver = new PrecompilationTagHelperTypeResolver(compilation);

            // Act
            var exportedTypes = tagHelperResolver.GetExportedTypes(compilation.AssemblyName);

            // Assert
            var actual = Assert.Single(exportedTypes);
            AssertEqual(expected, actual);

            var expectedProperties = expected.GetProperties();
            AssertAttributes<BaseAttribute>(
                expectedProperties.First(p => p.Name == nameof(DerivedTagHelper.DerivedProperty)), 
                actual.Properties.First(p => p.Name == nameof(DerivedTagHelper.DerivedProperty)),
                (expectedAttribute, actualAttribute) =>
                {
                    Assert.Equal(expectedAttribute.BaseProperty, actualAttribute.BaseProperty);
                });

            AssertAttributes<HtmlAttributeNameAttribute>(
                expectedProperties.First(p => p.Name == nameof(DerivedTagHelper.NewProperty) &&
                    p.PropertyType == typeof(Type)),
                actual.Properties.First(p => p.Name == nameof(DerivedTagHelper.NewProperty) &&
                    p.PropertyType.Name == typeof(Type).Name),
                (expectedAttribute, actualAttribute) =>
                {
                    Assert.Equal(expectedAttribute.Name, actualAttribute.Name);
                    Assert.Equal(
                        expectedAttribute.DictionaryAttributePrefix, 
                        actualAttribute.DictionaryAttributePrefix);
                });

            var expectedVirtualProperty = expectedProperties.First(
                p => p.Name == nameof(DerivedTagHelper.VirtualProperty));
            var actualVirtualProperty = actual.Properties.First(
                p => p.Name == nameof(DerivedTagHelper.VirtualProperty));

            Assert.Empty(expectedVirtualProperty.GetCustomAttributes<HtmlAttributeNotBoundAttribute>());
            Assert.Empty(actualVirtualProperty.GetCustomAttributes<HtmlAttributeNotBoundAttribute>());
        }

        [Fact]
        public void GetExportedTypes_CorrectlyIdentifiesIfTypeDerivesFromDictionary()
        {
            // Arrange
            var compilation = GetCompilation(nameof(TypeWithDictionaryProperties));
            var tagHelperResolver = new PrecompilationTagHelperTypeResolver(compilation);

            // Act
            var exportedTypes = tagHelperResolver.GetExportedTypes(compilation.AssemblyName);

            // Assert
            Assert.Collection(exportedTypes,
                typeInfo =>
                {
                    AssertEqual(typeof(TypeWithDictionaryProperties), typeInfo);
                },
                typeInfo =>
                {
                    AssertEqual(typeof(CustomType), typeInfo);
                });
        }

        [Fact]
        public void GetExportedTypes_WorksIfAttributesAreMalformedErrors()
        {
            // Arrange
            var compilation = GetCompilation("TypeWithMalformedAttribute", "BadFiles");
            var tagHelperResolver = new PrecompilationTagHelperTypeResolver(compilation);

            // Act
            var exportedTypes = tagHelperResolver.GetExportedTypes(compilation.AssemblyName);

            // Assert
            var actual = Assert.Single(exportedTypes);
            var targetElementAttribute = Assert.Single(actual.GetCustomAttributes<TargetElementAttribute>());
            Assert.Equal("img", targetElementAttribute.Tag);
            Assert.Null(targetElementAttribute.Attributes);

            var editorBrowsableAttribute = Assert.Single(actual.GetCustomAttributes<EditorBrowsableAttribute>());
            Assert.Equal(default(EditorBrowsableState), editorBrowsableAttribute.State);

            Assert.Throws<MissingMethodException>(() => actual.GetCustomAttributes<CustomValidationAttribute>());
        }

        [Fact]
        public void GetExportedTypes_WorksForPropertiesWithErrors()
        {
            // Arrange
            var compilation = GetCompilation("TypeWithMalformedProperties", "BadFiles");
            var tagHelperResolver = new PrecompilationTagHelperTypeResolver(compilation);

            // Act
            var exportedTypes = tagHelperResolver.GetExportedTypes(compilation.AssemblyName);

            // Assert
            var actual = Assert.Single(exportedTypes);
            Assert.Collection(actual.Properties,
                property =>
                {
                    Assert.Equal("DateTime", property.Name);
                    Assert.Equal(typeof(DateTime).Name, property.PropertyType.Name);
                    Assert.True(property.HasPublicGetter);
                    Assert.False(property.HasPublicSetter);
                },
                property =>
                {
                    Assert.Equal("DateTime2", property.Name);
                    Assert.Equal(typeof(int).Name, property.PropertyType.Name);
                    Assert.True(property.HasPublicGetter);
                    Assert.False(property.HasPublicSetter);
                },
                property =>
                {
                    Assert.Equal("CustomOrder", property.Name);
                    Assert.Equal(typeof(string).Name, property.PropertyType.Name);
                    Assert.True(property.HasPublicGetter);
                    Assert.True(property.HasPublicSetter);
                });
        }

        [Fact]
        public void GetExportedTypes_WorksForTypesWithErrors()
        {
            // Arrange
            var compilation = GetCompilation("TypeWithMissingReferences", "BadFiles");
            var tagHelperResolver = new PrecompilationTagHelperTypeResolver(compilation);

            // Act
            var exportedTypes = tagHelperResolver.GetExportedTypes(compilation.AssemblyName);

            // Assert
            var type = Assert.Single(exportedTypes);
            Assert.False(type.ImplementsInterface(TagHelperTypeInfo));
        }

        private static void AssertAttributes<TAttribute>(
            MemberInfo expected,
            IMemberInfo actual,
            Action<TAttribute, TAttribute> assertItem)
            where TAttribute : Attribute
        {
            Assert.Equal(
                expected.GetCustomAttributes<TAttribute>(),
                actual.GetCustomAttributes<TAttribute>(),
                new DelegateAssertion<TAttribute>(assertItem));
        }

        private static void AssertEqual(Type expected, ITypeInfo actual)
        {
            AssertEqual(new RuntimeTypeInfo(expected.GetTypeInfo()), actual, assertProperties: true);
        }

        private static void AssertEqual(ITypeInfo expected, ITypeInfo actual, bool assertProperties)
        {
            var actualFullName = actual.FullName.Replace(
               GeneratedFilesAssemblyName,
               RuntimeFilesAssembly.GetName().Name);

            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.FullName, actualFullName);
            Assert.Equal(expected.IsPublic, actual.IsPublic);
            Assert.Equal(expected.IsAbstract, actual.IsAbstract);
            Assert.Equal(expected.IsGenericType, actual.IsGenericType);
            Assert.Equal(
                expected.ImplementsInterface(TagHelperTypeInfo),
                actual.ImplementsInterface(TagHelperTypeInfo));
            Assert.Equal(
                expected.GetGenericDictionaryParameterNames(),
                actual.GetGenericDictionaryParameterNames());

            if (assertProperties)
            {
                Assert.Equal(
                    expected.Properties.OrderBy(p => p.Name),
                    actual.Properties.OrderBy(p => p.Name),
                    new DelegateAssertion<IPropertyInfo>((x, y) => AssertEqual(x, y)));
            }
        }

        private static void AssertEqual(IPropertyInfo expected, IPropertyInfo actual)
        {
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.HasPublicGetter, actual.HasPublicGetter);
            Assert.Equal(expected.HasPublicSetter, actual.HasPublicSetter);
            AssertEqual(expected.PropertyType, actual.PropertyType, assertProperties: false);
        }

        private CodeAnalysis.Compilation GetCompilation(string fileName, string directory = "Files")
        {
            var resourceText = ResourceFile.ReadResource(
                ExecutingAssembly,
                $"Precompilation.{directory}.{fileName}.cs",
                sourceFile: true);
            var assemblyVersion = RuntimeFilesAssembly.GetName().Version;

            var syntaxTrees = new[]
            {
                CSharpSyntaxTree.ParseText(resourceText),
                CSharpSyntaxTree.ParseText(
                    $"[assembly: {typeof(AssemblyVersionAttribute).FullName}(\"{assemblyVersion}\")]")
            };

            var references = RoslynCompilationService.GetApplicationReferences(
                new ConcurrentDictionary<string, CodeAnalysis.AssemblyMetadata>(StringComparer.Ordinal),
                CallContextServiceLocator.Locator.ServiceProvider.GetService<ILibraryExporter>(),
                ExecutingAssembly.GetName().Name);

            return CSharpCompilation.Create(
                GeneratedFilesAssemblyName,
                syntaxTrees,
                references);
        }

        private class DelegateAssertion<T> : IEqualityComparer<T>
        {
            private readonly Action<T, T> _assert;

            public DelegateAssertion(Action<T, T> assert)
            {
                _assert = assert;
            }

            public bool Equals(T x, T y)
            {
                _assert(x, y);
                return true;
            }

            public int GetHashCode(T obj) => 0;
        }
    }
}
