// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class CompilerCacheTest
    {
        [Fact]
        public void GetOrAdd_ReturnsCompilationResultFromFactory()
        {
            // Arrange
            var fileSystem = new TestFileSystem();
            var cache = new CompilerCache(Enumerable.Empty<RazorFileInfoCollection>(), fileSystem);
            var fileInfo = new TestFileInfo();
            var type = GetType();
            var expected = UncachedCompilationResult.Successful(type, "hello world");

            var runtimeFileInfo = new RelativeFileInfo(fileInfo, "Index.cshtml");

            // Act
            var actual = cache.GetOrAdd(runtimeFileInfo, () => expected);

            // Assert
            Assert.Same(expected, actual);
            Assert.Equal("hello world", actual.CompiledContent);
            Assert.Same(type, actual.CompiledType);
        }

        [Fact]
        public void GetOrAdd_ThrowsIfCompilationFails()
        {
            // Arrange
            var fileSystem = new TestFileSystem();
            var cache = new CompilerCache(Enumerable.Empty<RazorFileInfoCollection>(), fileSystem);
            var fileInfo = new TestFileInfo
            {
                PhysicalPath = "MyPath",
                Content = "source content"
            };
            var type = GetType();
            var messages = new[]
            {
                new CompilationMessage("Compilation failure message")
            };
            var expected = CompilationResult.Failed(fileInfo, "compiled content", messages);

            var runtimeFileInfo = new RelativeFileInfo(fileInfo, "Index.cshtml");

            // Act and Assert
            var ex = Assert.Throws<CompilationFailedException>(() =>
                            cache.GetOrAdd(runtimeFileInfo, () => expected));

            // Assert
            Assert.Equal(fileInfo.PhysicalPath, ex.FilePath);
            Assert.Equal(fileInfo.Content, ex.FileContent);
            Assert.Equal("compiled content", ex.CompiledContent);
            Assert.Equal(messages, ex.Messages);
        }

        [Fact]
        public void GetOrAdd_UsesValueFromCache_IfFileAndViewStartHaveNotChanged()
        {
            // Arrange
            var relativePath = "Views/Home/Index.cshtml";
            var viewStartPath = "_ViewStart.cshtml";
            var instance = new PreCompile();
            var fileSystem = new TestFileSystem();
            var fileInfo = new TestFileInfo();
            var runtimeFileInfo = new RelativeFileInfo(fileInfo, relativePath);

            var viewStartFileInfo = new TestFileInfo();
            fileSystem.AddFile(viewStartPath, viewStartFileInfo);

            var precompiledViews = new ViewCollection();
            precompiledViews.Add(viewStartPath, typeof(PrecompiledViewStart));
            precompiledViews.Add(relativePath, typeof(PreCompile));
            var cache = new CompilerCache(new[] { precompiledViews }, fileSystem);

            // Act
            var actual = cache.GetOrAdd(runtimeFileInfo,
                                        compile: () => { throw new Exception("shouldn't be invoked"); });

            var actualViewStart = cache.GetOrAdd(new RelativeFileInfo(viewStartFileInfo, viewStartPath),
                                                 compile: () => { throw new Exception("shouldn't be invoked"); });

            // Assert
            Assert.Equal(typeof(PreCompile), actual.CompiledType);
            Assert.Equal(typeof(PrecompiledViewStart), actualViewStart.CompiledType);
        }

        [Fact]
        public void GetOrAdd_UsesValueFromCache_IfUnrelatedViewStartHasChanged()
        {
            // Arrange
            var relativePath = "Views/Home/Index.cshtml";
            var viewStartPath = "_ViewStart.cshtml";
            var instance = new PreCompile();
            var fileSystem = new TestFileSystem();
            var fileInfo = new TestFileInfo();
            var runtimeFileInfo = new RelativeFileInfo(fileInfo, relativePath);

            var viewStartFileInfo = new TestFileInfo();
            fileSystem.AddFile(viewStartPath, viewStartFileInfo);
            var precompiledViews = new ViewCollection();
            precompiledViews.Add(viewStartPath, typeof(PrecompiledViewStart));
            precompiledViews.Add(relativePath, typeof(PreCompile));

            var cache = new CompilerCache(new[] { precompiledViews }, fileSystem);

            // Act
            fileSystem.GetTriggerTokenSource("Views/Shared/_ViewStart.cshtml").Cancel();
            var actual = cache.GetOrAdd(runtimeFileInfo,
                                        compile: () => { throw new Exception("shouldn't be invoked"); });

            var actualViewStart = cache.GetOrAdd(new RelativeFileInfo(viewStartFileInfo, viewStartPath),
                                                 compile: () => { throw new Exception("shouldn't be invoked"); });

            // Assert
            Assert.Equal(typeof(PreCompile), actual.CompiledType);
            Assert.Equal(typeof(PrecompiledViewStart), actualViewStart.CompiledType);
        }

        [Fact]
        public void GetOrAdd_RecompilesIfFileHasChanged()
        {
            // Arrange
            var relativePath = "Views/Home/Index.cshtml";
            var viewStartPath = "_ViewStart.cshtml";
            var instance = new PreCompile();
            var fileSystem = new TestFileSystem();
            var fileInfo = new TestFileInfo();
            var runtimeFileInfo = new RelativeFileInfo(fileInfo, relativePath);
            var viewStartFileInfo = new TestFileInfo();
            fileSystem.AddFile(viewStartPath, viewStartFileInfo);

            var precompiledViews = new ViewCollection();
            precompiledViews.Add(viewStartPath, typeof(PrecompiledViewStart));
            precompiledViews.Add(relativePath, typeof(PreCompile));

            var cache = new CompilerCache(new[] { precompiledViews }, fileSystem);

            // Act
            // Invoke the expiration trigger for the file.
            fileSystem.GetTriggerTokenSource(relativePath).Cancel();

            var actual = cache.GetOrAdd(runtimeFileInfo,
                                        compile: () => CompilationResult.Successful(typeof(RuntimeCompiled)));
            var actualViewStart = cache.GetOrAdd(new RelativeFileInfo(viewStartFileInfo, viewStartPath),
                                                 compile: () => { throw new Exception("shouldn't be invoked"); });

            // Assert
            Assert.Equal(typeof(RuntimeCompiled), actual.CompiledType);
            Assert.Equal(typeof(PrecompiledViewStart), actualViewStart.CompiledType);
        }

        public static TheoryData<string, string> FileWithViewStartData
        {
            get
            {
                return new TheoryData<string, string>
                {
                    { @"Views\Home\Index.cshtml", @"Views\Home\_ViewStart.cshtml" },
                    { @"Views\Home\Index.cshtml", @"Views\_ViewStart.cshtml" },
                    { @"Views\Home\Index.cshtml", "_ViewStart.cshtml" },
                    { @"Areas\MyArea\Views\Home\Index.cshtml", @"Areas\MyArea\Views\_ViewStart.cshtml" },
                    { @"Areas\MyArea\Views\Home\Index.cshtml", "_ViewStart.cshtml" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(FileWithViewStartData))]
        public void GetOrAdd_RecompilesFileAndViewIfViewStartWasModifiedOrDeleted(
            string filePath,
            string viewStartPath)
        {
            // Arrange
            var instance = new PreCompile();
            var fileSystem = new TestFileSystem();
            var fileInfo = new TestFileInfo();
            var runtimeFileInfo = new RelativeFileInfo(fileInfo, filePath);

            var viewStartFileInfo = new TestFileInfo();
            fileSystem.AddFile(viewStartPath, viewStartFileInfo);
            var precompiledViews = new ViewCollection();
            precompiledViews.Add(viewStartPath, typeof(PrecompiledViewStart));
            precompiledViews.Add(filePath, typeof(PreCompile));

            var cache = new CompilerCache(new[] { precompiledViews }, fileSystem);

            // Act
            // Invoke the expiration trigger for the _ViewStart.
            fileSystem.GetTriggerTokenSource(viewStartPath).Cancel();

            var actual = cache.GetOrAdd(runtimeFileInfo,
                                        compile: () => CompilationResult.Successful(typeof(RuntimeCompiled)));
            var actualViewStart = cache.GetOrAdd(new RelativeFileInfo(viewStartFileInfo, viewStartPath),
                                        compile: () => CompilationResult.Successful(typeof(RuntimeCompiledViewStart)));

            // Assert
            Assert.Equal(typeof(RuntimeCompiled), actual.CompiledType);
            Assert.Equal(typeof(RuntimeCompiledViewStart), actualViewStart.CompiledType);
        }

        [Theory]
        [MemberData(nameof(FileWithViewStartData))]
        public void GetOrAdd_RecompilesFileAndViewIfViewStartWasAdded(
            string filePath,
            string viewStartPath)
        {
            // Arrange
            var instance = new PreCompile();
            var fileSystem = new TestFileSystem();
            var fileInfo = new TestFileInfo();
            var runtimeFileInfo = new RelativeFileInfo(fileInfo, filePath);
            var viewStartFileInfo = new TestFileInfo();

            var precompiledViews = new ViewCollection();
            precompiledViews.Add(filePath, typeof(PreCompile));

            var cache = new CompilerCache(new[] { precompiledViews }, fileSystem);

            // Act
            // Trigger expiration trigger for _ViewStart which previously did not exist
            fileSystem.GetTriggerTokenSource(viewStartPath).Cancel();

            var actual = cache.GetOrAdd(runtimeFileInfo,
                                        compile: () => CompilationResult.Successful(typeof(RuntimeCompiled)));
            var actualViewStart = cache.GetOrAdd(new RelativeFileInfo(viewStartFileInfo, viewStartPath),
                                        compile: () => CompilationResult.Successful(typeof(RuntimeCompiledViewStart)));

            // Assert
            Assert.Equal(typeof(RuntimeCompiled), actual.CompiledType);
            Assert.Equal(typeof(RuntimeCompiledViewStart), actualViewStart.CompiledType);
        }

        private class PreCompile
        {
        }

        private class PrecompiledViewStart
        {
        }

        private class RuntimeCompiled
        {
        }

        private class RuntimeCompiledViewStart
        {
        }

        private class ViewCollection : RazorFileInfoCollection
        {
            private readonly List<RazorFileInfo> _fileInfos = new List<RazorFileInfo>();

            public ViewCollection()
            {
                FileInfos = _fileInfos;
            }

            public void Add(string relativePath, Type type)
            {
                var fileInfo = new RazorFileInfo
                {
                    RelativePath = relativePath,
                    FullTypeName = type.FullName
                };

                _fileInfos.Add(fileInfo);
            }
        }
    }
}