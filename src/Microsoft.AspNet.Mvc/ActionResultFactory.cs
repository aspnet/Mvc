using System;

namespace Microsoft.AspNet.Mvc
{
    public class ActionResultFactory : IActionResultFactory
    {
        public IActionResult CreateActionResult(Type declaredReturnType, object actionReturnValue, RequestContext requestContext)
        {
            if (actionReturnValue is int)
            {
                return new HttpStatusCodeResult((int)actionReturnValue);
            }

            return new ContentResult
            {
                ContentType = "text/plain",
                Content = Convert.ToString(actionReturnValue),
            };
        }
    }
}
