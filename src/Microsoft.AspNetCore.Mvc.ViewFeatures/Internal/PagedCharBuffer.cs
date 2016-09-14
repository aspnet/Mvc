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

        public PagedCharBuffer(ICharBufferSource bufferSource)
        {
            BufferSource = bufferSource;
        }

        public ICharBufferSource BufferSource { get; }

        public IList<char[]> Pages { get; } = new List<char[]>();

        public int Length
        {
            get
            {
                var length = _charIndex;
                if (Pages.Count > 1)
                {
                    length += PageSize * (Pages.Count - 1);
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
            for (var i = Pages.Count - 1; i > 0; i--)
            {
                var page = Pages[i];

                try
                {
                    Pages.RemoveAt(i);
                }
                finally
                {
                    BufferSource.Return(page);
                }
            }

            _charIndex = 0;
        }

        private char[] GetCurrentPage()
        {
            if (CurrentPage == null ||
                _charIndex == CurrentPage.Length)
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
                Pages.Add(page);
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
            for (var i = 0; i < Pages.Count; i++)
            {
                BufferSource.Return(Pages[i]);
            }

            Pages.Clear();
        }
    }
}
