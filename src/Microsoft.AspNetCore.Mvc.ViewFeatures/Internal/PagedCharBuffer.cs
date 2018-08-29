﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public class PagedCharBuffer : IDisposable
    {
        public const int PageSize = 1024;
        private int _charIndex;
        private List<char[]> _pages = new List<char[]>();

        public PagedCharBuffer(ICharBufferSource bufferSource)
        {
            BufferSource = bufferSource;
        }

        public ICharBufferSource BufferSource { get; }

        public IList<char[]> Pages => _pages;

        public int Length
        {
            get
            {
                var length = _charIndex;
                var pages = _pages;
                var fullPages = pages.Count - 1;
                for (var i = 0; i < fullPages; i++)
                {
                    length += pages[i].Length;
                }

                return length;
            }
        }

        private char[] CurrentPage { get; set; }

        public void Append(char value)
        {
            var page = GetCurrentPage();
            page[_charIndex++] = value;
        }

        public void Append(string value)
        {
            if (value == null)
            {
                return;
            }

            var index = 0;
            var count = value.Length;

            while (count > 0)
            {
                var page = GetCurrentPage();
                var copyLength = Math.Min(count, page.Length - _charIndex);
                Debug.Assert(copyLength > 0);

                value.CopyTo(
                    index,
                    page,
                    _charIndex,
                    copyLength);

                _charIndex += copyLength;
                index += copyLength;

                count -= copyLength;
            }
        }

        public void Append(char[] buffer, int index, int count)
        {
            while (count > 0)
            {
                var page = GetCurrentPage();
                var copyLength = Math.Min(count, page.Length - _charIndex);
                Debug.Assert(copyLength > 0);

                Array.Copy(
                    buffer,
                    index,
                    page,
                    _charIndex,
                    copyLength);

                _charIndex += copyLength;
                index += copyLength;
                count -= copyLength;
            }
        }

        /// <summary>
        /// Return all but one of the pages to the <see cref="ICharBufferSource"/>.
        /// This way if someone writes a large chunk of content, we can return those buffers and avoid holding them
        /// for extended durations.
        /// </summary>
        public void Clear()
        {
            var pages = _pages;
            for (var i = pages.Count - 1; i > 0; i--)
            {
                var page = pages[i];

                try
                {
                    pages.RemoveAt(i);
                }
                finally
                {
                    BufferSource.Return(page);
                }
            }

            _charIndex = 0;
            CurrentPage = pages.Count > 0 ? pages[0] : null;
        }

        private char[] GetCurrentPage()
        {
            if (CurrentPage == null || _charIndex == CurrentPage.Length)
            {
                CurrentPage = NewPage();
                _charIndex = 0;
            }

            return CurrentPage;
        }

        private char[] NewPage()
        {
            char[] page = null;
            try
            {
                page = BufferSource.Rent(PageSize);
                _pages.Add(page);
            }
            catch when (page != null)
            {
                BufferSource.Return(page);
                throw;
            }

            return page;
        }

        public void Dispose()
        {
            var pages = _pages;
            var count = pages.Count;
            for (var i = 0; i < count; i++)
            {
                BufferSource.Return(pages[i]);
            }

            pages.Clear();
        }
    }
}
