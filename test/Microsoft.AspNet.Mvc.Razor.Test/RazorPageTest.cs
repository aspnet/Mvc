// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Core;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.PageExecutionInstrumentation;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.AspNet.Testing;
using Microsoft.Framework.WebEncoders;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class RazorPageTest
    {
#pragma warning disable 1998
        private readonly RenderAsyncDelegate _nullRenderAsyncDelegate = async writer => { };
#pragma warning restore 1998

        [Fact]
        public async Task WritingScopesRedirectContentWrittenToViewContextWriter()
        {
            // Arrange
            var viewContext = CreateViewContext();
            var page = CreatePage(v =>
            {
                v.HtmlEncoder = new HtmlEncoder();
                v.Write("Hello Prefix");
                v.StartTagHelperWritingScope();
                v.Write("Hello from Output");
                v.ViewContext.Writer.Write("Hello from view context writer");
                var scopeValue = v.EndTagHelperWritingScope();
                v.Write("From Scope: " + scopeValue.ToString());
            });

            // Act
            await page.ExecuteAsync();
            var pageOutput = page.Output.ToString();

            // Assert
            Assert.Equal("Hello PrefixFrom Scope: Hello from OutputHello from view context writer", pageOutput);
        }

        [Fact]
        public async Task WritingScopesRedirectsContentWrittenToOutput()
        {
            // Arrange
            var viewContext = CreateViewContext();
            var page = CreatePage(v =>
            {
                v.HtmlEncoder = new HtmlEncoder();
                v.Write("Hello Prefix");
                v.StartTagHelperWritingScope();
                v.Write("Hello In Scope");
                var scopeValue = v.EndTagHelperWritingScope();
                v.Write("From Scope: " + scopeValue.ToString());
            });

            // Act
            await page.ExecuteAsync();
            var pageOutput = page.Output.ToString();

            // Assert
            Assert.Equal("Hello PrefixFrom Scope: Hello In Scope", pageOutput);
        }

        [Fact]
        public async Task WritingScopesCanNest()
        {
            // Arrange
            var viewContext = CreateViewContext();
            var page = CreatePage(v =>
            {
                v.HtmlEncoder = new HtmlEncoder();
                v.Write("Hello Prefix");
                v.StartTagHelperWritingScope();
                v.Write("Hello In Scope Pre Nest");

                v.StartTagHelperWritingScope();
                v.Write("Hello In Nested Scope");
                var scopeValue1 = v.EndTagHelperWritingScope();

                v.Write("Hello In Scope Post Nest");
                var scopeValue2 = v.EndTagHelperWritingScope();

                v.Write("From Scopes: " + scopeValue2.ToString() + scopeValue1.ToString());
            });

            // Act
            await page.ExecuteAsync();
            var pageOutput = page.Output.ToString();

            // Assert
            Assert.Equal("Hello PrefixFrom Scopes: Hello In Scope Pre NestHello In Scope Post NestHello In Nested Scope", pageOutput);
        }

        [Fact]
        public async Task StartNewWritingScope_CannotFlushInWritingScope()
        {
            // Arrange
            var viewContext = CreateViewContext();
            var page = CreatePage(async v =>
            {
                v.StartTagHelperWritingScope();
                await v.FlushAsync();
            });

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                                () => page.ExecuteAsync());

            // Assert
            Assert.Equal("You cannot flush while inside a writing scope.", ex.Message);
        }

        [Fact]
        public async Task StartNewWritingScope_CannotEndWritingScopeWhenNoWritingScope()
        {
            // Arrange
            var viewContext = CreateViewContext();
            var page = CreatePage(v =>
            {
                v.EndTagHelperWritingScope();
            });

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                                () => page.ExecuteAsync());

            // Assert
            Assert.Equal("There is no active writing scope to end.", ex.Message);
        }

        [Fact]
        public async Task EndTagHelperWritingScope_ReturnsAppropriateContent()
        {
            // Arrange
            var viewContext = CreateViewContext();

            // Act
            var page = CreatePage(v =>
            {
                v.HtmlEncoder = new HtmlEncoder();
                v.StartTagHelperWritingScope();
                v.Write("Hello World!");
                var returnValue = v.EndTagHelperWritingScope();

                // Assert
                var content = Assert.IsType<DefaultTagHelperContent>(returnValue);
                Assert.Equal("Hello World!", content.GetContent());
            });
            await page.ExecuteAsync();
        }

        [Fact]
        public async Task EndTagHelperWritingScope_CopiesContent_IfRazorTextWriter()
        {
            // Arrange
            var viewContext = CreateViewContext();

            // Act
            var page = CreatePage(v =>
            {
                v.HtmlEncoder = new HtmlEncoder();
                v.StartTagHelperWritingScope(new RazorTextWriter(TextWriter.Null, Encoding.UTF8));
                v.Write("Hello ");
                v.Write("World!");
                var returnValue = v.EndTagHelperWritingScope();

                // Assert
                var content = Assert.IsType<DefaultTagHelperContent>(returnValue);
                Assert.Equal("Hello World!", content.GetContent());
                Assert.Equal(new[] { "Hello ", "World!" }, content.AsArray());
            }, viewContext);
            await page.ExecuteAsync();
        }

        [Fact]
        public async Task DefineSection_ThrowsIfSectionIsAlreadyDefined()
        {
            // Arrange
            var viewContext = CreateViewContext();
            var page = CreatePage(v =>
            {
                v.DefineSection("qux", _nullRenderAsyncDelegate);
                v.DefineSection("qux", _nullRenderAsyncDelegate);
            });

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                                () => page.ExecuteAsync());

            // Assert
            Assert.Equal("Section 'qux' is already defined.", ex.Message);
        }

        [Fact]
        public async Task RenderSection_RendersSectionFromPreviousPage()
        {
            // Arrange
            var expected = "Hello world";
            var viewContext = CreateViewContext();
            var page = CreatePage(v =>
            {
                v.Write(v.RenderSection("bar"));
            });
            page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
            {
                { "bar", writer => writer.WriteAsync(expected) }
            };

            // Act
            await page.ExecuteAsync();

            // Assert
            Assert.Equal(expected, page.RenderedContent);
        }

        [Fact]
        public async Task RenderSection_ThrowsIfPreviousSectionWritersIsNotSet()
        {
            // Arrange
            Exception ex = null;
            var page = CreatePage(v =>
            {
                ex = Assert.Throws<InvalidOperationException>(() => v.RenderSection("bar"));
            });

            // Act
            await page.ExecuteAsync();

            // Assert
            Assert.Equal("RenderSection can only be called from a layout page.",
                         ex.Message);
        }

        [Fact]
        public async Task RenderSection_ThrowsIfRequiredSectionIsNotFound()
        {
            // Arrange
            var page = CreatePage(v =>
            {
                v.RenderSection("bar");
            });
            page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
            {
                { "baz", _nullRenderAsyncDelegate }
            };

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => page.ExecuteAsync());

            // Assert
            Assert.Equal("Section 'bar' is not defined.", ex.Message);
        }

        [Fact]
        public void IsSectionDefined_ThrowsIfPreviousSectionWritersIsNotRegistered()
        {
            // Arrange
            var page = CreatePage(v => { });

            // Act and Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => page.IsSectionDefined("foo"),
                "IsSectionDefined can only be called from a layout page.");
        }

        [Fact]
        public async Task IsSectionDefined_ReturnsFalseIfSectionNotDefined()
        {
            // Arrange
            bool? actual = null;
            var page = CreatePage(v =>
            {
                actual = v.IsSectionDefined("foo");
                v.RenderSection("baz");
                v.RenderBodyPublic();
            });
            page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
            {
                { "baz", _nullRenderAsyncDelegate }
            };
            page.RenderBodyDelegate = CreateBodyAction("body-content");

            // Act
            await page.ExecuteAsync();

            // Assert
            Assert.Equal(false, actual);
        }

        [Fact]
        public async Task IsSectionDefined_ReturnsTrueIfSectionDefined()
        {
            // Arrange
            bool? actual = null;
            var page = CreatePage(v =>
            {
                actual = v.IsSectionDefined("baz");
                v.RenderSection("baz");
                v.RenderBodyPublic();
            });
            page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
            {
                { "baz", _nullRenderAsyncDelegate }
            };
            page.RenderBodyDelegate = CreateBodyAction("body-content");

            // Act
            await page.ExecuteAsync();

            // Assert
            Assert.Equal(true, actual);
        }

        [Fact]
        public async Task RenderSection_ThrowsIfSectionIsRenderedMoreThanOnce()
        {
            // Arrange
            var expected = new HelperResult(action: null);
            var page = CreatePage(v =>
            {
                v.RenderSection("header");
                v.RenderSection("header");
            });
            page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
            {
                { "header", _nullRenderAsyncDelegate }
            };

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(page.ExecuteAsync);

            // Assert
            Assert.Equal("The section named 'header' has already been rendered.", ex.Message);
        }

        [Fact]
        public async Task RenderSectionAsync_ThrowsIfSectionIsRenderedMoreThanOnce()
        {
            // Arrange
            var expected = new HelperResult(action: null);
            var page = CreatePage(async v =>
            {
                await v.RenderSectionAsync("header");
                await v.RenderSectionAsync("header");
            });
            page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
            {
                { "header", _nullRenderAsyncDelegate }
            };

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(page.ExecuteAsync);

            // Assert
            Assert.Equal("The section named 'header' has already been rendered.", ex.Message);
        }

        [Fact]
        public async Task RenderSectionAsync_ThrowsIfSectionIsRenderedMoreThanOnce_WithSyncMethod()
        {
            // Arrange
            var expected = new HelperResult(action: null);
            var page = CreatePage(async v =>
            {
                v.RenderSection("header");
                await v.RenderSectionAsync("header");
            });
            page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
            {
                { "header", _nullRenderAsyncDelegate }
            };

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(page.ExecuteAsync);

            // Assert
            Assert.Equal("The section named 'header' has already been rendered.", ex.Message);
        }

        [Fact]
        public async Task RenderSectionAsync_ThrowsIfNotInvokedFromLayoutPage()
        {
            // Arrange
            var expected = new HelperResult(action: null);
            var page = CreatePage(async v =>
            {
                await v.RenderSectionAsync("header");
            });

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(page.ExecuteAsync);

            // Assert
            Assert.Equal("RenderSectionAsync can only be called from a layout page.", ex.Message);
        }

        [Fact]
        public async Task EnsureBodyAndSectionsWereRendered_ThrowsIfDefinedSectionIsNotRendered()
        {
            // Arrange
            var page = CreatePage(v =>
            {
                v.RenderSection("sectionA");
            });
            page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
            {
                { "header", _nullRenderAsyncDelegate },
                { "footer", _nullRenderAsyncDelegate },
                { "sectionA", _nullRenderAsyncDelegate },
            };

            // Act
            await page.ExecuteAsync();
            var ex = Assert.Throws<InvalidOperationException>(() => page.EnsureBodyAndSectionsWereRendered());

            // Assert
            Assert.Equal("The following sections have been defined but have not been rendered: 'header, footer'.",
                         ex.Message);
        }

        [Fact]
        public async Task EnsureBodyAndSectionsWereRendered_ThrowsIfRenderBodyIsNotCalledFromPage()
        {
            // Arrange
            var expected = new HelperResult(action: null);
            var page = CreatePage(v =>
            {
            });
            page.RenderBodyDelegate = CreateBodyAction("some content");

            // Act
            await page.ExecuteAsync();
            var ex = Assert.Throws<InvalidOperationException>(() => page.EnsureBodyAndSectionsWereRendered());

            // Assert
            Assert.Equal("RenderBody must be called from a layout page.", ex.Message);
        }

        [Fact]
        public async Task ExecuteAsync_RendersSectionsAndBody()
        {
            // Arrange
            var expected = string.Join(Environment.NewLine,
                                       "Layout start",
                                       "Header section",
                                       "Async Header section",
                                       "body content",
                                       "Async Footer section",
                                       "Footer section",
                                       "Layout end");
            var page = CreatePage(async v =>
            {
                v.WriteLiteral("Layout start" + Environment.NewLine);
                v.Write(v.RenderSection("header"));
                v.Write(await v.RenderSectionAsync("async-header"));
                v.Write(v.RenderBodyPublic());
                v.Write(await v.RenderSectionAsync("async-footer"));
                v.Write(v.RenderSection("footer"));
                v.WriteLiteral("Layout end");
            });
            page.RenderBodyDelegate = CreateBodyAction("body content" + Environment.NewLine);
            page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
            {
                {
                    "footer", writer => writer.WriteLineAsync("Footer section")
                },
                {
                    "header", writer => writer.WriteLineAsync("Header section")
                },
                {
                    "async-header", writer => writer.WriteLineAsync("Async Header section")
                },
                {
                    "async-footer", writer => writer.WriteLineAsync("Async Footer section")
                },
            };

            // Act
            await page.ExecuteAsync();

            // Assert
            var actual = page.RenderedContent;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task Href_ReadsUrlHelperFromServiceCollection()
        {
            // Arrange
            var expected = "urlhelper-url";
            var helper = new Mock<IUrlHelper>();
            helper.Setup(h => h.Content("url"))
                  .Returns(expected)
                  .Verifiable();
            var page = CreatePage(v =>
            {
                v.HtmlEncoder = new HtmlEncoder();
                v.Write(v.Href("url"));
            });
            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetService(typeof(IUrlHelper)))
                     .Returns(helper.Object);
            page.Context.RequestServices = services.Object;

            // Act
            await page.ExecuteAsync();

            // Assert
            var actual = page.RenderedContent;
            Assert.Equal(expected, actual);
            helper.Verify();
        }

        [Fact]
        public async Task FlushAsync_InvokesFlushOnWriter()
        {
            // Arrange
            var writer = new Mock<TextWriter>();
            var context = CreateViewContext(writer.Object);
            var page = CreatePage(async p =>
            {
                await p.FlushAsync();
            }, context);

            // Act
            await page.ExecuteAsync();

            // Assert
            writer.Verify(v => v.FlushAsync(), Times.Once());
        }

        [Fact]
        public async Task FlushAsync_ThrowsIfTheLayoutHasBeenSet()
        {
            // Arrange
            var expected = @"A layout page cannot be rendered after 'FlushAsync' has been invoked.";
            var writer = new Mock<TextWriter>();
            var context = CreateViewContext(writer.Object);
            var page = CreatePage(async p =>
            {
                p.Layout = "foo";
                await p.FlushAsync();
            }, context);

            // Act and Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => page.ExecuteAsync());
            Assert.Equal(expected, ex.Message);
        }

        [Fact]
        public async Task FlushAsync_DoesNotThrowWhenIsRenderingLayoutIsSet()
        {
            // Arrange
            var writer = new Mock<TextWriter>();
            var context = CreateViewContext(writer.Object);
            var page = CreatePage(p =>
            {
                p.Layout = "bar";
                p.DefineSection("test-section", async _ =>
                {
                    await p.FlushAsync();
                });
            }, context);

            // Act
            await page.ExecuteAsync();
            page.IsLayoutBeingRendered = true;

            // Assert (does not throw)
            var renderAsyncDelegate = page.SectionWriters["test-section"];
            await renderAsyncDelegate(TextWriter.Null);
        }

        [Fact]
        public async Task FlushAsync_ReturnsEmptyHtmlString()
        {
            // Arrange
            HtmlString actual = null;
            var writer = new Mock<TextWriter>();
            var context = CreateViewContext(writer.Object);
            var page = CreatePage(async p =>
            {
                actual = await p.FlushAsync();
            }, context);

            // Act
            await page.ExecuteAsync();

            // Assert
            Assert.Same(HtmlString.Empty, actual);
        }

        [Fact]
        public async Task WriteAttribute_CallsBeginAndEndContext_OnPageExecutionListenerContext()
        {
            // Arrange
            var page = CreatePage(p =>
            {
                p.HtmlEncoder = new HtmlEncoder();
                p.WriteAttribute("href",
                                 new PositionTagged<string>("prefix", 0),
                                 new PositionTagged<string>("suffix", 34),
                                 new AttributeValue(new PositionTagged<string>("prefix", 0),
                                                    new PositionTagged<object>("attr1-value", 8),
                                                    literal: true),
                                 new AttributeValue(new PositionTagged<string>("prefix2", 22),
                                                    new PositionTagged<object>("attr2", 29),
                                                    literal: false));
            });
            var context = new Mock<IPageExecutionContext>(MockBehavior.Strict);
            var sequence = new MockSequence();
            context.InSequence(sequence).Setup(f => f.BeginContext(0, 6, true)).Verifiable();
            context.InSequence(sequence).Setup(f => f.EndContext()).Verifiable();
            context.InSequence(sequence).Setup(f => f.BeginContext(8, 14, true)).Verifiable();
            context.InSequence(sequence).Setup(f => f.EndContext()).Verifiable();
            context.InSequence(sequence).Setup(f => f.BeginContext(22, 7, true)).Verifiable();
            context.InSequence(sequence).Setup(f => f.EndContext()).Verifiable();
            context.InSequence(sequence).Setup(f => f.BeginContext(29, 5, false)).Verifiable();
            context.InSequence(sequence).Setup(f => f.EndContext()).Verifiable();
            context.InSequence(sequence).Setup(f => f.BeginContext(34, 6, true)).Verifiable();
            context.InSequence(sequence).Setup(f => f.EndContext()).Verifiable();
            page.PageExecutionContext = context.Object;

            // Act
            await page.ExecuteAsync();

            // Assert
            context.Verify();
        }

        [Fact]
        public async Task WriteAttribute_CallsBeginAndEndContext_OnPrefixAndSuffixValues()
        {
            // Arrange
            var page = CreatePage(p =>
            {
                p.WriteAttribute("href",
                                 new PositionTagged<string>("prefix", 0),
                                 new PositionTagged<string>("tail", 7));
            });
            var context = new Mock<IPageExecutionContext>(MockBehavior.Strict);
            var sequence = new MockSequence();
            context.InSequence(sequence).Setup(f => f.BeginContext(0, 6, true)).Verifiable();
            context.InSequence(sequence).Setup(f => f.EndContext()).Verifiable();
            context.InSequence(sequence).Setup(f => f.BeginContext(7, 4, true)).Verifiable();
            context.InSequence(sequence).Setup(f => f.EndContext()).Verifiable();
            page.PageExecutionContext = context.Object;

            // Act
            await page.ExecuteAsync();

            // Assert
            context.Verify();
        }

        [Fact]
        public async Task Write_WithHtmlString_WritesValueWithoutEncoding()
        {
            // Arrange
            var writer = new RazorTextWriter(TextWriter.Null, Encoding.UTF8);
            var stringCollectionWriter = new StringCollectionTextWriter(Encoding.UTF8);
            stringCollectionWriter.Write("text1");
            stringCollectionWriter.Write("text2");

            var page = CreatePage(p =>
            {
                p.Write(new HtmlString("Hello world"));
                p.Write(new HtmlString(stringCollectionWriter));
            });
            page.ViewContext.Writer = writer;

            // Act
            await page.ExecuteAsync();

            // Assert
            var buffer = writer.BufferedWriter.Buffer;
            Assert.Equal(2, buffer.BufferEntries.Count);
            Assert.Equal("Hello world", buffer.BufferEntries[0]);
            Assert.Same(stringCollectionWriter.Buffer.BufferEntries, buffer.BufferEntries[1]);
        }

        public static TheoryData<TagHelperOutput, string> WriteTagHelper_InputData
        {
            get
            {
                // parameters: TagHelperOutput, expectedOutput
                return new TheoryData<TagHelperOutput, string>
                {
                    {
                        // parameters: TagName, Attributes, SelfClosing, PreContent, Content, PostContent
                        GetTagHelperOutput("div", new Dictionary<string, string>(), false, null, "Hello World!", null),
                        "<div>Hello World!</div>"
                    },
                    {
                        GetTagHelperOutput(null, new Dictionary<string, string>(), false, null, "Hello World!", null),
                        "Hello World!"
                    },
                    {
                        GetTagHelperOutput("  ", new Dictionary<string, string>(), false, null, "Hello World!", null),
                        "Hello World!"
                    },
                    {
                        GetTagHelperOutput(
                            "p",
                            new Dictionary<string, string>() { { "test", "testVal" } },
                            false,
                            null,
                            "Hello World!",
                            null),
                        "<p test=\"testVal\">Hello World!</p>"
                    },
                    {
                        GetTagHelperOutput(
                            "p",
                            new Dictionary<string, string>() { { "test", "testVal" }, { "something", "  spaced  " } },
                            false,
                            null,
                            "Hello World!",
                            null),
                        "<p test=\"testVal\" something=\"  spaced  \">Hello World!</p>"
                    },
                    {
                        GetTagHelperOutput(
                            "p",
                            new Dictionary<string, string>() { { "test", "testVal" } },
                            true,
                            null,
                            "Hello World!",
                            null),
                        "<p test=\"testVal\" />"
                    },
                    {
                        GetTagHelperOutput(
                            "p",
                            new Dictionary<string, string>() { { "test", "testVal" }, { "something", "  spaced  " } },
                            true,
                            null,
                            "Hello World!",
                            null),
                        "<p test=\"testVal\" something=\"  spaced  \" />"
                    },
                    {
                        GetTagHelperOutput("p", new Dictionary<string, string>(), false, "Hello World!", null, null),
                        "<p>Hello World!</p>"
                    },
                    {
                        GetTagHelperOutput("p", new Dictionary<string, string>(), false, null, "Hello World!", null),
                        "<p>Hello World!</p>"
                    },
                    {
                        GetTagHelperOutput("p", new Dictionary<string, string>(), false, null, null, "Hello World!"),
                        "<p>Hello World!</p>"
                    },
                    {
                        GetTagHelperOutput("p", new Dictionary<string, string>(), false, "Hello", "Test", "World!"),
                        "<p>HelloTestWorld!</p>"
                    },
                    {
                        GetTagHelperOutput("p", new Dictionary<string, string>(), true, "Hello", "Test", "World!"),
                        "<p />"
                    },
                    {
                        GetTagHelperOutput("custom", new Dictionary<string, string>(), false, "Hello", "Test", "World!"),
                        "<custom>HelloTestWorld!</custom>"
                    },
                    {
                        GetTagHelperOutput("random", new Dictionary<string, string>(), true, "Hello", "Test", "World!"),
                        "<random />"
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(WriteTagHelper_InputData))]
        public async Task WriteTagHelperAsync_WritesFormattedTagHelper(TagHelperOutput output, string expected)
        {
            // Arrange
            var writer = new StringCollectionTextWriter(Encoding.UTF8);
            var context = CreateViewContext(writer);
            var tagHelperExecutionContext = new TagHelperExecutionContext(
                tagName: output.TagName,
                selfClosing: output.SelfClosing,
                items: new Dictionary<object, object>(),
                uniqueId: string.Empty,
                executeChildContentAsync: () => Task.FromResult(result: true),
                startTagHelperWritingScope: () => { },
                endTagHelperWritingScope: () => new DefaultTagHelperContent());
            tagHelperExecutionContext.Output = output;

            // Act
            var page = CreatePage(p =>
            {
                p.HtmlEncoder = new HtmlEncoder();
                p.WriteTagHelperAsync(tagHelperExecutionContext).Wait();
            }, context);
            await page.ExecuteAsync();

            // Assert
            Assert.Equal(expected, writer.ToString());
        }

        [Theory]
        // This is a scenario where GetChildContentAsync is called.
        [InlineData(true, "HelloWorld!", "<p>HelloWorld!</p>")]
        // This is a scenario where ExecuteChildContentAsync is called.
        [InlineData(false, "HelloWorld!", "<p></p>")]
        public async Task WriteTagHelperAsync_WritesContentAppropriately(
            bool childContentRetrieved, string input, string expected)
        {
            // Arrange
            var defaultTagHelperContent = new DefaultTagHelperContent();
            var writer = new StringCollectionTextWriter(Encoding.UTF8);
            var context = CreateViewContext(writer);
            var tagHelperExecutionContext = new TagHelperExecutionContext(
                tagName: "p",
                selfClosing: false,
                items: new Dictionary<object, object>(),
                uniqueId: string.Empty,
                executeChildContentAsync: () => {
                    defaultTagHelperContent.SetContent(input);
                    return Task.FromResult(result: true);
                },
                startTagHelperWritingScope: () => { },
                endTagHelperWritingScope: () => defaultTagHelperContent);
            tagHelperExecutionContext.Output =
                new TagHelperOutput("p", new Dictionary<string, string>());
            if (childContentRetrieved)
            {
                await tagHelperExecutionContext.GetChildContentAsync();
            }

            // Act
            var page = CreatePage(p =>
            {
                p.HtmlEncoder = new HtmlEncoder();
                p.WriteTagHelperAsync(tagHelperExecutionContext).Wait();
            }, context);
            await page.ExecuteAsync();

            // Assert
            Assert.Equal(expected, writer.ToString());
        }

        [Fact]
        public async Task WriteTagHelperToAsync_WritesToSpecifiedWriter()
        {
            // Arrange
            var writer = new StringCollectionTextWriter(Encoding.UTF8);
            var context = CreateViewContext(new StringWriter());
            var tagHelperExecutionContext = new TagHelperExecutionContext(
                tagName: "p",
                selfClosing: false,
                items: new Dictionary<object, object>(),
                uniqueId: string.Empty,
                executeChildContentAsync: () => { return Task.FromResult(result: true); },
                startTagHelperWritingScope: () => { },
                endTagHelperWritingScope: () => new DefaultTagHelperContent());
            tagHelperExecutionContext.Output =
                new TagHelperOutput("p", new Dictionary<string, string>());
            tagHelperExecutionContext.Output.Content.SetContent("Hello World!");

            // Act
            var page = CreatePage(p =>
            {
                p.WriteTagHelperToAsync(writer, tagHelperExecutionContext).Wait();
            }, context);
            await page.ExecuteAsync();

            // Assert
            Assert.Equal("<p>Hello World!</p>", writer.ToString());
        }

        [Theory]
        [MemberData(nameof(WriteTagHelper_InputData))]
        public async Task WriteTagHelperToAsync_WritesFormattedTagHelper(TagHelperOutput output, string expected)
        {
            // Arrange
            var writer = new StringCollectionTextWriter(Encoding.UTF8);
            var context = CreateViewContext(new StringWriter());
            var tagHelperExecutionContext = new TagHelperExecutionContext(
                tagName: output.TagName,
                selfClosing: output.SelfClosing,
                items: new Dictionary<object, object>(),
                uniqueId: string.Empty,
                executeChildContentAsync: () => Task.FromResult(result: true),
                startTagHelperWritingScope: () => { },
                endTagHelperWritingScope: () => new DefaultTagHelperContent());
            tagHelperExecutionContext.Output = output;

            // Act
            var page = CreatePage(p =>
            {
                p.HtmlEncoder = new HtmlEncoder();
                p.WriteTagHelperToAsync(writer, tagHelperExecutionContext).Wait();
            }, context);
            await page.ExecuteAsync();

            // Assert
            Assert.Equal(expected, writer.ToString());
        }

        private static TagHelperOutput GetTagHelperOutput(
            string tagName,
            IDictionary<string, string> attributes,
            bool selfClosing,
            string preContent,
            string content,
            string postContent)
        {
            var output = new TagHelperOutput(tagName, attributes)
            {
                SelfClosing = selfClosing
            };

            output.PreContent.SetContent(preContent);
            output.Content.SetContent(content);
            output.PostContent.SetContent(postContent);

            return output;
        }

        private static TestableRazorPage CreatePage(Action<TestableRazorPage> executeAction,
                                                    ViewContext context = null)
        {
            return CreatePage(page =>
            {
                executeAction(page);
                return Task.FromResult(0);
            }, context);
        }


        private static TestableRazorPage CreatePage(Func<TestableRazorPage, Task> executeAction,
                                                    ViewContext context = null)
        {
            context = context ?? CreateViewContext();
            var view = new Mock<TestableRazorPage> { CallBase = true };
            if (executeAction != null)
            {
                view.Setup(v => v.ExecuteAsync())
                    .Returns(() =>
                    {
                        return executeAction(view.Object);
                    });
            }

            view.Object.ViewContext = context;
            return view.Object;
        }

        private static ViewContext CreateViewContext(TextWriter writer = null)
        {
            writer = writer ?? new StringWriter();
            var actionContext = new ActionContext(new DefaultHttpContext(), routeData: null, actionDescriptor: null);
            return new ViewContext(
                actionContext,
                Mock.Of<IView>(),
                null,
                Mock.Of<ITempDataDictionary>(),
                writer);
        }

        private static Action<TextWriter> CreateBodyAction(string value)
        {
            return writer => writer.Write(value);
        }

        public abstract class TestableRazorPage : RazorPage
        {
            public string RenderedContent
            {
                get
                {
                    var writer = Assert.IsType<StringWriter>(Output);
                    return writer.ToString();
                }
            }

            public HelperResult RenderBodyPublic()
            {
                return base.RenderBody();
            }
        }
    }
}