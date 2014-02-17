using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNet.Mvc
{
    public class TypeMethodBasedActionDescriptorProvider : IActionDescriptorProvider
    {
        private readonly IControllerAssemblyProvider _controllerAssemblyProvider;
        private readonly IActionDiscoveryConventions _conventions;
        private readonly IControllerDescriptorFactory _controllerDescriptorFactory;

        public TypeMethodBasedActionDescriptorProvider(IControllerAssemblyProvider controllerAssemblyProvider,
                                                       IActionDiscoveryConventions conventions,
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

                    ActionConvention convention = _conventions.GetRestAction(methodInfo);
                    if (convention != null)
                    {
                        var d = BuildDescriptor(cd, methodInfo, convention.ActionName);

                        ApplyRest(d, convention.HttpMethods);

                        yield return d;
                    }
                    else
                    {
                        convention = _conventions.GetRpcAction(methodInfo);

                        if (convention != null)
                        {
                            var d = BuildDescriptor(cd, methodInfo, convention.ActionName);

                            ApplyRpc(d, convention);

                            yield return d;
                        }
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

        private static void ApplyRpc(TypeMethodBasedActionDescriptor descriptor, ActionConvention convention)
        {
            descriptor.RouteConstraints.Add(new RouteDataActionConstraint("action", convention.ActionName));

            var methods = convention.HttpMethods;

            // rest action require specific methods, but RPC actions do not.
            if (methods != null)
            {
                descriptor.MethodConstraints = new List<HttpMethodConstraint>
                {
                    new HttpMethodConstraint(methods)
                };
            }
        }
    }
}
