using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    public class ValidateAntiForgeryTokenAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly AntiForgery _antiForgery;
        
        public ValidateAntiForgeryTokenAuthorizationFilter([NotNull] AntiForgery antiForgery)
        {
            _antiForgery = antiForgery;
        }

        public async Task OnAuthorizationAsync([NotNull] AuthorizationContext context)
        {
            await _antiForgery.ValidateAsync(context.HttpContext);
        }
    }
}