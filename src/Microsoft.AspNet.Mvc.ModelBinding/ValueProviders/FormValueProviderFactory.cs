﻿using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNet.Abstractions;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class FormValueProviderFactory : IValueProviderFactory
    {
        private const string FormEncodedConentType = "application/form-url-encoded";

        public async Task<IValueProvider> GetValueProvider(RequestContext requestContext)
        {
            var request = requestContext.HttpContext.Request;
            
            if (IsSupportedContentType(request))
            {
                var queryCollection = await request.GetFormAsync();
            
                // TODO: Tracked via https://github.com/aspnet/Helios/issues/2. Determine what's the right way to 
                // map Accept-Language to culture. 
                var culture = CultureInfo.CurrentCulture;

                return new ReadableStringCollectionValueProvider(queryCollection, culture);
            }

            return null;
        }

        private bool IsSupportedContentType(HttpRequest request)
        {
            var contentType = request.Headers["Content-Type"];
            return !String.IsNullOrEmpty(contentType) && 
                   contentType.Equals(FormEncodedConentType, StringComparison.OrdinalIgnoreCase);
        }
    }
}
