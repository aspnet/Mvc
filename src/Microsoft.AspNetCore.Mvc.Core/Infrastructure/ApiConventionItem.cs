// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Microsoft.AspNetCore.Mvc.Infrastructure
{
    /// <summary>
    /// Metadata associated with an action method via API convention.
    /// </summary>
    public sealed class ApiConventionItem
    {
        public ApiConventionItem(IReadOnlyList<IApiResponseMetadataProvider> responseMetadataProviders)
        {
            ResponseMetadataProviders = responseMetadataProviders ??
                throw new ArgumentNullException(nameof(responseMetadataProviders));
        }

        public IReadOnlyList<IApiResponseMetadataProvider> ResponseMetadataProviders { get; }

        internal static ApiConventionItem GetApiConventionItem(MethodInfo method, ApiConventionAttribute[] apiConventionAttributes)
        {
            foreach (var attribute in apiConventionAttributes)
            {
                var conventionMethod = GetConventionMethod(method, attribute.ConventionType);
                if (conventionMethod != null)
                {
                    var metadataProviders = conventionMethod.GetCustomAttributes(inherit: false)
                        .OfType<IApiResponseMetadataProvider>()
                        .ToArray();

                    return new ApiConventionItem(metadataProviders);
                }
            }

            return null;
        }

        private static MethodInfo GetConventionMethod(MethodInfo method, Type conventionType)
        {
            foreach (var conventionMethod in conventionType.GetMethods())
            {
                if (IsMatch(method, conventionMethod))
                {
                    return conventionMethod;
                }
            }

            return null;
        }

        internal static bool IsMatch(MethodInfo methodInfo, MethodInfo conventionMethod)
        {
            var methodNameMatchBehavior = GetNameMatchBehavior(conventionMethod);
            if (!IsNameMatch(methodInfo.Name, conventionMethod.Name, methodNameMatchBehavior))
            {
                return false;
            }

            var methodParameters = methodInfo.GetParameters();
            var conventionMethodParameters = conventionMethod.GetParameters();

            for (var i = 0; i < conventionMethodParameters.Length; i++)
            {
                var conventionParameter = conventionMethodParameters[i];
                if (conventionParameter.IsDefined(typeof(ParamArrayAttribute)))
                {
                    return true;
                }

                if (methodParameters.Length <= i)
                {
                    return false;
                }

                var nameMatchBehavior = GetNameMatchBehavior(conventionParameter);
                var typeMatchBehavior = GetTypeMatchBehavior(conventionParameter);

                if (!IsTypeMatch(methodParameters[i].ParameterType, conventionParameter.ParameterType, typeMatchBehavior) ||
                    !IsNameMatch(methodParameters[i].Name, conventionParameter.Name, nameMatchBehavior))
                {
                    return false;
                }
            }

            // Ensure convention has at least as many parameters as the method. params convention argument are handled
            // inside the for loop.
            return methodParameters.Length == conventionMethodParameters.Length;

            ApiConventionNameMatchBehavior GetNameMatchBehavior(ICustomAttributeProvider attributeProvider)
            {
                var attributes = attributeProvider.GetCustomAttributes(inherit: false);
                for (var i = 0; i < attributes.Length; i++)
                {
                    if (attributes[i] is ApiConventionNameMatchAttribute nameMatchAttribute)
                    {
                        return nameMatchAttribute.MatchBehavior;
                    }
                }

                return ApiConventionNameMatchBehavior.Exact;
            }

            ApiConventionTypeMatchBehavior GetTypeMatchBehavior(ParameterInfo parameter)
            {
                var typeMatchAttribute = parameter.GetCustomAttribute<ApiConventionTypeMatchAttribute>(inherit: false);
                return typeMatchAttribute?.MatchBehavior ?? ApiConventionTypeMatchBehavior.Exact;
            }
        }

        internal static bool IsNameMatch(string name, string conventionName, ApiConventionNameMatchBehavior nameMatchBehavior)
        {
            switch (nameMatchBehavior)
            {
                case ApiConventionNameMatchBehavior.Any:
                    return true;

                case ApiConventionNameMatchBehavior.Exact:
                    return string.Equals(name, conventionName, StringComparison.Ordinal);

                case ApiConventionNameMatchBehavior.Prefix:
                    return IsNameMatchPrefix();

                case ApiConventionNameMatchBehavior.Suffix:
                    return IsNameMatchSuffix();

                default:
                    return false;
            }

            bool IsNameMatchPrefix()
            {
                if (!name.StartsWith(conventionName, StringComparison.Ordinal))
                {
                    return false;
                }

                if (name.Length == conventionName.Length)
                {
                    return true;
                }

                return char.IsUpper(name[conventionName.Length]);
            }

            bool IsNameMatchSuffix()
            {
                // name = id, conventionName = id
                if (string.Equals(name, conventionName, StringComparison.Ordinal))
                {
                    return true;
                }

                if (name.Length <= conventionName.Length)
                {
                    return false;
                }

                // name = personId, conventionName = id
                var index = name.Length - conventionName.Length - 1;
                if (!char.IsLower(name[index]))
                {
                    return false;
                }

                index++;
                if (name[index] != char.ToUpper(conventionName[0]))
                {
                    return false;
                }

                index++;
                return string.Compare(name, index, conventionName, 1, conventionName.Length - 1, StringComparison.Ordinal) == 0;
            }
        }

        internal static bool IsTypeMatch(Type type, Type conventionType, ApiConventionTypeMatchBehavior typeMatchBehavior)
        {
            switch (typeMatchBehavior)
            {
                case ApiConventionTypeMatchBehavior.Any:
                    return true;

                case ApiConventionTypeMatchBehavior.Exact:
                    return type == conventionType;

                case ApiConventionTypeMatchBehavior.AssignableFrom:
                    return conventionType.IsAssignableFrom(type);

                default:
                    return false;
            }
        }
    }
}
