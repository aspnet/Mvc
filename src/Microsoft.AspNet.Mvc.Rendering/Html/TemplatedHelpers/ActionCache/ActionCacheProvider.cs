using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Abstractions;

namespace Microsoft.AspNet.Mvc.Rendering
{
    internal static class ActionCacheProvider
    {
        public static readonly string CacheItemId = Guid.NewGuid().ToString();

        public static Dictionary<string, ActionCacheItem> GetActionCacheItem(HttpContext httpContext)
        {
            Dictionary<string, ActionCacheItem> result;

            if (!httpContext.Items.Any(obj => obj.Equals(CacheItemId)))
            {
                result = new Dictionary<string, ActionCacheItem>(StringComparer.OrdinalIgnoreCase);
                httpContext.Items[CacheItemId] = result;
            }
            else
            {
                result = (Dictionary<string, ActionCacheItem>)httpContext.Items[CacheItemId];
            }

            return result;
        }
    }
}
