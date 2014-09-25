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
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Represents properties and methods that are needed in order to render a view that uses Razor syntax.
    /// </summary>
    public abstract class RazorPage : IRazorPage
    {
        private readonly HashSet<string> _renderedSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private TextWriter _scopedWriter;
        private Stack<TextWriter> _writerScopes;
        private IUrlHelper _urlHelper;
        private bool _renderedBody;

        public RazorPage()
        {
            SectionWriters = new Dictionary<string, HelperResult>(StringComparer.OrdinalIgnoreCase);

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

        public string Layout { get; set; }

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

                return _scopedWriter ?? ViewContext.Writer;
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
        public Dictionary<string, HelperResult> PreviousSectionWriters { get; set; }

        /// <inheritdoc />
        public Dictionary<string, HelperResult> SectionWriters { get; private set; }

        /// <inheritdoc />
        public abstract Task ExecuteAsync();

        /// <summary>
        /// Starts a new writing scope.
        /// </summary>
        /// <remarks>
        /// All writes to the <see cref="Output"/> after calling this method will be buffered until 
        /// <see cref="EndWritingScope"/> is called.
        /// </remarks>
        public void StartWritingScope()
        {
            StartWritingScope(writer: new StringWriter());
        }

        /// <summary>
        /// Starts a new writing scope with the given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">A writer to use for the writing scope.</param>
        /// <remarks>
        /// All writes to the <see cref="Output"/> after calling this method will be buffered by the given
        /// <paramref name="writer"/> until <see cref="EndWritingScope"/> is called.
        /// </remarks>
        public void StartWritingScope(TextWriter writer)
        {
            _scopedWriter = writer;

            _writerScopes.Push(_scopedWriter);
        }

        /// <summary>
        /// Ends the current writing scope that was started by calling <see cref="StartWritingScope"/> or 
        /// <see cref="StartWritingScope(TextWriter)"/>.
        /// </summary>
        /// <returns>The <see cref="TextWriter"/> that contains the content written to the <see cref="Output"/> during the 
        /// writing scope.</returns>
        public TextWriter EndWritingScope()
        {
            if (_writerScopes.Count == 0)
            {
                throw new InvalidOperationException(Resources.RazorPage_ThereIsNoActiveWritingScopeToEnd);
            }

            var writer = _writerScopes.Pop();

            if (_writerScopes.Count > 0)
            {
                _scopedWriter = _writerScopes.Peek();
            }
            else
            {
                _scopedWriter = null;
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
            if (value != null)
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
                        writer.Write(htmlString.ToString());
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
                _urlHelper = Context.RequestServices.GetService<IUrlHelper>();
            }

            return _urlHelper.Content(contentPath);
        }

        private void WritePositionTaggedLiteral(TextWriter writer, string value, int position)
        {
            WriteLiteralTo(writer, value);
        }

        private void WritePositionTaggedLiteral(TextWriter writer, PositionTagged<string> value)
        {
            WritePositionTaggedLiteral(writer, value.Value, value.Position);
        }

        protected virtual HelperResult RenderBody()
        {
            if (RenderBodyDelegate == null)
            {
                throw new InvalidOperationException(Resources.FormatRenderBodyCannotBeCalled("RenderBody"));
            }

            _renderedBody = true;
            return new HelperResult(RenderBodyDelegate);
        }

        /// <summary>
        /// Creates a named content section in the page that can be invoked in a Layout page using 
        /// <see cref="RenderSection(string)"/> or <see cref="RenderSection(string, bool)"/>.
        /// </summary>
        /// <param name="name">The name of the section to create.</param>
        /// <param name="section">The <see cref="HelperResult"/> to execute when rendering the section.</param>
        public void DefineSection(string name, HelperResult section)
        {
            if (SectionWriters.ContainsKey(name))
            {
                throw new InvalidOperationException(Resources.FormatSectionAlreadyDefined(name));
            }
            SectionWriters[name] = section;
        }

        public bool IsSectionDefined([NotNull] string name)
        {
            EnsureMethodCanBeInvoked("IsSectionDefined");
            return PreviousSectionWriters.ContainsKey(name);
        }

        public HelperResult RenderSection([NotNull] string name)
        {
            return RenderSection(name, required: true);
        }

        public HelperResult RenderSection([NotNull] string name, bool required)
        {
            EnsureMethodCanBeInvoked("RenderSection");
            if (_renderedSections.Contains(name))
            {
                throw new InvalidOperationException(Resources.FormatSectionAlreadyRendered("RenderSection", name));
            }

            HelperResult action;
            if (PreviousSectionWriters.TryGetValue(name, out action))
            {
                _renderedSections.Add(name);
                return action;
            }
            else if (required)
            {
                // If the section is not found, and it is not optional, throw an error.
                throw new InvalidOperationException(Resources.FormatSectionNotDefined(name));
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
        /// <returns>A <see cref="Task"/> that represents the asynchronous flush operation.</returns>
        public Task FlushAsync()
        {
            // If there are active writing scopes then we should throw. Cannot flush content that has the potential to
            // change.
            if(_writerScopes.Count > 0)
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

            return Output.FlushAsync();
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
                throw new InvalidOperationException(Resources.FormatRenderBodyNotCalled("RenderBody"));
            }
        }

        private void EnsureMethodCanBeInvoked(string methodName)
        {
            if (PreviousSectionWriters == null)
            {
                throw new InvalidOperationException(Resources.FormatView_MethodCannotBeCalled(methodName));
            }
        }
    }
}