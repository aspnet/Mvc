﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    public class DefaultRoslynCompilationServiceTest
    {
        [Fact]
        public void Compile_ReturnsCompilationResult()
        {
            // Arrange
            var content = @"
public class MyTestType  {}";

            var compilationService = new TestableRoslynCompilationService(
                GetDependencyContext(),
                GetOptions(),
                GetFileProviderAccessor());
            var relativeFileInfo = new RelativeFileInfo(
                new TestFileInfo { PhysicalPath = "SomePath" },
                "some-relative-path");

            // Act
            var result = compilationService.Compile(relativeFileInfo, content);

            // Assert
            Assert.Equal("MyTestType", result.CompiledType.Name);
        }

        [Fact]
        public void Compile_ReturnsCompilationFailureWithPathsFromLinePragmas()
        {
            // Arrange
            var viewPath = "some-relative-path";
            var fileContent = "test file content";
            var content = $@"
#line 1 ""{viewPath}""
this should fail";
            var fileProvider = new TestFileProvider();
            var fileInfo = fileProvider.AddFile(viewPath, fileContent);

            var compilationService = new TestableRoslynCompilationService(
                GetDependencyContext(),
                GetOptions(),
                GetFileProviderAccessor(fileProvider));
            var relativeFileInfo = new RelativeFileInfo(fileInfo, "some-relative-path");

            // Act
            var result = compilationService.Compile(relativeFileInfo, content);

            // Assert
            Assert.IsType<CompilationResult>(result);
            Assert.Null(result.CompiledType);
            var compilationFailure = Assert.Single(result.CompilationFailures);
            Assert.Equal(relativeFileInfo.RelativePath, compilationFailure.SourceFilePath);
            Assert.Equal(fileContent, compilationFailure.SourceFileContent);
        }

        [Fact]
        public void Compile_ReturnsGeneratedCodePath_IfLinePragmaIsNotAvailable()
        {
            // Arrange
            var fileContent = "file content";
            var content = @"this should fail";

            var compilationService = new TestableRoslynCompilationService(
                GetDependencyContext(),
                GetOptions(),
                GetFileProviderAccessor());
            var relativeFileInfo = new RelativeFileInfo(
                new TestFileInfo { Content = fileContent },
                "some-relative-path");

            // Act
            var result = compilationService.Compile(relativeFileInfo, content);

            // Assert
            Assert.IsType<CompilationResult>(result);
            Assert.Null(result.CompiledType);

            var compilationFailure = Assert.Single(result.CompilationFailures);
            Assert.Equal("Generated Code", compilationFailure.SourceFilePath);
            Assert.Equal(content, compilationFailure.SourceFileContent);
        }

        [Fact]
        public void Compile_DoesNotThrow_IfFileCannotBeRead()
        {
            // Arrange
            var path = "some-relative-path";
            var content = $@"
#line 1 ""{path}""
this should fail";

            var mockFileInfo = new Mock<IFileInfo>();
            mockFileInfo.Setup(f => f.CreateReadStream())
                .Throws(new Exception());
            var fileProvider = new TestFileProvider();
            fileProvider.AddFile(path, mockFileInfo.Object);

            var compilationService = new TestableRoslynCompilationService(
                GetDependencyContext(),
                GetOptions(),
                GetFileProviderAccessor());
            var relativeFileInfo = new RelativeFileInfo(mockFileInfo.Object, path);

            // Act
            var result = compilationService.Compile(relativeFileInfo, content);

            // Assert
            Assert.IsType<CompilationResult>(result);
            Assert.Null(result.CompiledType);
            var compilationFailure = Assert.Single(result.CompilationFailures);
            Assert.Equal(path, compilationFailure.SourceFilePath);
            Assert.Null(compilationFailure.SourceFileContent);
        }

        [Fact]
        public void Compile_UsesApplicationsCompilationSettings_ForParsingAndCompilation()
        {
            // Arrange
            var content = @"
#if MY_CUSTOM_DEFINE
public class MyCustomDefinedClass {}
#else
public class MyNonCustomDefinedClass {}
#endif
";

            var options = GetOptions();
            options.ParseOptions = options.ParseOptions.WithPreprocessorSymbols("MY_CUSTOM_DEFINE");

            var compilationService = new TestableRoslynCompilationService(
                 GetDependencyContext(),
                 options,
                 GetFileProviderAccessor());
            var relativeFileInfo = new RelativeFileInfo(
                new TestFileInfo { PhysicalPath = "SomePath" },
                "some-relative-path");

            // Act
            var result = compilationService.Compile(relativeFileInfo, content);

            // Assert
            Assert.NotNull(result.CompiledType);
            Assert.Equal("MyCustomDefinedClass", result.CompiledType.Name);
        }

        [Fact]
        public void GetCompilationFailedResult_ReturnsCompilationResult_WithGroupedMessages()
        {
            // Arrange
            var viewPath = "Views/Home/Index";
            var generatedCodeFileName = "Generated Code";
            var fileProvider = new TestFileProvider();
            fileProvider.AddFile(viewPath, "view-content");
            var options = new RazorViewEngineOptions();
            options.FileProviders.Add(fileProvider);

            var compilationService = new TestableRoslynCompilationService(
                 GetDependencyContext(),
                 options,
                 GetFileProviderAccessor(fileProvider));

            var assemblyName = "random-assembly-name";

            var diagnostics = new[]
            {
                Diagnostic.Create(
                    GetDiagnosticDescriptor("message-1"),
                    Location.Create(
                        viewPath,
                        new TextSpan(10, 5),
                        new LinePositionSpan(new LinePosition(10, 1), new LinePosition(10, 2)))),
                Diagnostic.Create(
                    GetDiagnosticDescriptor("message-2"),
                    Location.Create(
                        assemblyName,
                        new TextSpan(1, 6),
                        new LinePositionSpan(new LinePosition(1, 2), new LinePosition(3, 4)))),
                Diagnostic.Create(
                    GetDiagnosticDescriptor("message-3"),
                    Location.Create(
                        viewPath,
                        new TextSpan(40, 50),
                        new LinePositionSpan(new LinePosition(30, 5), new LinePosition(40, 12)))),
            };

            // Act
            var compilationResult = compilationService.GetCompilationFailedResult(
                viewPath,
                "compilation-content",
                assemblyName,
                diagnostics);

            // Assert
            Assert.Collection(compilationResult.CompilationFailures,
                failure =>
                {
                    Assert.Equal(viewPath, failure.SourceFilePath);
                    Assert.Equal("view-content", failure.SourceFileContent);
                    Assert.Collection(failure.Messages,
                        message =>
                        {
                            Assert.Equal("message-1", message.Message);
                            Assert.Equal(viewPath, message.SourceFilePath);
                            Assert.Equal(11, message.StartLine);
                            Assert.Equal(2, message.StartColumn);
                            Assert.Equal(11, message.EndLine);
                            Assert.Equal(3, message.EndColumn);
                        },
                        message =>
                        {
                            Assert.Equal("message-3", message.Message);
                            Assert.Equal(viewPath, message.SourceFilePath);
                            Assert.Equal(31, message.StartLine);
                            Assert.Equal(6, message.StartColumn);
                            Assert.Equal(41, message.EndLine);
                            Assert.Equal(13, message.EndColumn);
                        });
                },
                failure =>
                {
                    Assert.Equal(generatedCodeFileName, failure.SourceFilePath);
                    Assert.Equal("compilation-content", failure.SourceFileContent);
                    Assert.Collection(failure.Messages,
                        message =>
                        {
                            Assert.Equal("message-2", message.Message);
                            Assert.Equal(assemblyName, message.SourceFilePath);
                            Assert.Equal(2, message.StartLine);
                            Assert.Equal(3, message.StartColumn);
                            Assert.Equal(4, message.EndLine);
                            Assert.Equal(5, message.EndColumn);
                        });
                });
        }

        [Fact]
        public void Compile_RunsCallback()
        {
            // Arrange
            var content = "public class MyTestType  {}";
            RoslynCompilationContext usedCompilation = null;

            var compilationService = new TestableRoslynCompilationService(
                 GetDependencyContext(),
                 GetOptions(callback: c => usedCompilation = c),
                 GetFileProviderAccessor());

            var relativeFileInfo = new RelativeFileInfo(
                new TestFileInfo { PhysicalPath = "SomePath" },
                "some-relative-path");

            // Act
            var result = compilationService.Compile(relativeFileInfo, content);

            Assert.NotNull(usedCompilation);
            Assert.Single(usedCompilation.Compilation.SyntaxTrees);
        }

        [Fact]
        public void Compile_ThrowsIfDependencyContextIsNullAndTheApplicationFailsToCompileWithNoReferences()
        {
            // Arrange
            var content = "public class MyTestType  {}";
            var compilationService = new TestableRoslynCompilationService(
                dependencyContext: null,
                viewEngineOptions: GetOptions(),
                fileProviderAccessor: GetFileProviderAccessor());

            var relativeFileInfo = new RelativeFileInfo(
                new TestFileInfo { PhysicalPath = "SomePath" },
                "some-relative-path.cshtml");

            var expected = "The Razor page 'some-relative-path.cshtml' failed to compile. Ensure that your "
                 + "application's project.json sets the 'preserveCompilationContext' compilation property.";

            // Act and Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                compilationService.Compile(relativeFileInfo, content));
            Assert.Equal(expected, ex.Message);
        }

        [Fact]
        public void Compile_ThrowsIfDependencyContextReturnsNoReferencesAndTheApplicationFailsToCompile()
        {
            // Arrange
            var content = "public class MyTestType  {}";
            var dependencyContext = new DependencyContext(
                new TargetInfo("framework", "runtime", "signature", isPortable: true),
                Extensions.DependencyModel.CompilationOptions.Default,
                new CompilationLibrary[0],
                new RuntimeLibrary[0],
                Enumerable.Empty<RuntimeFallbacks>());
            var compilationService = new TestableRoslynCompilationService(
                dependencyContext: dependencyContext,
                viewEngineOptions: GetOptions(),
                fileProviderAccessor: GetFileProviderAccessor());

            var relativeFileInfo = new RelativeFileInfo(
                new TestFileInfo { PhysicalPath = "SomePath" },
                "some-relative-path.cshtml");

            var expected = "The Razor page 'some-relative-path.cshtml' failed to compile. Ensure that your "
                 + "application's project.json sets the 'preserveCompilationContext' compilation property.";

            // Act and Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                compilationService.Compile(relativeFileInfo, content));
            Assert.Equal(expected, ex.Message);
        }

        [Fact]
        public void Compile_DoesNotThrowIfReferencesWereClearedInCallback()
        {
            // Arrange
            var options = GetOptions(context =>
            {
                context.Compilation = context.Compilation.RemoveAllReferences();
            });
            var content = "public class MyTestType  {}";
            var compilationService = new TestableRoslynCompilationService(
                dependencyContext: GetDependencyContext(),
                viewEngineOptions: options,
                fileProviderAccessor: GetFileProviderAccessor());

            var relativeFileInfo = new RelativeFileInfo(
                new TestFileInfo { PhysicalPath = "SomePath" },
                "some-relative-path.cshtml");

            // Act
            var result = compilationService.Compile(relativeFileInfo, content);

            // Assert
            Assert.Single(result.CompilationFailures);
        }

        [Fact]
        public void Compile_SucceedsIfReferencesAreAddedInCallback()
        {
            // Arrange
            var options = GetOptions(context =>
            {
                var assemblyLocation = typeof(object).GetTypeInfo().Assembly.Location;

                context.Compilation = context
                    .Compilation
                    .AddReferences(MetadataReference.CreateFromFile(assemblyLocation));
            });
            var content = "public class MyTestType  {}";
            var compilationService = new TestableRoslynCompilationService(
                dependencyContext: null,
                viewEngineOptions: options,
                fileProviderAccessor: GetFileProviderAccessor());

            var relativeFileInfo = new RelativeFileInfo(
                new TestFileInfo { PhysicalPath = "SomePath" },
                "some-relative-path.cshtml");

            // Act
            var result = compilationService.Compile(relativeFileInfo, content);

            // Assert
            Assert.Null(result.CompilationFailures);
            Assert.NotNull(result.CompiledType);
        }

        private static DiagnosticDescriptor GetDiagnosticDescriptor(string messageFormat)
        {
            return new DiagnosticDescriptor(
                id: "someid",
                title: "sometitle",
                messageFormat: messageFormat,
                category: "some-category",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);
        }

        private static RazorViewEngineOptions GetOptions(Action<RoslynCompilationContext> callback = null)
        {
            return new RazorViewEngineOptions
            {
                CompilationCallback = callback ?? (c => { }),
            };
        }

        private IRazorViewEngineFileProviderAccessor GetFileProviderAccessor(IFileProvider fileProvider = null)
        {
            var options = new Mock<IRazorViewEngineFileProviderAccessor>();
            options.SetupGet(o => o.FileProvider)
                .Returns(fileProvider ?? new TestFileProvider());

            return options.Object;
        }

        private DependencyContext GetDependencyContext()
        {
            var assembly = typeof(DefaultRoslynCompilationServiceTest).GetTypeInfo().Assembly;
            return DependencyContext.Load(assembly);
        }

        private class TestableRoslynCompilationService : DefaultRoslynCompilationService
        {
            private readonly DependencyContext _dependencyContext;

            public TestableRoslynCompilationService(
                DependencyContext dependencyContext,
                RazorViewEngineOptions viewEngineOptions,
                IRazorViewEngineFileProviderAccessor fileProviderAccessor)
                : base(
                      Mock.Of<IHostingEnvironment>(),
                      GetAccessor(viewEngineOptions),
                      fileProviderAccessor,
                      NullLoggerFactory.Instance)
            {
                _dependencyContext = dependencyContext;
            }

            private static IOptions<RazorViewEngineOptions> GetAccessor(RazorViewEngineOptions options)
            {
                var optionsAccessor = new Mock<IOptions<RazorViewEngineOptions>>();
                optionsAccessor.SetupGet(a => a.Value).Returns(options);
                return optionsAccessor.Object;
            }

            protected override DependencyContext GetDependencyContext(IHostingEnvironment hostingEnvironment)
                => _dependencyContext;
        }
    }
}
