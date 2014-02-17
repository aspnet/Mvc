using System.Reflection;

namespace Microsoft.AspNet.Mvc
{
    public interface IControllerActionConventions
    {
        bool IsController(TypeInfo typeInfo);

        bool TryRestAction(MethodInfo methodInfo, out string actionName, out string[] httpMethods);

        bool TryRpcAction(MethodInfo methodInfo, out string actionName, out string[] httpMethods);
    }
}
