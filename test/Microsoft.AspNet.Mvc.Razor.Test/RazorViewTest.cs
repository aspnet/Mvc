﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor.Test
{
    public class RazorViewTest
    {
        private const string LayoutPath = "~/Shared/_Layout.cshtml";

        [Fact]
        public async Task DefineSection_ThrowsIfSectionIsAlreadyDefined()
        {
            // Arrange
            var view = CreateView(v =>
            {
                v.DefineSection("qux", new HelperResult(action: null));
                v.DefineSection("qux", new HelperResult(action: null));
            });
            var viewContext = CreateViewContext(layoutView: null);

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                                () => view.RenderAsync(viewContext));

            // Assert
            Assert.Equal("Section 'qux' is already defined.", ex.Message);
        }

        [Fact]
        public async Task RenderSection_RendersSectionFromPreviousPage()
        {
            // Arrange
            var expected = new HelperResult(action: null);
            HelperResult actual = null;
            var view = CreateView(v =>
            {
                v.DefineSection("bar", expected);
                v.Layout = LayoutPath;
            });
            var layoutView = CreateView(v =>
            {
                actual = v.RenderSection("bar");
                v.RenderBodyPublic();
            });
            var viewContext = CreateViewContext(layoutView);

            // Act
            await view.RenderAsync(viewContext);

            // Assert
            Assert.Same(actual, expected);
        }

        [Fact]
        public async Task RenderSection_ThrowsIfRequiredSectionIsNotFound()
        {
            // Arrange
            var expected = new HelperResult(action: null);
            var view = CreateView(v =>
            {
                v.DefineSection("baz", expected);
                v.Layout = LayoutPath;
            });
            var layoutView = CreateView(v =>
            {
                v.RenderSection("bar");
            });
            var viewContext = CreateViewContext(layoutView);

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => view.RenderAsync(viewContext));

            // Assert
            Assert.Equal("Section 'bar' is not defined.", ex.Message);
        }

        [Fact]
        public async Task RenderSection_ThrowsIfSectionIsRenderedMoreThanOnce()
        {
            // Arrange
            var expected = new HelperResult(action: null);
            var view = CreateView(v =>
            {
                v.DefineSection("header", expected);
                v.Layout = LayoutPath;
            });
            var layoutView = CreateView(v =>
            {
                v.RenderSection("header");
                v.RenderSection("header");
            });
            var viewContext = CreateViewContext(layoutView);

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => view.RenderAsync(viewContext));

            // Assert
            Assert.Equal("RenderSection has already been called for the section named 'header'.", ex.Message);
        }

        [Fact]
        public async Task RenderAsync_ThrowsIfDefinedSectionIsNotRendered()
        {
            // Arrange
            var expected = new HelperResult(action: null);
            var view = CreateView(v =>
            {
                v.DefineSection("header", expected);
                v.DefineSection("footer", expected);
                v.DefineSection("sectionA", expected);
                v.Layout = LayoutPath;
            });
            var layoutView = CreateView(v =>
            {
                v.RenderSection("sectionA");
                v.RenderBodyPublic();
            });
            var viewContext = CreateViewContext(layoutView);

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => view.RenderAsync(viewContext));

            // Assert
            Assert.Equal("The following sections have been defined but have not been rendered: 'header, footer'.", ex.Message);
        }

        [Fact]
        public async Task RenderAsync_ThrowsIfRenderBodyIsNotCalledFromPage()
        {
            // Arrange
            var expected = new HelperResult(action: null);
            var view = CreateView(v =>
            {
                v.Layout = LayoutPath;
            });
            var layoutView = CreateView(v =>
            {
            });
            var viewContext = CreateViewContext(layoutView);

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => view.RenderAsync(viewContext));

            // Assert
            Assert.Equal("RenderBody must be called from a layout page.", ex.Message);
        }

        [Fact]
        public async Task RenderAsync_RendersSectionsAndBody()
        {
            // Arrange
            var expected = @"Layout start
Header section
body content
Footer section
Layout end
";
            var view = CreateView(v =>
            {
                v.Layout = LayoutPath;
                v.WriteLiteral("body content" + Environment.NewLine);

                v.DefineSection("footer", new HelperResult(writer =>
                {
                    writer.WriteLine("Footer section");
                }));

                v.DefineSection("header", new HelperResult(writer =>
                {
                    writer.WriteLine("Header section");
                }));
            });
            var layoutView = CreateView(v =>
            {
                v.WriteLiteral("Layout start" + Environment.NewLine);
                v.Write(v.RenderSection("header"));
                v.Write(v.RenderBodyPublic());
                v.Write(v.RenderSection("footer"));
                v.WriteLiteral("Layout end" + Environment.NewLine);

            });
            var viewContext = CreateViewContext(layoutView);

            // Act
            await view.RenderAsync(viewContext);

            // Assert
            var actual = ((StringWriter)viewContext.Writer).ToString();
            Assert.Equal(expected, actual);
        }

        private static TestableRazorView CreateView(Action<TestableRazorView> executeAction)
        {
            var view = new Mock<TestableRazorView> { CallBase = true };
            if (executeAction != null)
            {
                view.Setup(v => v.ExecuteAsync())
                    .Callback(() => executeAction(view.Object))
                    .Returns(Task.FromResult(0));
            }

            return view.Object;
        }

        private static ViewContext CreateViewContext(IView layoutView)
        {
            var viewFactory = new Mock<IVirtualPathViewFactory>();
            viewFactory.Setup(v => v.CreateInstance(LayoutPath))
                       .Returns(layoutView);
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(f => f.GetService(typeof(IVirtualPathViewFactory)))
                            .Returns(viewFactory.Object);
            return new ViewContext(serviceProvider.Object, httpContext: null, viewEngineContext: null)
            {
                Writer = new StringWriter()
            };
        }

        public abstract class TestableRazorView : RazorView
        {
            public HtmlString RenderBodyPublic()
            {
                return base.RenderBody();
            }
        }
    }
}