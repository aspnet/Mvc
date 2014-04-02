using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Abstractions;

namespace Microsoft.AspNet.Mvc.Rendering
{
    internal static class ActionCacheProvider
    {
        public static readonly string CacheItemId = Guid.NewGuid().ToString();

        internal static Dictionary<string, ActionCacheItem> GetActionCache(HtmlHelper html)
        {
            HttpContextBase context = html.ViewContext.HttpContext;
            Dictionary<string, ActionCacheItem> result;

            if (!context.Items.Contains(CacheItemId))
            {
                result = new Dictionary<string, ActionCacheItem>();
                context.Items[CacheItemId] = result;
            }
            else
            {
                result = (Dictionary<string, ActionCacheItem>)context.Items[CacheItemId];
            }

            return result;
        }
    }
}