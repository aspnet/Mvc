using System;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc
{
    public class MediaTypeCollection : Collection<StringSegment>
    {
        public MediaTypeCollection()
        {
        }

        public void Add(MediaTypeHeaderValue item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Add(new StringSegment(item.ToString()));
        }

        public void Insert(int index, MediaTypeHeaderValue item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Insert(index, new StringSegment(item.ToString()));
        }

        public void Remove(MediaTypeHeaderValue item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Remove(new StringSegment(item.ToString()));
        }
    }
}
