using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNet.Mvc.ModelBinding.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class QueryStringValueProviderFactory : IValueProviderFactory
    {
        private static readonly object _cacheKey = new object();

        public IValueProvider GetValueProvider(ActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            // Process the query string once-per request. 
            IDictionary<object, object> storage = actionContext.HttpContext.Items;
            object value;
            if (!storage.TryGetValue(_cacheKey, out value))
            {
                var provider = new QueryStringValueProvider(actionContext.HttpContext, CultureInfo.InvariantCulture);
                storage[_cacheKey] = provider;
                return provider;
            }

            return (QueryStringValueProvider)value;
        }
    }
}
