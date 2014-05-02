using Microsoft.AspNet.Abstractions;
namespace Microsoft.AspNet.Mvc.Core.Test
{
    // An ITokenStore that can be passed to MoQ
    public abstract class MockableTokenStore : ITokenStore
    {
        public abstract object GetCookieToken(HttpContext httpContext);
        public abstract object GetFormToken(HttpContext httpContext);
        public abstract void SaveCookieToken(HttpContext httpContext, object token);

        AntiForgeryToken ITokenStore.GetCookieToken(HttpContext httpContext)
        {
            return (AntiForgeryToken)GetCookieToken(httpContext);
        }

        void ITokenStore.SaveCookieToken(HttpContext httpContext, AntiForgeryToken token)
        {
            SaveCookieToken(httpContext, (AntiForgeryToken)token);
        }

        #pragma warning disable 1998
        async System.Threading.Tasks.Task<AntiForgeryToken> ITokenStore.GetFormTokenAsync(HttpContext httpContext)
        {
            return (AntiForgeryToken)GetFormToken(httpContext);
        }
        #pragma warning restore 1998
    }
}