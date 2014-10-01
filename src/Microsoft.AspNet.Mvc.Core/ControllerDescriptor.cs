// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.AspNet.Mvc
{
    public class ControllerDescriptor
    {
        public ControllerDescriptor(TypeInfo controllerTypeInfo)
        {
            if (controllerTypeInfo == null)
            {
                throw new ArgumentNullException("controllerTypeInfo");
            }

            ControllerTypeInfo = controllerTypeInfo;
        }

        public string Name { get; set; }

        public TypeInfo ControllerTypeInfo { get; private set; }
    }
}
