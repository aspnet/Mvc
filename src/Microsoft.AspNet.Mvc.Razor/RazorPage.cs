// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.PageExecutionInstrumentation;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Represents properties and methods that are needed in order to render a view that uses Razor syntax.
    /// </summary>
    public abstract class RazorPage : IRazorPage
    {
        private readonly HashSet<string> _renderedSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<TextWriter> _writerScopes;
        private TextWriter _originalWriter;
        private IUrlHelper _urlHelper;
        private ITagHelperActivator _tagHelperActivator;
        private bool _renderedBody;

        public RazorPage()
        {
            SectionWriters = new Dictionary<string, RenderAsyncDelegate>(StringComparer.OrdinalIgnoreCase);

            _writerScopes = new Stack<TextWriter>();
        }

        public HttpContext Context
        {
            get
            {
                if (ViewContext == null)
                {
                    return null;
                }

                return ViewContext.HttpContext;
            }
        }

        /// <inheritdoc />
        public string Path { get; set; }

        /// <inheritdoc />
        public ViewContext ViewContext { get; set; }

        /// <inheritdoc />
        public string Layout { get; set; }

        /// <inheritdoc />
        public bool IsPartial { get; set; }

        /// <inheritdoc />
        public IPageExecutionContext PageExecutionContext { get; set; }

        /// <summary>
        /// Gets the TextWriter that the page is writing output to.
        /// </summary>
        public virtual TextWriter Output
        {
            get
            {
                if (ViewContext == null)
                {
                    var message = Resources.FormatViewContextMustBeSet("ViewContext", "Output");
                    throw new InvalidOperationException(message);
                }

                return ViewContext.Writer;
            }
        }

        public virtual IPrincipal User
        {
            get
            {
                if (Context == null)
                {
                    return null;
                }

                return Context.User;
            }
        }

        public dynamic ViewBag
        {
            get
            {
                return (ViewContext == null) ? null : ViewContext.ViewBag;
            }
        }

        /// <inheritdoc />
        public Action<TextWriter> RenderBodyDelegate { get; set; }

        /// <inheritdoc />
        public bool IsLayoutBeingRendered { get; set; }

        /// <inheritdoc />
        public Dictionary<string, RenderAsyncDelegate> PreviousSectionWriters { get; set; }

        /// <inheritdoc />
        public Dictionary<string, RenderAsyncDelegate> SectionWriters { get; private set; }

        /// <inheritdoc />
        public abstract Task ExecuteAsync();

        private ITagHelperActivator TagHelperActivator
        {
            get
            {
                if (_tagHelperActivator == null)
                {
                    _tagHelperActivator = ViewContext.HttpContext.RequestServices.GetRequiredService<ITagHelperActivator>();
                }

                return _tagHelperActivator;
            }
        }
        /// <summary>
        /// Creates and activates a <see cref="ITagHelper"/>.
        /// </summary>
        /// <typeparam name="TTagHelper">A <see cref="ITagHelper"/> type.</typeparam>
        /// <returns>The activated <see cref="ITagHelper"/>.</returns>
        /// <remarks>
        /// <typeparamref name="TTagHelper"/> must have a parameterless constructor.
        /// </remarks>
        public TTagHelper CreateTagHelper<TTagHelper>() where TTagHelper : ITagHelper, new()
        {
            var tagHelper = new TTagHelper();

            TagHelperActivator.Activate(tagHelper, ViewContext);

            return tagHelper;
        }

        /// <summary>
        /// Starts a new writing scope.
        /// </summary>
        /// <remarks>
        /// All writes to the <see cref="Output"/> or <see cref="ViewContext.Writer"/> after calling this method will
        /// be buffered until <see cref="EndWritingScope"/> is called.
        /// </remarks>
        public void StartWritingScope()
        {
            // If there isn't a base writer take the ViewContext.Writer
            if (_originalWriter == null)
            {
                _originalWriter = ViewContext.Writer;
            }

            // We need to replace the ViewContext's Writer to ensure that all content (including content written
            // from HTML helpers) is redirected.
            ViewContext.Writer = new StringWriter();

            _writerScopes.Push(ViewContext.Writer);
        }

        /// <summary>
        /// Ends the current writing scope that was started by calling <see cref="StartWritingScope"/>.
        /// </summary>
        /// <returns>The <see cref="TextWriter"/> that contains the content written to the <see cref="Output"/> or
        /// <see cref="ViewContext.Writer"/> during the writing scope.</returns>
        public TextWriter EndWritingScope()
        {
            if (_writerScopes.Count == 0)
            {
                throw new InvalidOperationException(Resources.RazorPage_ThereIsNoActiveWritingScopeToEnd);
            }

            var writer = _writerScopes.Pop();

            if (_writerScopes.Count > 0)
            {
                ViewContext.Writer = _writerScopes.Peek();
            }
            else
            {
                ViewContext.Writer = _originalWriter;

                // No longer a base writer
                _originalWriter = null;
            }

            return writer;
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> with HTML encoding to <see cref="Output"/>.
        /// </summary>
        /// <param name="value">The <see cref="object"/> to write.</param>
        public virtual void Write(object value)
        {
            WriteTo(Output, value);
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> with HTML encoding to <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="object"/> to write.</param>
        /// <remarks>
        /// <paramref name="value"/>s of type <see cref="HtmlString"/> are written without encoding and the
        /// <see cref="HelperResult.WriteTo(TextWriter)"/> is invoked for <see cref="HelperResult"/> types.
        /// For all other types, the encoded result of <see cref="object.ToString"/> is written to the
        /// <paramref name="writer"/>.
        /// </remarks>
        public virtual void WriteTo(TextWriter writer, object value)
        {
            if (value != null && value != HtmlString.Empty)
            {
                var helperResult = value as HelperResult;
                if (helperResult != null)
                {
                    helperResult.WriteTo(writer);
                }
                else
                {
                    var htmlString = value as HtmlString;
                    if (htmlString != null)
                    {
                        writer.Write(htmlString);
                    }
                    else
                    {
                        WriteTo(writer, value.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> with HTML encoding to <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="string"/> to write.</param>
        public virtual void WriteTo(TextWriter writer, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                writer.Write(WebUtility.HtmlEncode(value));
            }
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> without HTML encoding to <see cref="Output"/>.
        /// </summary>
        /// <param name="value">The <see cref="object"/> to write.</param>
        public virtual void WriteLiteral(object value)
        {
            WriteLiteralTo(Output, value);
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> without HTML encoding to the <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="object"/> to write.</param>
        public virtual void WriteLiteralTo(TextWriter writer, object value)
        {
            if (value != null)
            {
                WriteLiteralTo(writer, value.ToString());
            }
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> without HTML encoding to <see cref="Output"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to write.</param>
        public virtual void WriteLiteralTo(TextWriter writer, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                writer.Write(value);
            }
        }

        public virtual void WriteAttribute(string name,
                                           PositionTagged<string> prefix,
                                           PositionTagged<string> suffix,
                                           params AttributeValue[] values)
        {
            WriteAttributeTo(Output, name, prefix, suffix, values);
        }

        public virtual void WriteAttributeTo(TextWriter writer,
                                             string name,
                                             PositionTagged<string> prefix,
                                             PositionTagged<string> suffix,
                                             params AttributeValue[] values)
        {
            var first = true;
            var wroteSomething = false;
            if (values.Length == 0)
            {
                // Explicitly empty attribute, so write the prefix and suffix
                WritePositionTaggedLiteral(writer, prefix);
                WritePositionTaggedLiteral(writer, suffix);
            }
            else
            {
                for (var i = 0; i < values.Length; i++)
                {
                    var attrVal = values[i];
                    var val = attrVal.Value;
                    var next = i == values.Length - 1 ?
                        suffix : // End of the list, grab the suffix
                        values[i + 1].Prefix; // Still in the list, grab the next prefix

                    if (val.Value == null)
                    {
                        // Nothing to write
                        continue;
                    }

                    // The special cases here are that the value we're writing might already be a string, or that the
                    // value might be a bool. If the value is the bool 'true' we want to write the attribute name
                    // instead of the string 'true'. If the value is the bool 'false' we don't want to write anything.
                    // Otherwise the value is another object (perhaps an HtmlString) and we'll ask it to format itself.
                    string stringValue;

                    // Intentionally using is+cast here for performance reasons. This is more performant than as+bool?
                    // because of boxing.
                    if (val.Value is bool)
                    {
                        if ((bool)val.Value)
                        {
                            stringValue = name;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        stringValue = val.Value as string;
                    }

                    if (first)
                    {
                        WritePositionTaggedLiteral(writer, prefix);
                        first = false;
                    }
                    else
                    {
                        WritePositionTaggedLiteral(writer, attrVal.Prefix);
                    }

                    // Calculate length of the source span by the position of the next value (or suffix)
                    var sourceLength = next.Position - attrVal.Value.Position;

                    BeginContext(attrVal.Value.Position, sourceLength, isLiteral: attrVal.Literal);
                    // The extra branching here is to ensure that we call the Write*To(string) overload whe
                    // possible.
                    if (attrVal.Literal && stringValue != null)
                    {
                        WriteLiteralTo(writer, stringValue);
                    }
                    else if (attrVal.Literal)
                    {
                        WriteLiteralTo(writer, val.Value);
                    }
                    else if (stringValue != null)
                    {
                        WriteTo(writer, stringValue);
                    }
                    else
                    {
                        WriteTo(writer, val.Value);
                    }

                    EndContext();
                    wroteSomething = true;
                }
                if (wroteSomething)
                {
                    WritePositionTaggedLiteral(writer, suffix);
                }
            }
        }

        public virtual string Href([NotNull] string contentPath)
        {
            if (_urlHelper == null)
            {
                _urlHelper = Context.RequestServices.GetRequiredService<IUrlHelper>();
            }

            return _urlHelper.Content(contentPath);
        }

        private void WritePositionTaggedLiteral(TextWriter writer, string value, int position)
        {
            BeginContext(position, value.Length, isLiteral: true);
            WriteLiteralTo(writer, value);
            EndContext();
        }

        private void WritePositionTaggedLiteral(TextWriter writer, PositionTagged<string> value)
        {
            WritePositionTaggedLiteral(writer, value.Value, value.Position);
        }

        protected virtual HelperResult RenderBody()
        {
            if (RenderBodyDelegate == null)
            {
                var message = Resources.FormatRazorPage_MethodCannotBeCalled(nameof(RenderBody));
                throw new InvalidOperationException(message);
            }

            _renderedBody = true;
            return new HelperResult(RenderBodyDelegate);
        }

        /// <summary>
        /// Creates a named content section in the page that can be invoked in a Layout page using
        /// <see cref="RenderSection(string)"/> or <see cref="RenderSectionAsync(string, bool)"/>.
        /// </summary>
        /// <param name="name">The name of the section to create.</param>
        /// <param name="section">The <see cref="RenderAsyncDelegate"/> to execute when rendering the section.</param>
        public void DefineSection(string name, RenderAsyncDelegate section)
        {
            if (SectionWriters.ContainsKey(name))
            {
                throw new InvalidOperationException(Resources.FormatSectionAlreadyDefined(name));
            }
            SectionWriters[name] = section;
        }

        public bool IsSectionDefined([NotNull] string name)
        {
            EnsureMethodCanBeInvoked(nameof(IsSectionDefined));
            return PreviousSectionWriters.ContainsKey(name);
        }

        /// <summary>
        /// In layout pages, renders the content of the section named <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the section to render.</param>
        /// <returns>Returns <see cref="HtmlString.Empty"/> to allow the <see cref="Write(object)"/> call to
        /// succeed.</returns>
        /// <remarks>The method writes to the <see cref="Output"/> and the value returned is a token
        /// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
        /// value does not represent the rendered content.</remarks>
        public HtmlString RenderSection([NotNull] string name)
        {
            return RenderSection(name, required: true);
        }

        /// <summary>
        /// In layout pages, renders the content of the section named <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The section to render.</param>
        /// <param name="required">Indicates if this section must be rendered.</param>
        /// <returns>Returns <see cref="HtmlString.Empty"/> to allow the <see cref="Write(object)"/> call to
        /// succeed.</returns>
        /// <remarks>The method writes to the <see cref="Output"/> and the value returned is a token
        /// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
        /// value does not represent the rendered content.</remarks>
        public HtmlString RenderSection([NotNull] string name, bool required)
        {
            EnsureMethodCanBeInvoked(nameof(RenderSection));

            var task = RenderSectionAsyncCore(name, required);
            return TaskHelper.WaitAndThrowIfFaulted(task);
        }

        /// <summary>
        /// In layout pages, asynchronously renders the content of the section named <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The section to render.</param>
        /// <returns>A <see cref="Task{HtmlString}"/> that on completion returns <see cref="HtmlString.Empty"/> that
        /// allows the <see cref="Write(object)"/> call to succeed.</returns>
        /// <remarks>The method writes to the <see cref="Output"/> and the value returned is a token
        /// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
        /// value does not represent the rendered content.</remarks>
        public Task<HtmlString> RenderSectionAsync([NotNull] string name)
        {
            return RenderSectionAsync(name, required: true);
        }

        /// <summary>
        /// In layout pages, asynchronously renders the content of the section named <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The section to render.</param>
        /// <returns>A <see cref="Task{HtmlString}"/> that on completion returns <see cref="HtmlString.Empty"/> that
        /// allows the <see cref="Write(object)"/> call to succeed.</returns>
        /// <remarks>The method writes to the <see cref="Output"/> and the value returned is a token
        /// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
        /// value does not represent the rendered content.</remarks>
        /// <exception cref="InvalidOperationException">if <paramref name="required"/> is <c>true</c> and the section
        /// was not registered using the <c>@section</c> in the Razor page.</exception>
        public async Task<HtmlString> RenderSectionAsync([NotNull] string name, bool required)
        {
            EnsureMethodCanBeInvoked(nameof(RenderSectionAsync));
            return await RenderSectionAsyncCore(name, required);
        }

        private async Task<HtmlString> RenderSectionAsyncCore(string sectionName, bool required)
        {
            if (_renderedSections.Contains(sectionName))
            {
                var message = Resources.FormatSectionAlreadyRendered(sectionName);
                throw new InvalidOperationException(message);
            }

            RenderAsyncDelegate renderDelegate;
            if (PreviousSectionWriters.TryGetValue(sectionName, out renderDelegate))
            {
                _renderedSections.Add(sectionName);
                await renderDelegate(Output);

                // Return a token value that allows the Write call that wraps the RenderSection \ RenderSectionAsync
                // to succeed.
                return HtmlString.Empty;
            }
            else if (required)
            {
                // If the section is not found, and it is not optional, throw an error.
                throw new InvalidOperationException(Resources.FormatSectionNotDefined(sectionName));
            }
            else
            {
                // If the section is optional and not found, then don't do anything.
                return null;
            }
        }

        /// <summary>
        /// Invokes <see cref="TextWriter.FlushAsync"/> on <see cref="Output"/> writing out any buffered
        /// content to the <see cref="HttpResponse.Body"/>.
        /// </summary>
        /// <returns>A<see cref="Task{HtmlString}"/> that represents the asynchronous flush operation and on
        /// completion returns a <see cref="HtmlString.Empty"/>.</returns>
        /// <remarks>The value returned is a token value that allows FlushAsync to work directly in an HTML
        /// section. However the value does not represent the rendered content.</remarks>
        public async Task<HtmlString> FlushAsync()
        {
            // If there are active writing scopes then we should throw. Cannot flush content that has the potential to
            // change.
            if (_writerScopes.Count > 0)
            {
                throw new InvalidOperationException(Resources.RazorPage_YouCannotFlushWhileInAWritingScope);
            }

            // Calls to Flush are allowed if the page does not specify a Layout or if it is executing a section in the
            // Layout.
            if (!IsLayoutBeingRendered && !string.IsNullOrEmpty(Layout))
            {
                var message = Resources.FormatLayoutCannotBeRendered(nameof(FlushAsync));
                throw new InvalidOperationException(message);
            }

            await Output.FlushAsync();
            return HtmlString.Empty;
        }

        /// <inheritdoc />
        public void EnsureBodyAndSectionsWereRendered()
        {
            // If PreviousSectionWriters is set, ensure all defined sections were rendered.
            if (PreviousSectionWriters != null)
            {
                var sectionsNotRendered = PreviousSectionWriters.Keys.Except(_renderedSections,
                                                                             StringComparer.OrdinalIgnoreCase);
                if (sectionsNotRendered.Any())
                {
                    var sectionNames = string.Join(", ", sectionsNotRendered);
                    throw new InvalidOperationException(Resources.FormatSectionsNotRendered(sectionNames));
                }
            }

            // If BodyContent is set, ensure it was rendered.
            if (RenderBodyDelegate != null && !_renderedBody)
            {
                // If a body was defined, then RenderBody should have been called.
                var message = Resources.FormatRenderBodyNotCalled(nameof(RenderBody));
                throw new InvalidOperationException(message);
            }
        }

        public void BeginContext(int position, int length, bool isLiteral)
        {
            PageExecutionContext?.BeginContext(position, length, isLiteral);
        }

        public void EndContext()
        {
            PageExecutionContext?.EndContext();
        }

        private void EnsureMethodCanBeInvoked(string methodName)
        {
            if (PreviousSectionWriters == null)
            {
                throw new InvalidOperationException(Resources.FormatRazorPage_MethodCannotBeCalled(methodName));
            }
        }
    }
}