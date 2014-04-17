using Microsoft.AspNet.Mvc;

namespace MvcSample.Web.Filters
{
    public class BlockAnonymous : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(AuthorizationContext context)
        {
            if (!HasAllowAnonymous(context))
            
            var userAnonymous = 
                user == null || 
                user.Identity == null || 
                !user.Identity.IsAuthenticated;

            if( userAnonymous &&
                !context.HasAllowAnonymous())
            {
                context.Result = new HttpStatusCodeResult(401);
            }
        }
    }
}