// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.Runtime;
using Moq;
using Xunit;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.Core
{
    public class DefaultAssemblyProviderTests
    {
        [Fact]
        public void CandidateAssemblies_IgnoresMvcAssemblies()
        {
            // Arrange
            var manager = new Mock<ILibraryManager>();
            manager.Setup(f => f.GetReferencingLibraries(It.IsAny<string>()))
                   .Returns(new[]
                   {
                        CreateLibraryInfo("Microsoft.AspNet.Mvc.Core"),
                        CreateLibraryInfo("Microsoft.AspNet.Mvc"),
                        CreateLibraryInfo("Microsoft.AspNet.Mvc.ModelBinding"),
                        CreateLibraryInfo("SomeRandomAssembly"),
                   })
                   .Verifiable();
            var provider = new TestAssemblyProvider(manager.Object);

            // Act
            var candidates = provider.GetCandidateLibraries();

            // Assert
            Assert.Equal(new[] { "SomeRandomAssembly" }, candidates.Select(a => a.Name));

            var context = new Mock<HttpContext>();
        }

        [Fact]
        public void CandidateAssemblies_ReturnsLibrariesReferencingAnyMvcAssembly()
        {
            // Arrange
            var manager = new Mock<ILibraryManager>();
            manager.Setup(f => f.GetReferencingLibraries(It.IsAny<string>()))
                  .Returns(Enumerable.Empty<ILibraryInformation>());
            manager.Setup(f => f.GetReferencingLibraries("Microsoft.AspNet.Mvc.Core"))
                   .Returns(new[] { CreateLibraryInfo("Foo") });
            manager.Setup(f => f.GetReferencingLibraries("Microsoft.AspNet.Mvc.ModelBinding"))
                   .Returns(new[] { CreateLibraryInfo("Bar") });
            manager.Setup(f => f.GetReferencingLibraries("Microsoft.AspNet.Mvc"))
                   .Returns(new[] { CreateLibraryInfo("Baz") });
            var provider = new TestAssemblyProvider(manager.Object);

            // Act
            var candidates = provider.GetCandidateLibraries();

            // Assert
            Assert.Equal(new[] { "Baz", "Foo", "Bar" }, candidates.Select(a => a.Name));
        }

        [Fact]
        public void CandidateAssemblies_ReturnsLibrariesReferencingDefaultAssemblies()
        {
            // Arrange
            var defaultProvider = new TestAssemblyProvider(CreateLibraryManager());

            // Act
            var defaultProviderCandidates = defaultProvider.GetCandidateLibraries();

            // Assert
            Assert.Equal(new[] { "Baz" }, defaultProviderCandidates.Select(a => a.Name));
        }

        [Fact]
        public void CandidateAssemblies_ReturnsLibrariesReferencingOverriddenAssemblies()
        {
            // Arrange
            var overriddenProvider = new OverriddenAssemblyProvider(CreateLibraryManager());

            // Act
            var overriddenProviderCandidates = overriddenProvider.GetCandidateLibraries();

            // Assert
            Assert.Equal(new[] { "Foo", "Bar" }, overriddenProviderCandidates.Select(a => a.Name));
        }

        [Fact]
        public void CandidateAssemblies_ReturnsEmptySequenceWhenReferenceAssembliesIsNull()
        {
            // Arrange
            var nullProvider = new NullAssemblyProvider(CreateLibraryManager());

            // Act
            var nullProviderCandidates = nullProvider.GetCandidateLibraries();

            // Assert
            Assert.Empty(nullProviderCandidates.Select(a => a.Name));
        }

        [Fact]
        public void ReferenceAssemblies_ReturnsExpectedReferenceAssemblies()
        {
            // Arrange
            var provider = new MvcAssembliesTestingProvider();
            var allAssemblies = provider.AllLoadedAssemblies;
            var expected = provider.ExpectedReferenceAssemblies;

            // Act
            var referenceAssemblies = provider.DefaultAssemblyProviderReferenceAssemblies;

            // Assert
            Assert.Contains("Microsoft.AspNet.Mvc", allAssemblies);
            Assert.True(expected.SetEquals(referenceAssemblies));
        }

        [Fact]
        public void ReferenceAssemblies_ReturnsLoadedReferenceAssemblies()
        {
            // Arrange
            var provider = new MvcAssembliesTestingProvider();
            var allAssemblies = provider.AllLoadedAssemblies;
            var expected = provider.LoadedMvcReferenceAssemblies;

            // Act
            var actualAssemblies = provider.LoadedMvcReferenceAssemblies;

            // Assert
            Assert.Contains("Microsoft.AspNet.Mvc", allAssemblies);
            Assert.True(expected.SetEquals(actualAssemblies));
        }

        private static ILibraryInformation CreateLibraryInfo(string name)
        {
            var info = new Mock<ILibraryInformation>();
            info.SetupGet(b => b.Name).Returns(name);
            return info.Object;
        }

        private static ILibraryManager CreateLibraryManager()
        {
            var manager = new Mock<ILibraryManager>();
            manager.Setup(f => f.GetReferencingLibraries(It.IsAny<string>()))
                  .Returns(Enumerable.Empty<ILibraryInformation>());
            manager.Setup(f => f.GetReferencingLibraries("Microsoft.AspNet.Mvc.Core"))
                   .Returns(new[] { CreateLibraryInfo("Baz") });
            manager.Setup(f => f.GetReferencingLibraries("MyAssembly"))
                   .Returns(new[] { CreateLibraryInfo("Foo") });
            manager.Setup(f => f.GetReferencingLibraries("AnotherAssembly"))
                   .Returns(new[] { CreateLibraryInfo("Bar") });
            return manager.Object;
        }

        private class TestAssemblyProvider : DefaultAssemblyProvider
        {
            public new IEnumerable<ILibraryInformation> GetCandidateLibraries()
            {
                return base.GetCandidateLibraries();
            }

            public TestAssemblyProvider(ILibraryManager libraryManager) : base(libraryManager)
            {
            }
        }

        private class OverriddenAssemblyProvider : TestAssemblyProvider
        {
            protected override HashSet<string> ReferenceAssemblies
            {
                get
                {
                    return new HashSet<string>
                    {
                        "MyAssembly",
                        "AnotherAssembly"
                    };
                }
            }

            public OverriddenAssemblyProvider(ILibraryManager libraryManager) : base(libraryManager)
            {
            }
        }

        private class NullAssemblyProvider : TestAssemblyProvider
        {
            protected override HashSet<string> ReferenceAssemblies
            {
                get
                {
                    return null;
                }
            }

            public NullAssemblyProvider(ILibraryManager libraryManager) : base(libraryManager)
            {
            }
        }

        private class MvcAssembliesTestingProvider : DefaultAssemblyProvider
        {
            private static readonly ILibraryManager _libraryManager = GetLibraryManager();
            private static readonly string _mvcName = "Microsoft.AspNet.Mvc";
            private static IEnumerable<string> _assemblies; 

            public MvcAssembliesTestingProvider() : base(_libraryManager)
            { }

            public HashSet<string> LoadedMvcReferenceAssemblies
            {
                get
                {
                    var mvcAssemblies = AllLoadedAssemblies
                        .Where(
                            n => n.StartsWith(_mvcName)
                            && !n.EndsWith("WebApiCompatShim")
                            && !n.Contains("Test"))
                        .ToList();

                    // The following assemblies are not reachable from Microsoft.AspNet.Mvc
                    mvcAssemblies.Add("Microsoft.AspNet.Mvc.TagHelpers");
                    mvcAssemblies.Add("Microsoft.AspNet.Mvc.Xml");
                    mvcAssemblies.Add("Microsoft.AspNet.PageExecutionInstrumentation.Interfaces");

                    return new HashSet<string>(mvcAssemblies.Distinct());
                }
            }

            public HashSet<string> ExpectedReferenceAssemblies
            {
                get
                {
                    return new HashSet<string>
                    {
                        "Microsoft.AspNet.Mvc",
                        "Microsoft.AspNet.Mvc.Common",
                        "Microsoft.AspNet.Mvc.Core",
                        "Microsoft.AspNet.Mvc.ModelBinding",
                        "Microsoft.AspNet.Mvc.Razor.Host",
                        "Microsoft.AspNet.Mvc.Razor",
                        "Microsoft.AspNet.Mvc.TagHelpers",
                        "Microsoft.AspNet.Mvc.Xml",
                        "Microsoft.AspNet.PageExecutionInstrumentation.Interfaces",
                    };
                }
            }

            public IEnumerable<string> DefaultAssemblyProviderReferenceAssemblies
            {
                get
                {
                    return base.ReferenceAssemblies;
                }
            }

            public IEnumerable<string> AllLoadedAssemblies
            {
                get
                {
                    _assemblies = _assemblies ??
                        _libraryManager.GetLibraries().Select(n => n.Name).Distinct();

                    return _assemblies;
                }
            }

            private static ILibraryManager GetLibraryManager()
            {
                var services = Hosting.HostingServices.Create();
                var builder = services.BuildServiceProvider();

                return builder.GetRequiredService<ILibraryManager>();
            }
        }
    }
}