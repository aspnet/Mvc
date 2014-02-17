using System;
using System.Reflection;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultActionDiscoveryConventions : IActionDiscoveryConventions
    {
        private static readonly string[] _supportedHttpMethodsByConvention = 
        { 
            "GET", 
            "POST", 
            "PUT", 
            "DELETE", 
            "HEAD", 
            "OPTIONS", 
            "PATCH",
        };

        public virtual bool IsController(TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException("typeInfo");
            }

            if (!typeInfo.IsClass ||
                typeInfo.IsAbstract ||
                typeInfo.ContainsGenericParameters)
            {
                return false;
            }

            if (typeInfo.Name.Equals("Controller", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return typeInfo.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase) ||
                   typeof(Controller).GetTypeInfo().IsAssignableFrom(typeInfo);

        }

        public ActionConvention GetRestAction(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException("methodInfo");
            }

            if (!IsValidMethod(methodInfo))
            {
                return null;
            }

            for (var i = 0; i < _supportedHttpMethodsByConvention.Length; i++)
            {
                if (methodInfo.Name.StartsWith(_supportedHttpMethodsByConvention[i], StringComparison.OrdinalIgnoreCase))
                {
                    return new ActionConvention()
                    {
                        HttpMethods = new[] { _supportedHttpMethodsByConvention[i] },
                        ActionName = methodInfo.Name,
                    };
                }
            }

            return null;
        }

        public ActionConvention GetRpcAction(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException("methodInfo");
            }

            if (!IsValidMethod(methodInfo))
            {
                return null;
            }

            // support action name attribute
            return new ActionConvention()
            {
                ActionName = methodInfo.Name,
            };
        }

        public virtual bool IsValidMethod(MethodInfo method)
        {
            return
                method.IsPublic &&
                !method.IsAbstract &&
                !method.IsConstructor &&
                !method.IsGenericMethod &&
                !method.IsSpecialName;
        }
    }
}
