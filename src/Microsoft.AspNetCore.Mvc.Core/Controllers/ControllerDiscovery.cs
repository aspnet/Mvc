// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.Controllers
{
    public static class ControllerDiscovery
    {
        private const string ControllerTypeNameSuffix = "Controller";

        public static bool IsControllerType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!type.IsClass)
            {
                return false;
            }

            if (type.IsAbstract)
            {
                return false;
            }

            // We only consider public top-level classes as controllers. IsPublic returns false for nested
            // classes, regardless of visibility modifiers
            if (!type.IsPublic)
            {
                return false;
            }

            if (type.ContainsGenericParameters)
            {
                return false;
            }

            if (type.IsDefined(typeof(NonControllerAttribute)))
            {
                return false;
            }

            if (!type.Name.EndsWith(ControllerTypeNameSuffix, StringComparison.OrdinalIgnoreCase) &&
                !type.IsDefined(typeof(ControllerAttribute)))
            {
                return false;
            }

            return true;
        }
    }
}
