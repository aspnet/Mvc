using System.Security.Claims;
using Microsoft.AspNet.Abstractions;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    // A TokenProvider that can be passed to MoQ
    public abstract class MockableTokenProvider : ITokenValidator, ITokenGenerator
    {
        public abstract object GenerateCookieToken();
        public abstract object GenerateFormToken(HttpContext httpContext, ClaimsIdentity identity, object cookieToken);
        public abstract bool IsCookieTokenValid(object cookieToken);
        public abstract void ValidateTokens(HttpContext httpContext, ClaimsIdentity identity, object cookieToken, object formToken);

        AntiForgeryToken ITokenGenerator.GenerateCookieToken()
        {
            return (AntiForgeryToken)GenerateCookieToken();
        }

        AntiForgeryToken ITokenGenerator.GenerateFormToken(HttpContext httpContext, ClaimsIdentity identity, AntiForgeryToken cookieToken)
        {
            return (AntiForgeryToken)GenerateFormToken(httpContext, identity, (AntiForgeryToken)cookieToken);
        }

        bool ITokenValidator.IsCookieTokenValid(AntiForgeryToken cookieToken)
        {
            return IsCookieTokenValid((AntiForgeryToken)cookieToken);
        }

        void ITokenValidator.ValidateTokens(HttpContext httpContext, ClaimsIdentity identity, AntiForgeryToken cookieToken, AntiForgeryToken formToken)
        {
            ValidateTokens(httpContext, identity, (AntiForgeryToken)cookieToken, (AntiForgeryToken)formToken);
        }
    }
}