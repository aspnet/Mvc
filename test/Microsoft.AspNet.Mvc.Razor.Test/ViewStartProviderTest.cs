// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Framework.Runtime;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor.Test
{
    public class ViewStartProviderTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetViewStartLocations_ReturnsEmptySequenceIfViewPathIsEmpty(string viewPath)
        {
            // Arrange
            var appPath = @"x:\test";
            var provider = new ViewStartProvider(GetAppEnv(appPath), Mock.Of<IRazorPageFactory>());

            // Act
            var result = provider.GetViewStartLocations(viewPath);

            // Assert
            Assert.Empty(result);
        }

        public static IEnumerable<object[]> GetViewStartLocations_ReturnsPotentialViewStartLocationsData
        {
            get
            {
                yield return new object[]
                {
                    @"x:\test\myapp",
                    "/Views/Home/View.cshtml",
                    new[]
                    {
                        @"x:\test\myapp\Views\Home\_ViewStart.cshtml",
                        @"x:\test\myapp\Views\_ViewStart.cshtml",
                        @"x:\test\myapp\_ViewStart.cshtml",
                    }
                };

                yield return new object[]
                {
                    @"x:\test\myapp",
                    "Views/Home/View.cshtml",
                    new[]
                    {
                        @"x:\test\myapp\Views\Home\_ViewStart.cshtml",
                        @"x:\test\myapp\Views\_ViewStart.cshtml",
                        @"x:\test\myapp\_ViewStart.cshtml",
                    }
                };

                yield return new object[]
                {
                    @"x:\test\myapp\",
                    "Views/Areas/MyArea/Home/View.cshtml",
                    new[]
                    {
                        @"x:\test\myapp\Views\Areas\MyArea\Home\_ViewStart.cshtml",
                        @"x:\test\myapp\Views\Areas\MyArea\_ViewStart.cshtml",
                        @"x:\test\myapp\Views\Areas\_ViewStart.cshtml",
                        @"x:\test\myapp\Views\_ViewStart.cshtml",
                        @"x:\test\myapp\_ViewStart.cshtml",
                    }
                };

                yield return new object[]
                {
                    @"x:",
                    "/Views/View.cshtml",
                    new[]
                    {
                        @"X:\Views\_ViewStart.cshtml",
                        @"X:\_ViewStart.cshtml",
                    }
                };

                yield return new object[]
                {
                    @"x:\test\myapp",
                    "View.cshtml",
                    new[]
                    {
                        @"x:\test\myapp\_ViewStart.cshtml",
                    }
                };

                // TODO: Needs unit tests for Unix-style paths. Tracked via #835
                // Running it on Windows causes incorrect results since Path.GetFullName resolves non-rooted paths 
                // to the current directory
            }
        }

        [Theory]
        [MemberData("GetViewStartLocations_ReturnsPotentialViewStartLocationsData")]
        public void GetViewStartLocations_ReturnsPotentialViewStartLocations(string appPath,
                                                                             string viewPath,
                                                                             IEnumerable<string> expected)
        {
            // Arrange
            var provider = new ViewStartProvider(GetAppEnv(appPath), Mock.Of<IRazorPageFactory>());

            // Act
            var result = provider.GetViewStartLocations(viewPath);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetViewStartPages_ReturnsViewStartsPages_ThatAreAvailableAtSpecifiedPaths()
        {
            // Arrange
            var viewStart1 = Mock.Of<IRazorPage>();
            var viewStart2 = Mock.Of<IRazorPage>();
            var pageFactory = new Mock<IRazorPageFactory>();

            pageFactory.Setup(p => p.CreateInstance(@"Z:\views\_ViewStart.cshtml"))
                       .Returns(viewStart1);
            pageFactory.Setup(p => p.CreateInstance(@"Z:\views\MyArea\MyAreaController\_ViewStart.cshtml"))
                       .Returns(viewStart2);

            var provider = new ViewStartProvider(GetAppEnv(@"z:\"), pageFactory.Object);

            // Act
            var result = provider.GetViewStartPages(@"views\MyArea\MyAreaController\Home.cshtml");

            // Assert
            pageFactory.Verify();
            Assert.Equal(new[] { viewStart1, viewStart2 }, result);
        }

        private static IApplicationEnvironment GetAppEnv(string appPath)
        {
            var appEnv = new Mock<IApplicationEnvironment>();
            appEnv.Setup(p => p.ApplicationBasePath)
                  .Returns(appPath);
            return appEnv.Object;
        }
    }
}