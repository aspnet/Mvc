// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc.Razor
{
    /// <summary>
    /// Represents properties and methods that are needed in order to render a view that uses Razor syntax.
    /// </summary>
    public abstract class RazorPage : IRazorPage
    {
        private readonly HashSet<string> _renderedSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<TagHelperScopeInfo> _tagHelperScopes = new Stack<TagHelperScopeInfo>();
        private IUrlHelper _urlHelper;
        private ITagHelperActivator _tagHelperActivator;
        private ITypeActivatorCache _typeActivatorCache;
        private bool _renderedBody;
        private AttributeInfo _attributeInfo;
        private TagHelperAttributeInfo _tagHelperAttributeInfo;
        private HtmlContentWrapperTextWriter _valueBuffer;
        private IViewBufferScope _bufferScope;
        private bool _ignoreBody;
        private HashSet<string> _ignoredSections;

        public RazorPage()
        {
            SectionWriters = new Dictionary<string, RenderAsyncDelegate>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// An <see cref="HttpContext"/> representing the current request execution.
        /// </summary>
        public HttpContext Context => ViewContext?.HttpContext;

        /// <inheritdoc />
        public string Path { get; set; }

        /// <inheritdoc />
        public ViewContext ViewContext { get; set; }

        /// <inheritdoc />
        public string Layout { get; set; }

        /// <summary>
        /// Gets the <see cref="System.Text.Encodings.Web.HtmlEncoder"/> to use when this <see cref="RazorPage"/>
        /// handles non-<see cref="IHtmlContent"/> C# expressions.
        /// </summary>
        [RazorInject]
        public HtmlEncoder HtmlEncoder { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="DiagnosticSource.DiagnosticSource"/> instance used to instrument the page execution.
        /// </summary>
        [RazorInject]
        public DiagnosticSource DiagnosticSource { get; set; }

        /// <summary>
        /// Gets the <see cref="TextWriter"/> that the page is writing output to.
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

        /// <summary>
        /// Gets the <see cref="ClaimsPrincipal"/> of the current logged in user.
        /// </summary>
        public virtual ClaimsPrincipal User => Context?.User;

        /// <summary>
        /// Gets the dynamic view data dictionary.
        /// </summary>
        public dynamic ViewBag => ViewContext?.ViewBag;

        /// <summary>
        /// Gets the <see cref="ITempDataDictionary"/> from the <see cref="ViewContext"/>.
        /// </summary>
        /// <remarks>Returns null if <see cref="ViewContext"/> is null.</remarks>
        public ITempDataDictionary TempData => ViewContext?.TempData;

        /// <inheritdoc />
        public IHtmlContent BodyContent { get; set; }

        /// <inheritdoc />
        public bool IsLayoutBeingRendered { get; set; }

        /// <inheritdoc />
        public IDictionary<string, RenderAsyncDelegate> PreviousSectionWriters { get; set; }

        /// <inheritdoc />
        public IDictionary<string, RenderAsyncDelegate> SectionWriters { get; }

        /// <inheritdoc />
        public abstract Task ExecuteAsync();

        private ITagHelperActivator TagHelperActivator
        {
            get
            {
                if (_tagHelperActivator == null)
                {
                    var services = ViewContext.HttpContext.RequestServices;
                    _tagHelperActivator = services.GetRequiredService<ITagHelperActivator>();
                }

                return _tagHelperActivator;
            }
        }

        private ITypeActivatorCache TypeActivatorCache
        {
            get
            {
                if (_typeActivatorCache == null)
                {
                    var services = ViewContext.HttpContext.RequestServices;
                    _typeActivatorCache = services.GetRequiredService<ITypeActivatorCache>();
                }

                return _typeActivatorCache;
            }
        }

        private IViewBufferScope BufferScope
        {
            get
            {
                if (_bufferScope == null)
                {
                    var services = ViewContext.HttpContext.RequestServices;
                    _bufferScope = services.GetRequiredService<IViewBufferScope>();
                }

                return _bufferScope;
            }
        }

        /// <summary>
        /// Format an error message about using an indexer when the tag helper property is <c>null</c>.
        /// </summary>
        /// <param name="attributeName">Name of the HTML attribute associated with the indexer.</param>
        /// <param name="tagHelperTypeName">Full name of the tag helper <see cref="Type"/>.</param>
        /// <param name="propertyName">Dictionary property in the tag helper.</param>
        /// <returns>An error message about using an indexer when the tag helper property is <c>null</c>.</returns>
        public static string InvalidTagHelperIndexerAssignment(
            string attributeName,
            string tagHelperTypeName,
            string propertyName)
        {
            return Resources.FormatRazorPage_InvalidTagHelperIndexerAssignment(
                attributeName,
                tagHelperTypeName,
                propertyName);
        }

        /// <summary>
        /// Creates and activates a <see cref="ITagHelper"/>.
        /// </summary>
        /// <typeparam name="TTagHelper">A <see cref="ITagHelper"/> type.</typeparam>
        /// <returns>The activated <see cref="ITagHelper"/>.</returns>
        /// <remarks>
        /// <typeparamref name="TTagHelper"/> must have a parameterless constructor.
        /// </remarks>
        public TTagHelper CreateTagHelper<TTagHelper>() where TTagHelper : ITagHelper
        {
            var tagHelper = TypeActivatorCache.CreateInstance<TTagHelper>(
                ViewContext.HttpContext.RequestServices,
                typeof(TTagHelper));

            TagHelperActivator.Activate(tagHelper, ViewContext);

            return tagHelper;
        }

        /// <summary>
        /// Starts a new writing scope and optionally overrides <see cref="HtmlEncoder"/> within that scope.
        /// </summary>
        /// <param name="encoder">
        /// The <see cref="System.Text.Encodings.Web.HtmlEncoder"/> to use when this <see cref="RazorPage"/> handles
        /// non-<see cref="IHtmlContent"/> C# expressions. If <c>null</c>, does not change <see cref="HtmlEncoder"/>.
        /// </param>
        /// <remarks>
        /// All writes to the <see cref="Output"/> or <see cref="ViewContext.Writer"/> after calling this method will
        /// be buffered until <see cref="EndTagHelperWritingScope"/> is called.
        /// </remarks>
        public void StartTagHelperWritingScope(HtmlEncoder encoder)
        {
            _tagHelperScopes.Push(new TagHelperScopeInfo(HtmlEncoder, ViewContext.Writer));

            // If passed an HtmlEncoder, override the property.
            if (encoder != null)
            {
                HtmlEncoder = encoder;
            }

            // We need to replace the ViewContext's Writer to ensure that all content (including content written
            // from HTML helpers) is redirected.
            var buffer = new ViewBuffer(BufferScope, Path);
            var writer = new HtmlContentWrapperTextWriter(buffer, ViewContext.Writer.Encoding);
            ViewContext.Writer = writer;
        }

        /// <summary>
        /// Ends the current writing scope that was started by calling <see cref="StartTagHelperWritingScope"/>.
        /// </summary>
        /// <returns>The buffered <see cref="TagHelperContent"/>.</returns>
        public TagHelperContent EndTagHelperWritingScope()
        {
            if (_tagHelperScopes.Count == 0)
            {
                throw new InvalidOperationException(Resources.RazorPage_ThereIsNoActiveWritingScopeToEnd);
            }

            // Get the content written during the current scope.
            var writer = ViewContext.Writer as HtmlContentWrapperTextWriter;
            var tagHelperContent = new DefaultTagHelperContent();
            tagHelperContent.AppendHtml(writer?.ContentBuilder);

            // Restore previous scope.
            var scopeInfo = _tagHelperScopes.Pop();
            HtmlEncoder = scopeInfo.Encoder;
            ViewContext.Writer = scopeInfo.Writer;

            return tagHelperContent;
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
        /// <paramref name="value"/>s of type <see cref="IHtmlContent"/> are written using
        /// <see cref="IHtmlContent.WriteTo(TextWriter, HtmlEncoder)"/>.
        /// For all other types, the encoded result of <see cref="object.ToString"/> is written to the
        /// <paramref name="writer"/>.
        /// </remarks>
        public virtual void WriteTo(TextWriter writer, object value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            WriteTo(writer, HtmlEncoder, value);
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> with HTML encoding to given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="encoder">
        /// The <see cref="System.Text.Encodings.Web.HtmlEncoder"/> to use when encoding <paramref name="value"/>.
        /// </param>
        /// <param name="value">The <see cref="object"/> to write.</param>
        /// <remarks>
        /// <paramref name="value"/>s of type <see cref="IHtmlContent"/> are written using
        /// <see cref="IHtmlContent.WriteTo(TextWriter, HtmlEncoder)"/>.
        /// For all other types, the encoded result of <see cref="object.ToString"/> is written to the
        /// <paramref name="writer"/>.
        /// </remarks>
        public static void WriteTo(TextWriter writer, HtmlEncoder encoder, object value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (encoder == null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            if (value == null || value == HtmlString.Empty)
            {
                return;
            }

            var htmlContent = value as IHtmlContent;
            if (htmlContent != null)
            {
                var htmlTextWriter = writer as HtmlTextWriter;
                if (htmlTextWriter == null)
                {
                    htmlContent.WriteTo(writer, encoder);
                }
                else
                {
                    // This special case allows us to keep buffering as IHtmlContent until we get to the 'final'
                    // TextWriter.
                    htmlTextWriter.Write(htmlContent);
                }

                return;
            }

            WriteTo(writer, encoder, value.ToString());
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> with HTML encoding to <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="string"/> to write.</param>
        public virtual void WriteTo(TextWriter writer, string value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            WriteTo(writer, HtmlEncoder, value);
        }

        private static void WriteTo(TextWriter writer, HtmlEncoder encoder, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                encoder.Encode(writer, value);
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
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (value != null)
            {
                WriteLiteralTo(writer, value.ToString());
            }
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> without HTML encoding to <see cref="Output"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="string"/> to write.</param>
        public virtual void WriteLiteralTo(TextWriter writer, string value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (!string.IsNullOrEmpty(value))
            {
                writer.Write(value);
            }
        }

        public virtual void BeginWriteAttribute(
            string name,
            string prefix,
            int prefixOffset,
            string suffix,
            int suffixOffset,
            int attributeValuesCount)
        {
            BeginWriteAttributeTo(Output, name, prefix, prefixOffset, suffix, suffixOffset, attributeValuesCount);
        }

        public virtual void BeginWriteAttributeTo(
            TextWriter writer,
            string name,
            string prefix,
            int prefixOffset,
            string suffix,
            int suffixOffset,
            int attributeValuesCount)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (prefix == null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            if (suffix == null)
            {
                throw new ArgumentNullException(nameof(suffix));
            }

            _attributeInfo = new AttributeInfo(name, prefix, prefixOffset, suffix, suffixOffset, attributeValuesCount);

            // Single valued attributes might be omitted in entirety if it the attribute value strictly evaluates to
            // null  or false. Consequently defer the prefix generation until we encounter the attribute value.
            if (attributeValuesCount != 1)
            {
                WritePositionTaggedLiteral(writer, prefix, prefixOffset);
            }
        }

        public void WriteAttributeValue(
            string prefix,
            int prefixOffset,
            object value,
            int valueOffset,
            int valueLength,
            bool isLiteral)
        {
            WriteAttributeValueTo(Output, prefix, prefixOffset, value, valueOffset, valueLength, isLiteral);
        }

        public void WriteAttributeValueTo(
            TextWriter writer,
            string prefix,
            int prefixOffset,
            object value,
            int valueOffset,
            int valueLength,
            bool isLiteral)
        {
            if (_attributeInfo.AttributeValuesCount == 1)
            {
                if (IsBoolFalseOrNullValue(prefix, value))
                {
                    // Value is either null or the bool 'false' with no prefix; don't render the attribute.
                    _attributeInfo.Suppressed = true;
                    return;
                }

                // We are not omitting the attribute. Write the prefix.
                WritePositionTaggedLiteral(writer, _attributeInfo.Prefix, _attributeInfo.PrefixOffset);

                if (IsBoolTrueWithEmptyPrefixValue(prefix, value))
                {
                    // The value is just the bool 'true', write the attribute name instead of the string 'True'.
                    value = _attributeInfo.Name;
                }
            }

            // This block handles two cases.
            // 1. Single value with prefix.
            // 2. Multiple values with or without prefix.
            if (value != null)
            {
                if (!string.IsNullOrEmpty(prefix))
                {
                    WritePositionTaggedLiteral(writer, prefix, prefixOffset);
                }

                BeginContext(valueOffset, valueLength, isLiteral);

                WriteUnprefixedAttributeValueTo(writer, value, isLiteral);

                EndContext();
            }
        }

        public virtual void EndWriteAttribute()
        {
            EndWriteAttributeTo(Output);
        }

        public virtual void EndWriteAttributeTo(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (!_attributeInfo.Suppressed)
            {
                WritePositionTaggedLiteral(writer, _attributeInfo.Suffix, _attributeInfo.SuffixOffset);
            }
        }

        public void BeginAddHtmlAttributeValues(
            TagHelperExecutionContext executionContext,
            string attributeName,
            int attributeValuesCount)
        {
            _tagHelperAttributeInfo = new TagHelperAttributeInfo(executionContext, attributeName, attributeValuesCount);
            _valueBuffer = null;
        }

        public void AddHtmlAttributeValue(
            string prefix,
            int prefixOffset,
            object value,
            int valueOffset,
            int valueLength,
            bool isLiteral)
        {
            Debug.Assert(_tagHelperAttributeInfo.ExecutionContext != null);
            if (_tagHelperAttributeInfo.AttributeValuesCount == 1)
            {
                if (IsBoolFalseOrNullValue(prefix, value))
                {
                    // The first value was 'null' or 'false' indicating that we shouldn't render the attribute. The
                    // attribute is treated as a TagHelper attribute so it's only available in
                    // TagHelperContext.AllAttributes for TagHelper authors to see (if they want to see why the
                    // attribute was removed from TagHelperOutput.Attributes).
                    _tagHelperAttributeInfo.ExecutionContext.AddTagHelperAttribute(
                        _tagHelperAttributeInfo.Name,
                        value?.ToString() ?? string.Empty);
                    _tagHelperAttributeInfo.Suppressed = true;
                    return;
                }
                else if (IsBoolTrueWithEmptyPrefixValue(prefix, value))
                {
                    _tagHelperAttributeInfo.ExecutionContext.AddHtmlAttribute(
                        _tagHelperAttributeInfo.Name,
                        _tagHelperAttributeInfo.Name);
                    _tagHelperAttributeInfo.Suppressed = true;
                    return;
                }
            }

            if (value != null)
            {
                if (_valueBuffer == null)
                {
                    var buffer = new ViewBuffer(BufferScope, Path);
                    _valueBuffer = new HtmlContentWrapperTextWriter(buffer, Output.Encoding);
                }

                if (!string.IsNullOrEmpty(prefix))
                {
                    WriteLiteralTo(_valueBuffer, prefix);
                }

                WriteUnprefixedAttributeValueTo(_valueBuffer, value, isLiteral);
            }
        }

        public void EndAddHtmlAttributeValues(TagHelperExecutionContext executionContext)
        {
            if (!_tagHelperAttributeInfo.Suppressed)
            {
                executionContext.AddHtmlAttribute(
                    _tagHelperAttributeInfo.Name,
                    (IHtmlContent)_valueBuffer?.ContentBuilder ?? HtmlString.Empty);
                _valueBuffer = null;
            }
        }

        public virtual string Href(string contentPath)
        {
            if (contentPath == null)
            {
                throw new ArgumentNullException(nameof(contentPath));
            }

            if (_urlHelper == null)
            {
                var services = Context.RequestServices;
                var factory = services.GetRequiredService<IUrlHelperFactory>();
                _urlHelper = factory.GetUrlHelper(ViewContext);
            }

            return _urlHelper.Content(contentPath);
        }

        private void WriteUnprefixedAttributeValueTo(TextWriter writer, object value, bool isLiteral)
        {
            var stringValue = value as string;

            // The extra branching here is to ensure that we call the Write*To(string) overload where possible.
            if (isLiteral && stringValue != null)
            {
                WriteLiteralTo(writer, stringValue);
            }
            else if (isLiteral)
            {
                WriteLiteralTo(writer, value);
            }
            else if (stringValue != null)
            {
                WriteTo(writer, stringValue);
            }
            else
            {
                WriteTo(writer, value);
            }
        }

        private void WritePositionTaggedLiteral(TextWriter writer, string value, int position)
        {
            BeginContext(position, value.Length, isLiteral: true);
            WriteLiteralTo(writer, value);
            EndContext();
        }

        /// <summary>
        /// In a Razor layout page, renders the portion of a content page that is not within a named section.
        /// </summary>
        /// <returns>The HTML content to render.</returns>
        protected virtual IHtmlContent RenderBody()
        {
            if (BodyContent == null)
            {
                var message = Resources.FormatRazorPage_MethodCannotBeCalled(nameof(RenderBody), Path);
                throw new InvalidOperationException(message);
            }

            _renderedBody = true;
            return BodyContent;
        }

        /// <summary>
        /// In a Razor layout page, ignores rendering the portion of a content page that is not within a named section.
        /// </summary>
        /// <returns>Returns <see cref="HtmlString.Empty"/> to allow the <see cref="Write(object)"/> call to
        /// succeed.</returns>
        public HtmlString IgnoreBody()
        {
            _ignoreBody = true;

            return HtmlString.Empty;
        }

        /// <summary>
        /// Creates a named content section in the page that can be invoked in a Layout page using
        /// <see cref="RenderSection(string)"/> or <see cref="RenderSectionAsync(string, bool)"/>.
        /// </summary>
        /// <param name="name">The name of the section to create.</param>
        /// <param name="section">The <see cref="RenderAsyncDelegate"/> to execute when rendering the section.</param>
        public void DefineSection(string name, RenderAsyncDelegate section)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            if (SectionWriters.ContainsKey(name))
            {
                throw new InvalidOperationException(Resources.FormatSectionAlreadyDefined(name));
            }
            SectionWriters[name] = section;
        }

        /// <summary>
        /// Returns a value that indicates whether the specified section is defined in the content page.
        /// </summary>
        /// <param name="name">The section name to search for.</param>
        /// <returns><c>true</c> if the specified section is defined in the content page; otherwise, <c>false</c>.</returns>
        public bool IsSectionDefined(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

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
        public HtmlString RenderSection(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

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
        public HtmlString RenderSection(string name, bool required)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            EnsureMethodCanBeInvoked(nameof(RenderSection));

            var task = RenderSectionAsyncCore(name, required);
            return task.GetAwaiter().GetResult();
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
        public Task<HtmlString> RenderSectionAsync(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

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
        public Task<HtmlString> RenderSectionAsync(string name, bool required)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            EnsureMethodCanBeInvoked(nameof(RenderSectionAsync));
            return RenderSectionAsyncCore(name, required);
        }

        private async Task<HtmlString> RenderSectionAsyncCore(string sectionName, bool required)
        {
            if (_renderedSections.Contains(sectionName))
            {
                var message = Resources.FormatSectionAlreadyRendered(nameof(RenderSectionAsync), Path, sectionName);
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
                var message = Resources.FormatSectionNotDefined(
                    ViewContext.ExecutingFilePath,
                    sectionName,
                    ViewContext.View.Path);
                throw new InvalidOperationException(message);
            }
            else
            {
                // If the section is optional and not found, then don't do anything.
                return null;
            }
        }

        /// <summary>
        /// In layout pages, ignores rendering the content of the section named <paramref name="sectionName"/>.
        /// </summary>
        /// <param name="sectionName">The section to ignore.</param>
        /// <returns>Returns <see cref="HtmlString.Empty"/> to allow the <see cref="Write(object)"/> call to
        /// succeed.</returns>
        public HtmlString IgnoreSection(string sectionName)
        {
            if (sectionName == null)
            {
                throw new ArgumentNullException(nameof(sectionName));
            }

            if (!PreviousSectionWriters.ContainsKey(sectionName))
            {
                // If the section is not defined, throw an error.
                throw new InvalidOperationException(Resources.FormatSectionNotDefined(sectionName, Path));
            }

            if (_ignoredSections == null)
            {
                _ignoredSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            _ignoredSections.Add(sectionName);

            return HtmlString.Empty;
        }

        /// <summary>
        /// Invokes <see cref="TextWriter.FlushAsync"/> on <see cref="Output"/> and <see cref="Stream.FlushAsync"/>
        /// on the response stream, writing out any buffered content to the <see cref="HttpResponse.Body"/>.
        /// </summary>
        /// <returns>A <see cref="Task{HtmlString}"/> that represents the asynchronous flush operation and on
        /// completion returns <see cref="HtmlString.Empty"/>.</returns>
        /// <remarks>The value returned is a token value that allows FlushAsync to work directly in an HTML
        /// section. However the value does not represent the rendered content.
        /// This method also writes out headers, so any modifications to headers must be done before
        /// <see cref="FlushAsync"/> is called. For example, call <see cref="SetAntiforgeryCookieAndHeader"/> to send
        /// antiforgery cookie token and X-Frame-Options header to client before this method flushes headers out.
        /// </remarks>
        public async Task<HtmlString> FlushAsync()
        {
            // If there are active scopes, then we should throw. Cannot flush content that has the potential to change.
            if (_tagHelperScopes.Count > 0)
            {
                throw new InvalidOperationException(
                    Resources.FormatRazorPage_CannotFlushWhileInAWritingScope(nameof(FlushAsync), Path));
            }

            // Calls to Flush are allowed if the page does not specify a Layout or if it is executing a section in the
            // Layout.
            if (!IsLayoutBeingRendered && !string.IsNullOrEmpty(Layout))
            {
                var message = Resources.FormatLayoutCannotBeRendered(Path, nameof(FlushAsync));
                throw new InvalidOperationException(message);
            }

            await Output.FlushAsync();
            await Context.Response.Body.FlushAsync();
            return HtmlString.Empty;
        }

        /// <inheritdoc />
        public void EnsureRenderedBodyOrSections()
        {
            // a) all sections defined for this page are rendered.
            // b) if no sections are defined, then the body is rendered if it's available.
            if (PreviousSectionWriters != null && PreviousSectionWriters.Count > 0)
            {
                var sectionsNotRendered = PreviousSectionWriters.Keys.Except(
                    _renderedSections,
                    StringComparer.OrdinalIgnoreCase);

                string[] sectionsNotIgnored;
                if (_ignoredSections != null)
                {
                    sectionsNotIgnored = sectionsNotRendered.Except(_ignoredSections, StringComparer.OrdinalIgnoreCase).ToArray();
                }
                else
                {
                    sectionsNotIgnored = sectionsNotRendered.ToArray();
                }

                if (sectionsNotIgnored.Length > 0)
                {
                    var sectionNames = string.Join(", ", sectionsNotIgnored);
                    throw new InvalidOperationException(Resources.FormatSectionsNotRendered(Path, sectionNames, nameof(IgnoreSection)));
                }
            }
            else if (BodyContent != null && !_renderedBody && !_ignoreBody)
            {
                // There are no sections defined, but RenderBody was NOT called.
                // If a body was defined and the body not ignored, then RenderBody should have been called.
                var message = Resources.FormatRenderBodyNotCalled(nameof(RenderBody), Path, nameof(IgnoreBody));
                throw new InvalidOperationException(message);
            }
        }

        public void BeginContext(int position, int length, bool isLiteral)
        {
            const string BeginContextEvent = "Microsoft.AspNetCore.Mvc.Razor.BeginInstrumentationContext";

            if (DiagnosticSource?.IsEnabled(BeginContextEvent) == true)
            {
                DiagnosticSource.Write(
                    BeginContextEvent,
                    new
                    {
                        httpContext = Context,
                        path = Path,
                        position = position,
                        length = length,
                        isLiteral = isLiteral,
                    });
            }
        }

        public void EndContext()
        {
            const string EndContextEvent = "Microsoft.AspNetCore.Mvc.Razor.EndInstrumentationContext";

            if (DiagnosticSource?.IsEnabled(EndContextEvent) == true)
            {
                DiagnosticSource.Write(
                    EndContextEvent,
                    new
                    {
                        httpContext = Context,
                        path = Path,
                    });
            }
        }

        /// <summary>
        /// Sets antiforgery cookie and X-Frame-Options header on the response.
        /// </summary>
        /// <returns><see cref="HtmlString.Empty"/>.</returns>
        /// <remarks> Call this method to send antiforgery cookie token and X-Frame-Options header to client
        /// before <see cref="FlushAsync"/> flushes the headers. </remarks>
        public virtual HtmlString SetAntiforgeryCookieAndHeader()
        {
            var antiforgery = Context.RequestServices.GetRequiredService<IAntiforgery>();
            antiforgery.SetCookieTokenAndHeader(Context);

            return HtmlString.Empty;
        }

        private bool IsBoolFalseOrNullValue(string prefix, object value)
        {
            return string.IsNullOrEmpty(prefix) &&
                (value == null ||
                (value is bool && !(bool)value));
        }

        private bool IsBoolTrueWithEmptyPrefixValue(string prefix, object value)
        {
            // If the value is just the bool 'true', use the attribute name as the value.
            return string.IsNullOrEmpty(prefix) &&
                (value is bool && (bool)value);
        }

        private void EnsureMethodCanBeInvoked(string methodName)
        {
            if (PreviousSectionWriters == null)
            {
                throw new InvalidOperationException(Resources.FormatRazorPage_MethodCannotBeCalled(methodName, Path));
            }
        }

        private struct AttributeInfo
        {
            public AttributeInfo(
                string name,
                string prefix,
                int prefixOffset,
                string suffix,
                int suffixOffset,
                int attributeValuesCount)
            {
                Name = name;
                Prefix = prefix;
                PrefixOffset = prefixOffset;
                Suffix = suffix;
                SuffixOffset = suffixOffset;
                AttributeValuesCount = attributeValuesCount;

                Suppressed = false;
            }

            public int AttributeValuesCount { get; }

            public string Name { get; }

            public string Prefix { get; }

            public int PrefixOffset { get; }

            public string Suffix { get; }

            public int SuffixOffset { get; }

            public bool Suppressed { get; set; }
        }

        private struct TagHelperAttributeInfo
        {
            public TagHelperAttributeInfo(
                TagHelperExecutionContext tagHelperExecutionContext,
                string name,
                int attributeValuesCount)
            {
                ExecutionContext = tagHelperExecutionContext;
                Name = name;
                AttributeValuesCount = attributeValuesCount;

                Suppressed = false;
            }

            public string Name { get; }

            public TagHelperExecutionContext ExecutionContext { get; }

            public int AttributeValuesCount { get; }

            public bool Suppressed { get; set; }
        }

        private struct TagHelperScopeInfo
        {
            public TagHelperScopeInfo(HtmlEncoder encoder, TextWriter writer)
            {
                Encoder = encoder;
                Writer = writer;
            }

            public HtmlEncoder Encoder { get; }

            public TextWriter Writer { get; }
        }
    }
}