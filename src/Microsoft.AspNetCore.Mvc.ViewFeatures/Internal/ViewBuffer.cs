// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    /// <summary>
    /// An <see cref="IHtmlContentBuilder"/> that is backed by a buffer provided by <see cref="IViewBufferScope"/>.
    /// </summary>
    [DebuggerDisplay("{DebuggerToString()}")]
    public class ViewBuffer : IHtmlContentBuilder
    {
        public static readonly int PartialViewPageSize = 32;
        public static readonly int TagHelperPageSize = 32;
        public static readonly int ViewComponentPageSize = 32;
        public static readonly int ViewPageSize = 256;

        private readonly IViewBufferScope _bufferScope;
        private readonly string _name;
        private readonly int _pageSize;
        private ViewBufferPage _currentPage;         // Limits allocation if the ViewBuffer has only one page (frequent case).
        private List<ViewBufferPage> _multiplePages; // Allocated only if necessary

        /// <summary>
        /// Initializes a new instance of <see cref="ViewBuffer"/>.
        /// </summary>
        /// <param name="bufferScope">The <see cref="IViewBufferScope"/>.</param>
        /// <param name="name">A name to identify this instance.</param>
        /// <param name="pageSize">The size of buffer pages.</param>
        public ViewBuffer(IViewBufferScope bufferScope, string name, int pageSize)
        {
            if (bufferScope == null)
            {
                throw new ArgumentNullException(nameof(bufferScope));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            _bufferScope = bufferScope;
            _name = name;
            _pageSize = pageSize;
        }

        /// <summary>
        /// Get the <see cref="ViewBufferPage"/> count.
        /// </summary>
        public int Count
        {
            get
            {
                if (_multiplePages != null)
                {
                    return _multiplePages.Count;
                }
                if (_currentPage != null)
                {
                    return 1;
                }
                return 0;
            }
        }

        /// <summary>
        /// Gets a <see cref="ViewBufferPage"/>.
        /// </summary>
        public ViewBufferPage this[int index]
        {
            get
            {
                if (_multiplePages != null)
                {
                    return _multiplePages[index];
                }
                if (index == 0 && _currentPage != null)
                {
                    return _currentPage;
                }
                throw new IndexOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public IHtmlContentBuilder Append(string unencoded)
        {
            if (unencoded == null)
            {
                return this;
            }

            // Text that needs encoding is the uncommon case in views, which is why it
            // creates a wrapper and pre-encoded text does not.
            AppendValue(new ViewBufferValue(new EncodingWrapper(unencoded)));
            return this;
        }

        /// <inheritdoc />
        public IHtmlContentBuilder AppendHtml(IHtmlContent content)
        {
            if (content == null)
            {
                return this;
            }

            AppendValue(new ViewBufferValue(content));
            return this;
        }

        /// <inheritdoc />
        public IHtmlContentBuilder AppendHtml(string encoded)
        {
            if (encoded == null)
            {
                return this;
            }

            AppendValue(new ViewBufferValue(encoded));
            return this;
        }

        private void AppendValue(ViewBufferValue value)
        {
            var page = GetCurrentPage();
            page.Append(value);
        }

        private ViewBufferPage GetCurrentPage()
        {
            if (_currentPage == null || _currentPage.IsFull)
            {
                AddPage(new ViewBufferPage(_bufferScope.GetPage(_pageSize)));
            }
            return _currentPage;
        }

        private void AddPage(ViewBufferPage page)
        {
            if (_multiplePages != null)
            {
                _multiplePages.Add(page);
            }
            else if (_currentPage != null)
            {
                _multiplePages = new List<ViewBufferPage>(2);
                _multiplePages.Add(_currentPage);
                _multiplePages.Add(page);
            }

            _currentPage = page;
        }

        /// <inheritdoc />
        public IHtmlContentBuilder Clear()
        {
            _multiplePages = null;
            _currentPage = null;
            return this;
        }

        /// <inheritdoc />
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (encoder == null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            for (var i = 0; i < Count; i++)
            {
                var page = this[i];
                for (var j = 0; j < page.Count; j++)
                {
                    var value = page.Buffer[j];

                    if (value.Value is string valueAsString)
                    {
                        writer.Write(valueAsString);
                        continue;
                    }

                    if (value.Value is IHtmlContent valueAsHtmlContent)
                    {
                        valueAsHtmlContent.WriteTo(writer, encoder);
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Writes the buffered content to <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/>.</param>
        /// <param name="encoder">The <see cref="HtmlEncoder"/>.</param>
        /// <returns>A <see cref="Task"/> which will complete once content has been written.</returns>
        public async Task WriteToAsync(TextWriter writer, HtmlEncoder encoder)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (encoder == null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            for (var i = 0; i < Count; i++)
            {
                var page = this[i];
                for (var j = 0; j < page.Count; j++)
                {
                    var value = page.Buffer[j];

                    if (value.Value is string valueAsString)
                    {
                        await writer.WriteAsync(valueAsString);
                        continue;
                    }

                    if (value.Value is ViewBuffer valueAsViewBuffer)
                    {
                        await valueAsViewBuffer.WriteToAsync(writer, encoder);
                        continue;
                    }

                    if (value.Value is IHtmlContent valueAsHtmlContent)
                    {
                        valueAsHtmlContent.WriteTo(writer, encoder);
                        await writer.FlushAsync();
                        continue;
                    }
                }
            }
        }

        private string DebuggerToString() => _name;

        public void CopyTo(IHtmlContentBuilder destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            for (var i = 0; i < Count; i++)
            {
                var page = this[i];
                for (var j = 0; j < page.Count; j++)
                {
                    var value = page.Buffer[j];

                    string valueAsString;
                    IHtmlContentContainer valueAsContainer;
                    if ((valueAsString = value.Value as string) != null)
                    {
                        destination.AppendHtml(valueAsString);
                    }
                    else if ((valueAsContainer = value.Value as IHtmlContentContainer) != null)
                    {
                        valueAsContainer.CopyTo(destination);
                    }
                    else
                    {
                        destination.AppendHtml((IHtmlContent)value.Value);
                    }
                }
            }
        }

        public void MoveTo(IHtmlContentBuilder destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            // Perf: We have an efficient implementation when the destination is another view buffer,
            // we can just insert our pages as-is.
            if (destination is ViewBuffer other)
            {
                MoveTo(other);
                return;
            }

            for (var i = 0; i < Count; i++)
            {
                var page = this[i];
                for (var j = 0; j < page.Count; j++)
                {
                    var value = page.Buffer[j];

                    string valueAsString;
                    IHtmlContentContainer valueAsContainer;
                    if ((valueAsString = value.Value as string) != null)
                    {
                        destination.AppendHtml(valueAsString);
                    }
                    else if ((valueAsContainer = value.Value as IHtmlContentContainer) != null)
                    {
                        valueAsContainer.MoveTo(destination);
                    }
                    else
                    {
                        destination.AppendHtml((IHtmlContent)value.Value);
                    }
                }
            }

            for (var i = 0; i < Count; i++)
            {
                var page = this[i];
                Array.Clear(page.Buffer, 0, page.Count);
                _bufferScope.ReturnSegment(page.Buffer);
            }

            Clear();
        }

        private void MoveTo(ViewBuffer destination)
        {
            for (var i = 0; i < Count; i++)
            {
                var page = this[i];

                var destinationPage = destination.Count == 0 ? null : destination[destination.Count - 1];

                // If the source page is less or equal to than half full, let's copy it's content to the destination
                // page if possible.
                var isLessThanHalfFull = 2 * page.Count <= page.Capacity;
                if (isLessThanHalfFull &&
                    destinationPage != null &&
                    destinationPage.Capacity - destinationPage.Count >= page.Count)
                {
                    // We have room, let's copy the items.
                    Array.Copy(
                        sourceArray: page.Buffer,
                        sourceIndex: 0,
                        destinationArray: destinationPage.Buffer,
                        destinationIndex: destinationPage.Count,
                        length: page.Count);

                    destinationPage.Count += page.Count;

                    // Now we can return the source page, and it can be reused in the scope of this request.
                    Array.Clear(page.Buffer, 0, page.Count);
                    _bufferScope.ReturnSegment(page.Buffer);

                }
                else
                {
                    // Otherwise, let's just add the source page to the other buffer.
                    destination.AddPage(page);
                }

            }

            Clear();
        }

        private class EncodingWrapper : IHtmlContent
        {
            private readonly string _unencoded;

            public EncodingWrapper(string unencoded)
            {
                _unencoded = unencoded;
            }

            public void WriteTo(TextWriter writer, HtmlEncoder encoder)
            {
                encoder.Encode(writer, _unencoded);
            }
        }
    }
}
