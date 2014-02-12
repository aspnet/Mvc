namespace Microsoft.AspNet.Mvc
{
    public class ControllerActionBasedRouteContextProvider : IRouteContextProvider
    {
        public RouteContext CreateDescriptor(RequestContext requestContext)
        {
            string controllerName = (string)requestContext.RouteValues["controller"];
            string actionName = (string)requestContext.RouteValues["action"];

            return new ControllerActionRouteContext
            {
                ControllerName = controllerName,
                ActionName = actionName
            };
        }
    }
}
