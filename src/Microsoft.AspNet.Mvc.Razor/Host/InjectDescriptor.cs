// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Represents information about an injected property.
    /// </summary>
    public class InjectDescriptor
    {
        public InjectDescriptor(string typeName, string memberName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, "typeName");
            }

            if (string.IsNullOrEmpty(memberName))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, "memberName");
            }

            TypeName = typeName;
            MemberName = memberName;
        }

        /// <summary>
        /// Gets the type name of the injected property
        /// </summary>
        public string TypeName { get; private set; }

        /// <summary>
        /// Gets the name of the injected property.
        /// </summary>
        public string MemberName { get; private set; }
    }
}