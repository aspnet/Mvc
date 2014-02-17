using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNet.Mvc
{
    public class TypeMethodBasedActionDescriptorProvider : IActionDescriptorProvider
    {
        private readonly IControllerAssemblyProvider _controllerAssemblyProvider;
        private readonly IControllerActionConventions _conventions;
        private readonly IControllerDescriptorFactory _controllerDescriptorFactory;

        public TypeMethodBasedActionDescriptorProvider(IControllerAssemblyProvider controllerAssemblyProvider,
                                                       IControllerActionConventions conventions,
                                                       IControllerDescriptorFactory controllerDescriptorFactory)
        {
            _controllerAssemblyProvider = controllerAssemblyProvider;
            _conventions = conventions;
            _controllerDescriptorFactory = controllerDescriptorFactory;
        }

        public IEnumerable<ActionDescriptor> GetDescriptors()
        {
            var assemblies = _controllerAssemblyProvider.Assemblies;
            var types = assemblies.SelectMany(a => a.DefinedTypes);
            var controllers = types.Where(_conventions.IsController);
            var controllerDescriptors = controllers.Select(t => _controllerDescriptorFactory.CreateControllerDescriptor(t)).ToArray();

            foreach (var cd in controllerDescriptors)
            {
                foreach (var methodInfo in cd.ControllerTypeInfo.DeclaredMethods)
                {
                    // Rest comes ahead of RPC in priority because Rest method looks like RPC as well.
                    string[] httpMethods;
                    string actionName;

                    if (_conventions.TryRestAction(methodInfo, out actionName, out httpMethods))
                    {
                        var d = BuildDescriptor(cd, methodInfo, actionName);

                        ApplyRest(d, httpMethods);

                        yield return d;
                    }
                    else if (_conventions.TryRpcAction(methodInfo, out actionName, out httpMethods))
                    {
                        var d = BuildDescriptor(cd, methodInfo, actionName);

                        ApplyRpc(d, actionName, httpMethods);

                        yield return d;
                    }
                }
            }
        }

        private static TypeMethodBasedActionDescriptor BuildDescriptor(ControllerDescriptor controllerDescriptor, MethodInfo methodInfo, string actionName)
        {
            return new TypeMethodBasedActionDescriptor
            {
                RouteConstraints = new List<RouteDataActionConstraint>
                {
                    new RouteDataActionConstraint("controller", controllerDescriptor.Name)
                },

                Name = actionName,
                ControllerDescriptor = controllerDescriptor,
                MethodInfo = methodInfo,
            };
        }

        private static void ApplyRest(TypeMethodBasedActionDescriptor descriptor, IEnumerable<string> httpMethods)
        {
            descriptor.MethodConstraints = new List<HttpMethodConstraint>
            {
                new HttpMethodConstraint(httpMethods)
            };

            descriptor.RouteConstraints.Add(new RouteDataActionConstraint("action", RouteKeyHandling.DenyKey));
        }

        private static void ApplyRpc(TypeMethodBasedActionDescriptor descriptor, string actionName, IEnumerable<string> httpMethods)
        {
            descriptor.RouteConstraints.Add(new RouteDataActionConstraint("action", actionName));

            // rest action require specific methods, but RPC actions do not.
            if (httpMethods != null)
            {
                descriptor.MethodConstraints = new List<HttpMethodConstraint>
                {
                    new HttpMethodConstraint(httpMethods)
                };
            }
        }
    }
}
