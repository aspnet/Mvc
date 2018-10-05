using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace MvcSandbox
{
    public class TranslationRoute : Route
    {
        private readonly Dictionary<string, Translations> _translations;

        public TranslationRoute(
            Dictionary<string, Translations> translations,
            IRouter target,
            string routeName,
            string routeTemplate,
            RouteValueDictionary defaults,
            IDictionary<string, object> constraints,
            RouteValueDictionary dataTokens,
            IInlineConstraintResolver inlineConstraintResolver)
            : base(target, routeName, routeTemplate, defaults, constraints, dataTokens, inlineConstraintResolver)
        {
            _translations = translations;
        }

        public override VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            var language = context.Values["language"] as string ?? context.AmbientValues["language"] as string;
            if (language != null && _translations.TryGetValue(language, out var translations))
            {
                // Forces the matching route to be used
                context.Values["language"] = language;

                context.Values["area"] = translations.Translate(context.Values["area"] as string ?? context.AmbientValues["area"] as string);
                context.Values["action"] = translations.Translate(context.Values["action"] as string);
                context.Values["controller"] = translations.Translate(context.Values["controller"] as string);
            }

            return base.GetVirtualPath(context);
        }

        protected override Task OnRouteMatched(RouteContext context)
        {
            var language = context.RouteData.Values["language"] as string;
            if (language != null && _translations.TryGetValue(language, out var translations))
            {
                context.RouteData.Values["area"] = translations.LookupInvariant(context.RouteData.Values["area"] as string);
                context.RouteData.Values["action"] = translations.LookupInvariant(context.RouteData.Values["action"] as string);
                context.RouteData.Values["controller"] = translations.LookupInvariant(context.RouteData.Values["controller"] as string);
            }

            return base.OnRouteMatched(context);
        }
    }
}
