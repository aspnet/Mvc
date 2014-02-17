using System.Reflection;

namespace Microsoft.AspNet.Mvc
{
    public interface IActionDiscoveryConventions
    {
        bool IsController(TypeInfo typeInfo);

        ActionConvention GetRestAction(MethodInfo methodInfo);

        ActionConvention GetRpcAction(MethodInfo methodInfo);
    }
}
