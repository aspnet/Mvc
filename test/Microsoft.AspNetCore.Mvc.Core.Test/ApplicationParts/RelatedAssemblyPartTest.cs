﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    public class RelatedAssemblyPartTest
    {
        private static readonly string AssemblyDirectory = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);

        [Fact]
        public void GetRelatedAssemblies_Noops_ForDynamicAssemblies()
        {
            // Arrange
            var name = new AssemblyName($"DynamicAssembly-{Guid.NewGuid()}");
            var assembly = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndCollect);

            // Act
            var result = RelatedAssemblyAttribute.GetRelatedAssemblies(assembly, throwOnError: true);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetRelatedAssemblies_ThrowsIfRelatedAttributeReferencesSelf()
        {
            // Arrange
            var expected = "RelatedAssemblyAttribute specified on MyAssembly cannot be self referential.";
            var assembly = new TestAssembly { AttributeAssembly = "MyAssembly" };

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => RelatedAssemblyAttribute.GetRelatedAssemblies(assembly, throwOnError: true));
            Assert.Equal(expected, ex.Message);
        }

        [Fact]
        public void GetRelatedAssemblies_ThrowsIfAssemblyCannotBeFound()
        {
            // Arrange
            var expected = $"Related assembly 'DoesNotExist' specified by assembly 'MyAssembly' could not be found in the directory {AssemblyDirectory}. Related assemblies must be co-located with the specifying assemblies.";
            var assemblyPath = Path.Combine(AssemblyDirectory, "MyAssembly.dll");
            var assembly = new TestAssembly
            {
                AttributeAssembly = "DoesNotExist"
            };

            // Act & Assert
            var ex = Assert.Throws<FileNotFoundException>(() => RelatedAssemblyAttribute.GetRelatedAssemblies(assembly, throwOnError: true));
            Assert.Equal(expected, ex.Message);
            Assert.Equal(Path.Combine(AssemblyDirectory, "DoesNotExist.dll"), ex.FileName);
        }

        [Fact]
        public void GetRelatedAssemblies_LoadsRelatedAssembly()
        {
            // Arrange
            var destination = Path.Combine(AssemblyDirectory, "RelatedAssembly.dll");
            var assembly = new TestAssembly
            {
                AttributeAssembly = "RelatedAssembly",
            };
            var relatedAssembly = typeof(RelatedAssemblyPartTest).Assembly;

            try
            {
                File.WriteAllBytes(destination, new byte[0]);
                var result = RelatedAssemblyAttribute.GetRelatedAssemblies(assembly, throwOnError: true, file =>
                {
                    Assert.Equal(file, destination);
                    return relatedAssembly;
                });
                Assert.Equal(new[] { relatedAssembly }, result);
            }
            finally
            {
                File.Delete(destination);
            }
        }

        private class TestAssembly : Assembly
        {
            public override AssemblyName GetName()
            {
                return new AssemblyName("MyAssembly");
            }

            public string AttributeAssembly { get; set; }

            public override string CodeBase => Path.Combine(AssemblyDirectory, "MyAssembly.dll");

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                var attribute = new RelatedAssemblyAttribute(AttributeAssembly);
                return new[] { attribute };
            }
        }
    }
}
