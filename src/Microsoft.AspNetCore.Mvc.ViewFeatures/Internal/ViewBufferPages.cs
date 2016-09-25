using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    internal struct ViewBufferPages : IReadOnlyList<ViewBufferPage>
    {
        private ViewBufferPage _page;

        private List<ViewBufferPage> _pages;

        public int Count
        {
            get
            {
                if (_pages != null)
                {
                    return _pages.Count;
                }
                if (_page != null)
                {
                    return 1;
                }
                return 0;
            }
        }

        internal ViewBufferPages Add(ViewBufferPage page)
        {
            switch (Count)
            {
                case 0:
                    _page = page;
                    break;
                case 1:
                    _pages = new List<ViewBufferPage>();
                    _pages.Add(_page);
                    _pages.Add(page);
                    _page = null;
                    break;
                default:
                    _pages.Add(page);
                    break;
            }

            return this;
        }

        public ViewBufferPage this[int index]
        {
            get
            {
                if (_pages != null)
                {
                    return _pages[index];
                }
                if (index == 0 && _page != null)
                {
                    return _page;
                }
                throw new IndexOutOfRangeException();
            }
        }

        internal ViewBufferPages Clear()
        {
            _page = null;
            _pages = null;

            return this;
        }

        #region IEnumerable

        IEnumerator<ViewBufferPage> IEnumerable<ViewBufferPage>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        private struct Enumerator : IEnumerator<ViewBufferPage>
        {
            private readonly ViewBufferPages _pages;

            private int _index;


            public Enumerator(ViewBufferPages pages)
            {
                _pages = pages;
                _index = -1;
            }

            object IEnumerator.Current => _pages[_index];

            ViewBufferPage IEnumerator<ViewBufferPage>.Current => _pages[_index];

            void IDisposable.Dispose()
            {
            }

            bool IEnumerator.MoveNext()
            {
                _index++;
                return _index < _pages.Count;
            }

            void IEnumerator.Reset()
            {
                _index = -1;
            }
        }

        #endregion
    }
}
