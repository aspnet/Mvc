using System;

namespace Microsoft.AspNet.Mvc.Razor
{
    public struct ViewLocationCacheItem
    {
        public ViewLocationCacheItem(Func<IRazorPage> razorPageFactory, string location)
        {
            PageFactory = razorPageFactory;
            Location = location;
        }

        public string Location { get; }

        public Func<IRazorPage> PageFactory { get; }
    }
}
