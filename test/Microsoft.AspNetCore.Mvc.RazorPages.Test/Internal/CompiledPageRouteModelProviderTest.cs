﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static Microsoft.AspNetCore.Razor.Hosting.TestRazorCompiledItem;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class CompiledPageRouteModelProviderTest
    {
        public CompiledPageRouteModelProviderTest()
        {
            FileProvider = new TestFileProvider();
            Project = new FileProviderRazorProject(
                Mock.Of<IRazorViewEngineFileProviderAccessor>(a => a.FileProvider == FileProvider),
                Mock.Of<IHostingEnvironment>(e => e.ContentRootPath == "BasePath"));
            TemplateEngine = new RazorTemplateEngine(RazorEngine.Create(), Project);

            PagesOptions = new RazorPagesOptions();
            Provider = new TestCompiledPageRouteModelProvider(new ApplicationPartManager(), Options.Create(PagesOptions), TemplateEngine, NullLoggerFactory.Instance);
        }

        public TestFileProvider FileProvider { get; }

        public RazorProject Project { get; }

        public RazorTemplateEngine TemplateEngine { get; }

        public RazorPagesOptions PagesOptions { get; }

        public TestCompiledPageRouteModelProvider Provider { get; }

        [Fact]
        public void OnProvidersExecuting_AddsModelsForCompiledViews()
        {
            // Arrange
            Provider.Descriptors.AddRange(new[]
            {
                CreateVersion_2_0_Descriptor("/Pages/About.cshtml"),
                CreateVersion_2_0_Descriptor("/Pages/Home.cshtml", "some-prefix"),
            });

            var context = new PageRouteModelProviderContext();

            // Act
            Provider.OnProvidersExecuting(context);

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
            Provider.Descriptors.AddRange(new[]
            {
                CreateVersion_2_1_Descriptor("/Pages/About.cshtml"),
                CreateVersion_2_1_Descriptor("/Pages/Home.cshtml", "some-prefix"),
            });

            var context = new PageRouteModelProviderContext();

            // Act
            Provider.OnProvidersExecuting(context);

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
            Provider.Descriptors.AddRange(new[]
            {
                CreateVersion_2_1_Descriptor("/Pages/About.cshtml", metadata: new object[]
                {
                    new RazorSourceChecksumAttribute("SHA1", GetChecksum("some content"), "/Pages/About.cshtml"),
                }),
            });

            FileProvider.AddFile("/Pages/About.cshtml", "some other content");

            var context = new PageRouteModelProviderContext();

            // Act
            Provider.OnProvidersExecuting(context);

            // Assert
            Assert.Empty(context.RouteModels);
        }

        [Fact]
        public void OnProvidersExecuting_ValidatesChecksum_AcceptsPageWhenContentMatches()
        {
            // Arrange
            Provider.Descriptors.AddRange(new[]
            {
                CreateVersion_2_1_Descriptor("/Pages/About.cshtml", metadata: new object[]
                {
                    new RazorSourceChecksumAttribute("SHA1", GetChecksum("some content"), "/Pages/About.cshtml"),
                    new RazorSourceChecksumAttribute("SHA1", GetChecksum("some import"), "/Pages/_ViewImports.cshtml"),
                }),
            });

            FileProvider.AddFile("/Pages/About.cshtml", "some content");
            FileProvider.AddFile("/Pages/_ViewImports.cshtml", "some import");

            var context = new PageRouteModelProviderContext();

            // Act
            Provider.OnProvidersExecuting(context);

            // Assert
            Assert.Collection(
                context.RouteModels,
                result => Assert.Equal("/Pages/About.cshtml", result.RelativePath));
        }

        [Fact]
        public void OnProvidersExecuting_ValidatesChecksum_SkipsValidationWhenMainSourceMissing()
        {
            // Arrange
            Provider.Descriptors.AddRange(new[]
            {
                CreateVersion_2_1_Descriptor("/Pages/About.cshtml", metadata: new object[]
                {
                    new RazorSourceChecksumAttribute("SHA1", GetChecksum("some content"), "/Pages/About.cshtml"),
                    new RazorSourceChecksumAttribute("SHA1", GetChecksum("some import"), "/Pages/_ViewImports.cshtml"),
                }),
            });

            FileProvider.AddFile("/Pages/_ViewImports.cshtml", "some other import");

            var context = new PageRouteModelProviderContext();

            // Act
            Provider.OnProvidersExecuting(context);

            // Assert
            Assert.Collection(
                context.RouteModels,
                result => Assert.Equal("/Pages/About.cshtml", result.RelativePath));
        }

        [Fact]
        public void OnProvidersExecuting_AddsModelsForCompiledAreaPages()
        {
            // Arrange
            Provider.Descriptors.AddRange(new[]
            {
                CreateVersion_2_0_Descriptor("/Features/Products/Files/About.cshtml"),
                CreateVersion_2_0_Descriptor("/Features/Products/Files/Manage/Index.cshtml"),
                CreateVersion_2_0_Descriptor("/Features/Products/Files/Manage/Edit.cshtml", "{id}"),
            });

            PagesOptions.AllowAreas = true;
            PagesOptions.AreaRootDirectory = "/Features";
            PagesOptions.RootDirectory = "/Files";

            var context = new PageRouteModelProviderContext();

            // Act
            Provider.OnProvidersExecuting(context);

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
            Provider.Descriptors.AddRange(new[]
            {
                CreateVersion_2_0_Descriptor("/Pages/About.cshtml"),
                CreateVersion_2_0_Descriptor("/Areas/Accounts/Pages/Home.cshtml"),
            });

            PagesOptions.AllowAreas = false;

            var context = new PageRouteModelProviderContext();

            // Act
            Provider.OnProvidersExecuting(context);

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
            Provider.Descriptors.AddRange(new[]
            {
                CreateVersion_2_0_Descriptor("/Areas/Accounts/Manage/Home.cshtml"),
                CreateVersion_2_0_Descriptor("/Areas/About.cshtml"),
                CreateVersion_2_0_Descriptor("/Contact.cshtml"),
            });

            PagesOptions.AllowAreas = true;
            PagesOptions.AreaRootDirectory = "/Areas";
            PagesOptions.RootDirectory = "/";

            var context = new PageRouteModelProviderContext();

            // Act
            Provider.OnProvidersExecuting(context);

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
            Provider.Descriptors.AddRange(new[]
            {
                CreateVersion_2_0_Descriptor("/Pages/Index.cshtml"),
                CreateVersion_2_0_Descriptor("/Pages/Admin/Index.cshtml", "some-template"),
            });

            PagesOptions.RootDirectory = "/";

            var context = new PageRouteModelProviderContext();

            // Act
            Provider.OnProvidersExecuting(context);

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
            Provider.Descriptors.AddRange(new[]
            {
                CreateVersion_2_0_Descriptor("/Pages/Index.cshtml"),
                CreateVersion_2_0_Descriptor("/Pages/Admin/Index.cshtml", "some-template"),
            });

            var context = new PageRouteModelProviderContext();

            // Act
            Provider.OnProvidersExecuting(context);

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
            Provider.Descriptors.AddRange(new[]
            {
                CreateVersion_2_0_Descriptor("/Pages/Index.cshtml", "~/some-other-prefix"),
                CreateVersion_2_0_Descriptor("/Pages/Home.cshtml", "/some-prefix"),
            });

            var context = new PageRouteModelProviderContext();

            // Act
            Provider.OnProvidersExecuting(context);

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

        private static CompiledViewDescriptor CreateVersion_2_0_Descriptor(string path, string routeTemplate = "")
        {
            return new CompiledViewDescriptor
            {
                RelativePath = path,
                ViewAttribute = new RazorPageAttribute(path, typeof(object), routeTemplate),
            };
        }

        private static CompiledViewDescriptor CreateVersion_2_1_Descriptor(
            string path,
            string routeTemplate = "",
            object[] metadata = null)
        {
            return new CompiledViewDescriptor
            {
                RelativePath = path,
                ViewAttribute = new RazorPageAttribute(path, typeof(object), routeTemplate),
                Item = new TestRazorCompiledItem(typeof(object), "mvc.1.0.razor-page", path, metadata ?? Array.Empty<object>()),
            };
        }

        public class TestCompiledPageRouteModelProvider : CompiledPageRouteModelProvider
        {
            public TestCompiledPageRouteModelProvider(
                ApplicationPartManager partManager,
                IOptions<RazorPagesOptions> options,
                RazorTemplateEngine templateEngine,
                ILoggerFactory loggerFactory)
                : base(partManager, options, templateEngine, loggerFactory)
            {
            }

            public List<CompiledViewDescriptor> Descriptors { get; } = new List<CompiledViewDescriptor>();

            protected override IEnumerable<CompiledViewDescriptor> GetViewDescriptors(ApplicationPartManager applicationManager) => Descriptors;
        }
    }
}
