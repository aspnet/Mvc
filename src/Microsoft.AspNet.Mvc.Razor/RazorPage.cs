// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Antiforgery;
using Microsoft.AspNet.Html.Abstractions;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.AspNet.Mvc.Razor.Internal;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.AspNet.PageExecutionInstrumentation;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.WebEncoders;

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
        private ITypeActivatorCache _typeActivatorCache;
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

        /// <summary>
        /// Gets the <see cref="IHtmlEncoder"/> to be used for encoding HTML.
        /// </summary>
        [RazorInject]
        public IHtmlEncoder HtmlEncoder { get; set; }

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

        public virtual ClaimsPrincipal User
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
                return ViewContext?.ViewBag;
            }
        }

        /// <summary>
        /// Gets the <see cref="ITempDataDictionary"/> from the <see cref="ViewContext"/>.
        /// </summary>
        /// <remarks>Returns null if <see cref="ViewContext"/> is null.</remarks>
        public ITempDataDictionary TempData
        {
            get
            {
                return ViewContext?.TempData;
            }
        }

        /// <inheritdoc />
        public Func<TextWriter, Task> RenderBodyDelegateAsync { get; set; }

        /// <inheritdoc />
        public bool IsLayoutBeingRendered { get; set; }

        /// <inheritdoc />
        public IDictionary<string, RenderAsyncDelegate> PreviousSectionWriters { get; set; }

        /// <inheritdoc />
        public IDictionary<string, RenderAsyncDelegate> SectionWriters { get; private set; }

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
        /// Starts a new writing scope.
        /// </summary>
        /// <remarks>
        /// All writes to the <see cref="Output"/> or <see cref="ViewContext.Writer"/> after calling this method will
        /// be buffered until <see cref="EndTagHelperWritingScope"/> is called.
        /// </remarks>
        public void StartTagHelperWritingScope()
        {
            StartTagHelperWritingScope(new StringCollectionTextWriter(Output.Encoding));
        }

        /// <summary>
        /// Starts a new writing scope with the given <paramref name="writer"/>.
        /// </summary>
        /// <remarks>
        /// All writes to the <see cref="Output"/> or <see cref="ViewContext.Writer"/> after calling this method will
        /// be buffered until <see cref="EndTagHelperWritingScope"/> is called.
        /// </remarks>
        public void StartTagHelperWritingScope(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            // If there isn't a base writer take the ViewContext.Writer
            if (_originalWriter == null)
            {
                _originalWriter = ViewContext.Writer;
            }

            // We need to replace the ViewContext's Writer to ensure that all content (including content written
            // from HTML helpers) is redirected.
            ViewContext.Writer = writer;

            _writerScopes.Push(ViewContext.Writer);
        }

        /// <summary>
        /// Ends the current writing scope that was started by calling <see cref="StartTagHelperWritingScope"/>.
        /// </summary>
        /// <returns>The <see cref="TextWriter"/> that contains the content written to the <see cref="Output"/> or
        /// <see cref="ViewContext.Writer"/> during the writing scope.</returns>
        public TagHelperContent EndTagHelperWritingScope()
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

            var tagHelperContentWrapperTextWriter = new TagHelperContentWrapperTextWriter(Output.Encoding);
            var razorWriter = writer as RazorTextWriter;
            if (razorWriter != null)
            {
                razorWriter.CopyTo(tagHelperContentWrapperTextWriter);
            }
            else
            {
                var stringCollectionTextWriter = writer as StringCollectionTextWriter;
                if (stringCollectionTextWriter != null)
                {
                    stringCollectionTextWriter.CopyTo(tagHelperContentWrapperTextWriter, HtmlEncoder);
                }
                else
                {
                    tagHelperContentWrapperTextWriter.Write(writer.ToString());
                }
            }

            return tagHelperContentWrapperTextWriter.Content;
        }

        /// <summary>
        /// Writes the content of a specified <paramref name="tagHelperExecutionContext"/>.
        /// </summary>
        /// <param name="tagHelperExecutionContext">The execution context containing the content.</param>
        /// <returns>
        /// A <see cref="Task"/> that on completion writes the <paramref name="tagHelperExecutionContext"/> content.
        /// </returns>
        public Task WriteTagHelperAsync(TagHelperExecutionContext tagHelperExecutionContext)
        {
            if (tagHelperExecutionContext == null)
            {
                throw new ArgumentNullException(nameof(tagHelperExecutionContext));
            }

            return WriteTagHelperToAsync(Output, tagHelperExecutionContext);
        }

        /// <summary>
        /// Writes the content of a specified <paramref name="tagHelperExecutionContext"/> to the specified
        /// <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="tagHelperExecutionContext">The execution context containing the content.</param>
        /// <returns>
        /// A <see cref="Task"/> that on completion writes the <paramref name="tagHelperExecutionContext"/> content
        /// to the <paramref name="writer"/>.
        /// </returns>
        public async Task WriteTagHelperToAsync(
            TextWriter writer,
            TagHelperExecutionContext tagHelperExecutionContext)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (tagHelperExecutionContext == null)
            {
                throw new ArgumentNullException(nameof(tagHelperExecutionContext));
            }

            var tagHelperOutput = tagHelperExecutionContext.Output;
            var isTagNameNullOrWhitespace = string.IsNullOrWhiteSpace(tagHelperOutput.TagName);

            WriteTo(writer, tagHelperOutput.PreElement);

            if (!isTagNameNullOrWhitespace)
            {
                writer.Write('<');
                writer.Write(tagHelperOutput.TagName);

                foreach (var attribute in tagHelperOutput.Attributes)
                {
                    writer.Write(' ');
                    writer.Write(attribute.Name);

                    if (!attribute.Minimized)
                    {
                        writer.Write("=\"");
                        WriteTo(writer, HtmlEncoder, attribute.Value, escapeQuotes: true);
                        writer.Write('"');
                    }
                }

                if (tagHelperOutput.TagMode == TagMode.SelfClosing)
                {
                    writer.Write(" /");
                }

                writer.Write('>');
            }

            if (isTagNameNullOrWhitespace || tagHelperOutput.TagMode == TagMode.StartTagAndEndTag)
            {
                WriteTo(writer, tagHelperOutput.PreContent);
                if (tagHelperOutput.IsContentModified)
                {
                    WriteTo(writer, tagHelperOutput.Content);
                }
                else if (tagHelperExecutionContext.ChildContentRetrieved)
                {
                    var childContent = await tagHelperExecutionContext.GetChildContentAsync(useCachedResult: true);
                    WriteTo(writer, childContent);
                }
                else
                {
                    await tagHelperExecutionContext.ExecuteChildContentAsync();
                }

                WriteTo(writer, tagHelperOutput.PostContent);
            }

            if (!isTagNameNullOrWhitespace && tagHelperOutput.TagMode == TagMode.StartTagAndEndTag)
            {
                writer.Write(string.Format(CultureInfo.InvariantCulture, "</{0}>", tagHelperOutput.TagName));
            }

            WriteTo(writer, tagHelperOutput.PostElement);
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
        /// <see cref="IHtmlContent.WriteTo(TextWriter, IHtmlEncoder)"/>.
        /// For all other types, the encoded result of <see cref="object.ToString"/> is written to the
        /// <paramref name="writer"/>.
        /// </remarks>
        public virtual void WriteTo(TextWriter writer, object value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            WriteTo(writer, HtmlEncoder, value, escapeQuotes: false);
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> with HTML encoding to given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="encoder">The <see cref="IHtmlEncoder"/> to use when encoding <paramref name="value"/>.</param>
        /// <param name="value">The <see cref="object"/> to write.</param>
        /// <param name="escapeQuotes">
        /// If <c>true</c> escapes double quotes in a <paramref name="value"/> of type <see cref="HtmlString"/>.
        /// Otherwise writes <see cref="HtmlString"/> values as-is.
        /// </param>
        /// <remarks>
        /// <paramref name="value"/>s of type <see cref="IHtmlContent"/> are written using
        /// <see cref="IHtmlContent.WriteTo(TextWriter, IHtmlEncoder)"/>.
        /// For all other types, the encoded result of <see cref="object.ToString"/> is written to the
        /// <paramref name="writer"/>.
        /// </remarks>
        public static void WriteTo(
            TextWriter writer,
            IHtmlEncoder encoder,
            object value,
            bool escapeQuotes)
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
                if (escapeQuotes)
                {
                    // In this case the text likely came directly from the Razor source. Since the original string is
                    // an attribute value that may have been quoted with single quotes, must handle any double quotes
                    // in the value. Writing the value out surrounded by double quotes.
                    //
                    // This is really not optimal from a perf point of view, but it's the best we can do for right now.
                    using (var stringWriter = new StringWriter())
                    {
                        htmlContent.WriteTo(stringWriter, encoder);

                        var stringValue = stringWriter.ToString();
                        if (stringValue.Contains("\""))
                        {
                            stringValue = stringValue.Replace("\"", "&quot;");
                        }

                        writer.Write(stringValue);
                        return;
                    }
                }

                var htmlTextWriter = writer as HtmlTextWriter;
                if (htmlTextWriter == null)
                {
                    htmlContent.WriteTo(writer, encoder);
                }
                else
                {
                    // This special case alows us to keep buffering as IHtmlContent until we get to the 'final'
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

        private static void WriteTo(TextWriter writer, IHtmlEncoder encoder, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                encoder.HtmlEncode(value, writer);
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

        public virtual void WriteAttribute(
            string name,
            PositionTagged<string> prefix,
            PositionTagged<string> suffix,
            params AttributeValue[] values)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            if (suffix == null)
            {
                throw new ArgumentNullException(nameof(suffix));
            }

            WriteAttributeTo(Output, name, prefix, suffix, values);
        }

        public virtual void WriteAttributeTo(
            TextWriter writer,
            string name,
            PositionTagged<string> prefix,
            PositionTagged<string> suffix,
            params AttributeValue[] values)
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

            if (values.Length == 0)
            {
                // Explicitly empty attribute, so write the prefix
                WritePositionTaggedLiteral(writer, prefix);
            }
            else if (IsSingleBoolFalseOrNullValue(values))
            {
                // Value is either null or the bool 'false' with no prefix; don't render the attribute.
                return;
            }
            else if (UseAttributeNameAsValue(values))
            {
                var attributeValue = values[0];
                var positionTaggedAttributeValue = attributeValue.Value;

                WritePositionTaggedLiteral(writer, prefix);

                var sourceLength = suffix.Position - positionTaggedAttributeValue.Position;
                var nameAttributeValue = new AttributeValue(
                    attributeValue.Prefix,
                    new PositionTagged<object>(name, attributeValue.Value.Position),
                    literal: attributeValue.Literal);

                // The value is just the bool 'true', write the attribute name instead of the string 'True'.
                WriteAttributeValue(writer, nameAttributeValue, sourceLength);
            }
            else
            {
                // This block handles two cases.
                // 1. Single value with prefix.
                // 2. Multiple values with or without prefix.
                WritePositionTaggedLiteral(writer, prefix);
                for (var i = 0; i < values.Length; i++)
                {
                    var attributeValue = values[i];
                    var positionTaggedAttributeValue = attributeValue.Value;

                    if (positionTaggedAttributeValue.Value == null)
                    {
                        // Nothing to write
                        continue;
                    }

                    var next = i == values.Length - 1 ?
                        suffix : // End of the list, grab the suffix
                        values[i + 1].Prefix; // Still in the list, grab the next prefix

                    // Calculate length of the source span by the position of the next value (or suffix)
                    var sourceLength = next.Position - attributeValue.Value.Position;

                    WriteAttributeValue(writer, attributeValue, sourceLength);
                }
            }

            WritePositionTaggedLiteral(writer, suffix);
        }

        public void AddHtmlAttributeValues(
            string attributeName,
            TagHelperExecutionContext executionContext,
            params AttributeValue[] values)
        {
            if (IsSingleBoolFalseOrNullValue(values))
            {
                // The first value was 'null' or 'false' indicating that we shouldn't render the attribute. The
                // attribute is treated as a TagHelper attribute so it's only available in
                // TagHelperContext.AllAttributes for TagHelper authors to see (if they want to see why the attribute
                // was removed from TagHelperOutput.Attributes).
                executionContext.AddTagHelperAttribute(
                    attributeName,
                    values[0].Value.Value?.ToString() ?? string.Empty);

                return;
            }
            else if (UseAttributeNameAsValue(values))
            {
                executionContext.AddHtmlAttribute(attributeName, attributeName);
            }
            else
            {
                var valueBuffer = new StringCollectionTextWriter(Output.Encoding);

                foreach (var value in values)
                {
                    if (value.Value.Value == null)
                    {
                        // Skip null values
                        continue;
                    }

                    if (!string.IsNullOrEmpty(value.Prefix))
                    {
                        WriteLiteralTo(valueBuffer, value.Prefix);
                    }

                    WriteUnprefixedAttributeValueTo(valueBuffer, value);
                }

                using (var stringWriter = new StringWriter())
                {
                    valueBuffer.Content.WriteTo(stringWriter, HtmlEncoder);

                    var htmlString = new HtmlString(stringWriter.ToString());
                    executionContext.AddHtmlAttribute(attributeName, htmlString);
                }
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
                _urlHelper = Context.RequestServices.GetRequiredService<IUrlHelper>();
            }

            return _urlHelper.Content(contentPath);
        }

        private void WriteAttributeValue(TextWriter writer, AttributeValue attributeValue, int sourceLength)
        {
            if (!string.IsNullOrEmpty(attributeValue.Prefix))
            {
                WritePositionTaggedLiteral(writer, attributeValue.Prefix);
            }

            BeginContext(attributeValue.Value.Position, sourceLength, isLiteral: attributeValue.Literal);

            WriteUnprefixedAttributeValueTo(writer, attributeValue);

            EndContext();
        }

        private void WriteUnprefixedAttributeValueTo(TextWriter writer, AttributeValue attributeValue)
        {
            var positionTaggedAttributeValue = attributeValue.Value;
            var stringValue = positionTaggedAttributeValue.Value as string;

            // The extra branching here is to ensure that we call the Write*To(string) overload where possible.
            if (attributeValue.Literal && stringValue != null)
            {
                WriteLiteralTo(writer, stringValue);
            }
            else if (attributeValue.Literal)
            {
                WriteLiteralTo(writer, positionTaggedAttributeValue.Value);
            }
            else if (stringValue != null)
            {
                WriteTo(writer, stringValue);
            }
            else
            {
                WriteTo(writer, positionTaggedAttributeValue.Value);
            }
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
            if (RenderBodyDelegateAsync == null)
            {
                var message = Resources.FormatRazorPage_MethodCannotBeCalled(nameof(RenderBody), Path);
                throw new InvalidOperationException(message);
            }

            _renderedBody = true;
            return new HelperResult(RenderBodyDelegateAsync);
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
                throw new InvalidOperationException(Resources.FormatSectionNotDefined(sectionName, Path));
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
        /// <returns>A <see cref="Task{HtmlString}"/> that represents the asynchronous flush operation and on
        /// completion returns a <see cref="HtmlString.Empty"/>.</returns>
        /// <remarks>The value returned is a token value that allows FlushAsync to work directly in an HTML
        /// section. However the value does not represent the rendered content.
        /// This method also writes out headers, so any modifications to headers must be done before
        /// <see cref="FlushAsync"/> is called. For example, call <see cref="SetAntiforgeryCookieAndHeader"/> to send
        /// antiforgery cookie token and X-Frame-Options header to client before this method flushes headers out.
        /// </remarks>
        public async Task<HtmlString> FlushAsync()
        {
            // If there are active writing scopes then we should throw. Cannot flush content that has the potential to
            // change.
            if (_writerScopes.Count > 0)
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

                if (sectionsNotRendered.Any())
                {
                    var sectionNames = string.Join(", ", sectionsNotRendered);
                    throw new InvalidOperationException(Resources.FormatSectionsNotRendered(Path, sectionNames));
                }
            }
            else if (RenderBodyDelegateAsync != null && !_renderedBody)
            {
                // There are no sections defined, but RenderBody was NOT called.
                // If a body was defined, then RenderBody should have been called.
                var message = Resources.FormatRenderBodyNotCalled(nameof(RenderBody), Path);
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

        /// <summary>
        /// Sets antiforgery cookie and X-Frame-Options header on the response.
        /// </summary>
        /// <returns>A <see cref="HtmlString"/> that returns a <see cref="HtmlString.Empty"/>.</returns>
        /// <remarks> Call this method to send antiforgery cookie token and X-Frame-Options header to client
        /// before <see cref="FlushAsync"/> flushes the headers. </remarks>
        public virtual HtmlString SetAntiforgeryCookieAndHeader()
        {
            var antiforgery = Context.RequestServices.GetRequiredService<IAntiforgery>();
            antiforgery.SetCookieTokenAndHeader(Context);

            return HtmlString.Empty;
        }

        private bool IsSingleBoolFalseOrNullValue(AttributeValue[] values)
        {
            if (values.Length == 1 && string.IsNullOrEmpty(values[0].Prefix) &&
                (values[0].Value.Value is bool || values[0].Value.Value == null))
            {
                var attributeValue = values[0];
                var positionTaggedAttributeValue = attributeValue.Value;

                if (positionTaggedAttributeValue.Value == null || !(bool)positionTaggedAttributeValue.Value)
                {
                    return true;
                }
            }

            return false;
        }

        private bool UseAttributeNameAsValue(AttributeValue[] values)
        {
            // If the value is just the bool 'true', use the attribute name as the value.
            return values.Length == 1 &&
                string.IsNullOrEmpty(values[0].Prefix) &&
                values[0].Value.Value is bool &&
                (bool)values[0].Value.Value;
        }

        private void EnsureMethodCanBeInvoked(string methodName)
        {
            if (PreviousSectionWriters == null)
            {
                throw new InvalidOperationException(Resources.FormatRazorPage_MethodCannotBeCalled(methodName, Path));
            }
        }
    }
}