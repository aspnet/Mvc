using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.AspNet.Mvc
{
    public class ControllerActionConventions : IControllerActionConventions
    {
        private static readonly string[] _supportedHttpMethodsByConvention = 
        { 
            "Get", 
            "Post", 
            "Put", 
            "Delete", 
            "Head", 
            "Options", 
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
                   typeof (Controller).GetTypeInfo().IsAssignableFrom(typeInfo);

        }

        public bool TryRestAction(MethodInfo methodInfo, out string actionName, out string[] httpMethods)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException("methodInfo");
            }

            actionName = null;
            httpMethods = null;

            if (!IsValidMethod(methodInfo))
            {
                return false;
            }

            for (var i = 0; i < _supportedHttpMethodsByConvention.Length; i++)
            {
                if (methodInfo.Name.StartsWith(_supportedHttpMethodsByConvention[i], StringComparison.OrdinalIgnoreCase))
                {
                    httpMethods = new[] {_supportedHttpMethodsByConvention[i]};
                    actionName = methodInfo.Name;
                    return true;
                }
            }

            return false;
        }

        public bool TryRpcAction(MethodInfo methodInfo, out string actionName, out string[] httpMethods)
        {
            httpMethods = null;

            if (methodInfo == null)
            {
                throw new ArgumentNullException("methodInfo");
            }

            if (!IsValidMethod(methodInfo))
            {
                actionName = null;
                return false;
            }

            // support action name attribute
            actionName = methodInfo.Name;

            return true;
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
