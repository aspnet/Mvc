// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            "PATCH",
        };

        public virtual bool IsController([NotNull] TypeInfo typeInfo)
        {
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

        // If the convention is All methods starting with Get do not have an action name,
        // for a input GetXYZ methodInfo, the return value will be
        // { { HttpMethods = "GET", ActionName = "GetXYZ", RequireActionNameMatch = false }}
        public virtual IEnumerable<ActionInfo> GetActions(
            [NotNull] MethodInfo methodInfo,
            [NotNull] TypeInfo controllerTypeInfo)
        {
            if (!IsValidActionMethod(methodInfo))
            {
                return null;
            }

            var actionInfos = GetActionsForMethodsWithCustomAttributes(methodInfo);
            if (actionInfos.Any())
            {
                return actionInfos;
            }
            else
            {
                actionInfos = GetActionsForMethodsWithoutCustomAttributes(methodInfo, controllerTypeInfo);
            }

            return actionInfos;
        }

        protected virtual bool IsValidActionMethod(MethodInfo method)
        {
            return
                method.IsPublic &&
                !method.IsAbstract &&
                !method.IsConstructor &&
                !method.IsGenericMethod &&

                // The SpecialName bit is set to flag members that are treated in a special way by some compilers 
                // (such as property accessors and operator overloading methods).
                !method.IsSpecialName &&
                !method.IsDefined(typeof(NonActionAttribute));
        }

        public virtual IEnumerable<string> GetSupportedHttpMethods(MethodInfo methodInfo)
        {
            var supportedHttpMethods =
                _supportedHttpMethodsByConvention.FirstOrDefault(
                    httpMethod => methodInfo.Name.Equals(httpMethod, StringComparison.OrdinalIgnoreCase));
            
            if (supportedHttpMethods != null)
            {
                yield return supportedHttpMethods;
            }
        }

        private bool HasCustomAttributes(MethodInfo methodInfo)
        {
            var actionAttributes = GetActionCustomAttributes(methodInfo);
            return actionAttributes.Any();
        }

        private ActionAttributes GetActionCustomAttributes(MethodInfo methodInfo)
        {
            var attributes = methodInfo.GetCustomAttributes();
            var actionNameAttribute = attributes.OfType<ActionNameAttribute>().FirstOrDefault();
            var httpMethodConstraints = attributes.OfType<IActionHttpMethodProvider>();
            return new ActionAttributes()
            {
                HttpMethodProviderAttributes = httpMethodConstraints,
                ActionNameAttribute = actionNameAttribute
            };
        }

        private IEnumerable<ActionInfo> GetActionsForMethodsWithCustomAttributes(MethodInfo methodInfo)
        {
            var actionAttributes = GetActionCustomAttributes(methodInfo);
            if (!actionAttributes.Any())
            {
                // If the action is not decorated with any of the attributes, 
                // it would be handled by convention.
                yield break;
            }

            var actionNameAttribute = actionAttributes.ActionNameAttribute;
            var actionName = actionNameAttribute != null ? actionNameAttribute.Name : methodInfo.Name;

            var httpMethodProviders = actionAttributes.HttpMethodProviderAttributes;
            var httpMethods = httpMethodProviders.SelectMany(x => x.HttpMethods).Distinct().ToArray();

            yield return new ActionInfo()
            {
                HttpMethods = httpMethods,
                ActionName = actionName,
                RequireActionNameMatch = true
            };
        }

        private IEnumerable<ActionInfo> GetActionsForMethodsWithoutCustomAttributes(MethodInfo methodInfo, TypeInfo controllerTypeInfo)
        {
            var actionInfos = new List<ActionInfo>();
            var httpMethods = GetSupportedHttpMethods(methodInfo);
            if (httpMethods != null && httpMethods.Any())
            {
                return new[]
                {
                    new ActionInfo()
                    {
                        HttpMethods = httpMethods.ToArray(),
                        ActionName = methodInfo.Name,
                        RequireActionNameMatch = false,
                    }
                };
            }

            actionInfos.Add(
                new ActionInfo()
                {
                    ActionName = methodInfo.Name,
                    RequireActionNameMatch = true,
                });

            return actionInfos;
        }

        private class ActionAttributes
        {
            public IEnumerable<IActionHttpMethodProvider> HttpMethodProviderAttributes { get; set; }
            public ActionNameAttribute ActionNameAttribute { get; set; }

            public bool Any()
            {
                return ActionNameAttribute != null || HttpMethodProviderAttributes.Any();
            }
        }
    }
}