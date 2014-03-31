using Microsoft.AspNet.Mvc.Filters;

namespace Microsoft.AspNet.Mvc
{
    // Provides configuration information about the anti-forgery system.
    public interface IAntiForgeryConfig
    {
        // Provides additional data to go into the tokens.
        IAntiForgeryAdditionalDataProvider AdditionalDataProvider { get; }

        // Name of the cookie to use.
        string CookieName { get; }

        // Name of the form field to use.
        string FormFieldName { get; }

        // Whether SSL is mandatory for this request.
        bool RequireSSL { get; }

        // Skip ClaimsIdentity & related logic.
        bool SuppressIdentityHeuristicChecks { get; }

        // Skip X-FRAME-OPTIONS header.
        bool SuppressXFrameOptionsHeader { get; }
    }
}
