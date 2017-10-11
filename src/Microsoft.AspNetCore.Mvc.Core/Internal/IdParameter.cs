// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public static class IdParameter
    {
        // Check if the parameter is named "id" (e.g. int id) or ends in Id (e.g. personId)
        public static bool IsIdParameter(ApiParameterDescription parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return IsIdParameter(parameter.Name);
        }

        // Check if the parameter is named "id" (e.g. int id) or ends in Id (e.g. personId)
        public static bool IsIdParameter(ParameterDescriptor parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return IsIdParameter(parameter.Name);
        }

        private static bool IsIdParameter(string name)
        {
            if (name == null)
            {
                return false;
            }

            if (string.Equals("id", name, StringComparison.Ordinal))
            {
                return true;
            }

            // We're looking for a name ending with Id, but preceded by a lower case letter. This should match
            // the normal PascalCase naming conventions.
            if (name.Length >= 3 &&
                name.EndsWith("Id", StringComparison.Ordinal) &&
                char.IsLower(name, name.Length - 3))
            {
                return true;
            }

            return false;
        }
    }
}
