using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;

namespace MvcSample.Web
{
    public class LanguageViewLocationExpander : IViewLocationExpander
    {
        public IImmutableList<string> ExpandViewLocations(IImmutableList<string> viewLocations, 
                                                   IDictionary<string, string> values)
        {
            var result = viewLocations;
            if (values.ContainsKey("language-short"))
            {
                result = result.InsertRange(0, viewLocations.Select(v => v.Replace("{view}", "{language-short}/{view}")));
            }

            if (values.ContainsKey("language"))
            {
                result = viewLocations.InsertRange(0, viewLocations.Select(v => v.Replace("{view}", "{language}/{view}")));
            }

            return result;
        }

        public Task PopulateValuesAsync(ActionContext actionContext, IDictionary<string, string> values)
        {
            var request = actionContext.HttpContext.Request;
            var language = request.Query["language"];
            if (!string.IsNullOrEmpty(language))
            {
                values["language"] = language;
            }
            else
            {
                // en-gb,en,quality=0.8
                var acceptLanguageTokens = request.Headers["Accept-Language"].Split(',');
                values["language"] = acceptLanguageTokens[0];
                values["language-short"] = acceptLanguageTokens[1];
            }

            return Task.FromResult(0);
        }
    }
}