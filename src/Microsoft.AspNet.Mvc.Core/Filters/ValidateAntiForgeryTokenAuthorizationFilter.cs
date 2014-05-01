using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    public class ValidateAntiForgeryTokenAuthorizationFilter : IAsyncAuthorizationFilter, IOrderedFilter
    {
        private AntiForgery _antiForgeryInstance;

        public int Order { get; private set; }

        public ValidateAntiForgeryTokenAuthorizationFilter([NotNull] AntiForgery antiForgeryInstance)
        {
            _antiForgeryInstance = antiForgeryInstance;
            Order = -1;
        }

        public async Task OnAuthorizationAsync([NotNull] AuthorizationContext context)
        {
            await _antiForgeryInstance.ValidateAsync(context.HttpContext);
        }
    }
}