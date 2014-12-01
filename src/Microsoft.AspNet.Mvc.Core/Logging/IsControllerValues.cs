// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Logged to indicate the state of a class during controller discovery. Logs the type
    /// of the controller as well as the <see cref="ControllerStatus"/>.
    /// </summary>
    public class IsControllerValues : LoggerStructureBase
    {
        public IsControllerValues([NotNull] TypeInfo typeInfo)
        {
            Type = typeInfo.AsType();
            if (!typeInfo.IsClass)
            {
                Status = ControllerStatus.IsNotAClass;
            }
            else if (typeInfo.IsAbstract)
            {
                Status = ControllerStatus.IsAbstract;
            }
            else if (!typeInfo.IsPublic)
            {
                Status = ControllerStatus.IsNotPublicOrTopLevel;
            }
            else if (typeInfo.ContainsGenericParameters)
            {
                Status = ControllerStatus.ContainsGenericParameters;
            }
            else if (typeInfo.Name.Equals("Controller", StringComparison.OrdinalIgnoreCase))
            {
                Status = ControllerStatus.NameIsController;
            }
            else if (!typeInfo.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
            {
                Status = ControllerStatus.DoesNotEndWithController;
            }
            else if (!typeof(Controller).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                Status = ControllerStatus.IsNotAssignable;
            }
            else
            {
                Status = ControllerStatus.IsController;
            }
        }

        public Type Type { get; }

        public ControllerStatus Status { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}