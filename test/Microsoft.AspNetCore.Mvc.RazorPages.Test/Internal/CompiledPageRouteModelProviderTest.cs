﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using static Microsoft.AspNetCore.Razor.Hosting.TestRazorCompiledItem;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class CompiledPageRouteModelProviderTest
    {
        [Fact]
        public void OnProvidersExecuting_AddsModelsForCompiledViews()
        {
            // Arrange
            var descriptors = new[]
            {
                CreateVersion_2_0_Descriptor("/Pages/About.cshtml"),
                CreateVersion_2_0_Descriptor("/Pages/Home.cshtml", "some-prefix"),
            };

            var provider = CreateProvider(descriptors: descriptors);
            var context = new PageRouteModelProviderContext();

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            Assert.Collection(
                context.RouteModels,
                result =>
                {
                    Assert.Equal("/Pages/About.cshtml", result.RelativePath);
                    Assert.Equal("/About", result.ViewEnginePath);
                    Assert.Collection(
                        result.Selectors,
                        selector => Assert.Equal("About", selector.AttributeRouteModel.Template));
                    Assert.Collection(
                        result.RouteValues.OrderBy(k => k.Key),
                        kvp =>
                        {
                            Assert.Equal("page", kvp.Key);
                            Assert.Equal("/About", kvp.Value);
                        });
                },
                result =>
                {
                    Assert.Equal("/Pages/Home.cshtml", result.RelativePath);
                    Assert.Equal("/Home", result.ViewEnginePath);
                    Assert.Collection(
                        result.Selectors,
                        selector => Assert.Equal("Home/some-prefix", selector.AttributeRouteModel.Template));
                    Assert.Collection(
                        result.RouteValues.OrderBy(k => k.Key),
                        kvp =>
                        {
                            Assert.Equal("page", kvp.Key);
                            Assert.Equal("/Home", kvp.Value);
                        });
                });
        }

        [Fact] // 2.1 adds some additional metadata to the view descriptors. We want to make sure both versions work.
        public void OnProvidersExecuting_AddsModelsForCompiledViews_Version_2_1()
        {
            // Arrange
            var descriptors = new[]
            {
                CreateVersion_2_1_Descriptor("/Pages/About.cshtml"),
                CreateVersion_2_1_Descriptor("/Pages/Home.cshtml", metadata: new[]
                {
                    new RazorCompiledItemMetadataAttribute("RouteTemplate", "some-prefix"),
                }),
            };

            var provider = CreateProvider(descriptors: descriptors);
            var context = new PageRouteModelProviderContext();

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            Assert.Collection(
                context.RouteModels,
                result =>
                {
                    Assert.Equal("/Pages/About.cshtml", result.RelativePath);
                    Assert.Equal("/About", result.ViewEnginePath);
                    Assert.Collection(
                        result.Selectors,
                        selector => Assert.Equal("About", selector.AttributeRouteModel.Template));
                    Assert.Collection(
                        result.RouteValues.OrderBy(k => k.Key),
                        kvp =>
                        {
                            Assert.Equal("page", kvp.Key);
                            Assert.Equal("/About", kvp.Value);
                        });
                },
                result =>
                {
                    Assert.Equal("/Pages/Home.cshtml", result.RelativePath);
                    Assert.Equal("/Home", result.ViewEnginePath);
                    Assert.Collection(
                        result.Selectors,
                        selector => Assert.Equal("Home/some-prefix", selector.AttributeRouteModel.Template));
                    Assert.Collection(
                        result.RouteValues.OrderBy(k => k.Key),
                        kvp =>
                        {
                            Assert.Equal("page", kvp.Key);
                            Assert.Equal("/Home", kvp.Value);
                        });
                });
        }

        [Fact]
        public void OnProvidersExecuting_ValidatesChecksum_RejectsPageWhenContentDoesntMatch()
        {
            // Arrange
            var descriptors = new[]
            {
                CreateVersion_2_1_Descriptor("/Pages/About.cshtml", metadata: new object[]
                {
                    new RazorSourceChecksumAttribute("SHA1", GetChecksum("some content"), "/Pages/About.cshtml"),
                }),
            };

            var fileSystem = new VirtualRazorProjectFileSystem();
            fileSystem.Add(new TestRazorProjectItem("/Pages/About.cshtml", "some other content"));

            var provider = CreateProvider(descriptors: descriptors, fileSystem: fileSystem);
            var context = new PageRouteModelProviderContext();

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            Assert.Empty(context.RouteModels);
        }

        [Fact]
        public void OnProvidersExecuting_ValidatesChecksum_AcceptsPageWhenContentMatches()
        {
            // Arrange
            var descriptors = new[]
            {
                CreateVersion_2_1_Descriptor("/Pages/About.cshtml", metadata: new object[]
                {
                    new RazorSourceChecksumAttribute("SHA1", GetChecksum("some content"), "/Pages/About.cshtml"),
                    new RazorSourceChecksumAttribute("SHA1", GetChecksum("some import"), "/Pages/_ViewImports.cshtml"),
                }),
            };

            var fileSystem = new VirtualRazorProjectFileSystem();
            fileSystem.Add(new TestRazorProjectItem("/Pages/About.cshtml", "some content"));
            fileSystem.Add(new TestRazorProjectItem("/Pages/_ViewImports.cshtml", "some import"));

            var provider = CreateProvider(descriptors: descriptors, fileSystem: fileSystem);
            var context = new PageRouteModelProviderContext();

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            Assert.Collection(
                context.RouteModels,
                result => Assert.Equal("/Pages/About.cshtml", result.RelativePath));
        }

        [Fact]
        public void OnProvidersExecuting_ValidatesChecksum_SkipsValidationWhenMainSourceMissing()
        {
            // Arrange
            var descriptors = new[]
            {
                CreateVersion_2_1_Descriptor("/Pages/About.cshtml", metadata: new object[]
                {
                    new RazorSourceChecksumAttribute("SHA1", GetChecksum("some content"), "/Pages/About.cshtml"),
                    new RazorSourceChecksumAttribute("SHA1", GetChecksum("some import"), "/Pages/_ViewImports.cshtml"),
                }),
            };

            var fileSystem = new VirtualRazorProjectFileSystem();
            fileSystem.Add(new TestRazorProjectItem("/Pages/_ViewImports.cshtml", "some other import"));

            var provider = CreateProvider(descriptors: descriptors, fileSystem: fileSystem);
            var context = new PageRouteModelProviderContext();

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            Assert.Collection(
                context.RouteModels,
                result => Assert.Equal("/Pages/About.cshtml", result.RelativePath));
        }

        [Fact]
        public void OnProvidersExecuting_AddsModelsForCompiledAreaPages()
        {
            // Arrange
            var descriptors = new[]
            {
                CreateVersion_2_0_Descriptor("/Features/Products/Files/About.cshtml"),
                CreateVersion_2_0_Descriptor("/Features/Products/Files/Manage/Index.cshtml"),
                CreateVersion_2_0_Descriptor("/Features/Products/Files/Manage/Edit.cshtml", "{id}"),
            };

            var options = new RazorPagesOptions
            {
                AllowAreas = true,
                AreaRootDirectory = "/Features",
                RootDirectory = "/Files",
            };

            var provider = CreateProvider(options: options, descriptors: descriptors);
            var context = new PageRouteModelProviderContext();

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            Assert.Collection(
                context.RouteModels,
                result =>
                {
                    Assert.Equal("/Features/Products/Files/About.cshtml", result.RelativePath);
                    Assert.Equal("/About", result.ViewEnginePath);
                    Assert.Collection(
                        result.Selectors,
                        selector => Assert.Equal("Products/About", selector.AttributeRouteModel.Template));
                    Assert.Collection(
                        result.RouteValues.OrderBy(k => k.Key),
                        kvp =>
                        {
                            Assert.Equal("area", kvp.Key);
                            Assert.Equal("Products", kvp.Value);
                        },
                        kvp =>
                        {
                            Assert.Equal("page", kvp.Key);
                            Assert.Equal("/About", kvp.Value);
                        });
                },
                result =>
                {
                    Assert.Equal("/Features/Products/Files/Manage/Index.cshtml", result.RelativePath);
                    Assert.Equal("/Manage/Index", result.ViewEnginePath);
                    Assert.Collection(result.Selectors,
                        selector => Assert.Equal("Products/Manage/Index", selector.AttributeRouteModel.Template),
                        selector => Assert.Equal("Products/Manage", selector.AttributeRouteModel.Template));
                    Assert.Collection(
                        result.RouteValues.OrderBy(k => k.Key),
                        kvp =>
                        {
                            Assert.Equal("area", kvp.Key);
                            Assert.Equal("Products", kvp.Value);
                        },
                        kvp =>
                        {
                            Assert.Equal("page", kvp.Key);
                            Assert.Equal("/Manage/Index", kvp.Value);
                        });
                },
                result =>
                {
                    Assert.Equal("/Features/Products/Files/Manage/Edit.cshtml", result.RelativePath);
                    Assert.Equal("/Manage/Edit", result.ViewEnginePath);
                    Assert.Collection(
                        result.Selectors,
                        selector => Assert.Equal("Products/Manage/Edit/{id}", selector.AttributeRouteModel.Template));
                    Assert.Collection(
                        result.RouteValues.OrderBy(k => k.Key),
                        kvp =>
                        {
                            Assert.Equal("area", kvp.Key);
                            Assert.Equal("Products", kvp.Value);
                        },
                        kvp =>
                        {
                            Assert.Equal("page", kvp.Key);
                            Assert.Equal("/Manage/Edit", kvp.Value);
                        });
                });
        }

        [Fact]
        public void OnProvidersExecuting_DoesNotAddsModelsForAreaPages_IfFeatureIsDisabled()
        {
            // Arrange
            var descriptors = new[]
            {
                CreateVersion_2_0_Descriptor("/Pages/About.cshtml"),
                CreateVersion_2_0_Descriptor("/Areas/Accounts/Pages/Home.cshtml"),
            };

            var options = new RazorPagesOptions { AllowAreas = false };

            var provider = CreateProvider(options: options, descriptors: descriptors);
            var context = new PageRouteModelProviderContext();

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            Assert.Collection(
                context.RouteModels,
                result =>
                {
                    Assert.Equal("/Pages/About.cshtml", result.RelativePath);
                    Assert.Equal("/About", result.ViewEnginePath);
                    Assert.Collection(
                        result.Selectors,
                        selector => Assert.Equal("About", selector.AttributeRouteModel.Template));
                    Assert.Collection(
                        result.RouteValues.OrderBy(k => k.Key),
                        kvp =>
                        {
                            Assert.Equal("page", kvp.Key);
                            Assert.Equal("/About", kvp.Value);
                        });
                });
        }

        [Fact]
        public void OnProvidersExecuting_DoesNotAddAreaAndNonAreaRoutesForAPage()
        {
            // Arrange
            var descriptors = new[]
            {
                CreateVersion_2_0_Descriptor("/Areas/Accounts/Manage/Home.cshtml"),
                CreateVersion_2_0_Descriptor("/Areas/About.cshtml"),
                CreateVersion_2_0_Descriptor("/Contact.cshtml"),
            };

            var options = new RazorPagesOptions
            {
                AllowAreas = true,
                AreaRootDirectory = "/Areas",
                RootDirectory = "/",
            };

            var provider = CreateProvider(options: options, descriptors: descriptors);
            var context = new PageRouteModelProviderContext();

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            Assert.Collection(
                context.RouteModels,
                result =>
                {
                    Assert.Equal("/Areas/Accounts/Manage/Home.cshtml", result.RelativePath);
                    Assert.Equal("/Manage/Home", result.ViewEnginePath);
                    Assert.Collection(
                        result.Selectors,
                        selector => Assert.Equal("Accounts/Manage/Home", selector.AttributeRouteModel.Template));
                    Assert.Collection(
                        result.RouteValues.OrderBy(k => k.Key),
                        kvp =>
                        {
                            Assert.Equal("area", kvp.Key);
                            Assert.Equal("Accounts", kvp.Value);
                        },
                        kvp =>
                        {
                            Assert.Equal("page", kvp.Key);
                            Assert.Equal("/Manage/Home", kvp.Value);
                        });
                },
                result =>
                {
                    Assert.Equal("/Contact.cshtml", result.RelativePath);
                    Assert.Equal("/Contact", result.ViewEnginePath);
                    Assert.Collection(
                        result.Selectors,
                        selector => Assert.Equal("Contact", selector.AttributeRouteModel.Template));
                    Assert.Collection(
                        result.RouteValues.OrderBy(k => k.Key),
                        kvp =>
                        {
                            Assert.Equal("page", kvp.Key);
                            Assert.Equal("/Contact", kvp.Value);
                        });
                });
        }

        [Fact]
        public void OnProvidersExecuting_AddsMultipleSelectorsForIndexPage_WithIndexAtRoot()
        {
            // Arrange
            var descriptors = new[]
            {
                CreateVersion_2_0_Descriptor("/Pages/Index.cshtml"),
                CreateVersion_2_0_Descriptor("/Pages/Admin/Index.cshtml", "some-template"),
            };
            var options = new RazorPagesOptions { RootDirectory = "/" };

            var provider = CreateProvider(options: options, descriptors: descriptors);
            var context = new PageRouteModelProviderContext();

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            Assert.Collection(
                context.RouteModels,
                result =>
                {
                    Assert.Equal("/Pages/Index.cshtml", result.RelativePath);
                    Assert.Equal("/Pages/Index", result.ViewEnginePath);
                    Assert.Collection(
                        result.Selectors,
                        selector => Assert.Equal("Pages/Index", selector.AttributeRouteModel.Template),
                        selector => Assert.Equal("Pages", selector.AttributeRouteModel.Template));
                },
                result =>
                {
                    Assert.Equal("/Pages/Admin/Index.cshtml", result.RelativePath);
                    Assert.Equal("/Pages/Admin/Index", result.ViewEnginePath);
                    Assert.Collection(
                        result.Selectors,
                        selector => Assert.Equal("Pages/Admin/Index/some-template", selector.AttributeRouteModel.Template),
                        selector => Assert.Equal("Pages/Admin/some-template", selector.AttributeRouteModel.Template));
                });
        }

        [Fact]
        public void OnProvidersExecuting_AddsMultipleSelectorsForIndexPage()
        {
            // Arrange
            var descriptors = new[]
            {
                CreateVersion_2_0_Descriptor("/Pages/Index.cshtml"),
                CreateVersion_2_0_Descriptor("/Pages/Admin/Index.cshtml", "some-template"),
            };

            var provider = CreateProvider(descriptors: descriptors);
            var context = new PageRouteModelProviderContext();

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            Assert.Collection(
                context.RouteModels,
                result =>
                {
                    Assert.Equal("/Pages/Index.cshtml", result.RelativePath);
                    Assert.Equal("/Index", result.ViewEnginePath);
                    Assert.Collection(
                        result.Selectors,
                        selector => Assert.Equal("Index", selector.AttributeRouteModel.Template),
                        selector => Assert.Equal("", selector.AttributeRouteModel.Template));
                },
                result =>
                {
                    Assert.Equal("/Pages/Admin/Index.cshtml", result.RelativePath);
                    Assert.Equal("/Admin/Index", result.ViewEnginePath);
                    Assert.Collection(
                        result.Selectors,
                        selector => Assert.Equal("Admin/Index/some-template", selector.AttributeRouteModel.Template),
                        selector => Assert.Equal("Admin/some-template", selector.AttributeRouteModel.Template));
                });
        }

        [Fact]
        public void OnProvidersExecuting_AllowsRouteTemplatesWithOverridePattern()
        {
            // Arrange
            var descriptors = new[]
            {
                CreateVersion_2_0_Descriptor("/Pages/Index.cshtml", "~/some-other-prefix"),
                CreateVersion_2_0_Descriptor("/Pages/Home.cshtml", "/some-prefix"),
            };

            var provider = CreateProvider(descriptors: descriptors);
            var context = new PageRouteModelProviderContext();

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            Assert.Collection(
                context.RouteModels,
                result =>
                {
                    Assert.Equal("/Pages/Index.cshtml", result.RelativePath);
                    Assert.Equal("/Index", result.ViewEnginePath);
                    Assert.Collection(
                        result.Selectors,
                        selector => Assert.Equal("some-other-prefix", selector.AttributeRouteModel.Template));
                },
                result =>
                {
                    Assert.Equal("/Pages/Home.cshtml", result.RelativePath);
                    Assert.Equal("/Home", result.ViewEnginePath);
                    Assert.Collection(
                        result.Selectors,
                        selector => Assert.Equal("some-prefix", selector.AttributeRouteModel.Template));
                });
        }

        [Fact]
        public void OnProvidersExecuting_UsesTheFirstDescriptorForEachPath()
        {
            // ViewsFeature may contain duplicate entries for the same Page - for instance when an app overloads a library's views.
            // It picks the first entry for each path. In the ordinary case, this should ensure that the app's Razor Pages are prefered
            // to a Razor Page added by a library.

            // Arrange
            var descriptors = new[]
            {
                // Page coming from the app
                CreateVersion_2_1_Descriptor("/Pages/About.cshtml", metadata: new object[]
                {
                    new RazorSourceChecksumAttribute("SHA1", GetChecksum("some content"), "/Pages/About.cshtml"),
                }),
                CreateVersion_2_1_Descriptor("/Pages/Home.cshtml", metadata: new object[]
                {
                    new RazorSourceChecksumAttribute("SHA1", GetChecksum("some content"), "/Pages/Index.cshtml"),
                }),
                // Page coming from the app
                CreateVersion_2_1_Descriptor("/Pages/About.cshtml", metadata: new object[]
                {
                    new RazorSourceChecksumAttribute("SHA1", GetChecksum("some content"), "/Pages/About.cshtml"),
                }),
            };

            var provider = CreateProvider(descriptors: descriptors);
            var context = new PageRouteModelProviderContext();

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            Assert.Collection(
                context.RouteModels,
                result =>
                {
                    Assert.Equal("/Pages/About.cshtml", result.RelativePath);
                    Assert.Equal("/About", result.ViewEnginePath);
                },
                result =>
                {
                    Assert.Equal("/Pages/Home.cshtml", result.RelativePath);
                    Assert.Equal("/Home", result.ViewEnginePath);
                });
        }

        [Fact]
        public void GetRouteTemplate_ReturnsPathFromRazorPageAttribute()
        {
            // Arrange
            var expected = "test";
            var descriptor = CreateVersion_2_0_Descriptor("/Pages/Home.cshtml", expected);

            // Act
            var result = CompiledPageRouteModelProvider.GetRouteTemplate(descriptor);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetRouteTemplate_ReturnsNull_IfPageAttributeDoesNotHaveTemplate()
        {
            // Arrange
            var descriptor = CreateVersion_2_0_Descriptor("/Pages/Home.cshtml", routeTemplate: null);

            // Act
            var result = CompiledPageRouteModelProvider.GetRouteTemplate(descriptor);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetRouteTemplate_ReturnsPathFromMetadataAttribute()
        {
            // Arrange
            var expected = "test";
            var descriptor = CreateVersion_2_1_Descriptor("/Pages/About.cshtml", metadata: new object[]
            {
                new RazorCompiledItemMetadataAttribute("RouteTemplate", expected),
            });

            // Act
            var result = CompiledPageRouteModelProvider.GetRouteTemplate(descriptor);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetRouteTemplate_ReturnsNull_IfAttributeDoesNotExist()
        {
            // Arrange
            var descriptor = CreateVersion_2_1_Descriptor("/Pages/About.cshtml");

            // Act
            var result = CompiledPageRouteModelProvider.GetRouteTemplate(descriptor);

            // Assert
            Assert.Null(result);
        }

        private TestCompiledPageRouteModelProvider CreateProvider(
           RazorPagesOptions options = null,
           IList<CompiledViewDescriptor> descriptors = null,
           VirtualRazorProjectFileSystem fileSystem = null)
        {
            options = options ?? new RazorPagesOptions();
            fileSystem = fileSystem ?? new VirtualRazorProjectFileSystem();
            var projectEngine = RazorProjectEngine.Create(RazorConfiguration.Default, fileSystem);

            var provider = new TestCompiledPageRouteModelProvider(
                new ApplicationPartManager(),
                Options.Create(options),
                projectEngine,
                NullLogger<CompiledPageRouteModelProvider>.Instance);

            provider.Descriptors.AddRange(descriptors ?? Array.Empty<CompiledViewDescriptor>());

            return provider;
        }

        private static CompiledViewDescriptor CreateVersion_2_0_Descriptor(string path, string routeTemplate = "")
        {
            return new CompiledViewDescriptor
            {
                IsPrecompiled = true,
                RelativePath = path,
                ViewAttribute = new RazorPageAttribute(path, typeof(object), routeTemplate),
            };
        }

        private static CompiledViewDescriptor CreateVersion_2_1_Descriptor(
            string path,
            object[] metadata = null)
        {
            return new CompiledViewDescriptor
            {
                IsPrecompiled = true,
                RelativePath = path,
                Item = new TestRazorCompiledItem(typeof(object), "mvc.1.0.razor-page", path, metadata ?? Array.Empty<object>()),
            };
        }

        public class TestCompiledPageRouteModelProvider : CompiledPageRouteModelProvider
        {
            public TestCompiledPageRouteModelProvider(
                ApplicationPartManager partManager,
                IOptions<RazorPagesOptions> options,
                RazorProjectEngine projectEngine,
                ILogger<CompiledPageRouteModelProvider> logger)
                : base(partManager, options, projectEngine, logger)
            {
            }

            public List<CompiledViewDescriptor> Descriptors { get; } = new List<CompiledViewDescriptor>();

            protected override ViewsFeature GetViewFeature(ApplicationPartManager applicationManager)
            {
                var feature = new ViewsFeature();
                foreach (var descriptor in Descriptors)
                {
                    feature.ViewDescriptors.Add(descriptor);
                }

                return feature;
            }
        }
    }
}